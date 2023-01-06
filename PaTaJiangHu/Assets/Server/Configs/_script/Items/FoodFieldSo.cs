using Data;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "id_秘籍名字", menuName = "配置/秘籍")]
    internal class FoodFieldSo : FoodBase
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private int 价钱;

        public override int Id => _id;
        public override string Name => _name;
        public override int Price => 价钱;
    }

    public abstract class FoodBase :ScriptableObject,IDataElement
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract int Price { get; }
    }
}