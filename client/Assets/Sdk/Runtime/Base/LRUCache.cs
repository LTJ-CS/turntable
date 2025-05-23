using System.Collections;
using System.Collections.Generic;

namespace Sdk.Runtime.Base
{
    /// <summary>
    /// lru算法，最近被使用的放在字段的最后
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LRUCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly DoubleLinkedListNode<TKey, TValue> _head;
 
        private readonly DoubleLinkedListNode<TKey, TValue> _tail;
 
        private readonly Dictionary<TKey, DoubleLinkedListNode<TKey, TValue>> _dictionary;
 
        private readonly int _capacity;
 
        public LRUCache(int capacity)
        {
            _capacity = capacity;
            _head = new DoubleLinkedListNode<TKey, TValue>();
            _tail = new DoubleLinkedListNode<TKey, TValue>();
            _head.Next = _tail;
            _tail.Previous = _head;
            _dictionary = new Dictionary<TKey, DoubleLinkedListNode<TKey, TValue>>();
        }
 
        public TValue Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var node))
            {
                RemoveNode(node);
                AddLastNode(node);
                return node.Value;
            }
 
            return default;
        }
 
        public TValue Put(TKey key, TValue value)
        {
             TValue removeValue = default;
            if (_dictionary.TryGetValue(key, out var node))
            {
                removeValue = node.Value.Equals(value) ? default : node.Value;
                RemoveNode(node);
                AddLastNode(node);
                node.Value = value;
            }
            else
            {
                if (_dictionary.Count == _capacity)
                {
                    var firstNode = RemoveFirstNode();
                    removeValue = firstNode.Value;
                    _dictionary.Remove(firstNode.Key);
                }
 
                var newNode = new DoubleLinkedListNode<TKey, TValue>(key, value);
                AddLastNode(newNode);
                _dictionary.Add(key, newNode);
            }

            return removeValue;
        }
 
        public TValue Remove(TKey key)
        {
            if (_dictionary.Remove(key, out var node))
            {
                RemoveNode(node);
                return node.Value;
            }

            return default;
        }
 
        private void AddLastNode(DoubleLinkedListNode<TKey, TValue> node)
        {
            node.Previous = _tail.Previous;
            node.Next = _tail;
            _tail.Previous.Next = node;
            _tail.Previous = node;
        }
 
        private DoubleLinkedListNode<TKey, TValue> RemoveFirstNode()
        {
            var firstNode = _head.Next;
            _head.Next = firstNode.Next;
            firstNode.Next.Previous = _head;
            firstNode.Next = null;
            firstNode.Previous = null;
            return firstNode;
        }
 
        private void RemoveNode(DoubleLinkedListNode<TKey, TValue> node)
        {
            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;
            node.Next = null;
            node.Previous = null;
        }
    
        internal class DoubleLinkedListNode<TKey, TValue>
        {    
            public DoubleLinkedListNode()
            {
            }
 
            public DoubleLinkedListNode(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
 
            public TKey Key { get; set; }
        
            public TValue Value { get; set; }
 
            public DoubleLinkedListNode<TKey, TValue> Previous { get; set; }
 
            public DoubleLinkedListNode<TKey, TValue> Next { get; set; }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kv in _dictionary)
            {
                yield return  new KeyValuePair<TKey, TValue>(kv.Value.Key, kv.Value.Value);
            }
        }

        public void Clear()
        {
            _dictionary.Clear();
            _head.Next = _tail;
            _tail.Previous = _head;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
