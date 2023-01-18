using UnityEngine;

namespace Server.Configs.Characters
{
    public enum DiziProps
    {
        [InspectorName("力量")] Strength,
        [InspectorName("敏捷")] Agility,
        [InspectorName("血")] Hp,
        [InspectorName("内")] Mp
    }
}