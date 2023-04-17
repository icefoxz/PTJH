using Data;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗/武学/内功")]
    internal class ForceFieldSo : SkillFieldSo, IDataElement
    {
        public override SkillType SkillType => SkillType.Force;
    }

}