namespace AOT._AOT.Core
{
    public enum AdvEquipmentTypes
    {
        Horse,
        Pill,
    }
    public interface IAdvEquipment
    {
        int Id { get; }
        string Name { get; }
        AdvEquipmentTypes Type { get; }
    }
}