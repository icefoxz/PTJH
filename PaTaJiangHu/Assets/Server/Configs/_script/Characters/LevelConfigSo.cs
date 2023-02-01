﻿using System;
using System.Linq;
using BattleM;
using MyBox;
using UnityEngine;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "LevelingConfig", menuName = "配置/弟子/升等配置")]
    internal class LevelConfigSo : ScriptableObject
    {
        [SerializeField] private int[] 升等经验;
        [SerializeField] private LevelingConfig[] 力量成长;
        [SerializeField] private LevelingConfig[] 敏捷成长;
        [SerializeField] private LevelingConfig[] 血成长;
        [SerializeField] private LevelingConfig[] 内成长;
        private DiziProps[] _props;
        public DiziProps[] GetProsArray => _props ??= new[]
        {
            DiziProps.Strength, 
            DiziProps.Agility, 
            DiziProps.Hp, 
            DiziProps.Mp
        };
        
        private LevelingConfig[] StrengthGrowingRate => 力量成长;
        private LevelingConfig[] AgilityGrowingRate => 敏捷成长;
        private LevelingConfig[] HpGrowingRate => 血成长;
        private LevelingConfig[] MpGrowingRate => 内成长;
        private int[] UpgradeExp => 升等经验;
        public int MaxLevel => new[]
            { StrengthGrowingRate.Length, AgilityGrowingRate.Length, HpGrowingRate.Length, MpGrowingRate.Length }.Min();
        public int GetMaxExp(int level) => UpgradeExp[level - 1];
        private int LevelToIndex(int level) => level - 2;
        public int GetLeveledValue(DiziProps prop, int baseValue, int level)
        {
            if (level <= 0) throw new InvalidOperationException($"{prop}.等级不可以小于1!");
            if (level == 1) return baseValue;
            return prop switch
            {
                DiziProps.Strength => LevelingFormula(level, baseValue, StrengthGrowingRate),
                DiziProps.Agility => LevelingFormula(level, baseValue, AgilityGrowingRate),
                DiziProps.Hp => LevelingFormula(level, baseValue, HpGrowingRate),
                DiziProps.Mp => LevelingFormula(level, baseValue, MpGrowingRate),
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };
        }
        private int LevelingFormula(int level, int baseValue, LevelingConfig[] rates)
        {
            var index = LevelToIndex(level);
            if (rates.Length <= index)
                throw new InvalidOperationException($"等级已超过最大限制[{rates.Length + 1}],当前等级{level}！");
            return (int)(rates[index].Ratio * baseValue);
        }

        [Serializable]
        private class LevelingConfig
        {
            private bool Rename()
            {
                _name = $"倍率{Ratio * 100}%";
                return true;
            }

            [ConditionalField(true,nameof(Rename))][ReadOnly][SerializeField] private string _name;
            [SerializeField] private float 倍率;
            public float Ratio => 倍率;
        }
    }
}
