using System;
using System.Linq;
using MyBox;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.Skills
{
    public interface ISkillLevelMap
    {
        int Level { get; }
        float Rate { get; }
        int MinCost { get; }
        int BookCost { get; }
    }

    [CreateAssetMenu(fileName = "levelingStrSo", menuName = "战斗/武学/领悟策略")]
    internal class SkillLevelingSo : AutoBacktickNamingObject
    {
        [SerializeField] private LevelMap[] _levelMap;
        [Serializable] private class LevelMap : ISkillLevelMap
        {
            public bool SetName()
            {
                _name = $"[{Level}]等概率:{Rate}%, 时长:{MinCost}分钟, 秘籍数:{BookCost}";
                return true;
            }

            [ConditionalField(true, nameof(SetName))][ReadOnly][SerializeField] private string _name;
            [SerializeField] private int 等级;
            [SerializeField] private int 领悟分钟;
            [SerializeField] private float 成功率;
            [SerializeField] private int 消耗秘籍数 = 1;

            public int Level => 等级;
            public float Rate => 成功率;
            public int MinCost => 领悟分钟;
            public int BookCost => 消耗秘籍数;
        }
        public ISkillLevelMap GetLevelMap(int level) => _levelMap.SingleOrDefault(l => l.Level == level);
    }
}