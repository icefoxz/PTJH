using System;
using System.Linq;
using BattleM;
using UnityEngine;
using Visual.BattleUi;

namespace So
{
    public class TestBattleStage : MonoBehaviour
    {
        [SerializeField] private CombatManager.Judgment 战斗模式;
        [SerializeField] private BattleWindow _battleWindow;
        [SerializeField] private BattleUnit[] _battleUnits;
        private CombatManager.Judgment Judge => 战斗模式;
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
            [SerializeField] private MartialFieldSo 武功;
            [SerializeField] private DodgeFieldSo 轻功;
            [SerializeField] private Equipment 装备;
            private IEquipment equipment1;
            public string Name => _name;
            public int Strength => 力量;
            public int Agility => 敏捷;
            public int Hp => 血;
            public int Mp => 内;
            public IForce Force => 内功;
            public IEquipment Equipment => equipment1 ??= new BattleM.Equipment(装备);
            public IDodge Dodge => 轻功;
            public IMartial Martial => 武功;

            public CombatUnit InstanceCombatUnit()
            {
                var status = CombatStatus.Instance(Hp, Mp);
                return CombatUnit.Instance(Name, Strength, Agility, status, Force, Martial, Dodge, Equipment);
            }
        }
        [Serializable] private class Equipment : IEquip
        {
            [SerializeField] private WeaponFieldSo 武器;
            [SerializeField] private ArmorFieldSo 防具;
            [SerializeField] private bool 暗器与武器是同一个;
            //[ConditionalField(nameof(暗器与武器是同一个), true)] 
            [SerializeField] private WeaponFieldSo 暗器;
            private bool IsWeaponFling => 暗器与武器是同一个;
            public IWeapon Weapon => 武器;
            public IWeapon Fling => IsWeaponFling ? 武器 : 暗器;
            public IArmor Armor => 防具;
        }
    }
}