using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弟子战斗演示器
/// </summary>
public class DiziBattleAnimator
{
    private DiziCombatConfigSo CombatCfg { get; }
    private Dictionary<int, CharacterOperator> OpMap { get; }
    private MonoBehaviour Mono { get; }
    public DiziBattleAnimator(DiziCombatConfigSo combatCfg, Dictionary<int, CharacterOperator> opMap,
        MonoBehaviour mono)
    {
        CombatCfg = combatCfg;
        OpMap = opMap;
        Mono = mono;
    }

    public void PlayRound(RoundInfo<DiziCombatUnit, DiziCombatPerformInfo> roundInfo, Action callbackAction) =>
        Mono.StartCoroutine(PlayRoundCo(roundInfo, callbackAction));

    private IEnumerator PlayRoundCo(RoundInfo<DiziCombatUnit, DiziCombatPerformInfo> roundInfo, Action callback)
    {
        foreach (var (pfm, performInfos) in roundInfo.UnitInfoMap)
        {
            var performOp = OpMap[pfm.InstanceId];
            var performPos = GetLocationPoint(performOp);
            foreach (var info in performInfos)
            {
                var response = info.Response;
                var targetOp = OpMap[response.Target.InstanceId];
                var targetPos = GetLocationPoint(targetOp);
                yield return CombatCfg.PlayOffendMove(info, performOp, targetPos); //move
                CombatCfg.PlayPerform(info, performOp); //perform
                Mono.StartCoroutine(PlayTargetResponse(response, targetOp)); //response

                yield return new WaitForSeconds(0.2f);
                CombatCfg.PlayOffendReturn(performOp); //move return
                SetOpPos(targetOp.transform, targetPos); //align target position
                yield return performOp.OffendReturnMove(performPos);
            }
        }

        callback?.Invoke();
        yield return null;

        void SetOpPos(Transform tran, float pos) => tran.transform.SetX(pos);

        IEnumerator PlayTargetResponse(CombatResponseInfo<DiziCombatUnit> response, CharacterOperator targetOp)
        {
            CombatCfg.PlayResponse(response, targetOp);
            return CombatCfg.PlayResponseEffects(response, targetOp);
        }
    }

    private float GetLocationPoint(CharacterOperator op) => op.transform.localPosition.x;

}