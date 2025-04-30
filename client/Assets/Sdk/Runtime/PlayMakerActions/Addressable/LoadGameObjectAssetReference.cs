using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Sdk.Runtime.PlayMakerActions.Addressable
{ 
    /// <summary>
    /// 加载 AssetReference 的 PlayMaker Action
    /// </summary>
    [ActionCategory("MyActions/Sdk/Addressable")]
    [HutongGames.PlayMaker.Tooltip("加载指定 GameObject 类型的 AssetReference 并可以存储到指定的变量中")]
    public class LoadGameObjectAssetReference : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        [RequiredField]
        [HutongGames.PlayMaker.Tooltip("要加载的 Prefab(GameObject)")]
        public AssetReferenceWrapper AssetWrapper;
        
        [UIHint(UIHint.Variable)]
        [HutongGames.PlayMaker.Tooltip("加载后实例化的 GameObject")]
        public FsmGameObject StoreObject;
        
        [HutongGames.PlayMaker.Tooltip("Event to send when the data has finished loading (progress = 1).")]
        public FsmEvent IsDone;
		
        [HutongGames.PlayMaker.Tooltip("Event to send if there was an error.")]
        public FsmEvent IsError;

        public override void InitEditor(Fsm fsmOwner)
        {
            base.InitEditor(fsmOwner);
            if (AssetWrapper == null)
            {
                AssetWrapper = ScriptableObject.CreateInstance<AssetReferenceWrapper>();
            }
        }
        
        public override void Reset()
        {
            if (AssetWrapper != null && AssetWrapper.asset.IsValid())
            {
                AssetWrapper.asset.ReleaseAsset();
            }
            AssetWrapper = ScriptableObject.CreateInstance<AssetReferenceWrapper>();;
            StoreObject = null;
        }

        public override void OnEnter()
        {
            if (AssetWrapper.asset.RuntimeKeyIsValid())
            {
                AssetWrapper.asset.LoadAssetAsync<GameObject>().Completed += (handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        StoreObject.Value = handle.Result;
                        Fsm.Event(IsDone);
                    }
                    else
                    {
                        Fsm.Event(IsError);
                    }
                    Finish();
                });
                return;
            }
            Finish();
        }
        
    }
}