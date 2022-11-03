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

        public int Id => id;
        public string Name => _name;
        public Way.Armed Armed => 类型;
        public IList<ICombatForm> Combats => 招式;

        [Serializable] private class CombatForm : ICombatForm
        {
            [SerializeField] private string name;
            [SerializeField] private int 气消耗;
            [SerializeField] private int 息;
            [SerializeField] private int 进攻内使用;
            [SerializeField] private int 招架值;
            [SerializeField] private int 招架内消耗;
            [SerializeField] private int 对方硬直;
            [SerializeField] private int 己方硬直;
            [SerializeField] private int[] 连击伤害表;

            public string Name => name;
            public int Tp => 气消耗;
            public int CombatMp => 进攻内使用;
            public int Breath => 息;
            public int TarBusy => 对方硬直;
            public int Parry => 招架值;
            public int ParryMp => 招架内消耗;

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
    }
}