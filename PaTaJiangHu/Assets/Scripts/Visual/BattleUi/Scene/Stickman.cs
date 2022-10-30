using System;
using System.Collections;
using BattleM;
using DG.Tweening;
using UnityEngine;

namespace Visual.BattleUi.Scene
{
    /// <summary>
    /// 火柴人ui控制器
    /// </summary>
    public class Stickman : MonoBehaviour
    {
        [SerializeField] private Animator _anim;
        [SerializeField] private RectTransform _rect;
        private const string AttackString = "attack";
        private const string ParryString = "parry";
        private const string DodgeString = "dodge";
        private const string DeathString = "death";
        private const string SufferString = "suffer";
        public enum Anims
        {
            Attack,
            Parry,
            Dodge,
            Suffer,
            Death
        }
        private CombatUnit _unit;
        public int TargetId => _unit.Target?.CombatId ?? -1;
        public int CombatId => _unit.CombatId;
        public void Init(CombatUnit combatUnit) => _unit = combatUnit;
        public void SetAnim(Anims action)
        {
            var param = action switch
            {
                Anims.Attack => AttackString,
                Anims.Parry => ParryString,
                Anims.Dodge => DodgeString,
                Anims.Suffer => SufferString,
                Anims.Death => DeathString,
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
            _anim.SetTrigger(param);
        }

        public void SetOriented(bool faceRight)
        {
            var x = faceRight ? 1 : -1;
            var scale = transform.localScale;
            transform.localScale = new Vector3(x, scale.y, scale.z);
        }

        public void ResetPosition() => transform.localPosition = Vector3.zero;

        public void Dash(float duration, Action onCompleteAction)
        {
            StopAllCoroutines();
            StartCoroutine(DashCoroutine(duration, onCompleteAction));
        }

        private IEnumerator DashCoroutine(float duration, Action onCompleteAction)
        {
            yield return transform.DOLocalMove(Vector3.zero, duration)
                .OnComplete(() => onCompleteAction?.Invoke())
                .WaitForCompletion();
        }
    }
}