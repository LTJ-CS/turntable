using System.Text;
using ClientProto;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// 执行服务器gm
/// </summary>
public class GmEditor : EditorWindow
{
    [MenuItem("Tools/Gm")]
    public static void ShowExample()
    {
        GmEditor wnd = GetWindow<GmEditor>();
        wnd.titleContent = new GUIContent("GmEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;


        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/GameScript/Editor/Gm/GmEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/GameScript/Editor/Gm/GmEditor.uss");

        var submitButton = root.Q<Button>("SubmitButton");
        submitButton.RegisterCallback<ClickEvent>(OnSubmitButtonClick);

        // 获取GM列表
        InitGmList();
    }

    /// <summary>
    /// 初始GM列表
    /// </summary>
    private async void InitGmList()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var gmListView= rootVisualElement.Q<TextField>("GmListView");
       

        var response = await NetManager.PostLogicProtoAsync<GmListRequest, GmListResponse>(new GmListRequest(), showErrMsg: false);
        if (response.success)
        {
            StringBuilder sb= new StringBuilder();
            foreach (var gm in response.retData.GmList)
            {
                sb.Append(JsonConvert.SerializeObject(gm)).Append("\n");
            }
            gmListView.value = sb.ToString();
        }

    }

    /// <summary>
    /// 发送执行GM
    /// </summary>
    /// <param name="evt"></param>
    private async void OnSubmitButtonClick(ClickEvent evt)
    {
        var gmInput = rootVisualElement.Q<TextField>("GmInput");
        GmRequest gmRequest = new GmRequest();
        var gmCmd = gmInput.value;
        var split = gmCmd.Split(' ');
        gmRequest.Cmd = split[0];
        for (int i = 1; i < split.Length; i++)
        {
            gmRequest.Args.Add(split[i]);
        }

        var response = await NetManager.PostTestProtoAsync(gmRequest, typeof(GmResponse));
        rootVisualElement.Q<Label>("gmExecuteResultLabel").text = response;

    }

}