using System;
using System.Reflection;
using UnityEngine;

public class Singleton<T> where T : class
{
   private static T _instance;
   private static readonly object syncLock = new object();

   public static T Instance
   {
      get
      {
         if (_instance == null)
         {
            lock (syncLock)
            {
               if (_instance == null)
               {
                  Type t = typeof(T);
                  ConstructorInfo[] ctors = t.GetConstructors();
                  if (ctors.Length > 0)
                  {
                     throw new InvalidOperationException(string.Format("{0} has at least one accesible ctor making it impossible to enforce Singleton behaviour", t.Name));
                  }
                  _instance = (T)Activator.CreateInstance(t, true);
               }
            }
         }

         return _instance;
      }
   }
}



public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
   private static bool UseShareGOFlag = true;
   private static GameObject ShareGORoot = null;
   private static readonly string ShareGOName = "MonoSingletonShareRoot";
   protected static bool ApplicationQuitFlag { get; private set; }
   protected static bool GolbalFlag = true;
   private static T _instance;
   private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (FindObjectsOfType<T>().Length > 1)
                        {
                            if (Debug.isDebugBuild)
                            {
                                Debug.LogWarning("MonoSingleton[Singleton] " + typeof(T).Name + " should never be more than 1 in scene!");
                            }
                            return _instance;
                        }
                        if (_instance == null)
                        {
                            if (UseShareGOFlag)
                            {
                                if (ShareGORoot == null)
                                {
                                    ShareGORoot = GameObject.Find(ShareGOName);
                                    if (ShareGORoot == null)
                                    {
                                        ShareGORoot = new GameObject();
                                        ShareGORoot.name = ShareGOName;
                                        if (GolbalFlag && Application.isPlaying)
                                        {
                                            DontDestroyOnLoad(ShareGORoot);
                                        }
                                    }
                                }
                                _instance = ShareGORoot.GetComponent<T>();
                                if (_instance == null)
                                {
                                    _instance = ShareGORoot.AddComponent<T>();
                                }
                            }
                            else
                            {
                                string singletonName = "(singleton)" + typeof(T);
                                GameObject singletonObj = GameObject.Find(singletonName);
                                if (singletonObj == null)
                                {
                                    singletonObj = new GameObject();
                                    singletonObj.name = singletonName;
                                    _instance = singletonObj.AddComponent<T>();
                                }
                                else
                                {
                                    _instance = singletonObj.GetComponent<T>();
                                    if(_instance == null)
                                    {
                                        _instance = singletonObj.AddComponent<T>();
                                    }
                                }
                                if (GolbalFlag && Application.isPlaying)
                                {
                                    DontDestroyOnLoad(singletonObj);
                                }
                            }
                            return _instance;
                        }
                    }
                    return _instance;
                }
            }
            return _instance;
        }
    }
   protected virtual void Awake()
   {
      if (_instance != null && _instance != this as T)
      {
         Destroy(gameObject);
         return;
      }
      if (_instance == null)
         _instance = this as T;
      if (GolbalFlag && Application.isPlaying)
      {
         DontDestroyOnLoad(gameObject);
      }
   }
   

   protected virtual void OnDestroy()
   {
       if (_instance == this)
           _instance = null;
   }

   protected virtual void OnApplicationQuit()
   {
      ApplicationQuitFlag = true;
   }
}
