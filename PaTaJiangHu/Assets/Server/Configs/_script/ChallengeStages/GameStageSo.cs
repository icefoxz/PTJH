using System;
using Models;
using MyBox;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.Fields;
using Server.Configs.Items;
using Server.Configs.Skills;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.ChallengeStages
{
    public interface ISingleCombatNpc : ICombatNpc
    {
        string Faction { get; }
        IGameReward Reward { get; }
    }

    /// <summary>
    /// Npc基本信息类
    /// </summary>
    public interface INpc
    {
        string Name { get; }
        int Level { get; }
        Sprite Icon { get; }
    }
    /// <summary>
    /// 关卡npc类
    /// </summary>
    public interface ICombatNpc : INpc
    {
        bool IsBoss { get; }
        DiziCombatUnit GetDiziCombat();
        IDiziReward DiziReward { get; }
        int Hp { get; }
        int Mp { get; }
        int Strength { get; }
        int Agility { get; }
        IDiziEquipment Equipment { get; }
        ICombatSet GetCombatSet();
        ISkillMap<ISkillInfo> ForceSkillInfo { get; }
        ISkillMap<ICombatSkillInfo> CombatSkillInfo { get; }
        ISkillMap<ISkillInfo> DodgeSkillInfo { get; }
    }
    //对单个弟子的奖励
    public interface IDiziReward
    {
        int Exp { get; }
    }

    [CreateAssetMenu(fileName = "id_关卡", menuName = "关卡玩法/成就/关卡")]
    internal class GameStageSo : AutoAtNamingObject
    {
        [SerializeField] private int 回合限制 = 20;

        [FormerlySerializedAs("挑战")] [SerializeField]
        private CombatNpc[] 关卡;

        public int RoundLimit => 回合限制;

        public ISingleCombatNpc[] Npcs => 关卡;

        public ISingleCombatNpc GetNpc(int challengeIndex) => Npcs[challengeIndex];

        [Serializable]
        internal class CombatNpc : ISingleCombatNpc
        {
            #region ChangeName

            private bool ChangeElementName()
            {
                _name = _npc != null ? (Boss ? "Boss: " : string.Empty) + _npc.Name : "未设置NPC!";
                return true;
            }

            #endregion

            [ConditionalField(true, nameof(ChangeElementName))] [SerializeField] [ReadOnly]
            private string _name;

            [SerializeField] private CombatNpcSo _npc;
            [SerializeField] private bool Boss;
            [SerializeField] private int 等级 = 1;
            [SerializeField] private string 所属势力;
            [SerializeField] private RewardField 奖励;
            [SerializeField] private DiziRewardField 弟子奖励;

            public IGameReward Reward => 奖励;
            public IDiziReward DiziReward => 弟子奖励;
            public int Hp => _npc.Hp;
            public int Mp => _npc.Mp;
            public int Strength => _npc.Strength;
            public int Agility => _npc.Agility;
            public IDiziEquipment Equipment => _npc.Equipment;
            public ICombatSet GetCombatSet() => _npc.GetCombatSet();
            public ISkillMap<ISkillInfo> ForceSkillInfo => _npc.GetForceSkillinfo();
            public ISkillMap<ICombatSkillInfo> CombatSkillInfo => _npc.GetCombatSkillInfo();
            public ISkillMap<ISkillInfo> DodgeSkillInfo => _npc.GetDodgeSkillInfo();
            public bool IsBoss => Boss;
            public string Name => _npc.Name;
            public int Level => 等级;
            public Sprite Icon => _npc.Icon;
            public string Faction => 所属势力;

            private CombatNpcSo Npc => _npc;
            public DiziCombatUnit GetDiziCombat() => new(teamId: 1, npc: Npc);
        }
    }
}