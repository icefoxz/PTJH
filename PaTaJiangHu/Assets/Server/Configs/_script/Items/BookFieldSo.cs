using System;
using System.Linq;
using Core;
using Data;
using MyBox;
using UnityEngine;

namespace Server.Configs.Items
{
    public interface IBook : IGameItem
    {
        int GetUpgradeRate(int currentLevel);
    }

    [CreateAssetMenu(fileName = "id_秘籍名字",menuName = "物件/弟子/秘籍")]
    internal class BookFieldSo : BookSoBase
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private BookFieldSo So;
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 价钱;
        [SerializeField] private LevelMap[] _levelMap;
        [SerializeField][TextArea] private string 说明;
        [ConditionalField(true,nameof(CountMaxLevel))][ReadOnly] [SerializeField] private int 最高等级;
        public override int Id => id;
        public override string Name => _name;

        public override string About => 说明;

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

    public abstract class BookSoBase : ScriptableObject, IBook
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract string About { get; }
        public ItemType Type => ItemType.Book;
        public abstract int Price { get; }
        public abstract int GetUpgradeRate(int currentLevel);
    }
}