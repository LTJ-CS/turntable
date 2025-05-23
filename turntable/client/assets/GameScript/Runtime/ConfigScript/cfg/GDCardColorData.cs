
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace ClientCfg.cfg
{
public sealed partial class GDCardColorData : Luban.BeanBase
{
    public GDCardColorData(JSONNode _buf) 
    {
        { if(!_buf["id"].IsNumber) { throw new SerializationException(); }  Id = _buf["id"]; }
        { if(!_buf["weight"].IsNumber) { throw new SerializationException(); }  Weight = _buf["weight"]; }
        { if(!_buf["resources"].IsString) { throw new SerializationException(); }  Resources = _buf["resources"]; }
    }

    public static GDCardColorData DeserializeGDCardColorData(JSONNode _buf)
    {
        return new cfg.GDCardColorData(_buf);
    }

    /// <summary>
    /// 卡牌颜色
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// 初始权重
    /// </summary>
    public readonly int Weight;
    /// <summary>
    /// 对应资源
    /// </summary>
    public readonly string Resources;
   
    public const int __ID__ = -1057649386;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
        
        
        
    }

    public override string ToString()
    {
        return "{ "
        + "id:" + Id + ","
        + "weight:" + Weight + ","
        + "resources:" + Resources + ","
        + "}";
    }
}

}
