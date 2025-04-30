using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
#if !DISABLE_SRDEBUGGER
    [Category("本地缓存"), DisplayName("删除缓存")]
    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
#endif
}