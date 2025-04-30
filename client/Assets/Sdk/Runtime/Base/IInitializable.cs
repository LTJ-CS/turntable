using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace Sdk.Runtime.Base
{
    
    public interface IInitializable 
    {
        void GetPreloadPath(ref List<string> pathList);
        UniTask InitializeAsync();
        void Register();
    }
    
}
