using System;
using System.Collections;
using BattleM;
using UnityEngine;

namespace Visual.BattleUi
{
    /// <summary>
    /// 战斗节奏控制器,用于控制<see cref="ICombatFragmentPlayer"/>的各种战斗事件触发时机
    /// </summary>
    public class CombatTempoController
    {
        private ICombatFragmentPlayer FragmentPlayer { get; }
        public CombatTempoController(ICombatFragmentPlayer fragmentPlayer)
        {
            FragmentPlayer = fragmentPlayer;
        }

        public IEnumerator Play(FightRoundRecord rec)
        {
            Action consumeAnim = null;
            Action statusUpdate = null;
            Action positionAnim = null;
            Action attackAnim = null;
            Action respondAnim = null;
            Action eventAnim = null;
            foreach (var fragment in rec.Records)
            {
                fragment.OnRec(con =>
                    {
                        consumeAnim = ()=> FragmentPlayer.OnConsumeAnim(con);
                        statusUpdate += () => FragmentPlayer.OnStatusUpdate(con.Before, con.After);
                        eventAnim += () => FragmentPlayer.OnEventAnim(con);
                    }, pos =>
                    {
                        positionAnim = ()=> FragmentPlayer.OnReposAnim(pos);
                        eventAnim += () => FragmentPlayer.OnEventAnim(pos);
                    },
                    att =>
                    {
                        attackAnim =()=> FragmentPlayer.OnAttackAnim(att);
                        respondAnim =()=> FragmentPlayer.OnSufferAnim(att);
                        eventAnim += () => FragmentPlayer.OnEventAnim(att);
                    },
                    dod =>
                    {
                        if (!dod.DodgeFormula.IsSuccess) return;
                        respondAnim =()=> FragmentPlayer.OnDodgeAnim(dod);
                        eventAnim += () => FragmentPlayer.OnEventAnim(dod);
                    },
                    par =>
                    {
                        if (!par.ParryFormula.IsSuccess) return;
                        respondAnim = () => FragmentPlayer.OnParryAnim(par);
                        eventAnim += () => FragmentPlayer.OnEventAnim(par);
                    },
                    eve =>
                    {
                        eventAnim += () => FragmentPlayer.OnEventAnim(eve);
                    },
                    swi =>
                    {
                        eventAnim += () => FragmentPlayer.OnSwitchTarget(swi);
                    });
                FragmentPlayer.OnFragmentUpdate();
                
            }
            eventAnim?.Invoke();
            consumeAnim?.Invoke();
            if (positionAnim !=null)
            {
                positionAnim();
                yield return new WaitForSeconds(0.4f);
            }
            statusUpdate?.Invoke();
            if (attackAnim!=null)
            {
                attackAnim.Invoke();
                respondAnim?.Invoke();
                yield return new WaitForSeconds(0.6f);
            }

        }
    }
}