using AOT.Core;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    public interface IAdvItem : IFunctionItem
    {
        int GetValue(int max);
    }

    [CreateAssetMenu(fileName = "id_advItem", menuName = "物件/历练道具")]
    public class AdvItemFieldSo : AutoUnderscoreNamingObject, IAdvItem
    {
        [SerializeField] private int 体力恢复;
        [SerializeField] private bool 是百分比;
        [SerializeField][TextArea] private string 说明;
        [SerializeField] private Sprite 图标;

        private bool Percentage => 是百分比;
        public int Value => 体力恢复;
        public Sprite Icon => 图标;
        public string About => 说明;
        public ItemType Type => ItemType.FunctionItem;
        public FunctionItemType FunctionType => FunctionItemType.AdvItem;

        public int GetValue(int max)
        {
            if (!Percentage) return Value;
            return Value / max * 100;
        }

    }
}