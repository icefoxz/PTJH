using System;
using System.Linq;
using Data.LitJson;
using Server;
using Server.Controllers.Adventures;
using UnityEngine;

public class TestAdv : MonoBehaviour
{
    [SerializeField] private AdventureUnit[] _units;
    private Adventure Current { get; set; }
    internal AdvUnit[] GetUnits() => _units.Select(u => u.GetUnit()).ToArray();
    public void AdventureStart(int id)
    {
        Game.MessagingManager.RegEvent(EventString.Test_AdventureStart,
            arg => Current = JsonMapper.ToObject<Adventure>(arg)); //ObjectBag.LoadParam<Adventure>(arg));
        TestCaller.Instance.StartAdventure(id, GetUnits());
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