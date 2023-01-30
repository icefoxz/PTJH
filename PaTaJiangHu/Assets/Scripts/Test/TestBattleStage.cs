using System;
using System.Linq;
using BattleM;
using Server.Configs.Battles;
using Server.Configs.Items;
using UnityEngine;
using Visual.BattleUi;

namespace Test
{
    public class TestBattleStage : MonoBehaviour
    {
        [SerializeField] private CombatUnitManager.Judgment 战斗模式;
        [SerializeField] private BattleWindow _battleWindow;
        [SerializeField] private BattleUnit[] _battleUnits;
        private CombatUnitManager.Judgment Judge => 战斗模式;
        public IBattleWindow BattleWindow => _battleWindow;
        void Start() => Init();
        public void Init() => BattleWindow.Init(OnBattleResult);
        public void StartBattle()
        {
            BattleWindow.Show();
            BattleWindow.BattleSetup(GetCombatUnit(), Judge);
            BattleWindow.StartBattle();
        }
        private (int, CombatUnit)[] GetCombatUnit() => _battleUnits.Select(b => (Stance: b.立场, b.InstanceCombatUnit())).ToArray();
        private void OnBattleResult(bool isWin)
        {
            var stance = isWin ? 0 : 1;
            var obj = _battleWindow.Stage.GetCombatUnits().First(c => c.StandingPoint == stance);
            var msg = $"{obj.Name}胜利!";
            Debug.Log(msg);
        }
        [Serializable] private class BattleUnit
        {
            [SerializeField] private string _name;
            public int 立场;
            [SerializeField] private int 力量;
            [SerializeField] private int 敏捷;
            [SerializeField] private int 血;
            [SerializeField] private int 内;
            [SerializeField] private ForceFieldSo 内功;
            [SerializeField] private CombatFieldSo 武功;
            [SerializeField] private DodgeFieldSo 轻功;
            [SerializeField] private Equipment 装备;
            private IEquip equipCache;
            public string Name => _name;
            public int Strength => 力量;
            public int Agility => 敏捷;
            public int Hp => 血;
            public int Mp => 内;
            public IForceSkill Force => 内功.GetMaxLevel();
            public IEquip Equipment => equipCache ??= new BattleM.Equipment(装备);
            public IDodgeSkill Dodge => 轻功.GetMaxLevel();
            private CombatFieldSo CombatSkill => 武功;

            public CombatUnit InstanceCombatUnit()
            {
                var status = CombatStatus.Instance(Hp, Mp);
                return CombatUnit.Instance(Name, Strength, Agility, status, Force, CombatSkill.GetMaxLevel(), Dodge, Equipment);
            }
        }

        [Serializable]
        private class Equipment : IEquip
        {
            [SerializeField] private WeaponFieldSo 武器;
            [SerializeField] private ArmorFieldSo 防具;
            [SerializeField] private bool 暗器与武器是同一个;

            //[ConditionalField(nameof(暗器与武器是同一个), true)] 
            [SerializeField] private WeaponFieldSo 暗器;
            private IWeapon _weapon;
            private IWeapon _fling;
            private IArmor _armor;
            private bool IsWeaponFling => 暗器与武器是同一个;

            public IWeapon Weapon => _weapon ??= 武器.Instance();
            public IWeapon Fling => _fling ??= IsWeaponFling ? 武器.Instance() : 暗器.Instance();
            public IArmor Armor => _armor ??= 防具.Instance();
            public void FlingConsume() => throw new NotImplementedException();
        }
    }
}