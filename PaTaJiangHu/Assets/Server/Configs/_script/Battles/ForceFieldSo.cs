using Data;
using MyBox;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗/武学/内功")]
    internal class ForceFieldSo : SkillFieldSo, IDataElement
    {
        public override SkillType SkillType => SkillType.Force;
    }

}