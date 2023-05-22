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
    public const string Test_AdvMapLoad = "Test_AdvMapLoad";
    public const string Test_AutoAdvDiziInit = "Test_AutoAdvDiziInit";

    //页面
    public const string Page_TreasureHouse = "Page_TreasureHouse";
    public const string Page_DiziList = "Page_DiziList";
    public const string Page_Faction = "Page_Faction";
    public const string Page_Adventure = "Page_Adventure";
    public const string Page_DiziRecruit = "Page_DiziRecruit";

    //奖励
    public const string Rewards_Propmt = "Rewards_Propmt";//多个奖励
    public const string Reward_Propmt = "Reward_Propmt";//单个奖励

    //招募
    public const string Recruit_DiziGenerated = "Recruit_DiziGenerated";
    public const string Recruit_DiziInSlot = "Recruit_DiziInSlot";

    //弟子dizi
    public const string Dizi_Params_StaminaUpdate = "Dizi_Params_StaminaUpdate";//弟子体力更新
    public const string Dizi_Params_StateUpdate = "Dizi_Params_StateUpdate";//弟子状态更新
    public const string Dizi_ConditionManagement = "Dizi_ConditionManagement";//弟子状态管理(打开窗口吃药)
    public const string Dizi_ConditionUpdate = "Dizi_ConditionUpdate";//弟子状态更新
    public const string Dizi_EquipmentManagement = "Dizi_EquipmentManagement";//装备管理
    public const string Dizi_ItemEquipped = "Dizi_ItemEquipped";//装备物品
    public const string Dizi_ItemUnEquipped = "Dizi_ItemUnEquipped";//卸下物品
    public const string Dizi_Props_Update = "Dizi_Props_Update";

    //弟子活动
    public const string Dizi_Activity_Reward = "Dizi_Activity_Reward";//弟子奖励活动
    public const string Dizi_Activity_Adjust = "Dizi_Activity_Adjust";//弟子调整活动
    public const string Dizi_Activity_Message = "Dizi_Activity_Messasge";//弟子活动信息

    //弟子历练
    public const string Dizi_Adv_EventMessage = "Dizi_Adv_Message";//弟子历练单个事件信息
    public const string Dizi_Adv_Start = "Dizi_Adv_Start";//弟子历练开始
    public const string Dizi_Adv_Recall = "Dizi_Adv_Recall";//弟子回程
    public const string Dizi_Adv_End = "Dizi_Adv_End";//弟子历练结束
    public const string Dizi_Adv_Finalize = "Dizi_Adv_Finalize";//弟子历练结算
    public const string Dizi_Adv_SlotManagement = "Dizi_Adv_SlotManagement";//弟子历练道具管理
    public const string Dizi_Adv_SlotUpdate = "Dizi_Adv_SlotUpdate";//弟子历练道具更新
    public const string Dizi_Adv_Terminate = "Dizi_Adv_Terminate";//弟子历练被强制中断

    //弟子闲置
    public const string Dizi_Idle_EventMessage = "Dizi_Idle_EventMessage";//弟子闲置状态更新
    public const string Dizi_Idle_Start = "Dizi_Idle_Start";//弟子闲置状态开始
    public const string Dizi_Idle_Stop = "Dizi_Idle_Stop";//弟子停止闲置通知

    //弟子失踪
    public const string Dizi_Lost_Start = "Dizi_Lost_Start";//失踪开始
    public const string Dizi_Lost_End = "Dizi_Lost_End";//失踪结束

    //弟子技能
    public const string Dizi_Skill_ComprehendFailed = "Dizi_Skill_ComprehendFailed";//弟子技能领悟失败
    public const string Dizi_Skill_LevelUp = "Dizi_Skill_ComprehendSuccess";//弟子技能领悟成功
    public const string Dizi_Skill_Update = "Dizi_Skill_Update"; //弟子技能更新

    //门派faction
    public const string Faction_DiziAdd = "Faction_DiziAdd";//给门派加弟子
    public const string Faction_Init = "Faction_Init";//实例门派
    public const string Faction_DiziListUpdate = "Faction_DiziListUpdate";//门派弟子列表更新
    public const string Faction_SilverUpdate = "Faction_SilverUpdate";//门派银两更新
    public const string Faction_YuanBaoUpdate = "Faction_YuanBaoUpdate";//门派元宝更新
    public const string Faction_FoodUpdate = "Faction_FoodUpdate"; //门派食物更新
    public const string Faction_WineUpdate = "Faction_WineUpdate"; //门派酒更新
    public const string Faction_PillUpdate = "Faction_PillUpdate"; //门派丹药更新
    public const string Faction_HerbUpdate = "Faction_HerbUpdate"; //门派草药更新
    public const string Faction_Params_ActionLingUpdate = "Faction_ActionLingUpdate";//门派行动令更新
    public const string Faction_AdvItemsUpdate = "Faction_AdvItemsUpdate";//门派历练道具更新
    public const string Faction_AdvPackageUpdate = "Faction_AdvPackageUpdate";//门派包裹更新
    public const string Faction_BookUpdate = "Faction_BookUpdate";//门派书籍更新
    public const string Faction_FunctionItemUpdate = "Faction_FunctionItemUpdate";//门派功能道具更新
    public const string Faction_Challenge_Update = "Faction_Challenge_Update";//门派挑战进度更新
    public const string Faction_Challenge_BattleEnd = "Faction_Challenge_BattleEnd";//门派挑战战斗结束
    public const string Faction_Stage_update = "Faction_Stage_update";//门派关卡更新

    //战斗演示
    public const string Battle_Performer_update = "Battle_Performer_update";//进攻者信息更新
    public const string Battle_Reponder_Update = "Battle_Reponder_Update";//反馈者信息更新
    public const string Battle_Init = "Battle_Init";//战斗开始
    public const string Battle_RoundUpdate = "Battle_RoundUpdate";//战斗回合更新
    public const string Battle_End = "Battle_Reset";//战斗场地清除

    //窗口
    public const string Win_PopUp = "Win_PopUp";//弹窗ui
}