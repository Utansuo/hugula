﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hugula.Utils
{
    /// <summary>
    /// this copy from https://bitbucket.org/Unity-Technologies/ui/src/b5f9aae6ff7c2c63a521a1cb8b3e3da6939b191b/UnityEngine.UI/UI/Core/Utility/ObjectPool.cs?at=5.3&fileviewer=file-view-default
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly System.Action<T> m_ActionOnGet;
        private readonly System.Action<T> m_ActionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get { return m_Stack.Count; } }

        public ObjectPool(Action<T> actionOnGet, Action<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }

    /// <summary>
    /// this copy from https://bitbucket.org/Unity-Technologies/ui/src/b5f9aae6ff7c2c63a521a1cb8b3e3da6939b191b/UnityEngine.UI/UI/Core/Utility/ListPool.cs?at=5.3&fileviewer=file-view-default
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }

    /// <summary>
    /// pool dictionary
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public static class DictionaryPool<K, V>
    {
        private static readonly ObjectPool<Dictionary<K, V>> s_ListPool = new ObjectPool<Dictionary<K, V>>(null, l => l.Clear());

        public static Dictionary<K, V> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(Dictionary<K, V> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }

    public interface IReleaseToPool
    {
        void ReleaseToPool();
    }

    public interface IReset
    {
        void Reset();
    }

    public class GameObjectPool
    {
        private static GameObjectPool m_Instance;
        public static GameObjectPool Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new GameObjectPool();

                return m_Instance;
            }
        }
        private Dictionary<int, Stack<Component>> m_StackDic = new Dictionary<int, Stack<Component>>();
        private readonly System.Action<Component> m_ActionOnGet;
        private readonly System.Action<Component> m_ActionOnRelease;
        public Dictionary<int, Component> m_Source = new Dictionary<int, Component>();

        // public int countAll { get; private set; }
        // public int countActive { get { return countAll - countInactive; } }
        // public int countInactive { get { return m_StackDic.Count; } }
        public GameObjectPool() { }
        public GameObjectPool(System.Action<Component> actionOnGet, System.Action<Component> actionOnRelease)
        {
            this.m_ActionOnGet = actionOnGet;
            this.m_ActionOnRelease = actionOnRelease;
        }
        public void Clear(int key)
        {
            m_Source.Remove(key);
            m_StackDic.Remove(key);
        }

        public void Add(int key, Component comp)
        {
            if (m_Source.ContainsKey(key)) m_Source.Remove(key);
            if (m_StackDic.ContainsKey(key)) m_StackDic.Remove(key);
            m_Source.Add(key, comp);
            m_StackDic.Add(key, new Stack<Component>());
        }

        public Component Get(int key, out bool isNew)
        {
            isNew = false;
            Stack<Component> m_Stack;
            if (!m_StackDic.TryGetValue(key, out m_Stack)) return null;

            Component element = null;

            if (m_Stack.Count == 0)
            {
                Component source = null;
                if (m_Source.TryGetValue(key, out source))
                {
                    isNew = true;
                    element = GameObject.Instantiate(source);
                }
                // countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public Component Get(int key)
        {
            Stack<Component> m_Stack;
            if (!m_StackDic.TryGetValue(key, out m_Stack)) return null;

            Component element = null;

            if (m_Stack.Count == 0)
            {
                Component source = null;
                if (m_Source.TryGetValue(key, out source))
                    element = GameObject.Instantiate(source);
                // countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            element.gameObject.SetActive(true);
            return element;
        }

        public void Release(int key, Component element)
        {
            if (element == null) return;
            Stack<Component> m_Stack;
            if (!m_StackDic.TryGetValue(key, out m_Stack)) return;

            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }
}