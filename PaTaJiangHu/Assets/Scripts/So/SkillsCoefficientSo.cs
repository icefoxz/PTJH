using System;
using MyBox;
using UnityEngine;

namespace _GameClient.Models
{

    /// <summary>
    /// 武功系数配置
    /// </summary>
    [CreateAssetMenu(fileName = "技能系数配置", menuName = "配置/简易战斗/技能系数配置")]
    internal class SkillsCoefficientSo : ScriptableObject
    {

        [ConditionalField(true,nameof(SetName))][ReadOnly][SerializeField]private string _maxLevel;
        [SerializeField] private LevelConfig[] 等级;
        [SerializeField] private CoRate 设置;

        private CoRate CoefficientRate => 设置;
        private LevelConfig[] Levels => 等级;

        private bool SetName()
        {
            _maxLevel = Levels.Length > 0 ? $"最大等级[{Levels.Length}]" : string.Empty;
            for (var index = 0; index < Levels.Length; index++)
            {
                var level = index + 1;
                var field = Levels[index];
                field._name = $"等级{level}";
            }

            return true;
        }

        public int GetCoefficient(int level, SkillGrades grade)
        {
            if (level == 0) return 0;
            var index = level - 1;
            if (index < 0 || index >= Levels.Length) throw new IndexOutOfRangeException($"Level = {level}");
            var cfg = Levels[index];
            return grade switch
            {
                SkillGrades.E => cfg.E,
                SkillGrades.D => cfg.D,
                SkillGrades.C => cfg.C,
                SkillGrades.B => cfg.B,
                SkillGrades.A => cfg.A,
                SkillGrades.S => cfg.S,
                _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
            };
        }

        private enum Sub
        {
            Offend,Defend
        }
        private int GetSubCoefficient(int level, SkillGrades grade,Sub sub)
        {
            var co = CoefficientRate;
            if (co.OffendRate is < 0 or > 100 || co.DefendRate is < 0 or > 100)
                throw new InvalidOperationException(
                    $"{nameof(GetSubCoefficient)}:{name} offend = {co.OffendRate}, defend = {co.DefendRate}");
            if (!co.IsSeparate)
                throw new InvalidOperationException($"{nameof(GetSubCoefficient)}:{name}.未分开设置");
            return sub switch
            {
                Sub.Offend => (int)(GetCoefficient(level, grade) * co.OffendRate * 0.01f),
                Sub.Defend => (int)(GetCoefficient(level, grade) * co.DefendRate * 0.01f),
                _ => throw new ArgumentOutOfRangeException(nameof(sub), sub, null)
            };
        }
        public int GetOffendCoefficient(int level, SkillGrades grade) => GetSubCoefficient(level, grade, Sub.Offend);
        public int GetDefendCoefficient(int level, SkillGrades grade) => GetSubCoefficient(level, grade, Sub.Defend);

        [Serializable] private class LevelConfig
        {
            [ReadOnly] public string _name;
            [SerializeField] private int _s;
            [SerializeField] private int _a;
            [SerializeField] private int _b;
            [SerializeField] private int _c;
            [SerializeField] private int _d;
            [SerializeField] private int _e;
            public int S => _s;
            public int A => _a;
            public int B => _b;
            public int C => _c;
            public int D => _d;
            public int E => _e;
        }
        [Serializable]private class CoRate
        {
            private bool AuToSetDef()
            {
                _defendRate = 100 - _offendRate;
                return true;
            }

            [ConditionalField(true,nameof(AuToSetDef))][SerializeField] private bool 分开设置系数;
            [ConditionalField(nameof(分开设置系数))][SerializeField] private float _offendRate;
            [ConditionalField(nameof(分开设置系数))][ReadOnly][SerializeField] private float _defendRate;

            public bool IsSeparate => 分开设置系数;
            public float OffendRate => _offendRate;
            public float DefendRate => _defendRate;
        }
    }

    public interface ISimulation
    {
        float Offend { get; }
        float Defend { get; }
        float Strength { get; }
        float Agility { get; }
        float Weapon { get; }
        float Armor { get; }
        float Combat { get; }
        float Force { get; }
        float Dodge { get; }
    }
}
