using System;
using System.Linq;
using Data;
using MyBox;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "id_秘籍名字",menuName = "配置/秘籍")]
    [Serializable] public class ScrollFieldSo : ScrollSoBase
    {
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 价钱;
        [SerializeField] private LevelMap[] _levelMap;
        [ConditionalField(true,nameof(CountMaxLevel))][ReadOnly] [SerializeField] private int 最高等级;
        public override int Id => id;
        public override string Name => _name;
        public override int Price => 价钱;
        public bool CountMaxLevel()
        {
            if (_levelMap == null) return false;
            var needUpdate = _levelMap.Any(l => !l.HasName || l.Cache != l.Rate);
            if(needUpdate)
                for (var i = 0; i < _levelMap.Length; i++)
                {
                    var map = _levelMap[i];
                    map.SetName($"升等[{i + 2}]={map.Rate}%");
                }
            最高等级 = _levelMap.Length + 1;
            return true;
        }

        public override int GetUpgradeRate(int currentLevel)
        {
            if (currentLevel == 0)
                throw new NotImplementedException("等级不可为0");
            if (_levelMap.Length > currentLevel) return _levelMap[currentLevel].Rate;
            return 0;
        }

        [Serializable]private class LevelMap
        {
            [HideInInspector][SerializeField]private string _name;
            [SerializeField]private int 概率;
            public int Cache { get; set; }
            public int Rate => 概率;
            public bool HasName => !string.IsNullOrWhiteSpace(_name);
            public void SetName(string title)
            {
                _name = title;
                Cache = Rate;
            }
        }
    }
    //武功秘籍
    public abstract class ScrollSoBase : ScriptableObject, IDataElement
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract int Price { get; }
        public abstract int GetUpgradeRate(int currentLevel);
    }
}