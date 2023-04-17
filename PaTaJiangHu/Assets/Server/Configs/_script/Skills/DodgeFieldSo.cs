using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "dodgeSo", menuName = "战斗/武学/轻功")]
    internal class DodgeFieldSo : SkillFieldSo
    {
        public override SkillType SkillType => SkillType.Dodge;
    }
}