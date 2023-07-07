using System;
using GameClient.Modules.DiziM;

namespace GameClient.Modules.BattleM
{
    public interface ICombatArmedAptitude
    {
        float Unarmed { get; }
        float Sword { get; }
        float Blade { get; }
        float Staff { get; }
        float GetDamageRatio(WeaponArmed armed);
    }

    /// <summary>
    /// 武学类型资质
    /// </summary>
    public record CombatArmedAptitude : ICombatArmedAptitude
    {
        public float Unarmed { get; }
        public float Sword { get; }
        public float Blade { get; }
        public float Staff { get; }

        public CombatArmedAptitude()
        {
        
        }

        public CombatArmedAptitude(ICombatArmedAptitude c)
        {
            Unarmed = c.Unarmed;
            Sword = c.Sword;
            Blade = c.Blade;
            Staff = c.Staff;
        }

        public CombatArmedAptitude(float unarmed, float sword, float blade, float staff)
        {
            Unarmed = unarmed;
            Sword = sword;
            Blade = blade;
            Staff = staff;
        }

        public float GetDamageRatio(WeaponArmed armed) =>
            armed switch
            {
                WeaponArmed.Unarmed => Unarmed,
                WeaponArmed.Sword => Sword,
                WeaponArmed.Blade => Blade,
                WeaponArmed.Staff => Staff,
                _ => throw new ArgumentOutOfRangeException(nameof(armed), armed, null)
            } / 100f;
    }
}