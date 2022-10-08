using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.BattleUi.Scene
{
    public class SlotUi : MonoBehaviour
    {
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _centerColor;
        [SerializeField] private Image _image;
        private List<GameObject> ObjList { get; set; }
        public IEnumerable<GameObject> Objs => ObjList;
        public void Init()
        {
            _image.color = _defaultColor;
            gameObject.SetActive(true);
            ObjList = new List<GameObject>();
        }
        public void SetCenterColor(bool isCenter)
        {
            _image.color = isCenter ? _centerColor : _defaultColor;
        }

        public void PlaceObj(GameObject obj)
        {
            ObjList.Add(obj);
            obj.transform.SetParent(transform);
        }

        public void Remove(GameObject obj) => ObjList.Remove(obj);
        public void ResetUi()
        {
            ObjList.Clear();
            _image.color = _defaultColor;
        }
    }
}
