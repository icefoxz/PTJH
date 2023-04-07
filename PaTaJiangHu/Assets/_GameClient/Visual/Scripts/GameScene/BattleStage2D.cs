using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;

public class BattleStage2D : MonoBehaviour
{
    [SerializeField] private float _startingXPoint = 6;
    private Config.GameAnimConfig AnimConfig => Game.Config.GameAnimCfg;

    private DiziBattleAnimator CurrentBattleAnim { get; set; }
    private bool IsBusy => CurrentBattleAnim != null;

    public void InitBattle(DiziBattle battle)
    {
        if (IsBusy) throw new NotImplementedException($"{name} is busy!");
        var opMap = SetBattleStage(battle);
        CurrentBattleAnim =
            new DiziBattleAnimator(AnimConfig.DiziCombatCfg, opMap, Game.Game2DLand.CharacterUiSyncHandler, this);
    }

    public IEnumerator PlayRound(DiziRoundInfo info)
    {
        if (CurrentBattleAnim == null) throw new NotImplementedException($"{name} Animator is not set !");
        return CurrentBattleAnim.PlayRoundCo(info, null);
    }

    public void FinalizeBattle()
    {
        CurrentBattleAnim.Reset();
        CurrentBattleAnim = null;
    }

    private Dictionary<int, CharacterOperator> SetBattleStage(DiziBattle battle)
    {
        var map = battle.Fighters.Select(f =>
        {
            var op = Instantiate(AnimConfig.CharacterOpPrefab, transform);
            op.transform.localPosition = Vector3.zero;
            op.transform.localScale = Vector3.one;
            return new { f, op };
        }).ToDictionary(f => f.f, f => f.op);
        PlaceOperators(map);
        return map.ToDictionary(f => f.Key.InstanceId, f => f.Value);
    }

    private void PlaceOperators(Dictionary<DiziCombatUnit, CharacterOperator> map)
    {
        var playerTeamIndex = 0;//玩家组索引
        var enemyTeamIndex = 0;//敌方组索引
        foreach (var kv in map)
        {
            var op = kv.Value;
            var isPlayer = kv.Key.TeamId == 0;//玩家固定teamId = 0
            var queue = isPlayer ? playerTeamIndex : enemyTeamIndex;//递进排队值
            var align = isPlayer ? -1 : 1;//玩家与敌人站位修正
            var xPos = _startingXPoint * align + queue;//计算站位
            op.transform.SetLocalScaleX(align * -1);
            op.transform.SetLocalX(xPos);//设置站位
            if (isPlayer) playerTeamIndex++;//玩家递进:-6,-5,-4
            else enemyTeamIndex--;//敌人递进:6,5,4
        }
    }
}