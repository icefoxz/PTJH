using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Status
{
    public class BattleOrderAvatarUi : UiBase
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private RectTransform _rect;
        private int Size { get; set; }

        public void Init()
        {
            gameObject.SetActive(true);
            Size = (int)_rect.sizeDelta.x;
        }

        public void Set(string title)
        {
            _nameText.text = title;
        }
        public override void ResetUi()
        {
            _nameText.text = string.Empty;
            SetSize(1f);
            gameObject.SetActive(false);
        }
        public void SetSize(float size)
        {
            _rect.sizeDelta = new Vector2(size * Size, size * Size);
        }
        public void EnlargeAnim()
        {
            StopAllCoroutines();
            StartCoroutine(Enlarge());
        }

        private IEnumerator Enlarge()
        {
            yield return _rect.DOSizeDelta(new Vector2(1 * Size, 1 * Size), 0.3f);
        }
    }
}