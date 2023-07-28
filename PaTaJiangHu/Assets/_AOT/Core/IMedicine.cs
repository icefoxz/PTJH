using UnityEngine;

namespace AOT.Core
{
    public enum Treatments
    {
        [InspectorName("体力")] Stamina,
        [InspectorName("食物")] Food,
        [InspectorName("精神")] Emotion,
        [InspectorName("外伤")] Injury,
        [InspectorName("内伤")] Inner
    }

    public enum MedicineKinds
    {
        [InspectorName("体力类")] StaminaDrug,
        [InspectorName("食物类")] Food,
        [InspectorName("精神类")] EmoDrug,
        [InspectorName("外伤类")] InjuryDrug,
        [InspectorName("内伤类")] InnerDrug,
    }
    public interface ITreatment
    {
        Treatments Treatment { get; }
        int GetValue(int max);
    }
    public interface IMedicine : IFunctionItem
    {
        MedicineKinds Kind { get; }
        int Grade { get; }
        ITreatment[] Treatments { get; }
    }
}