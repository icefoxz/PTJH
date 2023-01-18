using System;
using _GameClient.Models;
using System.Collections.Generic;
using Server.Configs.Items;
using UnityEditor;
using UnityEngine;
using Utls;

namespace Test
{
    internal class Test_Faction : MonoBehaviour
    {
        [SerializeField] private ItemToAdd<WeaponFieldSo>[] _weapons;
        [SerializeField] private ItemToAdd<ArmorFieldSo>[] _armors;
        [SerializeField] private ItemToAdd<MedicineFieldSo>[] _medicines;

        private ItemToAdd<WeaponFieldSo>[] Weapons => _weapons;
        private ItemToAdd<ArmorFieldSo>[] Armors => _armors;
        private ItemToAdd<MedicineFieldSo>[] Medicines => _medicines;

        public static void TestFaction()
        {
            XDebug.Log("TestFaction Init!");
            Game.World.SetFaction(new Faction(silver: 10000, yuanBao: 500, actionLing: 1, diziMap: new List<Dizi>()));
        }
        public void AddItemToFaction()
        {
            var faction = Game.World.Faction;
            AddItem(Weapons, i => faction.AddWeapon(i.Instance()));
            AddItem(Armors, i => faction.AddArmor(i.Instance()));
            AddItem(Medicines, i => faction.AddMedicine(i, 1));
        }

        private void AddItem<T>(ItemToAdd<T>[] items, Action<T> addAction) where T : ScriptableObject
        {
            if (items == null) return;
            foreach (var obj in items)
            {
                for (var i = 0; i < obj.Amount; i++)
                    addAction(obj.Item);
            }

        }

        [Serializable] private class ItemToAdd<T> where T : ScriptableObject
        {
            [SerializeField] private int 数量 = 1;
            [SerializeField] private T 物品;

            public T Item => 物品;
            public int Amount => 数量;
        }
    }
    [CustomEditor(typeof(Test_Faction))]
    internal class Test_FactionEditor : Editor
    {
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Application.isPlaying) return;
            var script = (Test_Faction)target;
            if (GUILayout.Button("添加物品到门派中"))
            {
                script.AddItemToFaction();
            }
        }
    }
}
