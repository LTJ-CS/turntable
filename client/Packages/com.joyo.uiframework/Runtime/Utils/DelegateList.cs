using System;
using System.Collections.Generic;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace UIFramework.Utils
{
    /// <summary>
    /// 定义一个 DelegateList 的接口, 方便做统一的管理
    /// </summary>
    public interface IDelegateList
    {
        /// <summary>
        /// 返回当前回调函数的数量
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 清理掉所有的回调函数
        /// </summary>
        public void Clear();
    }
    
    /// <summary>
    /// 来自于 UnityEngine.ResourceManagement.Util 的DelegateList{T}, 方便我们对事件作管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateList<T> : IDelegateList
    {
        Func<Action<T>, LinkedListNode<Action<T>>> m_acquireFunc;
        Action<LinkedListNode<Action<T>>>          m_releaseFunc;
        LinkedList<Action<T>>                      m_callbacks;
        bool                                       m_invoking = false;

        public DelegateList(Func<Action<T>, LinkedListNode<Action<T>>> acquireFunc, Action<LinkedListNode<Action<T>>> releaseFunc)
        {
            if (acquireFunc == null)
                throw new ArgumentNullException("acquireFunc");
            if (releaseFunc == null)
                throw new ArgumentNullException("releaseFunc");
            m_acquireFunc = acquireFunc;
            m_releaseFunc = releaseFunc;
        }

        public int Count
        {
            get { return m_callbacks == null ? 0 : m_callbacks.Count; }
        }

        public void Add(Action<T> action)
        {
            var node = m_acquireFunc(action);
            if (m_callbacks == null)
                m_callbacks = new LinkedList<Action<T>>();
            m_callbacks.AddLast(node);
        }

        public void Remove(Action<T> action)
        {
            if (m_callbacks == null)
                return;

            var node = m_callbacks.First;
            while (node != null)
            {
                if (node.Value == action)
                {
                    if (m_invoking)
                    {
                        node.Value = null;
                    }
                    else
                    {
                        m_callbacks.Remove(node);
                        m_releaseFunc(node);
                    }

                    return;
                }

                node = node.Next;
            }
        }

        public void Invoke(T res)
        {
            if (m_callbacks == null)
                return;

            m_invoking = true;
            var node = m_callbacks.First;
            while (node != null)
            {
                if (node.Value != null)
                {
                    try
                    {
                        node.Value(res);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }

                node = node.Next;
            }

            m_invoking = false;
            var r = m_callbacks.First;
            while (r != null)
            {
                var next = r.Next;
                if (r.Value == null)
                {
                    m_callbacks.Remove(r);
                    m_releaseFunc(r);
                }

                r = next;
            }
        }

        public void Clear()
        {
            if (m_callbacks == null)
                return;
            var node = m_callbacks.First;
            while (node != null)
            {
                var next = node.Next;
                m_callbacks.Remove(node);
                m_releaseFunc(node);
                node = next;
            }
        }

        public static DelegateList<T> CreateWithGlobalCache()
        {
            if (!GlobalLinkedListNodeCache<Action<T>>.CacheExists)
                GlobalLinkedListNodeCache<Action<T>>.SetCacheSize(32);
            return new DelegateList<T>(GlobalLinkedListNodeCache<Action<T>>.Acquire, GlobalLinkedListNodeCache<Action<T>>.Release);
        }
    }
    
    /// <summary>
    /// 没有参数的 DelegateList
    /// </summary>
    public class DelegateList: IDelegateList
    {
        Func<Action, LinkedListNode<Action>> m_acquireFunc;
        Action<LinkedListNode<Action>>          m_releaseFunc;
        LinkedList<Action>                      m_callbacks;
        bool                                       m_invoking = false;

        public DelegateList(Func<Action, LinkedListNode<Action>> acquireFunc, Action<LinkedListNode<Action>> releaseFunc)
        {
            if (acquireFunc == null)
                throw new ArgumentNullException("acquireFunc");
            if (releaseFunc == null)
                throw new ArgumentNullException("releaseFunc");
            m_acquireFunc = acquireFunc;
            m_releaseFunc = releaseFunc;
        }

        public int Count
        {
            get { return m_callbacks == null ? 0 : m_callbacks.Count; }
        }

        public void Add(Action action)
        {
            var node = m_acquireFunc(action);
            if (m_callbacks == null)
                m_callbacks = new LinkedList<Action>();
            m_callbacks.AddLast(node);
        }

        public void Remove(Action action)
        {
            if (m_callbacks == null)
                return;

            var node = m_callbacks.First;
            while (node != null)
            {
                if (node.Value == action)
                {
                    if (m_invoking)
                    {
                        node.Value = null;
                    }
                    else
                    {
                        m_callbacks.Remove(node);
                        m_releaseFunc(node);
                    }

                    return;
                }

                node = node.Next;
            }
        }

        public void Invoke()
        {
            if (m_callbacks == null)
                return;

            m_invoking = true;
            var node = m_callbacks.First;
            while (node != null)
            {
                if (node.Value != null)
                {
                    try
                    {
                        node.Value();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }

                node = node.Next;
            }

            m_invoking = false;
            var r = m_callbacks.First;
            while (r != null)
            {
                var next = r.Next;
                if (r.Value == null)
                {
                    m_callbacks.Remove(r);
                    m_releaseFunc(r);
                }

                r = next;
            }
        }

        public void Clear()
        {
            if (m_callbacks == null)
                return;
            var node = m_callbacks.First;
            while (node != null)
            {
                var next = node.Next;
                m_callbacks.Remove(node);
                m_releaseFunc(node);
                node = next;
            }
        }

        public static DelegateList CreateWithGlobalCache()
        {
            if (!GlobalLinkedListNodeCache<Action>.CacheExists)
                GlobalLinkedListNodeCache<Action>.SetCacheSize(32);
            return new DelegateList(GlobalLinkedListNodeCache<Action>.Acquire, GlobalLinkedListNodeCache<Action>.Release);
        }
    }

    /// <summary>
    /// Cache for nodes of LinkedLists.  This can be used to eliminate GC allocations.
    /// </summary>
    /// <typeparam name="T">The type of node.</typeparam>
    public class LinkedListNodeCache<T>
    {
        int           m_NodesCreated = 0;
        LinkedList<T> m_NodeCache;

        /// <summary>
        /// Creates or returns a LinkedListNode of the requested type and set the value.
        /// </summary>
        /// <param name="val">The value to set to returned node to.</param>
        /// <returns>A LinkedListNode with the value set to val.</returns>
        public LinkedListNode<T> Acquire(T val)
        {
            if (m_NodeCache != null)
            {
                var n = m_NodeCache.First;
                if (n != null)
                {
                    m_NodeCache.RemoveFirst();
                    n.Value = val;
                    return n;
                }
            }

            m_NodesCreated++;
            return new LinkedListNode<T>(val);
        }

        /// <summary>
        /// Release the linked list node for later use.
        /// </summary>
        /// <param name="node"></param>
        public void Release(LinkedListNode<T> node)
        {
            if (m_NodeCache == null)
                m_NodeCache = new LinkedList<T>();

            node.Value = default(T);
            m_NodeCache.AddLast(node);
        }

        internal int CreatedNodeCount
        {
            get { return m_NodesCreated; }
        }

        internal int CachedNodeCount
        {
            get { return m_NodeCache == null ? 0 : m_NodeCache.Count; }
            set
            {
                if (m_NodeCache == null)
                    m_NodeCache = new LinkedList<T>();
                while (value < m_NodeCache.Count)
                    m_NodeCache.RemoveLast();
                while (value > m_NodeCache.Count)
                    m_NodeCache.AddLast(new LinkedListNode<T>(default));
            }
        }
    }

    internal static class GlobalLinkedListNodeCache<T>
    {
        static LinkedListNodeCache<T> m_globalCache;

        public static bool CacheExists => m_globalCache != null;

        public static void SetCacheSize(int length)
        {
            if (m_globalCache == null)
                m_globalCache = new LinkedListNodeCache<T>();
            m_globalCache.CachedNodeCount = length;
        }

        public static LinkedListNode<T> Acquire(T val)
        {
            if (m_globalCache == null)
                m_globalCache = new LinkedListNodeCache<T>();
            return m_globalCache.Acquire(val);
        }

        public static void Release(LinkedListNode<T> node)
        {
            if (m_globalCache == null)
                m_globalCache = new LinkedListNodeCache<T>();
            m_globalCache.Release(node);
        }
    }
}