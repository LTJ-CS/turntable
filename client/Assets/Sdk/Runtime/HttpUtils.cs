using System;
using System.Collections.Generic;
using System.Text;
using AccountProto;
using ClientProto;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using LitJson;
using Sdk.Runtime.Base;
using Sdk.Runtime.Crypto;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequest;
using EHttpErrorCode = AccountProto.EHttpErrorCode;

namespace Sdk.Runtime
{
    public enum WebRequestResult
    {
        Success,      // 请求成功
        NetworkError, // 网络错误 连接超时 网络不可用等
        ServerError,  // 服务器返回的错误 EHttpErrorCode
    }

    /// <summary>
    /// 一些访问 GO Http 服务器的辅助函数
    /// </summary>
    public static class HttpUtils
    {
        const string JsonContentType     = "application/json";
        const string ProtobufContentType = "application/x-protobuf";

        private static readonly MessageParser<LogicMsg_Ack> LogicParser = new(() => new LogicMsg_Ack());
        private static          ICrypto                     _crypto     = new XorCrypto();

        /// <summary>
        /// 定义 go gin http 服务器的返回结果
        /// </summary>
        private struct GoHttpResult<TData>
        {
            /// <summary>
            /// 错误码
            /// </summary>
            public int err_code;

            /// <summary>
            /// 错误信息
            /// </summary>
            public string err_msg;

            /// <summary>
            /// 返回数据
            /// </summary>
            public TData data;
        }

        /// <summary>
        /// 向服务器发送 json 请求
        /// </summary>
        /// <param name="loginReq">发送请求的 json 内容</param>
        /// <param name="url">发送的 url</param>
        /// <param name="isPost">是否使用 post 方式，否则使用 get 方式</param>
        /// <param name="secret">密钥</param>
        /// <param name="setTimeout">是否设置超时时间</param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="paramMap">额外的参数，可选</param>
        /// <returns></returns>
        public static async UniTask<(WebRequestResult, string error, JsonData jsonData)> JsonAsync(JsonData loginReq, string url, bool isPost = true, string secret = null, bool setTimeout = false,
                                                                                                   int timeOut = 10, Dictionary<string, string> paramMap = null)
        {
            // 构造完整 URL（如果有查询参数）
            if (paramMap is { Count: > 0 })
            {
                var queryParts = new List<string>();
                foreach (var (k, v) in paramMap)
                {
                    queryParts.Add($"{EscapeURL(k)}={EscapeURL(v)}");
                }
                var query = string.Join("&", queryParts);
                url = $"{url}?{query}";
                Debug.Log($"JsonAsync url with query : {url}");
            }

            var json = JsonMapper.ToJson(loginReq);
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, isPost ? kHttpVerbPOST : kHttpVerbGET);
            if (setTimeout)
                request.timeout = timeOut;
            request.uploadHandler = new UploadHandlerRaw(bodyRaw)
                                    {
                                        contentType = JsonContentType
                                    };
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", JsonContentType);
            if (!string.IsNullOrEmpty(secret))
            {
                request.SetRequestHeader("ad_sec_v2", secret);
                request.SetRequestHeader("User-Agent", "easygame2021/1.0.0");
            }
            try
            {
                Debug.Log("request" + json);
                Debug.Log("url" + url);
                // 发送请求并等待
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogError($"请求 {url} 失败 : {e.Message}");
                // 服务器错误
                return (WebRequestResult.NetworkError, e.Message, null);
            }

            // 检查是否有错误发生
            if (request.result != Result.Success)
            {
                Debug.LogError($"请求({url} : {json})失败. error: {request.error}, result: {request.result}");
                return (WebRequestResult.NetworkError, request.error, null);
            }

            return (WebRequestResult.Success, string.Empty, JsonMapper.ToObject(request.downloadHandler.text));
        }

        public static async UniTask<(WebRequestResult webRequestResult, TRet retData, string errMsg)> ProtoAsync<TSend, TRet>(TSend req, string url, bool isPost = true, string secret = null)
            where TSend: IMessage<TSend>, new()
            where TRet: IMessage<TRet>, new()
        {
            var bodyRaw = req.ToByteArray();

            using var request = new UnityWebRequest(url, isPost ? kHttpVerbPOST : kHttpVerbGET);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw)
                                    {
                                        contentType = ProtobufContentType
                                    };
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", ProtobufContentType);
            if (!string.IsNullOrEmpty(secret))
            {
                request.SetRequestHeader("ad_sec_v2", secret);
                request.SetRequestHeader("User-Agent", "easygame2021/1.0.0");
            }
            try
            {
                // 发送请求并等待
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogError($"请求 {url} 失败 : {e.Message}");
                // 服务器错误
                return (WebRequestResult.NetworkError, default, "请求 url 失败");
            }

            // 检查是否有错误发生
            if (request.result != Result.Success)
            {
                Debug.LogError($"请求({url})失败. error: {request.error}, result: {request.result}");
                return (WebRequestResult.NetworkError, default, $"请求({url})失败. error: {request.error}, result: {request.result}");
            }
            if (request.downloadHandler.data == null || request.downloadHandler.data.Length == 0)
            {
                Debug.LogError($"请求({url} : 失败. 服务器返回数据为空 text {request.downloadHandler.text}");
                return (WebRequestResult.ServerError, default, request.downloadHandler.text);
            }

            // 把 gin 返回的数据解包
            var parser = new MessageParser<TRet>(() => new TRet());
            var retData = parser.ParseFrom(request.downloadHandler.data);
            return (WebRequestResult.Success, retData, string.Empty);
        }

        /// <summary>
        /// 获取cdn资源文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async UniTask<(byte[], Result result, long requestCode, string)> GetCdnAsync(string url)
        {
            Result result = Result.Success;
            long requestCode = 0;
            string errMsg = string.Empty;
            // 进行 3 次重试
            for (var i = 0; i < 3; i++)
            {
                using var request = Get(url);
                // 设置 10 秒超时
                request.timeout = 10;
                try
                {
                    // 发送请求并等待
                    await request.SendWebRequest();
                }
                catch (Exception e)
                {
                    Debug.LogError($"请求CDN {url} 失败 : {e.Message}");
                    result = request.result;
                    requestCode = request.responseCode;
                    errMsg = e.Message;
                    // 服务器错误
                    continue;
                }

                result = request.result;
                requestCode = request.responseCode;
                errMsg = request.error;
                // 检查是否有错误发生
                if (request.result != Result.Success)
                {
                    Debug.LogError($"请求({url})失败. error: {request.error}, result: {request.result}");
                    continue;
                }

                return (request.downloadHandler.data, result, requestCode, errMsg);
            }

            return (null, result, requestCode, errMsg);
        }

        /// <summary>
        /// 向游戏服务器发送 protobuf 请求
        /// </summary>
        /// <param name="requestUrl">请求的 url</param>
        /// <param name="param">传递的参数</param>
        /// <param name="jwt">jwt 的密钥</param>
        /// <typeparam name="TSend">发送的 proto 结构</typeparam>
        /// <typeparam name="TRet">返回的 proto 结构</typeparam>
        /// <returns></returns>
        public static async UniTask<(WebRequestResult webRequestResult, TRet retData, string errMsg)> PostProtoAsync<TSend, TRet>(string requestUrl, TSend param, string jwt)
            where TSend: IMessage<TSend>, new()
            where TRet: IMessage<TRet>, new()
        {
            var body = new LogicMsgRequest()
                       {
                           Data = ByteString.CopyFrom(_crypto.Encrypt(jwt, ToByteArray(param))) 
                       };
            var bodyRaw = body.ToByteArray();
            using var request = new UnityWebRequest(requestUrl, kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw)
                                    {
                                        contentType = ProtobufContentType
                                    };
            request.timeout = 10;
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", ProtobufContentType);
            // TODO: 设置客户端版本方便服务器识别
            request.SetRequestHeader("b", GameVersion.GameVer);
            // 如果有 token，则传入 token
            if (!string.IsNullOrEmpty(jwt))
            {
                request.SetRequestHeader("t", jwt);
            }

            try
            {
                // 发送请求并等待
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                // 服务器错误
                return (WebRequestResult.NetworkError, default, $"{e.Message}");
            }

            // 检查是否有错误发生
            if (request.result != Result.Success)
            {
                Debug.LogError($"请求({requestUrl} : 失败. error: {request.error}, result: {request.result}");
                return (WebRequestResult.NetworkError, default, request.result.ToString());
            }

            if (request.downloadHandler.data == null || request.downloadHandler.data.Length == 0)
            {
                Debug.LogError($"请求({requestUrl} : 失败. 服务器返回数据为空 text {request.downloadHandler.text}");
                return (WebRequestResult.ServerError, default, request.downloadHandler.text);
            }

            // 把 gin 返回的数据解包
            var goHttpResult = LogicParser.ParseFrom(request.downloadHandler.data);
            // TODO: 逻辑层失败处理
            if (goHttpResult.ErrorCode != EHttpErrorCode.Success)
            {
                Debug.LogError($"请求({requestUrl}:{typeof(TSend)})失败. error: {goHttpResult.ErrorCode} {goHttpResult.ErrorMsg}");

                return (WebRequestResult.ServerError, default, goHttpResult.ErrorCode.ToString());
            }

            var parser = new MessageParser<TRet>(() => new TRet());
            var retData = parser.ParseFrom(goHttpResult.Data);
            return (WebRequestResult.Success, retData, string.Empty);
        }

        /// <summary>
        /// 向游戏服务器发送 protobuf 请求
        /// </summary>
        /// <param name="requestUrl">请求的 url</param>
        /// <param name="param">传递的参数</param>
        /// <param name="jwt">jwt 的密钥</param>
        /// <returns></returns>
        public static async UniTask<(WebRequestResult webRequestResult, string retData, string errMsg)> PostTestProtoAsync(string requestUrl, IMessage param, string jwt, Type recvType)
        {
            var body = new LogicMsgRequest()
                       {
                           Data = ByteString.CopyFrom(_crypto.Encrypt(jwt, ToByteArray(param))) 
                       };
            var bodyRaw = body.ToByteArray();
            using var request = new UnityWebRequest(requestUrl, kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw)
                                    {
                                        contentType = ProtobufContentType
                                    };
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", ProtobufContentType);
            // TODO: 设置客户端版本方便服务器识别
            request.SetRequestHeader("b", GameVersion.GameVer);
            // 如果有 token，则传入 token
            if (!string.IsNullOrEmpty(jwt))
            {
                request.SetRequestHeader("t", jwt);
            }

            try
            {
                // 发送请求并等待
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                // 服务器错误
                return (WebRequestResult.NetworkError, default, $"{e.Message}");
            }

            // 检查是否有错误发生
            if (request.result != Result.Success)
            {
                Debug.LogError($"请求({requestUrl} : 失败. error: {request.error}, result: {request.result}");
                return (WebRequestResult.NetworkError, default, request.result.ToString());
            }

            if (request.downloadHandler.data == null || request.downloadHandler.data.Length == 0)
            {
                Debug.LogError($"请求({requestUrl} : 失败. 服务器返回数据为空 text {request.downloadHandler.text}");
                return (WebRequestResult.ServerError, default, request.downloadHandler.text);
            }

            // 把 gin 返回的数据解包
            var goHttpResult = LogicParser.ParseFrom(request.downloadHandler.data);
            // TODO: 逻辑层失败处理
            if (goHttpResult.ErrorCode != EHttpErrorCode.Success)
            {
                return (WebRequestResult.ServerError, default, goHttpResult.ErrorCode.ToString());
            }
            // 通过反射获取 parse 方法
            var method = recvType.GetMethod("get_Parser", new Type[]
                                                          {
                                                          }); // 获取方法信息
            var parser = method.Invoke(null, null);           // 调用方法
            var retData = ((MessageParser)parser).ParseFrom(goHttpResult.Data);
            var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
            var json = jsonFormatter.Format(retData);
            return (WebRequestResult.Success, json, string.Empty);
        }

        /// <summary>
        /// 返回消息体对应的 OpCode
        /// </summary>
        /// <param name="message">指定要获取OpCode的消息体</param>
        /// <returns></returns>
        public static ushort GetMessageOpCode(IMessage message)
        {
            ushort opCode = 0;
            // 自动在消息结构体中查找定义的 OpCode
            foreach (var enumDescriptor in message.Descriptor.EnumTypes)
            {
                if (enumDescriptor.Name == "N")
                {
                    opCode = (ushort)enumDescriptor.FindValueByName("OpCode").Number;
                    break;
                }
            }

            if (opCode == 0)
            {
                throw new Exception($"未在消息体({message.Descriptor.Name})中找到对应的 OpCode 定义!");
            }

            return opCode;
        }

        /// <summary>
        /// 把给定的消息对象序列化成 byte[]
        /// </summary>
        /// <param name="message">指定要序列化的消息对象</param>
        /// <returns></returns>
        public static byte[] ToByteArray(IMessage message)
        {
            // 消息格式: 版本号 | OpCode | 消息内容(PB)
            //          1字节 | 2字节   |  n字节
            const int headerSize = 3;
            ProtoPreconditions.CheckNotNull(message, "message");
            var result = new byte[message.CalculateSize() + headerSize];
            var output = new CodedOutputStream(result);

            var opCode = GetMessageOpCode(message);
            // 这里必须与服务器保持一致
            // 写入 opcode. 即写入一个 Uint16
            var b0 = (byte)0;
            var b1 = (byte)((opCode & 0xff00) >> 8);
            var b2 = (byte)(opCode & 0xff);
            output.WriteRawTag(b0, b1, b2);

            // 写入消息体内容
            message.WriteTo(output);
            output.CheckNoSpaceLeft();

            return result;
        }

        /// <summary>
        /// Get请求一个文本文件
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public static async UniTask<(WebRequestResult webRequestResult, string retData, string errMsg)> GetTextFileAsync(string requestUrl)
        {
            using var request = new UnityWebRequest(requestUrl, kHttpVerbGET);
            request.timeout = 10;
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                // 发送请求并等待
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                // 服务器错误
                return (WebRequestResult.NetworkError, string.Empty, "网络错误");
            }

            // 检查是否有错误发生
            if (request.result != Result.Success)
            {
                Debug.LogError($"请求({requestUrl})失败. error: {request.error}, result: {request.result}");
                return (WebRequestResult.NetworkError, string.Empty, request.result.ToString());
            }

            if (request.downloadHandler.text == null || string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError($"请求({requestUrl})失败. 服务器返回数据为空 text {request.downloadHandler.text}");
                return (WebRequestResult.ServerError, string.Empty, request.downloadHandler.text ?? "Empty response");
            }

            var retData = Encoding.UTF8.GetString(request.downloadHandler.data); // request.downloadHandler.text
            return (WebRequestResult.Success, retData, string.Empty);
        }

        public static async UniTask<(WebRequestResult, string error, JsonData jsonData)> JsonAsyncWithToken(JsonData loginReq, string url, string jwt, bool isPost = true, string secret = null,
                                                                                                            bool setTimeout = false, int timeOut = 10)
        {
            var json = JsonMapper.ToJson(loginReq);
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, isPost ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbGET);
            if (setTimeout)
                request.timeout = timeOut;
            request.uploadHandler = new UploadHandlerRaw(bodyRaw)
                                    {
                                        contentType = JsonContentType
                                    };
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", JsonContentType);
            request.SetRequestHeader("b", GameVersion.GameVer);
            if (!string.IsNullOrEmpty(jwt))
            {
                request.SetRequestHeader("t", jwt);
            }
            if (!string.IsNullOrEmpty(secret))
            {
                request.SetRequestHeader("ad_sec_v2", secret);
                request.SetRequestHeader("User-Agent", "easygame2021/1.0.0");
            }
            try
            {
                // 发送请求并等待
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogError($"请求 {url} 失败 : {e.Message}");
                // 服务器错误
                return (WebRequestResult.NetworkError, e.Message, null);
            }

            // 检查是否有错误发生
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"请求({url} : {json})失败. error: {request.error}, result: {request.result}");
                return (WebRequestResult.NetworkError, request.error, null);
            }

            return (WebRequestResult.Success, string.Empty, JsonMapper.ToObject(request.downloadHandler.text));
        }
    }
}