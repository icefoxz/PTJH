using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;

[CreateAssetMenu(fileName = "挑战配置", menuName = "挑战/配置", order = 1)]
[Serializable]internal class ChallengeCfgSo : ScriptableObject
{
    [SerializeField] private int 升级连续过关次数 = 3;
    [SerializeField] private int 降级连续放弃次数 = 2;
    [SerializeField] private ChallengeLevel[] 配置;

    public int UpgradePassStreak => 升级连续过关次数;
    public int DowngradeAbandonStreak => 降级连续放弃次数;
    private ChallengeLevel[] Levels => 配置;

    public ChallengeStageSo GetRandomStage(int progress) => GetRandomStages(progress).RandomPick();
    public IEnumerable<ChallengeStageSo> GetRandomStages(int index) => Levels[index].Stages;

    [Serializable]private class ChallengeLevel
    {
        [SerializeField] private ChallengeStageSo[] 关卡;
        public ChallengeStageSo[] Stages => 关卡;
    }

    public ChallengeStageSo GetStage(int stageId) => Levels.SelectMany(level => level.Stages).FirstOrDefault(stage => stage.Id == stageId);

    public IChallengeNpc InstanceBattle(int stageId, int progress, int npcIndex) =>
        GetStage(stageId).GetChallengeNpcs(progress)[npcIndex];
}