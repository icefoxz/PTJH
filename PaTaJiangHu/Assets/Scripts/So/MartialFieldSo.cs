using System;
using System.Collections.Generic;
using BattleM;
using Data;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "martialSo", menuName = "战斗测试/武功")]
    [Serializable]
    public class MartialFieldSo : ScriptableObject, IMartial, IDataElement
    {
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private Way.Armed 类型;
        [SerializeField] private CombatForm[] 招式;
        [SerializeField] private ParryForm[] 招架;

        public int Id => id;
        public string Name => _name;
        public Way.Armed Armed => 类型;
        public IList<ICombatForm> Combats => 招式;
        public IList<IParryForm> Parries => 招架;

        [Serializable] private class CombatForm : ICombatForm
        {
            [SerializeField] private string name;
            [SerializeField] private int 气消耗;
            [SerializeField] private int 内消耗;
            [SerializeField] private int 使用息;
            [SerializeField] private int 对方硬直;
            [SerializeField] private int 己方硬直;
            [SerializeField] private int[] 连击伤害表;

            public string Name => name;
            public int Tp => 气消耗;
            public int Mp => 内消耗;
            public int Breath => 使用息;
            public int TarBusy => 对方硬直;
            public int OffBusy => 己方硬直;
            public ICombo Combo => 连击伤害表?.Length > 0 ? new ComboField(连击伤害表) : null;
            private class ComboField : ICombo
            {
                public int[] Rates { get; }

                public ComboField(int[] rates)
                {
                    Rates = rates;
                }
            }
        }

        [Serializable] private class ParryForm : IParryForm
        {
            [SerializeField] private string name;
            [SerializeField] private int 气消耗;
            [SerializeField] private int 内消耗;
            [SerializeField] private int 招架值;
            [SerializeField] private int 对方硬直;

            public string Name => name;
            public int Tp => 气消耗;
            public int Mp => 内消耗;
            public int Parry => 招架值;
            public int OffBusy => 对方硬直;
        }
    }
}