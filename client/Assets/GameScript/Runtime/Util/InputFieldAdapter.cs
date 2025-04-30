//微信输入法适配
#if PLATFORM_WEIXIN
using TMPro;
using UnityEngine;
using WeChatWASM;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 添加 InputField 组件的依赖
[RequireComponent(typeof(TMP_InputField))]
public class InputFieldAdapter : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    private  TMP_InputField _inputField;
    private bool _isShowKeyboard = false;

    private void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        ShowKeyboard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
        if (!_inputField.isFocused)
        {
            HideKeyboard();
        }
    }

    private void OnInput(OnKeyboardInputListenerResult v)
    {
        Debug.Log("onInput");
        Debug.Log(v.value);
        if (_inputField.isFocused)
        {
            _inputField.text = v.value;
        }
    }

    private void OnConfirm(OnKeyboardInputListenerResult v)
    {
        // 输入法confirm回调
        Debug.Log("onConfirm");
        Debug.Log(v.value);
        HideKeyboard();
    }

    private void OnComplete(OnKeyboardInputListenerResult v)
    {
        // 输入法complete回调
        Debug.Log("OnComplete");
        Debug.Log(v.value);
        HideKeyboard();
    }

    private void ShowKeyboard()
    {
        if (_isShowKeyboard) return;
        
        WX.ShowKeyboard(new ShowKeyboardOption()
        {
            defaultValue = "",
            maxLength = 100,
            confirmType = "go"
        });

        //绑定回调
        WX.OnKeyboardConfirm(this.OnConfirm);
        WX.OnKeyboardComplete(this.OnComplete);
        WX.OnKeyboardInput(this.OnInput);
        _isShowKeyboard = true;
    }

    private void HideKeyboard()
    {
        if (!_isShowKeyboard) return;
        
        WX.HideKeyboard(new HideKeyboardOption());
        //删除掉相关事件监听
        WX.OffKeyboardInput(this.OnInput);
        WX.OffKeyboardConfirm(this.OnConfirm);
        WX.OffKeyboardComplete(this.OnComplete);
        _isShowKeyboard = false;
    }
}
//抖音输入法适配
// 
#elif PLATFORM_DOUYIN
 using StarkSDKSpace;
 using TMPro;
 using UnityEngine;
 using UnityEngine.EventSystems;
 using UnityEngine.UI;

 public class InputFieldAdapter : MonoBehaviour
 {
     private TMP_InputField input;

     private void Start()
     {
         input = gameObject.GetComponent<TMP_InputField>();
         SetInputTexts();
         RegisterKeyboardEvents();
     }

     private void SetInputTexts()
     {
         input.text = "";
         var comp = input.GetComponent<ClickableInputField>();
         if (comp == null)
         {
             comp = input.gameObject.AddComponent<ClickableInputField>();
         }
         comp.multiple = false;
         comp.confirmType = input.text;
     }

     private void OnDestroy()
     {
         UnregisterKeyboardEvents();
     }

     private void RegisterKeyboardEvents()
     {
         StarkSDK.API.GetStarkKeyboard().onKeyboardInputEvent += OnKeyboardInput;
         StarkSDK.API.GetStarkKeyboard().onKeyboardConfirmEvent += OnKeyboardConfirm;
         StarkSDK.API.GetStarkKeyboard().onKeyboardCompleteEvent += OnKeyboardComplete;
     }

     private void UnregisterKeyboardEvents()
     {
         StarkSDK.API.GetStarkKeyboard().onKeyboardInputEvent -= OnKeyboardInput;
         StarkSDK.API.GetStarkKeyboard().onKeyboardConfirmEvent -= OnKeyboardConfirm;
         StarkSDK.API.GetStarkKeyboard().onKeyboardCompleteEvent -= OnKeyboardComplete;
     }

     private void OnKeyboardInput(string value)
     {
         Debug.Log($"OnKeyboardInput: {value}");
         if (input.isFocused)
         {
             input.text = value;
         }
     }

     private void OnKeyboardConfirm(string value)
     {
         Debug.Log($"OnKeyboardConfirm: {value}");
     }

     private void OnKeyboardComplete(string value)
     {
         Debug.Log($"OnKeyboardComplete: {value}");
     }
 }
 public class ClickableInputField : EventTrigger
 {
     public  string         confirmType    = "go"; // 可选值有: "done", "next", "search", "go", "send"
     public  int            maxInputLength = 100;    // 最大输入长度
     public  bool           multiple       = false;  // 是否多行输入
     private TMP_InputField _inputField;
     private void Start()
     {
         _inputField = GetComponent<TMP_InputField>();
     }
     public override void OnPointerClick(PointerEventData eventData)
     {
         if (_inputField != null)
         {
             if (_inputField.isFocused)
             {
                 StarkSDK.API.GetStarkKeyboard().ShowKeyboard(new StarkKeyboard.ShowKeyboardOptions()
                 {
                     maxLength = maxInputLength,
                     multiple = multiple,
                     defaultValue = _inputField.text,
                     confirmType = confirmType
                 });
             }
         }
     }
 }
#else
using TMPro;
using UnityEngine;
public class InputFieldAdapter : MonoBehaviour
{
    private TMP_InputField input;
}
#endif