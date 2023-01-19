using Core;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "id_advItem", menuName = "物件/弟子/历练道具")]
    internal class AdvItemFieldSo : AutoUnderscoreNamingObject, IGameItem
    {
        [SerializeField] private int 体力恢复;
        [SerializeField] private bool 是百分比;
        [SerializeField] private int 价钱;
        [SerializeField][TextArea] private string 说明;

        private bool Percentage => 是百分比;
        public int Value => 体力恢复;
        public string About => 说明;
        public ItemType Type => ItemType.AdvItems;
        public int Price => 价钱;

        public int GetValue(int max)
        {
            if (!Percentage) return Value;
            return Value / max * 100;
        }
    }
}