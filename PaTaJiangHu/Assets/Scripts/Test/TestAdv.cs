using System;
using System.Linq;
using Server;
using Server.Controllers.Adventures;
using UnityEngine;

public class TestAdv : MonoBehaviour
{
    [SerializeField] private AdventureUnit[] _units;
    private Adventure Current { get; set; }
    public AdvUnit[] GetUnits() => _units.Select(u => u.GetUnit()).ToArray();
    public void AdventureStart(int id)
    {
        Game.MessagingManager.RegEvent(EventString.Adventure_Start, arg =>
        {
            Current = new Adventure();
            Current.LoadParam(arg);
        });
        ServiceCaller.Instance.StartAdventure(id, GetUnits());
    }

    [Serializable] private class AdventureUnit
    {
        [SerializeField] private string _name;
        [SerializeField] private int _hp = 100;
        [SerializeField] private int _tp = 100;
        [SerializeField] private int _mp = 100;
        [SerializeField] private int _strength;
        [SerializeField] private int _agility;

        public AdvUnit GetUnit() => new AdvUnit()
            { Agility = _agility, Strength = _strength, Name = _name, Status = new UnitStatus(_hp, _tp, _mp) };
    }
}
