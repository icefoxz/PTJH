using System;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;

namespace GameClient.Args
{
    /// <summary>
    /// 战斗参数, 用于传递战斗信息, 所有信息都是当前状态记录, 除了<see cref="ICombatSet"/>
    /// </summary>
    public class CombatArgs : EventArgs
    {
        public int Round { get; set; }
        public DiziCombatUnit Caster { get; }
        public DiziCombatUnit Target { get; }

        public CombatArgs(DiziCombatUnit caster, DiziCombatUnit target, int round)
        {
            Caster = caster;
            Target = target;
            Round = round;
        }

        public (DiziCombatUnit self,DiziCombatUnit other) GetUnits(CombatUnit unit)
        {
            if (unit.InstanceId == Caster.InstanceId)
                return (Caster, Target);
            if (unit.InstanceId == Target.InstanceId)
                return (Target, Caster);
            throw new InvalidOperationException($"{unit.InstanceId}.{unit}不属于此数据组!");
        }

        public static DiziCombatUnit InstanceCombatUnit(string guid, string name, int hp, int mp, 
            int strength, int agility, int teamId, 
            ICombatSet combatSet, ISkillMap<ISkillInfo> forceInfo, 
            ISkillMap<ICombatSkillInfo> combatInfo, ISkillMap<ISkillInfo> dodgeInfo,
            IDiziEquipment equipment) => new(false, new DiziCombatUnit(guid: guid,
            teamId: teamId, name: name, strength: strength, 
            agility: agility, hp: hp, mp: mp, set: combatSet,
            forceInfo, combatInfo, dodgeInfo,
            equipment: equipment));

        public static DiziCombatUnit InstanceCombatUnit(Dizi dizi) => 
            InstanceCombatUnit(guid: dizi.Guid, name: dizi.Name,
                hp: dizi.Hp, mp: dizi.Mp, strength: dizi.Strength,
                agility: dizi.Agility, teamId: 0, combatSet: dizi.GetCombatSet(),
                dizi.Skill.Force, dizi.Skill.Combat, dizi.Skill.Dodge, dizi.Equipment);

        public static DiziCombatUnit InstanceCombatUnit(IDiziCombatUnit unit, bool fullCondition) =>
            new(fullCondition, unit);

        public static DiziCombatUnit InstanceCombatUnit(ICombatNpc npc) =>
            InstanceCombatUnit(guid: "npc", name: npc.Name,
                hp: npc.Hp, mp: npc.Mp, strength: npc.Strength,
                agility: npc.Agility, teamId: 1, combatSet: npc.GetCombatSet(),
                npc.ForceSkillInfo, npc.CombatSkillInfo, npc.DodgeSkillInfo, npc.Equipment);

        public static CombatArgs Instance(Dizi dizi,ICombatNpc npc, int round)
        {
            var aCombat = InstanceCombatUnit(dizi);
            var bCombat = InstanceCombatUnit(npc);
            return new CombatArgs(caster: aCombat, target: bCombat, round);
        }
        public static CombatArgs Instance(DiziCombatUnit a, DiziCombatUnit b, int round) => new(a, b, round);
    }
}