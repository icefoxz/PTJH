using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT._AOT.Views.BaseUis;
using GameClient.Args;
using GameClient.GameScene.Animators;
using GameClient.GameScene.Background;
using GameClient.Modules.BattleM;
using GameClient.SoScripts;
using GameClient.System;
using UnityEngine;

namespace GameClient.GameScene
{
    public class BattleStage2D : MonoBehaviour
    {
        [SerializeField] private float _startingXPoint = 6;
        private Config.GameAnimConfig AnimConfig { get; set; }

        private DiziBattleAnimator CurrentBattleAnim { get; set; }
        public bool IsBusy => CurrentBattleAnim != null;

        public void InitBattle(DiziBattle battle, Config.GameAnimConfig config)
        {
            if (IsBusy) throw new NotImplementedException($"{name} is busy!");
            AnimConfig = config;
            var opMap = SetBattleStage(battle);
            CurrentBattleAnim =
                new DiziBattleAnimator(AnimConfig.DiziCombatVisualCfg, opMap, Game.Game2DLand.CharacterUiSyncHandler, this,
                    Game.MainUi.Pool.transform);
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
                var op = AnimConfig.InstanceCharacterOp(transform);
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
                var scaleX = op.transform.localScale.x;
                var isPlayer = kv.Key.TeamId == 0;//玩家固定teamId = 0
                var queue = isPlayer ? playerTeamIndex : enemyTeamIndex;//递进排队值
                var alignPos = isPlayer ? -1 : 1;//玩家与敌人站位修正
                var xPos = _startingXPoint * alignPos + queue;//计算站位
                op.transform.SetLocalScaleX(alignPos * scaleX);
                op.transform.SetLocalX(xPos);//设置站位
                if (isPlayer) playerTeamIndex++;//玩家递进:-6,-5,-4
                else enemyTeamIndex--;//敌人递进:6,5,4
            }
        }
    }
}