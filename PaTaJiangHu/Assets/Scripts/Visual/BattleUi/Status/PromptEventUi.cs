using System;
using System.Collections;
using System.Collections.Generic;
using BattleM;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUis;

namespace Visual.BattleUi.Status
{
    public class PromptEventUi : BaseUi
    {
        public enum Events
        {
            None,
            Attack,
            Parry,
            Dodge,
            Suffer
        }
        [SerializeField] private Text _formText;
        [SerializeField] private Text _breathText;
        [SerializeField] private Image _attack;
        [SerializeField] private Image _parry;
        [SerializeField] private Image _dodge;
        [SerializeField] private Image _suffer;
        private Dictionary<Events, Image> _eventMap;
        private Dictionary<Events, Image> EventMap => _eventMap ??= new Dictionary<Events, Image>
        {
            { Events.Attack, _attack },
            { Events.Parry, _parry },
            { Events.Dodge, _dodge },
            { Events.Suffer, _suffer },
        };

        private void SetUi(string form, int breath)
        {
            transform.localScale = Vector3.one;
            _formText.text = form;
            _breathText.text = breath.ToString();
            Show();
        }

        public void Set(string title,int breath) => SetUi(title, breath);

        public void UpdateEvent(Events comEvent)
        {
            foreach (var (e, image) in EventMap) 
                image.gameObject.SetActive(e == comEvent);
            float scale;
            Sequence tween = DOTween.Sequence();
            switch (comEvent)
            {
                case Events.None:
                    scale = 1f;
                    tween.Append(transform.DOScale(scale, 0.2f));
                    break;
                case Events.Attack:
                    scale = 1.1f;
                    tween.Append(transform.DOScale(scale, 0.2f));
                    break;
                case Events.Parry:
                case Events.Dodge:
                case Events.Suffer:
                    scale = 0.8f;
                    tween.Append(transform.DOScale(scale, 0.2f));
                    tween.Append(transform.DOShakeScale(0.3f,0.3f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comEvent), comEvent, null);
            }

            StartCoroutine(PlayScale(tween));
        }

        private IEnumerator PlayScale(Tween tween)
        {
            yield return tween.WaitForCompletion();
        }

        public override void ResetUi()
        {
            _formText.text = string.Empty;
            _breathText.text = string.Empty;
            UpdateEvent(Events.None);
            Hide();
            transform.localScale = Vector3.one;
        }
    }
}