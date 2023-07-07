using System;
using GameClient.Modules.DiziM;
using UnityEngine;

namespace GameClient.SoScripts.Skills
{
    /// <summary>
    /// 武功系数
    /// </summary>
    public enum CombatCoefficients
    {
        [InspectorName("拳脚")]UnarmedCo,       // 拳脚系的武功
        [InspectorName("短兵")]ShortWeaponCo,   // 短兵器武功
        [InspectorName("长兵")]LongWeaponCo,    // 长兵器武功
    }

    [CreateAssetMenu(fileName = "restraint", menuName = "战斗/武功系数克制")]
    [Serializable] public class RestraintCfgSo : ScriptableObject
    {
        [SerializeField] private float 长兵对短兵百分比;
        [SerializeField] private float 短兵对拳脚百分比;
        [SerializeField] private float 拳脚对长兵百分比;
        public float LongVsShort => 长兵对短兵百分比;
        public float ShortVsUnarmed => 短兵对拳脚百分比;
        public float UnarmedVsLong => 拳脚对长兵百分比;

        private CombatCoefficients GetRestraintCo(WeaponArmed armed) =>
            armed switch
            {
                WeaponArmed.Unarmed => CombatCoefficients.UnarmedCo,
                WeaponArmed.Sword => CombatCoefficients.ShortWeaponCo,
                WeaponArmed.Blade => CombatCoefficients.ShortWeaponCo,
                WeaponArmed.Staff => CombatCoefficients.LongWeaponCo,
                _ => throw new ArgumentOutOfRangeException(nameof(armed), armed, null)
            };

        public float ResolveRate(WeaponArmed attackArmed, WeaponArmed targetArmed)
        {
            var attacker = GetRestraintCo(attackArmed);
            var target = GetRestraintCo(targetArmed);
            return attacker switch
            {
                CombatCoefficients.UnarmedCo when target == CombatCoefficients.LongWeaponCo => UnarmedVsLong,
                CombatCoefficients.ShortWeaponCo when target == CombatCoefficients.UnarmedCo => ShortVsUnarmed,
                CombatCoefficients.LongWeaponCo when target == CombatCoefficients.ShortWeaponCo => LongVsShort,
                _ => 100f
            };
        }
    }
}