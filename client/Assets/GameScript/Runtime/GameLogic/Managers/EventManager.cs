using System;
using System.Collections.Generic;

public static class EventManager
{
    private static readonly Dictionary<string, Delegate> _eventDictionary = new Dictionary<string, Delegate>();
    
    public static void Subscribe(string eventName, Action listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            _eventDictionary[eventName] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventDictionary[eventName] = listener;
        }
    }

    public static void UnSubscribe(string eventName, Action listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var currentDelegate = Delegate.Remove(existingDelegate, listener);
            if (currentDelegate == null)
            {
                _eventDictionary.Remove(eventName);
            }
            else
            {
                _eventDictionary[eventName] = currentDelegate;
            }
        }
    }

    public static void Publish(string eventName)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var callback = existingDelegate as Action;
            callback?.Invoke();
        }
    }
    
    public static void Subscribe<T1>(string eventName, Action<T1> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            _eventDictionary[eventName] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventDictionary[eventName] = listener;
        }
    }

    public static void UnSubscribe<T1>(string eventName, Action<T1> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var currentDelegate = Delegate.Remove(existingDelegate, listener);
            if (currentDelegate == null)
            {
                _eventDictionary.Remove(eventName);
            }
            else
            {
                _eventDictionary[eventName] = currentDelegate;
            }
        }
    }

    public static void Publish<T1>(string eventName, T1 param1)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var callback = existingDelegate as Action<T1>;
            callback?.Invoke(param1);
        }
    }
    
    public static void Subscribe<T1, T2>(string eventName, Action<T1, T2> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            _eventDictionary[eventName] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventDictionary[eventName] = listener;
        }
    }

    public static void UnSubscribe<T1, T2>(string eventName, Action<T1, T2> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var currentDelegate = Delegate.Remove(existingDelegate, listener);
            if (currentDelegate == null)
            {
                _eventDictionary.Remove(eventName);
            }
            else
            {
                _eventDictionary[eventName] = currentDelegate;
            }
        }
    }

    public static void Publish<T1, T2>(string eventName, T1 param1, T2 param2)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var callback = existingDelegate as Action<T1, T2>;
            callback?.Invoke(param1, param2);
        }
    }


    public static void Subscribe<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            _eventDictionary[eventName] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventDictionary[eventName] = listener;
        }
    }

    public static void UnSubscribe<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var currentDelegate = Delegate.Remove(existingDelegate, listener);
            if (currentDelegate == null)
            {
                _eventDictionary.Remove(eventName);
            }
            else
            {
                _eventDictionary[eventName] = currentDelegate;
            }
        }
    }

    public static void Publish<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
    {
        if (_eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            var callback = existingDelegate as Action<T1, T2, T3>;
            callback?.Invoke(param1, param2, param3);
        }
    }

    public static void Clear()
    {
        _eventDictionary.Clear();
    }


}