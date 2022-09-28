using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project.Src.Views.Bases
{
    public interface IView
    {
        //显示或隐藏
        void Display(bool display);
        //销毁的时候执行
        void Destroy();
    }
    public class UIView : IView
    {
        public GameObject GameObject { private set; get; }
        public Transform Transform { private set; get; }
        public RectTransform RectTransform { private set; get; }

        //在load scene的时候是否销毁
        public bool isDontDestroyOnLoad;

        public UIView()
        {
        }

        public UIView(GameObject go)
        {
            SetGameObject(go);
        }

        public UIView(GameObject go, Transform parent)
        {
            SetGameObject(go, parent);
        }

        private void SetGameObject(GameObject go, Transform parent = null)
        {
            GameObject = go;
            if (GameObject == null)
                return;
            Transform = GameObject.transform;
            if (parent != null)
                Transform.SetParent(parent);
            RectTransform = GameObject.GetComponent<RectTransform>();
        }

        public void Display(bool display) => GameObject.SetActive(display);
        
        public virtual void Destroy()
        {
            Display(false);
            UnityEngine.Object.Destroy(GameObject);
            GameObject = null;
            Transform = null;
            RectTransform = null;
        }
    }

}
