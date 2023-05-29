using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 弟子战斗演示器
/// </summary>
internal class DiziBattleAnimator
{
    private DiziCombatAnimator CombatAnimator { get; }
    private Dictionary<int, CharacterOperator> OpMap { get; }
    private MonoBehaviour Mono { get; }
    private CharacterUiSyncHandler UiHandler { get; set; }
    public DiziBattleAnimator(DiziCombatVisualConfigSo combatVisualCfg, 
        Dictionary<int, CharacterOperator> opMap,
        CharacterUiSyncHandler uiHandler,
        MonoBehaviour mono,
        Transform poolTransform)
    {
        CombatAnimator = new DiziCombatAnimator(combatVisualCfg, poolTransform);
        OpMap = opMap;
        Mono = mono;
        UiHandler = uiHandler;
        foreach (var op in OpMap.Values) UiHandler.AssignObjToUi(op);
    }

    public void PlayRound(DiziRoundInfo roundInfo, Action callbackAction) =>
        Mono.StartCoroutine(PlayRoundCo(roundInfo, callbackAction));

    public IEnumerator PlayRoundCo(DiziRoundInfo roundInfo, Action callback)
    {
        foreach (var (pfm, performInfos) in roundInfo.UnitInfoMap)
        {
            var performOp = OpMap[pfm.InstanceId];
            var performPos = GetLocationPoint(performOp);
            for (var index = 0; index < performInfos.Count; index++)
            {
                var info = performInfos[index];
                var response = info.Response;
                var targetOp = OpMap[response.Target.InstanceId];
                var targetPos = GetLocationPoint(targetOp);
                EventSend(EventString.Battle_Performer_update, info.Performer);
                yield return CombatAnimator.PlayOffendMove(info, performOp, targetPos); //move
                Mono.StartCoroutine(PlayTargetResponse(pfm.InstanceId, index, response, targetOp)); //response
                var tran = UiHandler.GetObjRect(performOp);
                CombatAnimator.PlayPerformAnim(index, info, performOp, tran); //perform
                EventSend(EventString.Battle_Reponder_Update, response.Target);

                yield return new WaitForSeconds(0.2f);
                SetOpPos(targetOp.transform, targetPos); //align target position
                yield return CombatAnimator.PlayOffendReturn(performPos, performOp); //move return
            }
        }

        callback?.Invoke();
        yield return null;

        void SetOpPos(Transform tran, float pos) => tran.transform.SetX(pos);

        IEnumerator PlayTargetResponse(int performerId,int performIndex,CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response, CharacterOperator targetOp)
        {
            var tran = UiHandler.GetObjRect(targetOp);
            CombatAnimator.PlayResponseAnim(performerId, performIndex, response, targetOp, tran);
            return CombatAnimator.PlayResponse2DEffects(response, targetOp);
        }
    }

    private void EventSend(string eventString, params object[] response) =>
        Game.MessagingManager?.SendParams(eventString, response);

    private float GetLocationPoint(CharacterOperator op) => op.transform.localPosition.x;

    public void Reset()
    {
        UiHandler.ClearAll();
        foreach (var instanceId in OpMap.Keys.ToArray())
        {
            var op = OpMap[instanceId];
            OpMap.Remove(instanceId);
            Object.Destroy(op.gameObject);
        }
    }
}