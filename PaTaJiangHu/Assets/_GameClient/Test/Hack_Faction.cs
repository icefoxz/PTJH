using System;
using System.Collections.Generic;
using AOT.Utls;
using GameClient.Models;
using GameClient.SoScripts.Items;
using GameClient.System;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace GameClient.Test
{
    // 事件编辑
    internal class Hack_Faction : MonoBehaviour
    {
        [SerializeField] private ItemToAdd<WeaponFieldSo>[] _weapons;
        [SerializeField] private ItemToAdd<ArmorFieldSo>[] _armors;
        [SerializeField] private ItemToAdd<ShoesFieldSo>[] _shoes;
        [SerializeField] private ItemToAdd<DecorationFieldSo>[] _decorations;
        [SerializeField] private ItemToAdd<MedicineFieldSo>[] _medicines;
        [SerializeField] private ItemToAdd<BookFieldSo>[] _books;

        private ItemToAdd<WeaponFieldSo>[] Weapons => _weapons;
        private ItemToAdd<ArmorFieldSo>[] Armors => _armors;
        private ItemToAdd<ShoesFieldSo>[] Shoes => _shoes;
        private ItemToAdd<DecorationFieldSo>[] Decorations => _decorations;
        private ItemToAdd<MedicineFieldSo>[] Medicines => _medicines;
        private ItemToAdd<BookFieldSo>[] Books => _books;

        private void Awake()
        {
            Game.StartGame += TestFaction;
        }
        public static void TestFaction()
        {
            XDebug.Log("测试门派...初始化!");
            Game.World.SetFaction(new Faction(silver: 10000, yuanBao: 500, actionLing: 999, actionLingMax: 100,
                diziMap: new List<Dizi>(),food: 1000,wine: 1000,pill: 1000,herb: 1000));
        }
        public void AddItemToFaction()
        {
            var faction = Game.World.Faction;
            AddItem(Weapons, i => faction.AddWeapon(i.Instance()));
            AddItem(Armors, i => faction.AddArmor(i.Instance()));
            AddItem(Shoes, i => faction.AddShoes(i.Instance()));
            AddItem(Decorations, i => faction.AddDecoration(i.Instance()));
            AddItem(Books, i => faction.AddBook(i.Instance()));
            AddItem(Medicines, i => faction.AddMedicine(i.Instance()));
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
