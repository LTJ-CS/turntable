using UnityEngine;
using Object = UnityEngine.Object;

namespace GameScript.Runtime.Common
{
    public class ConnectResComponent : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("连接的资源")]
        private Object[] m_ConnectRes;
        
    }
}
