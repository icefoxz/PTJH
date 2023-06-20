using Server.Configs.Items;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCfgSo", menuName = "游戏配置/数据配置")]
internal class DataCfgSo : ScriptableObject
{
    [SerializeField] private WeaponFieldSo[] 武器;
    [SerializeField] private ArmorFieldSo[] 防具;
    [SerializeField] private ShoesFieldSo[] 鞋子;
    [SerializeField] private DecorationFieldSo[] 挂件;
    [SerializeField] private MedicineFieldSo[] 药品;
    [SerializeField] private BookFieldSo[] 书籍;
    [SerializeField] private AdvItemFieldSo[] 历练道具;
    [SerializeField] private ComprehendItemSo[] 功能道具;

    public WeaponFieldSo[] Weapons => 武器;
    public ArmorFieldSo[] Armors => 防具;
    public ShoesFieldSo[] Shoes => 鞋子;
    public DecorationFieldSo[] Decorations => 挂件;
    public MedicineFieldSo[] Medicines => 药品;
    public BookFieldSo[] Books => 书籍;
    public AdvItemFieldSo[] AdvItems => 历练道具;

    public ComprehendItemSo[] FunctionItems => 功能道具;
}