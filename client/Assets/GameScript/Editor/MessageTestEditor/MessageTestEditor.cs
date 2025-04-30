using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Client2Server;
using ClientProto;
using Google.Protobuf;
using Protocol;
using Server2Client;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameScript.Editor.MessageTestEditor
{
    public class MessageTestEditor : EditorWindow
    {
        private VisualElement _rootEle;
        
        private Dictionary<string, Type> _client2ServerMsgTypes = new ();
        private Dictionary<string, Type> _server2ClientMsgTypes = new ();
        
        /// <summary>
        /// ShowExample 创建窗口
        /// </summary>
        [MenuItem("Tools/MessageTestEditor")]
        public static void ShowExample()
        {
            MessageTestEditor wnd = GetWindow<MessageTestEditor>();
            wnd.titleContent = new GUIContent("MessageTestEditor");
        }

        /// <summary>
        /// 创建 GUI
        /// </summary>
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            _rootEle = rootVisualElement;
            
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/GameScript/Editor/MessageTestEditor/MessageTestEditor.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            _rootEle.Add(labelFromUXML);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/GameScript/Editor/MessageTestEditor/MessageTestEditor.uss");

            InitMsgTypes();
            var btnGenMsg = _rootEle.Q<Button>("btnGenMsg");
            var btnSendMsg = _rootEle.Q<Button>("btnSendMsg");
            btnGenMsg.RegisterCallback<ClickEvent>(OnGenMsg);
            btnSendMsg.RegisterCallback<ClickEvent>(OnSendMsg);
        }

        /// <summary>
        /// 初始化消息类型
        /// </summary>
        private void InitMsgTypes()
        {
            _client2ServerMsgTypes.Clear();
            _server2ClientMsgTypes.Clear();
            // 根据text生成需要发送的 proto 消息体
            var req = new LoginRequest()
                      {
                      };
            var msgType = req.GetType();
            var types = msgType.Assembly.ExportedTypes;
            var dropDownSendMsg = _rootEle.Q<DropdownField>("dropDownSendMsg");
            dropDownSendMsg.choices = new List<string>();
            var dropDownRecvMsg = _rootEle.Q<DropdownField>("dropDownRecvMsg");
            dropDownRecvMsg.choices = new List<string>();
            foreach (var type in types)
            {
                // 如果 type.Name 匹配正则表达式 Client2Server.* 或者 Server2Client.*
                var client2ServerMsgPattern = @"\w+Request$";;
                var server2ClientMsgPattern = @"\w+Response$";
                if (type.Namespace == "ClientProto" && Regex.IsMatch(type.Name, client2ServerMsgPattern))
                {
                    dropDownSendMsg.choices.Add(type.Name);
                    _client2ServerMsgTypes.Add(type.Name, type);
                }
                else if (type.Namespace == "ClientProto" && Regex.IsMatch(type.Name, server2ClientMsgPattern))
                {
                    dropDownRecvMsg.choices.Add(type.Name);
                    _server2ClientMsgTypes.Add(type.Name, type);
                }
            }
        }
        
        /// <summary>
        /// 生成 json 消息体
        /// </summary>
        /// <param name="evt"></param>
        private void OnGenMsg(ClickEvent evt)
        {
            // 获取发送消息的类型
            var sendProtoName = _rootEle.Q<DropdownField>("dropDownSendMsg").value;
            // 获取接受消息的类型
            var recvProtoName = _rootEle.Q<DropdownField>("dropDownRecvMsg").value;
            if (sendProtoName == null || recvProtoName == null)
            {
                EditorUtility.DisplayDialog("warn", "需要选择发送的消息和返回的消息", "ok");
                return;
            }
            // 生成消息体
            var sendProtoType = _client2ServerMsgTypes.GetValueOrDefault(sendProtoName);
            var req = Activator.CreateInstance(sendProtoType) as IMessage;
            // 将消息体转换成 json 格式用于编辑
            var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
            var json = jsonFormatter.Format(req);
            // 将 json 显示到界面上
            _rootEle.Q<TextField>("txtMsgContent").SetValueWithoutNotify(json);
        } 

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        /// <param name="evt"></param>
        private async void OnSendMsg(ClickEvent evt)
        {
            // 获取发送消息的类型
            var sendProtoName = _rootEle.Q<DropdownField>("dropDownSendMsg").value;
            var sendProtoType = _client2ServerMsgTypes[sendProtoName];
            // 获取消息体 json
            var sendProtoJson = _rootEle.Q<TextField>("txtMsgContent").value;
            // 将 json 转换成消息体
            var method = sendProtoType.GetMethod("get_Parser", new Type[] {}); // 获取方法信息
            var parser = method.Invoke(null, null); // 调用方法
            var sendProto =  ((MessageParser)parser).ParseJson(sendProtoJson);
            // 获取接受消息的类型
            var recvProtoName = _rootEle.Q<DropdownField>("dropDownRecvMsg").value;
            var recvProtoType = _server2ClientMsgTypes[recvProtoName];
            var rep = await NetManager.PostTestProtoAsync(sendProto, recvProtoType);
            if (this == null)
            {
                return;
            }
            // 将返回消息显示到界面上
            var txtMsgRecv = _rootEle.Q<TextField>("txtMsgRecv");
            txtMsgRecv.SetValueWithoutNotify(rep);
        }
    }
}