public class Global
{
    public const string SceneCanvas = "SceneCanvas";
}

public class EventString
{
    public const string Test_DiziRecruit = "Test_DiziRecruit";
    public const string Test_DiziGenerate = "Test_DiziGenerate";
    public const string Test_StaminaWindow = "Test_StaminaWindow";
    public const string Test_UpdateHp = "Test_UpdateHp";
    public const string Test_UpdateMp = "Test_UpdateMp";
    public const string Test_MedicineWindow = "Test_MedicineWindow";
    public const string Test_StatusUpdate = "Test_StatusUpdate";
    public const string Test_AdventureMap = "Test_AdventureMap";
    public const string Test_AdvEventInvoke = "Test_EventInvoke";
    public const string Test_DiziLeveling = "Test_DiziLeveling";
    public const string Test_CombatSoList = "Test_CombatSoList";
    public const string Test_CombatSkillSelected = "Test_CombatSkillSelected";
    public const string Test_CombatSkillLeveling = "Test_CombatSkillLeveling";
    public const string Test_ForceSkillSelected = "Test_ForceSkillSelected";
    public const string Test_ForceSkillLeveling = "Test_ForceSkillLeveling";
    public const string Test_ForceSoList = "Test_ForceSoList";
    public const string Test_DodgeSoList = "Test_DodgeSoList";
    public const string Test_DodgeSkillLeveling = "Test_DodgeSkillLeveling";
    public const string Test_DodgeSkillSelected = "Test_DodgeSkillSelected";
    public const string Test_AdvMapLoad = "Test_AdvMapLoad";
    public const string Test_SimulationOutcome = "Test_SimulationOutcome";
    public const string Test_SimulationUpdateModel = "Test_SimulationUpdateModel";
    public const string Test_SimulationStart = "Test_SimulationStart";

    public const string Test_AutoAdvDiziInit = "Test_AutoAdvDiziInit";

    //弟子dizi
    public const string Recruit_DiziGenerated = "Recruit_DiziGenerated";
    public const string Recruit_DiziInSlot = "Recruit_DiziInSlot";
    public const string Dizi_AdvManagement = "Dizi_AdvManagement";//历练管理项
    public const string Dizi_Params_StaminaUpdate = "Dizi_Params_StaminaUpdate";
    public const string Dizi_ConditionManagement = "Dizi_ConditionManagement";
    public const string Dizi_ConditionUpdate = "Dizi_ConditionUpdate";//弟子状态更新
    public const string Dizi_EquipmentManagement = "Dizi_EquipmentManagement";//装备管理
    public const string Dizi_ItemEquipped = "Dizi_ItemEquipped";//装备物品
    public const string Dizi_ItemUnEquipped = "Dizi_ItemUnEquipped";//卸下物品
    //弟子历练
    public const string Dizi_Adv_EventMessage = "Dizi_Adv_Message";//弟子历练单个事件信息
    public const string Dizi_Adv_Start = "Dizi_Adv_Start";//弟子历练开始
    public const string Dizi_Adv_Recall = "Dizi_Adv_Recall";//弟子被叫回
    public const string Dizi_Adv_End = "Dizi_Adv_End";//弟子历练结束

    //门派faction
    public const string Faction_DiziAdd = "Faction_DiziAdd";//给门派加弟子
    public const string Faction_Init = "Faction_Init";//实例门派
    public const string Faction_DiziListUpdate = "Faction_DiziListUpdate";//门派弟子列表更新
    public const string Faction_DiziSelected = "Faction_DiziSelected";
    public const string Faction_SilverUpdate = "Faction_SilverUpdate";
    public const string Faction_YuanBaoUpdate = "Faction_YuanBaoUpdate";
    public const string Faction_Params_ActionLingUpdate = "Faction_ActionLingUpdate";
}