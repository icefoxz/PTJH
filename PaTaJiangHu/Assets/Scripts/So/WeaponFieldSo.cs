using System;
using BattleM;
using Data;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "weaponSo", menuName = "战斗测试/武器")]
    [Serializable] public class WeaponFieldSo : ScriptableObject, IWeapon,IDataElement
    {
        [SerializeField] private string _name;
        [SerializeField]private int id;
        [SerializeField] private Way.Armed 类型;
        [SerializeField] private int 伤害值;
        [SerializeField] private Weapon.Injuries 伤害类型;
        [SerializeField] private int 品级;
        [SerializeField] private int 投掷次数 = 1;

        public int Id => id;
        public string Name => _name;
        public Way.Armed Armed => 类型;
        public int Damage => 伤害值;
        public Weapon.Injuries Injury => 伤害类型;
        public int Grade => 品级;
        public int FlingTimes => 投掷次数;
    }
}