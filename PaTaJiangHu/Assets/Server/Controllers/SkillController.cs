using System;
using System.Linq;
using BattleM;
using Server.Configs.Battles;
using Server.Configs.Skills;
using UnityEngine;

namespace Server.Controllers
{
    public enum SkillGrades
    {
        E,
        D,
        C,
        B,
        A,
        S,
    }

    public interface ISkillController
    {
        void SelectCombat(int index);
        void OnCombatLeveling(int level);
        void ListCombatSkills();
        void SelectForce(int index);
        void OnForceLeveling(int level);
        void ListForceSkills();
        void SelectDodge(int index);
        void OnDodgeLeveling(int level);
        void ListDodgeSkills();
    }

    public class SkillController : ISkillController
    {
        private Configure Config { get; }

        internal SkillController(Configure config)
        {
            Config = config;
        }

        private CombatFieldSo SelectedCombat { get; set; }
        public class Combat
        {
            public string Name { get; set; }
            public Way.Armed Armed { get; set; }
            public Form[] Combats { get; set; }
            public Exert[] Exerts { get; set; }
            public Combat()
            {
            
            }
            internal Combat(ICombatSkill cs,CombatFieldSo so)
            {
                Name = cs.Name;
                Armed = cs.Armed;
                Combats = cs.Combats.Select(c => new Form(c, so.GetBuffSos(c))).ToArray();
                Exerts = cs.Exerts.Select(c => new Exert(c)).ToArray();
            }
            public class Form
            {
                public string Name { get; set; }
                public int Parry { get; set; }
                public int ParryMp { get; set; }
                public int TarBusy { get; set; }
                public int OffBusy { get; set; }
                public int Breath { get; set; }
                public ComboField Combo { get; set; }
                public int CombatMp { get; set; }
                public int DamageMp { get; set; }
                public Buff[] Buffs { get; set; }
                public Form()
                {
                
                }
                public Form(ICombatForm c, CombatBuffSoBase[] buffSos)
                {
                    Name = c.Name;
                    Parry = c.Parry;
                    ParryMp = c.ParryMp;
                    TarBusy = c.TarBusy;
                    OffBusy = c.OffBusy;
                    Breath = c.Breath;
                    Combo = new ComboField(c.Combo);
                    CombatMp = c.CombatMp;
                    DamageMp = c.DamageMp;
                    Buffs = buffSos.Select(b => new Buff(b)).ToArray();
                }
                public class ComboField
                {
                    public int[] Rates { get; set; }

                    public ComboField()
                    {
                    
                    }
                    public ComboField(ICombo combo)
                    {
                        Rates = combo == null ? Array.Empty<int>() : combo.Rates.ToArray();
                    }
                }
            }
            public class Exert
            {
                public string Name { get; set; }
                public IPerform.Activities Activity { get; set; }

                public Exert()
                {
                
                }
                public Exert(IExert e)
                {
                    Name = e.Name;
                    Activity = e.Activity;
                }
            }
        }
        public void SelectCombat(int index)
        {
            SelectedCombat = Config.CombatSos[index];
            var cs = SelectedCombat.GetMaxLevel();
            var dto = new Combat(cs, SelectedCombat);
            Game.MessagingManager.Send(EventString.Test_CombatSkillSelected, dto);
        }
        public void OnCombatLeveling(int level)
        {
            var cs = level == 0 ? SelectedCombat.GetMaxLevel() : SelectedCombat.GetFromLevel(level);
            var dto = new Combat(cs, SelectedCombat);
            Game.MessagingManager.Send(EventString.Test_CombatSkillLeveling, dto);
        }
        public void ListCombatSkills()
        {
            Game.MessagingManager.Send(EventString.Test_CombatSoList,
                Config.CombatSos.Select(s => new Combat(s.GetMaxLevel(), s)).ToArray());
        }

        private ForceLevelingSo SelectedForce { get; set; }
        public class Force
        {
            public string Name { get; set; }
            public int Breath { get; set; }
            public int ForceRate { get; set; }
            public int Recover { get; set; }
            public int MpCharge { get; set; }
            public int ArmorCost { get; set; }
            public int Armor { get; set; }
            public int MaxLevel { get; set; }
            public Buff[] Buffs { get; set; }

            public Force()
            {
            
            }

            internal Force(IForceSkill f,Buff[] buffs, int maxLevel)
            {
                Name = f.Name;
                Recover = f.Recover;
                Breath = f.Breath;
                ForceRate = f.ForceRate;
                MpCharge = f.MpCharge;
                ArmorCost = f.ArmorCost;
                Armor = f.Armor;
                Buffs = buffs;
                MaxLevel = maxLevel;
            }
        }

        public void SelectForce(int index)
        {
            SelectedForce = Config.ForceSos[index];
            var f = SelectedForce.GetMaxLevel();
            var dto = new Force(f, 
                SelectedForce.GetBuffs(SelectedForce.MaxLevel).Select(b => new Buff(b)).ToArray(),
                SelectedForce.MaxLevel);
            Game.MessagingManager.Send(EventString.Test_ForceSkillSelected, dto);
        }
        public void OnForceLeveling(int level)
        {
            var selectedLevel = level == 0 ? SelectedForce.MaxLevel : level;
            var f = SelectedForce.GetFromLevel(selectedLevel);
            var dto = new Force(f, 
                SelectedForce.GetBuffs(selectedLevel).Select(b => new Buff(b)).ToArray(),
                SelectedForce.MaxLevel);
            Game.MessagingManager.Send(EventString.Test_ForceSkillLeveling, dto);
        }
        public void ListForceSkills()
        {
            Game.MessagingManager.Send(EventString.Test_ForceSoList,
                Config.ForceSos.Select(s =>
                        new Force(s.GetMaxLevel(), s.GetBuffs(s.MaxLevel).Select(b => new Buff(b)).ToArray(), s.MaxLevel))
                    .ToArray());
        }
    
        private DodgeLevelingSo SelectedDodge { get; set; }
        public class DodgeSkill
        {
            public string Name { get; set; }
            public int Breath { get; set; }
            public int Dodge { get; set; }
            public int DodgeMp { get; set; }
            public int MaxLevel { get; set; }
            public Buff[] Buffs { get; set; }

            public DodgeSkill()
            {

            }

            internal DodgeSkill(IDodgeSkill d, Buff[] buffs, int maxLevel)
            {
                Name = d.Name;
                Breath = d.Breath;
                Dodge = d.Dodge;
                DodgeMp = d.DodgeMp;
                Buffs = buffs;
                MaxLevel = maxLevel;
            }

        }
        public void SelectDodge(int index)
        {
            SelectedDodge = Config.DodgeSos[index];
            var dodge = SelectedDodge.GetMaxLevel();
            var dto = new DodgeSkill(dodge,
                SelectedDodge.GetBuffs(SelectedDodge.MaxLevel).Select(b => new Buff(b)).ToArray(),
                SelectedDodge.MaxLevel);
            Game.MessagingManager.Send(EventString.Test_DodgeSkillSelected, dto);
        }
        public void OnDodgeLeveling(int level)
        {
            var selectedLevel = level == 0 ? SelectedDodge.MaxLevel : level;
            var dodge = SelectedDodge.GetFromLevel(selectedLevel);
            var dto = new DodgeSkill(dodge,
                SelectedDodge.GetBuffs(selectedLevel).Select(b => new Buff(b)).ToArray(),
                SelectedDodge.MaxLevel);
            Game.MessagingManager.Send(EventString.Test_DodgeSkillLeveling, dto);
        }
        public void ListDodgeSkills()
        {
            Game.MessagingManager.Send(EventString.Test_DodgeSoList,
                Config.DodgeSos.Select(d =>
                        new DodgeSkill(d.GetMaxLevel(), d.GetBuffs(d.MaxLevel).Select(b => new Buff(b)).ToArray(),
                            d.MaxLevel))
                    .ToArray());
        }

        public class Buff
        {
            public string SoName { get; set; }
            public int Lasting { get; set; }
            public int Stacks { get; set; }
            public ICombatBuff.Appends Append { get; set; }
            public ICombatBuff.Consumptions Consumption { get; set; }

            public Buff() { }
            public Buff(string name, ICombatBuff b)
            {
                SoName = name;
                Lasting = b.Lasting;
                Stacks = b.Stacks;
                Append = b.Append;
                Consumption = b.Consumption;
            }

            public Buff(CombatBuffSoBase so)
            {
                SoName = so.name;
                Lasting = so.Lasting;
                Stacks = so.Stacks;
                Append = so.Append;
                Consumption = so.Consumption;
            }
        }

        [Serializable]internal class Configure
        {
            [SerializeField] private CombatFieldSo[] _combatSos;
            public CombatFieldSo[] CombatSos => _combatSos;
            [SerializeField] private ForceLevelingSo[] _forceSos;
            public ForceLevelingSo[] ForceSos => _forceSos;
            [SerializeField] private DodgeLevelingSo[] _dodgeSos;
            public DodgeLevelingSo[] DodgeSos => _dodgeSos;
        }
    }
}