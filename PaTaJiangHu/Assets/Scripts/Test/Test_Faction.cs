using System;
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

        private ItemToAdd<WeaponFieldSo>[] Weapons => _weapons;
        private ItemToAdd<ArmorFieldSo>[] Armors => _armors;

        public void AddItemToFaction()
        {
            AddItem(Weapons, i => Game.World.Faction.AddWeapon(i.Instance()));
            AddItem(Armors, i => Game.World.Faction.AddArmor(i.Instance()));
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
