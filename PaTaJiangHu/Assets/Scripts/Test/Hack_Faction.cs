using System;
using _GameClient.Models;
using System.Collections.Generic;
using MyBox;
using Server.Configs.Items;
using UnityEditor;
using UnityEngine;
using Utls;

namespace Test
{
    internal class Hack_Faction : MonoBehaviour
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
            Game.World.SetFaction(new Faction(silver: 10000, yuanBao: 500, actionLing: 999, actionLingMax: 100,
                diziMap: new List<Dizi>(),food: 1000,wine: 1000,pill: 1000,herb: 1000));
            Game.MessagingManager.Send(EventString.Faction_Init, string.Empty);
        }
        public void AddItemToFaction()
        {
            var faction = Game.World.Faction;
            AddItem(Weapons, i => faction.AddWeapon(i.Instance()));
            AddItem(Armors, i => faction.AddArmor(i.Instance()));
            Medicines.ForEach(i => faction.AddMedicine(i.Item, i.Amount));
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
#if UNITY_EDITOR
    [CustomEditor(typeof(Hack_Faction))]
    internal class Hack_FactionEditor : Editor
    {
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Application.isPlaying) return;
            var script = (Hack_Faction)target;
            if (GUILayout.Button("添加物品到门派中"))
            {
                script.AddItemToFaction();
            }
        }
    }
#endif
}
