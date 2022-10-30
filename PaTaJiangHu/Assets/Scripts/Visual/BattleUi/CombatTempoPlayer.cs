using System;
using System.Collections;
using BattleM;
using UnityEngine;

namespace Visual.BattleUi
{
    /// <summary>
    /// 战斗播放器,用于播放<see cref="ICombatFragmentPlayer"/>的各种战斗事件触发时机
    /// </summary>
    public class CombatTempoPlayer
    {
        private float positionSecs = 0.4f;
        private float executeSecs = 1f;
        private ICombatFragmentPlayer FragmentPlayer { get; }
        public CombatTempoPlayer(ICombatFragmentPlayer fragmentPlayer)
        {
            FragmentPlayer = fragmentPlayer;
        }

        public IEnumerator Play(CombatRoundRecord rec)
        {
            yield return FragmentPlayer.FullRecord(rec);

            if (rec.SwitchTargetRec != null)
            {
                throw new NotImplementedException();
                //FragmentPlayer.OnSwitchTarget(rec.SwitchTargetRec);
            }

            switch (rec.Major)
            {
                case CombatRoundRecord.MajorEvents.None:
                    break;
                case CombatRoundRecord.MajorEvents.Combat:
                    yield return OnAttack(rec.AttackRec);
                    break;
                case CombatRoundRecord.MajorEvents.Recover:
                {
                    var con = rec.RecoverRec;
                    FragmentPlayer.OnStatusUpdate(con.Before, con.After);
                    break;
                }
                case CombatRoundRecord.MajorEvents.Escape:
                    yield return OnEscape(rec.EscapeRec);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var eventRecord in rec.OtherEventRec) FragmentPlayer.OnOtherEventAnim(eventRecord);
            foreach (var charge in rec.RechargeRec) FragmentPlayer.OnReCharge(charge);
        }

        private IEnumerator OnEscape(EscapeRecord esc)
        {
            if (esc.Attacker != null)
            {
                FragmentPlayer.OnEscapeAnim(esc);
                if (esc.DodgeFormula.IsSuccess)
                {
                    FragmentPlayer.OnDodgeAnim(esc.EscapeConsume);
                    yield return new WaitForSeconds(0.5f);
                    FragmentPlayer.OnDodgeAnim(esc.EscapeConsume);
                    yield break;
                }

                if (esc.ParryFormula.IsSuccess)
                {
                    FragmentPlayer.OnParryAnim(esc.ParryConsume);
                    yield break;
                }
                FragmentPlayer.OnSufferAnim(esc.Suffer);
            }
        }

        private IEnumerator OnAttack(AttackRecord att)
        {
            if (att.AttackPlacing != null)
            {
                var pos = att.AttackPlacing;
                yield return FragmentPlayer.OnReposAnim(pos);
                //yield return new WaitForSeconds(positionSecs);
            }
            FragmentPlayer.OnAttackAnim(att);
            if (att.DodgeFormula.IsSuccess)
            {
                FragmentPlayer.OnDodgeAnim(att.DodgeConsume);
                yield break;
            }
            if (att.ParryFormula.IsSuccess)
            {
                FragmentPlayer.OnParryAnim(att.ParryConsume);
                yield break;
            }
            FragmentPlayer.OnSufferAnim(att.Suffer);
        }
    }
}