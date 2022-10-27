using System;
using System.Collections;
using BattleM;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Visual.BattleUi
{
    /// <summary>
    /// 战斗节奏控制器,用于控制<see cref="ICombatFragmentPlayer"/>的各种战斗事件触发时机
    /// </summary>
    public class CombatTempoController
    {
        private float positionSecs = 0.4f;
        private float executeSecs = 1f;
        private ICombatFragmentPlayer FragmentPlayer { get; }
        public CombatTempoController(ICombatFragmentPlayer fragmentPlayer)
        {
            FragmentPlayer = fragmentPlayer;
        }

        public IEnumerator Play(FightRoundRecord rec)
        {
            var placingPos = false;
            var attackExecution = false;
            var posTween = DOTween.Sequence().Pause();
            var exeTween = DOTween.Sequence().Pause();
            var eventTween = DOTween.Sequence().Pause();
            var respondTween = DOTween.Sequence().Pause();
            foreach (var fragment in rec.Records)
            {
                switch (fragment.Type)
                {
                    case FightFragment.Types.Consume:
                    {
                        var con = (ConsumeRecord)fragment;
                        if (con.IsExecutor)
                        {
                            exeTween.AppendCallback(() =>
                            {
                                FragmentPlayer.OnConsumeAnim(con);
                                FragmentPlayer.OnStatusUpdate(con.Before, con.After);
                                FragmentPlayer.OnEventAnim(con);
                            });
                        }
                        else
                        {
                            respondTween.AppendCallback(() =>
                            {
                                FragmentPlayer.OnConsumeAnim(con);
                                FragmentPlayer.OnStatusUpdate(con.Before, con.After);
                                FragmentPlayer.OnEventAnim(con);
                            });
                        }
                        break;
                    }
                    case FightFragment.Types.Position:
                    {
                        var pos = (PositionRecord)fragment;
                        placingPos = true;
                        posTween.AppendCallback(() =>
                        {
                            FragmentPlayer.OnReposAnim(pos);
                            FragmentPlayer.OnEventAnim(pos);
                        });
                        break;
                    }
                    case FightFragment.Types.Attack:
                    case FightFragment.Types.Fling:
                    {
                        var att = (AttackRecord)fragment;
                        attackExecution = true;
                        exeTween.AppendCallback(() =>
                        {
                            FragmentPlayer.OnAttackAnim(att);
                            FragmentPlayer.OnEventAnim(att);
                            if(rec.Respond is FightFragment.Types.None)
                                FragmentPlayer.OnSufferAnim(att);
                        });
                        break;
                    }
                    case FightFragment.Types.Parry:
                    {
                        var par = (ParryRecord)fragment;
                        if (!par.ParryFormula.IsSuccess) break;
                        respondTween.AppendCallback(() =>
                        {
                            FragmentPlayer.OnParryAnim(par);
                            FragmentPlayer.OnEventAnim(par);
                        });
                        break;
                    }
                    case FightFragment.Types.Dodge:
                    {
                        var dod = (DodgeRecord)fragment;
                        if (!dod.DodgeFormula.IsSuccess) break;
                        respondTween.AppendCallback(() =>
                        {
                            FragmentPlayer.OnDodgeAnim(dod);
                            FragmentPlayer.OnEventAnim(dod);
                        });
                        break;
                    }
                    case FightFragment.Types.TryEscape:
                    case FightFragment.Types.Escaped:
                    case FightFragment.Types.Death:
                    case FightFragment.Types.Exhausted:
                    case FightFragment.Types.Wait:
                    {
                        var wait = (EventRecord)fragment;
                        eventTween.AppendCallback(() => FragmentPlayer.OnEventAnim(wait));
                        break;
                    }
                    case FightFragment.Types.SwitchTarget:
                    {
                        var switchTar = (SwitchTargetRecord)fragment;
                        eventTween.AppendCallback(()=>FragmentPlayer.OnSwitchTarget(switchTar));
                        break;
                    }
                    case FightFragment.Types.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            FragmentPlayer.OnFragmentUpdate();
            if (placingPos)
            {
                yield return posTween.Play().WaitForCompletion();
                yield return new WaitForSeconds(positionSecs);
            }

            yield return eventTween.Play().WaitForCompletion();
            if (!attackExecution) yield break;
            yield return exeTween.Append(respondTween).Play().WaitForCompletion();
            yield return new WaitForSeconds(executeSecs);
        }
        
        //public IEnumerator PlayExecute(FightRoundRecord rec)
        //{
        //    var consumeAnim = new UnityEvent();
        //    var statusUpdate = new UnityEvent();
        //    var positionAnim = new UnityEvent();
        //    var placingPos = false;
        //    var attackAnim = new UnityEvent();
        //    var attackExecution = false;
        //    var respondAnim = new UnityEvent();
        //    var eventAnim = new UnityEvent();
        //    foreach (var fragment in rec.Records)
        //    {
        //        fragment.OnRec(con =>
        //            {
        //                consumeAnim.AddListener(() => FragmentPlayer.OnConsumeAnim(con));
        //                statusUpdate.AddListener(() => FragmentPlayer.OnStatusUpdate(con.Before, con.After));
        //                eventAnim.AddListener(() => FragmentPlayer.OnEventAnim(con));
        //            }, pos =>
        //            {
        //                placingPos = true;
        //                positionAnim.AddListener(() => FragmentPlayer.OnReposAnim(pos));
        //                eventAnim.AddListener(() => FragmentPlayer.OnEventAnim(pos));
        //            },
        //            att =>
        //            {
        //                attackExecution = true;
        //                attackAnim.AddListener(() => FragmentPlayer.OnAttackAnim(att));
        //                attackAnim.AddListener(() => FragmentPlayer.OnSufferAnim(att));
        //                eventAnim.AddListener(() => FragmentPlayer.OnEventAnim(att));
        //            },
        //            dod =>
        //            {
        //                if (!dod.DodgeFormula.IsSuccess) return;
        //                respondAnim.RemoveAllListeners();
        //                respondAnim.AddListener(() => FragmentPlayer.OnDodgeAnim(dod));
        //                eventAnim.AddListener(() => FragmentPlayer.OnEventAnim(dod));
        //            },
        //            par =>
        //            {
        //                if (!par.ParryFormula.IsSuccess) return;
        //                respondAnim.RemoveAllListeners();
        //                respondAnim.AddListener(() => FragmentPlayer.OnParryAnim(par));
        //                eventAnim.AddListener(() => FragmentPlayer.OnEventAnim(par));
        //            },
        //            eve => { eventAnim.AddListener(() => FragmentPlayer.OnEventAnim(eve)); },
        //            swi => { eventAnim.AddListener(() => FragmentPlayer.OnSwitchTarget(swi)); });
        //        FragmentPlayer.OnFragmentUpdate();

        //    }

        //    eventAnim.Invoke();
        //    consumeAnim.Invoke();
        //    if (placingPos)
        //    {
        //        positionAnim.Invoke();
        //        yield return new WaitForSeconds(positionSecs);
        //    }

        //    statusUpdate.Invoke();
        //    if (!attackExecution) yield break;
        //    respondAnim.Invoke();
        //    attackAnim.Invoke();
        //    yield return new WaitForSeconds(executeSecs);
        //}
    }
}