using MyBox;
using Server.Configs.Battles;
using Server.Configs.ChallengeStages;
using Server.Configs.Fields;
using Server.Configs.Items;
using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 挑战关卡
/// </summary>
public interface IChallengeStage
{
    int Id { get; }
    string Name { get; }
    string About { get; }
    Sprite Image { get; }
    int StageCount { get; }
    IChallengeNpc[] GetChallengeNpcs(int index);
}

public interface IChallengeNpc : ICombatNpc
{
    IGameChest Chest { get; }
}

[CreateAssetMenu(fileName = "id_挑战", menuName = "挑战/关卡")]
internal class ChallengeStageSo : AutoHashNamingObject,IChallengeStage
{
    [SerializeField] private int 回合限制 = 20;
    [SerializeField] private Sprite 图标;
    [SerializeField] private StageBoss[] 关卡;
    [SerializeField][TextArea] private string 说明;
    private StageBoss[] Stages => 关卡;
    public Sprite Image => 图标;
    public int StageCount => Stages.Length;
    public string About => 说明;

    public IChallengeNpc[] GetChallengeNpcs(int index) => Stages[index].Npcs;

    [Serializable] private class StageBoss
    {
        private bool GetStageBossNames()
        {
            if (Npcs.Any(n => n == null))
            {
                _name = "未设置NPC!";
                return false;
            }
            _name = string.Join(',', Npcs.Select(n => n.NpcName));
            return true;
        }

        [ConditionalField(true,nameof(GetStageBossNames))][SerializeField][ReadOnly]private string _name;
        public IChallengeNpc[] Npcs => 关主;
        [SerializeField] private ChallengeNpc[] 关主;

    }

    [Serializable] private class ChallengeNpc : IChallengeNpc
    {
        #region ChangeName

        private bool ChangeElementName()
        {
            _name = _npc != null ? (Boss ? "Boss: " : string.Empty) + _npc.Name : "未设置NPC!";
            return true;
        }

        #endregion

        [ConditionalField(true, nameof(ChangeElementName))] [SerializeField] [ReadOnly] private string _name;

        [SerializeField] private CombatNpcSo _npc;
        [SerializeField] private bool Boss;
        [SerializeField] private int 等级 = 1;
        [SerializeField] private GameChestSo 宝箱;
        [SerializeField] private DiziRewardField 弟子奖励;

        public IDiziReward DiziReward => 弟子奖励;
        public bool IsBoss => Boss;
        public string NpcName => _npc.Name;
        public int Level => 等级;
        public Sprite Icon => _npc.Icon;
        public IGameChest Chest => 宝箱.GetChest();

        private CombatNpcSo Npc => _npc;
        public DiziCombatUnit GetDiziCombat() => new(teamId: 1, npc: Npc);
    }
}