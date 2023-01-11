using System;
using Core;
using MyBox;
using UnityEngine;

namespace Server.Configs.Items
{
    public enum Treatments
    {
        [InspectorName("体力")] Stamina,
        [InspectorName("食物")] Food,
        [InspectorName("精神")] Emotion,
        [InspectorName("外伤")] Injury,
        [InspectorName("内伤")] Inner
    }

    public interface ITreatment
    {
        Treatments Treatment { get; }
        int GetValue(int max);
    }
    public interface IMedicine
    {
        int Id { get; }
        string Name { get; }
        ITreatment[] Treatments { get; }
        int Amount { get; }
        void Consume(int value);
    }

    [CreateAssetMenu(fileName = "id_medicine", menuName = "物件/弟子/药品")]
    internal class MedicineFieldSo : AutoUnderscoreNamingObject , IGameItem
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }

        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private MedicineFieldSo So;
        [SerializeField] private int 价钱;
        [SerializeField]private TreatmentMap[] 药效;

        private TreatmentMap[] TreatmentMaps=> 药效;
        public ItemType Type => ItemType.Medicine;
        public int Price => 价钱;

        public IMedicine Instance(int amount) =>
            new Medicine(Id, Name, TreatmentMaps, amount);

        [Serializable] private class TreatmentMap : ITreatment
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
                    Treatments.Stamina => StaminaText,
                    Treatments.Food => FoodText,
                    Treatments.Emotion => EmotionText,
                    Treatments.Injury => InjuryText,
                    Treatments.Inner => InnerText,
                    _ => string.Empty
                };
                string valueText;
                valueText = Percentage ? $" : {Value}%" : $" : {Value}";
                _name = treatmentText + valueText;
                return true;
            }

            [ConditionalField(true,nameof(OnChangeElementName))][SerializeField][ReadOnly] private string _name;
            [SerializeField] private Treatments 治疗;
            [SerializeField] private int 值;
            [SerializeField] private bool 是百分比;

            private bool Percentage => 是百分比;
            public Treatments Treatment => 治疗;
            public int Value => 值;
            public int GetValue(int max)
            {
                if (!Percentage) return Value;
                return Value / max * 100;
            }
        }
        private class Medicine : IMedicine
        {
            public int Id { get; }
            public string Name { get; }
            public ITreatment[] Treatments { get; }
            public int Amount { get; private set; }

            public Medicine(int id, string name, ITreatment[] treatments, int amount)
            {
                Treatments = treatments;
                Amount = amount;
                Id = id;
                Name = name;
            }

            public void Consume(int value = 1)
            {
                if (value < 0) throw new NotImplementedException($"{Name} 请求 value = {value}!");
                if (value > Amount)
                    throw new System.NotImplementedException($"{Name}数量不够! amount = {Amount}, 需:{value}");
                Amount -= value;
            }
        }
    }
}