// 对象池泛型类

using System.Collections.Generic;
using UnityEngine;

namespace AOT._AOT.Utls
{
    public class ObjectPool<T> where T : Component
    {
        private T prefab; // 对象的预制体
        private Stack<T> objectStack = new Stack<T>(); // 对象的栈

        // 构造函数，用于初始化对象池
        public ObjectPool(T prefab, int initialCapacity)
        {
            this.prefab = prefab;

            // 预先创建一定数量的对象，加入到栈中
            for (int i = 0; i < initialCapacity; i++)
            {
                var obj = Object.Instantiate(prefab);
                obj.gameObject.SetActive(false);
                objectStack.Push(obj);
            }
        }

        // 从对象池中获取一个对象，如果栈中有可用对象，则返回栈顶对象；否则创建一个新对象
        public T Get()
        {
            if (objectStack.Count > 0)
            {
                var obj = objectStack.Pop();
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                var obj = Object.Instantiate(prefab);
                obj.gameObject.SetActive(true);
                return obj;
            }
        }

        // 将对象返回到对象池中，将对象放回栈中
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            objectStack.Push(obj);
        }
    }
}