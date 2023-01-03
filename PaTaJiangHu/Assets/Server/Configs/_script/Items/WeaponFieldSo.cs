using System;
using BattleM;
using Data;
using MyBox;
using Server.Configs._script.Skills;
using UnityEditor;
using UnityEngine;

namespace Server.Configs._script.Items
{
    [CreateAssetMenu(fileName = "weaponSo", menuName = "战斗测试/武器")]
    [Serializable] public class WeaponFieldSo : ScriptableObject, IDataElement
    {
        private bool ChangeName()
        {
            var path = AssetDatabase.GetAssetPath(this);
            var newName = $"{id}_{_name}";
            var err = AssetDatabase.RenameAsset(path, newName);

            if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
            return true;
        }

        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }

        [ConditionalField(true,nameof(GetItem))][ReadOnly][SerializeField] private WeaponFieldSo So;
        [SerializeField] private string _name;
        [ConditionalField(true,nameof(ChangeName))][SerializeField] private int id;
        [SerializeField] private Way.Armed 类型;
        [SerializeField] private int 力量加成;
        [SerializeField] private Weapon.Injuries 伤害类型;
        [SerializeField] private SkillGrades 品级;
        [SerializeField] private int 投掷次数 = 1;

        public int Id => id;
        public string Name => _name;
        public Way.Armed Armed => 类型;
        public int Damage => 力量加成;
        public Weapon.Injuries Injury => 伤害类型;
        public SkillGrades Grade => 品级;
        public int FlingTimes => 投掷次数;
        public IWeapon Instance() => new WeaponField(Id, Name, Armed, Damage, Injury, Grade, FlingTimes);
        private class WeaponField : IWeapon
        {
            public int Id { get; }
            public string Name { get; }
            public Way.Armed Armed { get; }
            public int Damage { get; }
            public Weapon.Injuries Injury { get; }
            public SkillGrades Grade { get; }
            public int FlingTimes { get;  }

            public WeaponField(int id, string name, Way.Armed armed, int damage, Weapon.Injuries injury, SkillGrades grade, int flingTimes)
            {
                Id = id;
                Name = name;
                Armed = armed;
                Damage = damage;
                Injury = injury;
                Grade = grade;
                FlingTimes = flingTimes;
            }
            public void AddFlingTime(int value)
            {
                throw new NotImplementedException();
            }
        }
    }
}