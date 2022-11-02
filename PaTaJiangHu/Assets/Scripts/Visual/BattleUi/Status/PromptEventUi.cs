using System;
using System.Collections;
using System.Collections.Generic;
using BattleM;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Status
{
    public class PromptEventUi : UiBase
    {
        public enum CombatEvents
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
        private Dictionary<CombatEvents, Image> _eventMap;
        private Dictionary<CombatEvents, Image> EventMap => _eventMap ??= new Dictionary<CombatEvents, Image>
        {
            { CombatEvents.Attack, _attack },
            { CombatEvents.Parry, _parry },
            { CombatEvents.Dodge, _dodge },
            { CombatEvents.Suffer, _suffer },
        };

        private void SetUi(string form, int breath)
        {
            transform.localScale = Vector3.one;
            _formText.text = form;
            _breathText.text = breath.ToString();
            Show();
        }

        public void Set(string title,int breath) => SetUi(title, breath);

        public void UpdateEvent(CombatEvents comEvent)
        {
            foreach (var (e, image) in EventMap) 
                image.gameObject.SetActive(e == comEvent);
            float scale;
            Sequence tween = DOTween.Sequence();
            switch (comEvent)
            {
                case CombatEvents.None:
                    scale = 1f;
                    tween.Append(transform.DOScale(scale, 0.2f));
                    break;
                case CombatEvents.Attack:
                    scale = 1.1f;
                    tween.Append(transform.DOScale(scale, 0.2f));
                    break;
                case CombatEvents.Parry:
                case CombatEvents.Dodge:
                case CombatEvents.Suffer:
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
            UpdateEvent(CombatEvents.None);
            Hide();
            transform.localScale = Vector3.one;
        }
    }
}