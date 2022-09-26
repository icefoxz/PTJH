using System;
using System.Collections.Generic;
using BattleM;
using Systems;
using UnityEngine;

namespace Test
{
    [CreateAssetMenu(fileName = "dodgeSo", menuName = "战斗测试/轻功")]
    public class DodgeFieldSo : ScriptableObject,IDodge,IDataElement
    {
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private Form[] 招式;

        public int Id => id;
        public string Name => _name;
        public IList<IDodgeForm> Forms => 招式;
        [Serializable] private class Form : IDodgeForm
        {
            [SerializeField] private string name;
            [SerializeField] private int 气消耗;
            [SerializeField] private int 内消耗;
            [SerializeField] private int 使用息;
            [SerializeField] private int 身法值;
            public string Name => name;
            public int Tp => 气消耗;
            public int Mp => 内消耗;
            public int Breath => 使用息;
            public int Dodge => 身法值;
        }

    }
}