using UnityEngine;
using Utls;

internal class FactionEditableModel : MonoBehaviour
{
    [SerializeField]private FactionDataSo 门派引用;
    private FactionDataSo So => 门派引用;

    public void Init()
    {
        RegEvents();
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Faction_DiziAdd, DiziListUpdate);
    }

    private void DiziListUpdate(ObjectBag bag)
    {
        //更新弟子列表
    }

    public void LoadFaction()
    {
        //读取So中的门派
    }

    public void SaveFaction()
    {
        //储存当前门派
    }
}