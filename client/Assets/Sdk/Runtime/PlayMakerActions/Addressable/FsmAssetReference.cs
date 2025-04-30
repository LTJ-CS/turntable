using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Sdk.Runtime.PlayMakerActions.Addressable
{
    /// <summary>
    /// AssetReference 的 Fsm 变量
    /// </summary>
    [Serializable]
    public class FsmAssetReference : NamedVariable
    {
        [SerializeField]
        private AssetReference value;

        public event Action OnChange;

        public AssetReference Value
        {
            get => CastVariable == null ? value : CastVariable.RawValue as AssetReference;
            set
            {
                if (value == this.value)
                    return;
                this.value = value;
                if (OnChange == null)
                    return;
                OnChange();
            }
        }

        public override Type ObjectType => typeof(GameObject);

        public override object RawValue
        {
            get => value;
            set => this.value = value as AssetReference;
        }

        public override void SafeAssign(object val) => value = val as AssetReference;

        public FsmAssetReference()
        {
        }

        public FsmAssetReference(string name)
            : base(name)
        {
        }

        public FsmAssetReference(FsmAssetReference source)
            : base(source)
        {
            if (source == null)
                return;
            value = source.value;
        }

        public override NamedVariable Clone() => new FsmAssetReference(this);

        public override void Clear() => value = null;

        public override VariableType VariableType => VariableType.GameObject;

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "None";
        }

        public static implicit operator FsmAssetReference(AssetReference value)
        {
            return new FsmAssetReference(string.Empty)
            {
                value = value
            };
        }
    }
}