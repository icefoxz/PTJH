using System;
using AOT.Core;
using AOT.Core.Dizi;
using MyBox;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    [CreateAssetMenu(fileName = "id_medicine", menuName = "物件/药品")]
    public class MedicineFieldSo : AutoUnderscoreNamingObject, IMedicine
    {
        [SerializeField] private MedicineKinds 类型;
        [SerializeField] private DiziGrades 品级;
        [SerializeField] private TreatmentMap[] 药效;
        [SerializeField] private Sprite 图标;
        [SerializeField][TextArea] private string 说明;
        public FunctionItemType FunctionType => FunctionItemType.Medicine;
        protected override string Suffix => Kind switch
        {
            MedicineKinds.StaminaDrug => "@体力",
            MedicineKinds.Food => "@食物",
            MedicineKinds.EmoDrug => "@精神",
            MedicineKinds.InjuryDrug => "@外伤",
            MedicineKinds.InnerDrug => "@内伤",
            _ => throw new ArgumentOutOfRangeException()
        };
        public ITreatment[] Treatments => 药效;
        public MedicineKinds Kind => 类型;
        public int Grade => (int)品级;
        public Sprite Icon => 图标;
        public string About => 说明;
        public ItemType Type => ItemType.FunctionItem;
        public IMedicine Instance()=> new Medicine(Id, Name, Kind, Grade, Treatments, About, Icon);

        [Serializable]
        private class TreatmentMap : ITreatment
        {
            private const string StaminaText = "体力";
            private const string FoodText = "食物";
            private const string EmotionText = "精神";
            private const string InjuryText = "外伤";
            private const string InnerText = "内伤";

            private bool OnChangeElementName()
            {
                var treatmentText = Treatment switch
                {
                    AOT.Core.Treatments.Stamina => StaminaText,
                    AOT.Core.Treatments.Food => FoodText,
                    AOT.Core.Treatments.Emotion => EmotionText,
                    AOT.Core.Treatments.Injury => InjuryText,
                    AOT.Core.Treatments.Inner => InnerText,
                    _ => string.Empty
                };
                string valueText;
                valueText = Percentage ? $" : {Value}%" : $" : {Value}";
                _name = treatmentText + valueText;
                return true;
            }

            [ConditionalField(true, nameof(OnChangeElementName))] [SerializeField] [ReadOnly]
            private string _name;

            [SerializeField] private Treatments 治疗;
            [SerializeField] private int 值;
            [SerializeField] private bool 是百分比;

            private bool Percentage => 是百分比;
            public int Value => 值;
            public Treatments Treatment => 治疗;

            public int GetValue(int max)
            {
                if (!Percentage) return Value;
                return Value / max * 100;
            }
        }

        private record Medicine : IMedicine
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public string Name { get; }
            public string About { get; }
            public ItemType Type => ItemType.FunctionItem;
            public MedicineKinds Kind { get; }
            public FunctionItemType FunctionType => FunctionItemType.Medicine;
            public int Grade { get; }
            public ITreatment[] Treatments { get; }

            public Medicine(int id, string name, MedicineKinds kind, int grade, ITreatment[] treatments, string about, Sprite icon)
            {
                Treatments = treatments;
                About = about;
                Icon = icon;
                Grade = grade;
                Kind = kind;
                Id = id;
                Name = name;
            }

        }
    }
}