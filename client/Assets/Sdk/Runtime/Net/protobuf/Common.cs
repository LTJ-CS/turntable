// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: common.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace ClientProto {

  /// <summary>Holder for reflection information generated from common.proto</summary>
  public static partial class CommonReflection {

    #region Descriptor
    /// <summary>File descriptor for common.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static CommonReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cgxjb21tb24ucHJvdG8SC0NsaWVudFByb3RvIloKDlJhbmtQbGF5ZXJJbmZv",
            "EgoKAmlkGAEgASgJEgwKBG5hbWUYAiABKAkSEQoJaGVhZEltYWdlGAMgASgJ",
            "EgwKBHJhbmsYBCABKAUSDQoFc2NvcmUYBSABKAMiIwoISXRlbUluZm8SCgoC",
            "aWQYASABKAUSCwoDbnVtGAIgASgDIkwKEEZ1bmN0aW9uT3BlbkluZm8SFAoM",
            "ZnVuY3Rpb25UeXBlGAEgASgFEhEKCXN0YXJ0VGltZRgCIAEoAxIPCgdlbmRU",
            "aW1lGAMgASgDKnoKCFBsYXRmb3JtEgsKB1VOS05PV04QABIGCgJXWBABEgYK",
            "AlFREAISBgoCVFQQAxIICgRWSVZPEAQSCAoET1BQTxAFEgYKAktTEAYSCAoE",
            "Sk9ZTxAHEgYKAkhXEAgSBwoDWmZiEAkSBwoDWGhzEAoSCQoEQkVUQRDnByrv",
            "BQoORUh0dHBFcnJvckNvZGUSBwoDTmlsEAASCwoHU3VjY2VzcxABEhIKDVBh",
            "cmFtc0ludmFsaWQQkU4SDgoJU3lzdGVtRXJyEJJOEhEKDFVuYXV0aG9yaXpl",
            "ZBCTThIQCgtEYXRhYmFzZUVychCUThIRCgxVc2VyTm90TG9naW4QlU4SEAoL",
            "VXNlclNpZ25FcnIQlk4SFAoPVXNlck5vdEV4aXN0RXJyEJdOEhgKE1VzZXJB",
            "bHJlYWR5RXhpc3RFcnIQmE4SFQoQSnNvblVubWFyc2hhbEVychCZThIPCgpE",
            "ZWNyeXB0RXJyEJpOEhYKEVNvY2tldE5vdEV4aXN0RXJyEJtOEhwKF0FkVmlk",
            "ZW9PdXRPZk1heFRpbWVzRXJyEJxOEhQKD1N5c3RlbVVubG9ja0VychCdThIO",
            "CglDb25maWdFcnIQnk4SEgoNQ29udGVudFNlY0VychCfThIWChFTZXJ2ZXJO",
            "b3RTZXJ2ZUVychCgThILCgZOb0RhdGEQoU4SFAoPTG9naW5WZXJzaW9uRXJy",
            "EKJOEhQKD05vTWF0Y2hQbGF5SW5mbxCjThIQCgtKV1RUb2tlbkVychCkThIZ",
            "ChRKV1RUb2tlbk91dE9mRGF0ZUVychClThIZChRKV1RUb2tlblVJRERlY29k",
            "ZUVychCmThIYChNKV1RUb2tlbk1vZGlmaWVkRXJyEKdOEhIKDVRvb01hbnlS",
            "ZXFFcnIQqE4SEwoOU2FtZUFzQ2FjaGVFcnIQqU4SEAoLSW50ZXJuYWxFcnIQ",
            "qk4SGAoTUGxhdGZvcm1Ob3RSaWdodEVychCrThIPCgpBY2NvdW50RXJyEKxO",
            "EhgKE0NsaWVudFZlcnNpb25Mb3dFcnIQrU4SGAoTV3hTZXNzaW9uS2V5SW52",
            "YWxpZBCuThIYChNXeFNlc3Npb25LZXlOb3RGaW5kEK9OEhAKC1Jlc3BEYXRh",
            "RXJyELBOEhgKE1BsYXRmb3JtTm90RXhpc3RFcnIQsU4qPAoLQWRXYXRjaFR5",
            "cGUSCwoHRGVmYXVsdBAAEhAKDEVuZXJneVN1cHBseRABEg4KCkdvbGRTdXBw",
            "bHkQAkIVWhMuL3Byb3RvYnVmO3Byb3RvYnVmYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::ClientProto.Platform), typeof(global::ClientProto.EHttpErrorCode), typeof(global::ClientProto.AdWatchType), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::ClientProto.RankPlayerInfo), global::ClientProto.RankPlayerInfo.Parser, new[]{ "Id", "Name", "HeadImage", "Rank", "Score" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ClientProto.ItemInfo), global::ClientProto.ItemInfo.Parser, new[]{ "Id", "Num" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::ClientProto.FunctionOpenInfo), global::ClientProto.FunctionOpenInfo.Parser, new[]{ "FunctionType", "StartTime", "EndTime" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum Platform {
    /// <summary>
    /// 未知（非法）
    /// </summary>
    [pbr::OriginalName("UNKNOWN")] Unknown = 0,
    /// <summary>
    /// 微信
    /// </summary>
    [pbr::OriginalName("WX")] Wx = 1,
    /// <summary>
    /// QQ
    /// </summary>
    [pbr::OriginalName("QQ")] Qq = 2,
    /// <summary>
    /// 抖音
    /// </summary>
    [pbr::OriginalName("TT")] Tt = 3,
    /// <summary>
    /// ViVo
    /// </summary>
    [pbr::OriginalName("VIVO")] Vivo = 4,
    /// <summary>
    /// Oppo
    /// </summary>
    [pbr::OriginalName("OPPO")] Oppo = 5,
    /// <summary>
    /// 快手
    /// </summary>
    [pbr::OriginalName("KS")] Ks = 6,
    /// <summary>
    /// 简游登录
    /// </summary>
    [pbr::OriginalName("JOYO")] Joyo = 7,
    /// <summary>
    /// 华为
    /// </summary>
    [pbr::OriginalName("HW")] Hw = 8,
    /// <summary>
    /// 支付宝
    /// </summary>
    [pbr::OriginalName("Zfb")] Zfb = 9,
    /// <summary>
    /// 小红书
    /// </summary>
    [pbr::OriginalName("Xhs")] Xhs = 10,
    /// <summary>
    /// 测试环境登录（仅测试环境可用）
    /// </summary>
    [pbr::OriginalName("BETA")] Beta = 999,
  }

  /// <summary>
  /// 定义 Http 的 ErrorCode
  /// </summary>
  public enum EHttpErrorCode {
    /// <summary>
    /// 无
    /// </summary>
    [pbr::OriginalName("Nil")] Nil = 0,
    /// <summary>
    /// 成功
    /// </summary>
    [pbr::OriginalName("Success")] Success = 1,
    /// <summary>
    /// 参数错误
    /// </summary>
    [pbr::OriginalName("ParamsInvalid")] ParamsInvalid = 10001,
    /// <summary>
    /// 系统错误
    /// </summary>
    [pbr::OriginalName("SystemErr")] SystemErr = 10002,
    /// <summary>
    /// 没有权限
    /// </summary>
    [pbr::OriginalName("Unauthorized")] Unauthorized = 10003,
    /// <summary>
    /// 数据库错误
    /// </summary>
    [pbr::OriginalName("DatabaseErr")] DatabaseErr = 10004,
    /// <summary>
    /// 没有登录
    /// </summary>
    [pbr::OriginalName("UserNotLogin")] UserNotLogin = 10005,
    /// <summary>
    /// 用户token过期，需要重新登录获取token
    /// </summary>
    [pbr::OriginalName("UserSignErr")] UserSignErr = 10006,
    /// <summary>
    /// 用户不存在
    /// </summary>
    [pbr::OriginalName("UserNotExistErr")] UserNotExistErr = 10007,
    /// <summary>
    /// 用户已存在
    /// </summary>
    [pbr::OriginalName("UserAlreadyExistErr")] UserAlreadyExistErr = 10008,
    /// <summary>
    /// JSON 解析失败
    /// </summary>
    [pbr::OriginalName("JsonUnmarshalErr")] JsonUnmarshalErr = 10009,
    /// <summary>
    /// 解密失败
    /// </summary>
    [pbr::OriginalName("DecryptErr")] DecryptErr = 10010,
    /// <summary>
    /// 长链接不存在
    /// </summary>
    [pbr::OriginalName("SocketNotExistErr")] SocketNotExistErr = 10011,
    /// <summary>
    /// 超出最大看视频次数
    /// </summary>
    [pbr::OriginalName("AdVideoOutOfMaxTimesErr")] AdVideoOutOfMaxTimesErr = 10012,
    /// <summary>
    /// 系统没解锁
    /// </summary>
    [pbr::OriginalName("SystemUnlockErr")] SystemUnlockErr = 10013,
    /// <summary>
    /// 配置错误
    /// </summary>
    [pbr::OriginalName("ConfigErr")] ConfigErr = 10014,
    /// <summary>
    /// 输入内容不合法
    /// </summary>
    [pbr::OriginalName("ContentSecErr")] ContentSecErr = 10015,
    /// <summary>
    /// 服务器不在服务中
    /// </summary>
    [pbr::OriginalName("ServerNotServeErr")] ServerNotServeErr = 10016,
    /// <summary>
    /// 没有此数据
    /// </summary>
    [pbr::OriginalName("NoData")] NoData = 10017,
    /// <summary>
    /// 登录版本过低
    /// </summary>
    [pbr::OriginalName("LoginVersionErr")] LoginVersionErr = 10018,
    /// <summary>
    /// 没有玩家的比赛数据
    /// </summary>
    [pbr::OriginalName("NoMatchPlayInfo")] NoMatchPlayInfo = 10019,
    /// <summary>
    /// token 错误
    /// </summary>
    [pbr::OriginalName("JWTTokenErr")] JwttokenErr = 10020,
    /// <summary>
    /// token 过期
    /// </summary>
    [pbr::OriginalName("JWTTokenOutOfDateErr")] JwttokenOutOfDateErr = 10021,
    /// <summary>
    /// token 中的 uid 解码失败
    /// </summary>
    [pbr::OriginalName("JWTTokenUIDDecodeErr")] JwttokenUiddecodeErr = 10022,
    /// <summary>
    /// token 被篡改
    /// </summary>
    [pbr::OriginalName("JWTTokenModifiedErr")] JwttokenModifiedErr = 10023,
    /// <summary>
    /// 访问太频繁
    /// </summary>
    [pbr::OriginalName("TooManyReqErr")] TooManyReqErr = 10024,
    /// <summary>
    /// 请求数据与缓存一致 304
    /// </summary>
    [pbr::OriginalName("SameAsCacheErr")] SameAsCacheErr = 10025,
    /// <summary>
    /// 内部服务错误
    /// </summary>
    [pbr::OriginalName("InternalErr")] InternalErr = 10026,
    /// <summary>
    /// 客户端平台不正确
    /// </summary>
    [pbr::OriginalName("PlatformNotRightErr")] PlatformNotRightErr = 10027,
    /// <summary>
    /// 账号异常，可能被封禁
    /// </summary>
    [pbr::OriginalName("AccountErr")] AccountErr = 10028,
    /// <summary>
    /// 客户端版本低需要升级到最新版本
    /// </summary>
    [pbr::OriginalName("ClientVersionLowErr")] ClientVersionLowErr = 10029,
    /// <summary>
    /// 微信 session key 无效
    /// </summary>
    [pbr::OriginalName("WxSessionKeyInvalid")] WxSessionKeyInvalid = 10030,
    /// <summary>
    /// 找不到客户端上传的key, 无法解密数据包
    /// </summary>
    [pbr::OriginalName("WxSessionKeyNotFind")] WxSessionKeyNotFind = 10031,
    /// <summary>
    /// 服务器返回的数据不正确
    /// </summary>
    [pbr::OriginalName("RespDataErr")] RespDataErr = 10032,
    /// <summary>
    /// 平台号不正确
    /// </summary>
    [pbr::OriginalName("PlatformNotExistErr")] PlatformNotExistErr = 10033,
  }

  /// <summary>
  /// 广告关卡类型
  /// </summary>
  public enum AdWatchType {
    [pbr::OriginalName("Default")] Default = 0,
    /// <summary>
    ///体力补给
    /// </summary>
    [pbr::OriginalName("EnergySupply")] EnergySupply = 1,
    /// <summary>
    ///金币补给
    /// </summary>
    [pbr::OriginalName("GoldSupply")] GoldSupply = 2,
  }

  #endregion

  #region Messages
  /// <summary>
  /// 排行榜玩家信息
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class RankPlayerInfo : pb::IMessage<RankPlayerInfo>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<RankPlayerInfo> _parser = new pb::MessageParser<RankPlayerInfo>(() => new RankPlayerInfo());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<RankPlayerInfo> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ClientProto.CommonReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public RankPlayerInfo() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public RankPlayerInfo(RankPlayerInfo other) : this() {
      id_ = other.id_;
      name_ = other.name_;
      headImage_ = other.headImage_;
      rank_ = other.rank_;
      score_ = other.score_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public RankPlayerInfo Clone() {
      return new RankPlayerInfo(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private string id_ = "";
    /// <summary>
    /// 玩家ID
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    /// <summary>
    /// 名称
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "headImage" field.</summary>
    public const int HeadImageFieldNumber = 3;
    private string headImage_ = "";
    /// <summary>
    /// 头像
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string HeadImage {
      get { return headImage_; }
      set {
        headImage_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "rank" field.</summary>
    public const int RankFieldNumber = 4;
    private int rank_;
    /// <summary>
    ///排名，从1开始
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Rank {
      get { return rank_; }
      set {
        rank_ = value;
      }
    }

    /// <summary>Field number for the "score" field.</summary>
    public const int ScoreFieldNumber = 5;
    private long score_;
    /// <summary>
    ///分数
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long Score {
      get { return score_; }
      set {
        score_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as RankPlayerInfo);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(RankPlayerInfo other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if (HeadImage != other.HeadImage) return false;
      if (Rank != other.Rank) return false;
      if (Score != other.Score) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (HeadImage.Length != 0) hash ^= HeadImage.GetHashCode();
      if (Rank != 0) hash ^= Rank.GetHashCode();
      if (Score != 0L) hash ^= Score.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (HeadImage.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(HeadImage);
      }
      if (Rank != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Rank);
      }
      if (Score != 0L) {
        output.WriteRawTag(40);
        output.WriteInt64(Score);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (HeadImage.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(HeadImage);
      }
      if (Rank != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Rank);
      }
      if (Score != 0L) {
        output.WriteRawTag(40);
        output.WriteInt64(Score);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (HeadImage.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(HeadImage);
      }
      if (Rank != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Rank);
      }
      if (Score != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Score);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(RankPlayerInfo other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.HeadImage.Length != 0) {
        HeadImage = other.HeadImage;
      }
      if (other.Rank != 0) {
        Rank = other.Rank;
      }
      if (other.Score != 0L) {
        Score = other.Score;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            HeadImage = input.ReadString();
            break;
          }
          case 32: {
            Rank = input.ReadInt32();
            break;
          }
          case 40: {
            Score = input.ReadInt64();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            HeadImage = input.ReadString();
            break;
          }
          case 32: {
            Rank = input.ReadInt32();
            break;
          }
          case 40: {
            Score = input.ReadInt64();
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  ///道具信息
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ItemInfo : pb::IMessage<ItemInfo>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ItemInfo> _parser = new pb::MessageParser<ItemInfo>(() => new ItemInfo());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ItemInfo> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ClientProto.CommonReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ItemInfo() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ItemInfo(ItemInfo other) : this() {
      id_ = other.id_;
      num_ = other.num_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ItemInfo Clone() {
      return new ItemInfo(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private int id_;
    /// <summary>
    /// 道具ID
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "num" field.</summary>
    public const int NumFieldNumber = 2;
    private long num_;
    /// <summary>
    ///数量
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long Num {
      get { return num_; }
      set {
        num_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ItemInfo);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ItemInfo other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Num != other.Num) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0) hash ^= Id.GetHashCode();
      if (Num != 0L) hash ^= Num.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Id != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Id);
      }
      if (Num != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(Num);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Id != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Id);
      }
      if (Num != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(Num);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Id);
      }
      if (Num != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Num);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ItemInfo other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      if (other.Num != 0L) {
        Num = other.Num;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Id = input.ReadInt32();
            break;
          }
          case 16: {
            Num = input.ReadInt64();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            Id = input.ReadInt32();
            break;
          }
          case 16: {
            Num = input.ReadInt64();
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// 功能开启信息
  /// 一般只发送通过后台控制的功能，其他功能客户端读配置文件；活动类型功能有开启和结束时间
  /// </summary>
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class FunctionOpenInfo : pb::IMessage<FunctionOpenInfo>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<FunctionOpenInfo> _parser = new pb::MessageParser<FunctionOpenInfo>(() => new FunctionOpenInfo());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<FunctionOpenInfo> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::ClientProto.CommonReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public FunctionOpenInfo() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public FunctionOpenInfo(FunctionOpenInfo other) : this() {
      functionType_ = other.functionType_;
      startTime_ = other.startTime_;
      endTime_ = other.endTime_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public FunctionOpenInfo Clone() {
      return new FunctionOpenInfo(this);
    }

    /// <summary>Field number for the "functionType" field.</summary>
    public const int FunctionTypeFieldNumber = 1;
    private int functionType_;
    /// <summary>
    /// 功能类型,对应excel的枚举
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int FunctionType {
      get { return functionType_; }
      set {
        functionType_ = value;
      }
    }

    /// <summary>Field number for the "startTime" field.</summary>
    public const int StartTimeFieldNumber = 2;
    private long startTime_;
    /// <summary>
    /// 开始时间
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long StartTime {
      get { return startTime_; }
      set {
        startTime_ = value;
      }
    }

    /// <summary>Field number for the "endTime" field.</summary>
    public const int EndTimeFieldNumber = 3;
    private long endTime_;
    /// <summary>
    /// 结束时间
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long EndTime {
      get { return endTime_; }
      set {
        endTime_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as FunctionOpenInfo);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(FunctionOpenInfo other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (FunctionType != other.FunctionType) return false;
      if (StartTime != other.StartTime) return false;
      if (EndTime != other.EndTime) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (FunctionType != 0) hash ^= FunctionType.GetHashCode();
      if (StartTime != 0L) hash ^= StartTime.GetHashCode();
      if (EndTime != 0L) hash ^= EndTime.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (FunctionType != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(FunctionType);
      }
      if (StartTime != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(StartTime);
      }
      if (EndTime != 0L) {
        output.WriteRawTag(24);
        output.WriteInt64(EndTime);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (FunctionType != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(FunctionType);
      }
      if (StartTime != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(StartTime);
      }
      if (EndTime != 0L) {
        output.WriteRawTag(24);
        output.WriteInt64(EndTime);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (FunctionType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(FunctionType);
      }
      if (StartTime != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(StartTime);
      }
      if (EndTime != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(EndTime);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(FunctionOpenInfo other) {
      if (other == null) {
        return;
      }
      if (other.FunctionType != 0) {
        FunctionType = other.FunctionType;
      }
      if (other.StartTime != 0L) {
        StartTime = other.StartTime;
      }
      if (other.EndTime != 0L) {
        EndTime = other.EndTime;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            FunctionType = input.ReadInt32();
            break;
          }
          case 16: {
            StartTime = input.ReadInt64();
            break;
          }
          case 24: {
            EndTime = input.ReadInt64();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            FunctionType = input.ReadInt32();
            break;
          }
          case 16: {
            StartTime = input.ReadInt64();
            break;
          }
          case 24: {
            EndTime = input.ReadInt64();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
