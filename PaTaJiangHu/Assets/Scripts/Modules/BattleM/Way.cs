using System;
using System.Linq;
using UnityEngine;

namespace BattleM
{
    /// <summary>
    /// 有效值，直接与伤害值挂钩的值<br/>
    /// 道，例：剑道，拳道，刃道，棍道，鞭道...
    /// </summary>
    public static class Way
    {
        public static Armed[] Armeds { get; } = Enum.GetValues(typeof(Armed)).Cast<Armed>().ToArray();
        public enum Armed
        {
            [InspectorName("空手")] Unarmed,
            [InspectorName("剑法")] Sword,
            [InspectorName("刀法")] Blade,
            [InspectorName("棍型")] Stick,
            [InspectorName("短兵")] Short,
            [InspectorName("鞭法")] Whip,
            [InspectorName("暗器")] Fling,
        }
        public enum Effect
        {
            [InspectorName("力量")]Strength,
            [InspectorName("敏捷")]Agility
        }

        public static bool InCombatRange(this Armed armed,int distance)
        {
            switch (armed)
            {
                case Armed.Unarmed:
                case Armed.Short:
                    return distance == 1;
                case Armed.Sword:
                case Armed.Blade:
                    return distance == 2;
                case Armed.Stick:
                case Armed.Whip:
                    return distance == 3;
                case Armed.Fling:
                    return distance > 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(armed), armed, null);
            }
        }
    }
}