using System;
using MyBox;
using Server.Configs.Items;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "autoName", menuName = "战斗/武学/差值策略")]
    [Serializable]
    internal class CombatDifferentialStrategySo : AutoAtNamingObject, ISkillAttribute
    {
        public enum Calculate
        {
            [InspectorName("除")] Divide,
            [InspectorName("乘")] Multiply,
        }

        public enum Compares
        {
            [InspectorName("力")] Strength,
            [InspectorName("敏")] Agility,
            [InspectorName("血")] Hp,
            [InspectorName("血上限")] HpMax,
            [InspectorName("内")] Mp,
            [InspectorName("内上限")] MpMax,
        }

        public enum Settings
        {
            [InspectorName("重击触发")] HardRate,
            [InspectorName("重击倍率")] HardDamageRate,
            [InspectorName("会心触发")] CriticalRate,
            [InspectorName("闪避触发")] DogeRate
        }
        //private bool SetLevelName()
        //{
        //    var text = _set switch
        //    {
        //        Settings.HardRate => "重击触发",
        //        Settings.HardDamageRate => "重击倍率",
        //        Settings.CriticalRate => "会心触发",
        //        Settings.DogeRate => "闪避触发",
        //        _ => throw new ArgumentOutOfRangeException()
        //    };
        //    var compare = Compare switch
        //    {
        //        Compares.Strength => "力",
        //        Compares.Agility => "敏",
        //        Compares.Hp => "血",
        //        Compares.HpMax => "血上限",
        //        Compares.Mp => "内",
        //        Compares.MpMax => "内上限",
        //        _ => throw new ArgumentOutOfRangeException()
        //    };
        //    var cal = Cal switch
        //    {
        //        Calculate.Multiply => "乘",
        //        Calculate.Divide => "除",
        //        _ => throw new ArgumentOutOfRangeException()
        //    };
        //    var offsetText = Offset == 0 ? string.Empty
        //        : Offset > 0 ? $"+{Offset}"
        //        : $"{Offset}";

        //    _name = $"[{text}]{compare}差{offsetText}: ({cal})系数({Factor})";
        //    return true;
        //}

        //private string SetSpecial() => "特殊招式未支持";

        //[ConditionalField(true, nameof(SetLevelName))] [SerializeField] [ReadOnly] private string _name;

        [SerializeField] private Settings _set;
        [SerializeField] private Compares 差值;
        [SerializeField] private float 校正;
        [SerializeField] private Calculate 计算;
        [SerializeField] private float 系数;
        [SerializeField] private ColorGrade 品级;
        [SerializeField] [TextArea] private string 说明;

        public Settings Set => _set;
        public Compares Compare => 差值;
        public Calculate Cal => 计算;
        public float Factor => 系数;
        public float Offset => 校正;
        public ColorGrade Grade => 品级;
        public string Intro => 说明;

        #region Calculate

        private float Calculation(CombatArgs arg)
        {
            return Cal switch
            {
                Calculate.Divide => DivideFactor(arg),
                Calculate.Multiply => MultiplyFactor(arg),
                _ => throw new ArgumentOutOfRangeException()
            };

            float DivideFactor(CombatArgs a)
            {
                var caster = GetCombatValue(a.Caster);
                var target = GetCombatValue(a.Target);
                return (caster - target + Offset) / Factor;
            }

            float MultiplyFactor(CombatArgs a)
            {
                var caster = GetCombatValue(a.Caster);
                var target = GetCombatValue(a.Target);
                return (caster - target + Offset) * Factor;
            }
        }

        private float GetCombatValue(DiziCombatUnit dizi) =>
            Compare switch
            {
                Compares.Strength => dizi.Strength,
                Compares.Agility => dizi.Agility,
                Compares.Hp => dizi.Hp,
                Compares.HpMax => dizi.MaxHp,
                Compares.Mp => dizi.Mp,
                Compares.MpMax => dizi.MaxMp,
                _ => throw new ArgumentOutOfRangeException()
            };

        #endregion

        public float GetHardRate(CombatArgs arg) => Calculation(arg);
        public float GetHardDamageRatio(CombatArgs arg) => Calculation(arg);
        public float GetCriticalRate(CombatArgs arg) => Calculation(arg);
        public float GetDodgeRate(CombatArgs arg) => Calculation(arg);

        public ISkillAttribute GetCombatAttribute() => this;
    }
}