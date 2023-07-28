using AOT.Core;
using AOT.Core.Dizi;
using MyBox;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    [CreateAssetMenu(fileName = "领悟道具", menuName = "物件/功能/领悟道具")]
    public class ComprehendItemSo : FunctionItemBase , IComprehendItem
    {
        [SerializeField] private ColorGrade 品级;
        [SerializeField] private float 领悟率;
        [SerializeField] private bool 等级限制;
        [ConditionalField(nameof(等级限制))][SerializeField] private MinMaxInt 等级范围;

        public ColorGrade Grade => 品级;
        public float Rate => 领悟率;

        private bool LevelRestriction => 等级限制;
        private MinMaxInt LevelRange => 等级范围;

        public override ItemType Type => ItemType.FunctionItem;
        public override FunctionItemType FunctionType => FunctionItemType.Comprehend;
        public bool IsLevelAvailable(int level) => !LevelRestriction || LevelRange.IsInRange(level);
    }

    public abstract class FunctionItemBase : AutoUnderscoreNamingObject,IFunctionItem
    {
        [SerializeField] private Sprite 图标;
        [SerializeField] private string 说明;
        public Sprite Icon => 图标;
        public string About => 说明;
        public abstract ItemType Type { get; }
        public abstract FunctionItemType FunctionType { get; }
    }
}