using System.Linq;
using Data;
using MyBox;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "dodgeSo", menuName = "战斗/武学/轻功")]
    internal class DodgeFieldSo : SkillFieldSo
    {
        public override SkillType SkillType => SkillType.Dodge;
    }
}