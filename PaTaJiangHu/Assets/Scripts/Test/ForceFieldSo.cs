using System;
using System.Collections.Generic;
using BattleM;
using Systems;
using UnityEngine;

[CreateAssetMenu(fileName = "forceSo", menuName = "战斗测试/内功")]
[Serializable] public class ForceFieldSo : ScriptableObject, IForce,IDataElement
{
    [SerializeField] private string _name;
    [SerializeField] private int id;
    [SerializeField] private int 内转化率;
    [SerializeField] private int 内护甲;
    [SerializeField] private Form[] 招式;

    public int Id => id;

    public string Name => _name;
    public int MpRate => 内转化率;
    public int MpArmor => 内护甲;
    public IList<IForceForm> Forms => 招式;
    [Serializable] private class Form : IForceForm
    {
        [SerializeField] private string name;
        [SerializeField] private int 使用息;

        public int Breath => 使用息;
        public string Name => name;
    }

}