using System;
using BattleM;
using UnityEngine;

namespace Server.Controllers.Items
{
    internal interface IMedicine
    {
        int Id { get; }
        string Name { get; }
        void Recover(ICombatStatus condition);
    }
    [CreateAssetMenu(fileName = "丹药",menuName = "配置/丹药")]
    internal class MedicineFieldsSo : ScriptableObject
    {
        [SerializeField] private Medicine[] _medicines;
        public IMedicine[] GetMedicines => _medicines;

        [Serializable]
        private class Medicine : IMedicine
        {
            [SerializeField] private string _name;
            [SerializeField] private int _id;
            [SerializeField] private int _hp;
            [SerializeField] private int _maxHp;
            [SerializeField] private int _tp;
            [SerializeField] private int _maxTp;
            [SerializeField] private int _mp;
            [SerializeField] private int _maxMp;

            public int Id => _id;
            public string Name => _name;
            private int GetHpRec(int max) => Recover(max, _hp);
            private int GetTpRec(int max) => Recover(max, _tp);
            private int GetMpRec(int max) => Recover(max, _mp);
            private int GetMaxHpRec(int max) => Recover(max, _maxHp);
            private int GetMaxTpRec(int max) => Recover(max, _maxTp);
            private int GetMaxMpRec(int max) => Recover(max, _maxMp);

            public void Recover(ICombatStatus c)
            {
                c.Hp.AddMax(GetMaxHpRec(c.Hp.Fix));
                c.Tp.AddMax(GetMaxTpRec(c.Tp.Fix));
                c.Mp.AddMax(GetMaxMpRec(c.Mp.Fix));
                c.Hp.Add(GetHpRec(c.Hp.Fix));
                c.Tp.Add(GetTpRec(c.Tp.Fix));
                c.Mp.Add(GetMpRec(c.Mp.Fix));
            }

            public int Recover(int fix, int value)
            {
                if (value < 0)
                {
                    var percent = -0.01f * value;
                    var result = (int)Mathf.Round(percent * fix);
                    return result;
                }
                return value;
            }

        }
    }
}
