using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Core.Systems.Messaging;
using AOT.Utls;
using AOT.Views.Abstract;
using AOT.Views.BaseUis;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.Modules.DiziM;
using GameClient.SoScripts;
using GameClient.SoScripts.Adventures;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    internal class Demo_Page_Main : PageUiManagerBase
    {
        private RectTransform rect_top { get; set; }
        private RectTransform rect_btm { get; set; }
        private RectTransform rect_game { get; set; }
        private Page_main_consumeRes main_consumeRes { get; set; }
        private Page_main_equipment main_equipment { get; set; }
        private Page_main_conprops main_conProps { get; set; }
        private Page_main_diziActivity main_diziActivity { get; set; }
        private Page_main_adventureMaps main_adventureMaps { get; set; }
        private Page_main_diziList main_diziList { get; set; }
        private Page_main_diziInfo main_diziInfo { get; set; }
        private Page_main_challengeStageSelector main_challengeStageSelector { get; set; }

        private Page_main_adventureView game_adventureView { get; set; }

        private FactionController FactionController => Game.Controllers.Get<FactionController>();
        private DiziController DiziController => Game.Controllers.Get<DiziController>();
        private DiziAdvController DiziAdvController => Game.Controllers.Get<DiziAdvController>();
        private static GameWorld.DiziState WorldState => Game.World.State;

        private Demo_v1Agent Agent { get; }

        public Demo_Page_Main(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Agent = uiAgent;
        }

        protected override string ViewName => "demo_page_main";

        protected override void Build(IView v)
        {
            rect_top = v.GetObject<RectTransform>("rect_top");
            rect_btm = v.GetObject<RectTransform>("rect_btm");
            rect_game = v.GetObject<RectTransform>("rect_game");
            var b = Game.UiBuilder;
            b.GetRes("game_adventureView", rect_game,
                (_, view) => { game_adventureView = new Page_main_adventureView(view); });
            b.GetRes("page_main_diziInfo", rect_top, (_, view) => { main_diziInfo = new Page_main_diziInfo(view); });
            b.GetRes("page_main_consumeRes", rect_top, (_, view) =>
            {
                main_consumeRes = new Page_main_consumeRes(view,
                    onResourceClick: (guid, res) => FactionController.ConsumeResourceByStep(guid, res),
                    onSilverAction: (guid, silver) => DiziController.UseSilver(guid, silver));
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start,
                    bag => main_consumeRes.Update(bag.GetString(0)));
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Finalize,
                    bag => main_consumeRes.Update(bag.GetString(0)));
                Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate,
                    bag => main_consumeRes.Update(bag.GetString(0)));
            });
            b.GetRes("page_main_equipment", rect_top, (_, view) =>
            {
                main_equipment = new Page_main_equipment(view, onItemSelection: Agent.Win_EquipmentManagement);
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start,
                    bag => main_equipment.Update(bag.GetString(0)));
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Finalize,
                    bag => main_equipment.Update(bag.GetString(0)));
                Game.MessagingManager.RegEvent(EventString.Dizi_EquipmentUpdate,
                    bag => main_equipment.Update(bag.GetString(0)));
            });
            b.GetRes("page_main_conProps", rect_top, (_, view) =>
            {
                main_conProps = new Page_main_conprops(view);
                Game.MessagingManager.RegEvent(EventString.Dizi_EquipmentUpdate,
                    bag => main_conProps.Update(bag.GetString(0)));
                Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate,
                    bag => main_conProps.Update(bag.GetString(0)));
            });
            b.GetRes("page_main_diziActivity", rect_btm, (_, view) =>
            {
                main_diziActivity = new Page_main_diziActivity(view,
                    onRecallAction: guid => DiziAdvController.AdventureRecall(guid),
                    onMapListAction: Dizi_AdvMapSelection,
                    onDiziForgetAction: guid => XDebug.LogWarning("当前弟子遗忘互动"),
                    onDiziBuyBackAction: guid => XDebug.LogWarning("当前弟子买回互动"),
                    onDiziReturnAction: guid => DiziAdvController.AdventureFinalize(guid),
                    onChallengeAction: UpdateChallengeSelector);
                Game.MessagingManager.RegEvent(EventString.Dizi_State_Update, b =>
                {
                    var guid = b.GetString(0);
                    main_diziActivity.ActivityUpdate(guid);
                });
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, b =>
                {
                    var guid = b.GetString(0);
                    main_diziActivity.ActivityUpdate(guid);
                });
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Waiting, b =>
                {
                    var guid = b.GetString(0);
                    main_diziActivity.ActivityUpdate(guid);
                });
                Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Recall, b =>
                {
                    var guid = b.GetString(0);
                    main_diziActivity.ActivityUpdate(guid);
                });
                Game.MessagingManager.RegEvent(EventString.Dizi_Activity_Message, b =>
                {
                    var guid = b.GetString(0);
                    var message = b.GetString(1);
                    main_diziActivity.AdventureLogUpdate(guid);
                });
                Game.MessagingManager.RegEvent(EventString.Dizi_Activity_Adjust, b =>
                {
                    var guid = b.GetString(0);
                    var adjust = b.GetString(1);
                    main_diziActivity.AdventureLogUpdate(guid);
                });
                Game.MessagingManager.RegEvent(EventString.Dizi_Activity_Reward, b =>
                {
                    var guid = b.GetString(0);
                    main_diziActivity.AdventureLogUpdate(guid);
                });
            });
            b.GetRes("page_main_adventureMaps", rect_btm, (_, view) =>
            {
                main_adventureMaps = new Page_main_adventureMaps(view,
                    () => XDebug.LogWarning("更新历练地图"),
                    onAdvStartAction: mapId => DiziAdvController.AdventureStart(SelectedDizi.Guid, mapId));
            });
            b.GetRes("page_main_diziList", rect_btm, (_, view) =>
            {
                main_diziList = new Page_main_diziList(view, dizi => { Agent.MainPage_Show(dizi.Guid); });
                Game.MessagingManager.RegEvent(EventString.Faction_Init, bag => main_diziList.SetElements());
                Game.MessagingManager.RegEvent(EventString.Dizi_State_Update, b => main_diziList.SetElements());
                Game.MessagingManager.RegEvent(EventString.Faction_DiziListUpdate, bag =>
                {
                    main_diziList.UpdateList();
                    main_diziList.SetElements();
                });
            });
            b.GetRes("page_main_challengeStageSelector", rect_btm, (_, view) =>
            {
                main_challengeStageSelector = new Page_main_challengeStageSelector(view,
                    onChallengeAction: challengeIndex => Agent.Dizi_ChallengeStart(challengeIndex));
            });
        }

        private Dizi SelectedDizi { get; set; }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_Update, _ => UpdateChallengeSelector());
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_BattleEnd, _ => UpdateChallengeSelector());
        }

        public void Set(Dizi dizi)
        {
            SelectedDizi = dizi;
            main_diziInfo.SetDizi(SelectedDizi.Guid);
            main_consumeRes.Set(dizi.Guid);
            main_equipment.Set(dizi.Guid);
            main_conProps.Set(dizi.Guid);
            main_diziActivity.Set(dizi.Guid);
            game_adventureView.Set(dizi);
            main_adventureMaps.Display(false);
        }

        // 历练地图选择
        private void Dizi_AdvMapSelection(string guid, int mapType)
        {
            SelectedDizi = Game.World.Faction.GetDizi(guid);
            var maps = DiziAdvController.AutoAdvMaps(mapType);
            main_adventureMaps.ListMap(maps);
            main_adventureMaps.Display(true);
        }

        // 获取挑战关卡并打开选择窗口
        public void UpdateChallengeSelector()
        {
            var faction = Game.World.Faction;
            if (!faction.IsChallenging)
            {
                Hide();
                return;
            }

            main_challengeStageSelector.Set(faction.GetChallengeStage());
            main_challengeStageSelector.Display(true);
        }

        private class Page_main_diziInfo : UiBase
        {
            private Image Img_diziAvatar { get; }
            private Text Text_diziName { get; }
            private View_Level LevelView { get; }
            private View_Stamina StaminaView { get; }

            public Page_main_diziInfo(IView v) : base(v, true)
            {
                v.ResetRectToZero();
                Img_diziAvatar = v.GetObject<Image>("img_diziAvatar");
                Text_diziName = v.GetObject<Text>("text_diziName");
                LevelView = new View_Level(v.GetObject<View>("view_level"));
                StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"));
            }

            public Dizi SelectedDizi { get; private set; }
            public void SetIcon(Sprite ico) => Img_diziAvatar.sprite = ico;

            public void SetName(string name, int color)
            {
                Text_diziName.text = name;
                Text_diziName.color = Game.GetColorFromGrade(color);
            }

            public void SetDizi(string guid)
            {
                var faction = Game.World.Faction;
                var dizi = faction.GetDizi(guid);
                SelectedDizi = dizi;
                SetDizi(dizi);
            }

            private void SetDizi(Dizi dizi)
            {
                SetName(dizi.Name, dizi.Grade);
                LevelView.SetLevel(dizi.Level);
                LevelView.SetExp(dizi.Exp.Value, dizi.Exp.Max);
                StaminaView.SetHour(dizi.Stamina.SecsPerRecover);
                UpdateDiziStamina(dizi.Guid);
            }

            public void UpdateDiziStamina(string diziGuid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != diziGuid)
                    return;
                var (stamina, max, min, sec) = SelectedDizi.Stamina.GetStaminaValue();
                StaminaView.SetStamina(stamina, max);
                StaminaView.SetTime(min, sec);
            }

            private class View_Level : UiBase
            {
                private Text Text_levelValue { get; }
                private Text Text_expValue { get; }
                private Text Text_expMax { get; }
                private Slider Slider_exp { get; }

                public View_Level(IView v) : base(v, true)
                {
                    Text_levelValue = v.GetObject<Text>("text_levelValue");
                    Text_expValue = v.GetObject<Text>("text_expValue");
                    Text_expMax = v.GetObject<Text>("text_expMax");
                    Slider_exp = v.GetObject<Slider>("slider_exp");
                }

                public void SetLevel(int level) => Text_levelValue.text = level.ToString();

                public void SetExp(int value, int max)
                {
                    Text_expValue.text = value.ToString();
                    Text_expMax.text = max.ToString();
                    Slider_exp.value = 1f * value / max;
                }
            }

            private class View_Stamina : UiBase
            {
                private View_Volume VolumeView { get; }
                private Text Text_hour { get; }
                private View_Time TimeView { get; }

                public View_Stamina(IView v) : base(v, true)
                {
                    VolumeView = new View_Volume(v.GetObject<View>("view_volume"));
                    TimeView = new View_Time(v.GetObject<View>("view_time"));
                    Text_hour = v.GetObject<Text>("text_hour");
                }

                public void SetHour(int hour) => Text_hour.text = hour.ToString();
                public void SetStamina(int value, int max) => VolumeView.SetVolume(value, max);
                public void SetTime(int min, int sec) => TimeView.SetTime(min, sec);

                private class View_Volume : UiBase
                {
                    private Text Text_value { get; }
                    private Text Text_max { get; }

                    public View_Volume(IView v) : base(v, true)
                    {
                        Text_value = v.GetObject<Text>("text_value");
                        Text_max = v.GetObject<Text>("text_max");
                    }

                    public void SetVolume(int value, int max)
                    {
                        Text_value.text = value.ToString();
                        Text_max.text = max.ToString();
                    }
                }

                private class View_Time : UiBase
                {
                    private Text Text_min { get; }
                    private Text Text_Separator { get; }
                    private Text Text_sec { get; }

                    public View_Time(IView v) : base(v, true)
                    {
                        Text_min = v.GetObject<Text>("text_min");
                        Text_Separator = v.GetObject<Text>("text_separator");
                        Text_sec = v.GetObject<Text>("text_sec");
                    }

                    public void SetTime(int min, int sec)
                    {
                        Text_Separator.gameObject.SetActive(min + sec >= 0);
                        Text_min.text = EmptyIfZero(min);
                        Text_sec.text = EmptyIfZero(sec);
                    }

                    private string EmptyIfZero(int value) => value <= 0 ? string.Empty : value.ToString("00");
                }
            }
        }

        private class Page_main_consumeRes : UiBase
        {
            private Element Silver { get; }
            private Element Food { get; }
            private Element Emotion { get; }
            private Element Injury { get; }
            private Element Inner { get; }

            public Page_main_consumeRes(IView v,
                Action<string, IAdjustment.Types> onResourceClick,
                Action<string, int> onSilverAction) : base(v, true)
            {
                v.ResetRectToZero();
                Silver = new Element(v.GetObject<View>("element_silver"),
                    () => onSilverAction?.Invoke(SelectedDizi?.Guid, 1));
                Food = new Element(v.GetObject<View>("element_food"),
                    () => onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Food));
                Emotion = new Element(v.GetObject<View>("element_emotion"),
                    () => onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Emotion));
                Injury = new Element(v.GetObject<View>("element_injury"),
                    () => onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Injury));
                Inner = new Element(v.GetObject<View>("element_inner"),
                    () => onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Inner));
            }

            private Dizi SelectedDizi { get; set; }

            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                SetDiziElement(SelectedDizi);
                var dizi = SelectedDizi;
                var isIdleState = dizi.Activity == DiziActivities.Idle;
                Silver.SetInteraction(isIdleState);
                Food.SetInteraction(isIdleState);
                Emotion.SetInteraction(isIdleState);
                Injury.SetInteraction(isIdleState);
                Inner.SetInteraction(isIdleState);
                if (isIdleState && dizi.Silver.Value == 100) Silver.SetInteraction(false);
                else Silver.SetInteraction(true);
            }

            private void SetDiziElement(Dizi dizi)
            {
                var controller = Game.Controllers.Get<DiziController>();
                var (silverText, sColor) = controller.GetSilverCfg(dizi.Silver.ValueMaxRatio);
                var (foodText, fColor) = controller.GetFoodCfg(dizi.Food.ValueMaxRatio);
                var (emotionText, eColor) = controller.GetEmotionCfg(dizi.Emotion.ValueMaxRatio);
                var (injuryText, jColor) = controller.GetInjuryCfg(dizi.Injury.ValueMaxRatio);
                var (innerText, nColor) = controller.GetInnerCfg(dizi.Inner.ValueMaxRatio);
                Silver.SetElement(1, sColor, silverText);
                Silver.SetValue(dizi.Silver.Value, dizi.Silver.Max);
                Food.SetElement(dizi.Capable.Food, fColor, foodText);
                Food.SetValue(dizi.Food.Value, dizi.Food.Max);
                Emotion.SetElement(dizi.Capable.Wine, eColor, emotionText);
                Emotion.SetValue(dizi.Emotion.Value, dizi.Emotion.Max);
                Injury.SetElement(dizi.Capable.Herb, jColor, injuryText);
                Injury.SetValue(dizi.Injury.Value, dizi.Injury.Max);
                Inner.SetElement(dizi.Capable.Pill, nColor, innerText);
                Inner.SetValue(dizi.Inner.Value, dizi.Inner.Max);

            }

            internal void Set(string guid)
            {
                var dizi = Game.World.Faction.GetDizi(guid);
                SelectedDizi = dizi;
                Update(SelectedDizi.Guid);
            }

            private class Element : UiBase
            {
                private Text Text_consume { get; }
                private Scrollbar Scrbar_status { get; }
                private Text Text_statusValue { get; }
                private Button Btn_status { get; }
                private Text Text_statusInfo { get; }
                private Image BgImg { get; }
                private Image HandleImg { get; }

                public Element(IView v, Action onClickAction) : base(v, true)
                {
                    Text_consume = v.GetObject<Text>("text_consume");
                    Scrbar_status = v.GetObject<Scrollbar>("scrbar_status");
                    HandleImg = Scrbar_status.image;
                    BgImg = Scrbar_status.GetComponent<Image>();
                    Text_statusValue = v.GetObject<Text>("text_statusValue");
                    Btn_status = v.GetObject<Button>("btn_status");
                    Btn_status.OnClickAdd(onClickAction);
                    Text_statusInfo = v.GetObject<Text>("text_statusInfo");
                }

                public void SetElement(int value, Color color, string info)
                {
                    SetConsume(value);
                    SetColor(color);
                    SetInfo(info);
                }

                public void SetConsume(int value) => Text_consume.text = value.ToString();
                public void SetInfo(string info) => Text_statusInfo.text = info;

                public void SetColor(Color color)
                {
                    HandleImg.color = color;
                    BgImg.color = new Color(color.r - 0.7f, color.g - 0.7f, color.b - 0.7f);
                }

                public void SetValue(int value, int max)
                {
                    Text_statusValue.text = value.ToString();
                    Scrbar_status.size = 1f * value / max;
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_status.interactable = isInteractable;
                }
            }
        }

        private class Page_main_equipment : UiBase
        {
            private Element element_weapon { get; }
            private Element element_armor { get; }
            private Element element_shoes { get; }
            private Element element_decoration { get; }

            public Page_main_equipment(IView v, Action<string, int> onItemSelection) : base(v, true)
            {
                v.ResetRectToZero();
                element_weapon = new Element(v.GetObject<View>("element_weapon"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 0));
                element_armor = new Element(v.GetObject<View>("element_armor"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 1));
                element_shoes = new Element(v.GetObject<View>("element_shoes"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 2));
                element_decoration = new Element(v.GetObject<View>("element_decoration"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 3));
            }

            private Dizi SelectedDizi { get; set; }

            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                UpdateDiziEquipment(SelectedDizi);
                var dizi = SelectedDizi;
                var isIdleState = dizi.Activity == DiziActivities.Idle;
                element_weapon.SetInteraction(isIdleState);
                element_armor.SetInteraction(isIdleState);
                element_shoes.SetInteraction(isIdleState);
                element_decoration.SetInteraction(isIdleState);
            }

            private void UpdateDiziEquipment(Dizi dizi)
            {
                if (dizi.Equipment.Weapon == null) element_weapon.ClearItem(element_weapon);
                else
                    element_weapon.SetItem(element_weapon, dizi.Equipment.Weapon.Name, (int)dizi.Equipment.Weapon.Grade,
                        $"{dizi.Equipment.Weapon.Quality}品", dizi.Equipment.Weapon.Icon);
                if (dizi.Equipment.Armor == null) element_armor.ClearItem(element_armor);
                else
                    element_armor.SetItem(element_armor, dizi.Equipment.Armor.Name, (int)dizi.Equipment.Armor.Grade,
                        $"{dizi.Equipment.Armor.Quality}品", dizi.Equipment.Armor.Icon);
                if (dizi.Equipment.Shoes == null) element_shoes.ClearItem(element_shoes);
                else
                    element_shoes.SetItem(element_shoes, dizi.Equipment.Shoes.Name, (int)dizi.Equipment.Shoes.Grade,
                        $"{dizi.Equipment.Shoes.Quality}品", dizi.Equipment.Shoes.Icon);
                if (dizi.Equipment.Decoration == null) element_decoration.ClearItem(element_decoration);
                else
                    element_decoration.SetItem(element_decoration, dizi.Equipment.Decoration.Name,
                        (int)dizi.Equipment.Decoration.Grade,
                        $"{dizi.Equipment.Decoration.Quality}品", dizi.Equipment.Decoration.Icon);
            }

            public void Set(string guid)
            {
                var dizi = Game.World.Faction.GetDizi(guid);
                SelectedDizi = dizi;
                Update(SelectedDizi.Guid);
            }

            private class Element : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_title { get; }
                private Text Text_short { get; }
                private Button Btn_element { get; }

                public Element(IView v, Action onClickAction) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_title = v.GetObject<Text>("text_title");
                    Text_short = v.GetObject<Text>("text_short");
                    Btn_element = v.GetObject<Button>("btn_element");
                    Btn_element.OnClickAdd(onClickAction);
                }

                private void SetImage(Sprite img)
                {
                    Img_ico.sprite = img;
                    Img_ico.gameObject.SetActive(true);
                }

                private void SetTitle(string title, int grade, string @short)
                {
                    Text_title.text = title;
                    Text_short.text = @short;
                    Text_title.color = Game.GetColorFromItemGrade(grade);
                    Text_title.gameObject.SetActive(true);
                    Text_short.gameObject.SetActive(true);
                }

                public void ClearItem(Element item)
                {
                    item.SetEmpty(true);
                    SetInteraction(false);
                }

                private void SetEmpty(bool empty)
                {
                    Img_ico.gameObject.SetActive(!empty);
                    Text_title.gameObject.SetActive(!empty);
                    Text_short.gameObject.SetActive(!empty);
                }

                public void SetItem(Element item, string itemName, int grade, string @short, Sprite icon)
                {
                    item.SetTitle(itemName, grade, @short);
                    SetImage(icon);
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_element.interactable = isInteractable;
                }
            }
        }

        private class Page_main_conprops : UiBase
        {
            public enum Props
            {
                Strength,
                Agility,
                Hp,
                Mp
            }

            private Element_prop Strength { get; }
            private Element_prop Agility { get; }
            private Element_prop Hp { get; }
            private Element_prop Mp { get; }

            public Page_main_conprops(IView v) : base(v, true)
            {
                v.ResetRectToZero();
                Strength = new Element_prop(v.GetObject<View>("element_strength"));
                Agility = new Element_prop(v.GetObject<View>("element_agility"));
                Hp = new Element_prop(v.GetObject<View>("element_hp"));
                Mp = new Element_prop(v.GetObject<View>("element_mp"));
            }

            private Dizi SelectedDizi { get; set; }

            public void Set(string guid)
            {
                var dizi = Game.World.Faction.GetDizi(guid);
                SelectedDizi = dizi;
                Update(SelectedDizi.Guid);
            }

            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                SetDizi(SelectedDizi);
            }

            private void SetDizi(Dizi dizi)
            {
                SetDiziProps(dizi);
            }

            private void SetDiziProps(Dizi dizi)
            {
                var s = dizi.StrengthProp;
                var a = dizi.AgilityProp;
                var h = dizi.HpProp;
                var m = dizi.MpProp;
                SetProps(Props.Strength, (int)s.LeveledValue, s.SkillBonus(), s.EquipmentBonus(), s.StateBonus());
                SetProps(Props.Agility, (int)a.LeveledValue, a.SkillBonus(), a.EquipmentBonus(), a.StateBonus());
                SetProps(Props.Hp, (int)h.LeveledValue, h.SkillBonus(), h.EquipmentBonus(), h.StateBonus());
                SetProps(Props.Mp, (int)m.LeveledValue, m.SkillBonus(), m.EquipmentBonus(), m.StateBonus());
            }

            //private void SetProp(Props props, int value, int skill, int equip = 0, int condition = 0) => Set(props, value, skill, equip, condition);
            private void SetProps(Props prop, int value, int skill, int equip, int condition) =>
                GetProp(prop).Set(value, skill, equip, condition);

            private Element_prop GetProp(Props prop) => prop switch
            {
                Props.Strength => Strength,
                Props.Agility => Agility,
                Props.Hp => Hp,
                Props.Mp => Mp,
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };

            public class Element_prop : UiBase
            {
                private Text Text_selfValue { get; }
                private Text Text_skillValue { get; }
                private Text Text_equipmentValue { get; }
                private Text Text_conValue { get; }

                public Element_prop(IView v) : base(v, true)
                {
                    Text_selfValue = v.GetObject<Text>("text_selfValue");
                    Text_skillValue = v.GetObject<Text>("text_skillValue");
                    Text_equipmentValue = v.GetObject<Text>("text_equipmentValue");
                    Text_conValue = v.GetObject<Text>("text_conValue");
                }

                public void Set(int value, int skill, int equip, int condition)
                {
                    Text_selfValue.text = SetText(value);
                    Text_skillValue.text = SetText(skill);
                    Text_equipmentValue.text = SetText(equip);
                    Text_conValue.text = SetText(condition);
                    Text_conValue.color = GetBuffColor(condition < 0);

                    string SetText(int v, string prefix = "") => v == 0 ? string.Empty : prefix + v;
                }
            }
        }

        private static Color GetBuffColor(bool isDebuff) => Game.Controllers.Get<DiziController>().BuffColor(isDebuff);

        private class Page_main_diziActivity : UiBase
        {
            private ScrollContentAligner Scroll_advLog { get; }
            private View_Buttons ButtonsView { get; }
            private Adventure_ActivityManager Adventure => Game.World.State.Adventure;
            private Idle_ActivityManager Idle => Game.World.State.Idle;

            public Page_main_diziActivity(IView v,
                Action<string> onRecallAction,
                Action<string, int> onMapListAction,
                Action<string> onDiziForgetAction,
                Action<string> onDiziBuyBackAction,
                Action<string> onDiziReturnAction,
                Action onChallengeAction
            ) : base(v, true)
            {
                v.ResetRectToZero();
                Scroll_advLog = v.GetObject<ScrollContentAligner>("scroll_advLog");
                Scroll_advLog.OnResetElement += OnAdvMsgReset;
                Scroll_advLog.OnSetElement += OnLogSet;
                Scroll_advLog.Init();

                ButtonsView = new View_Buttons(v.GetObject<View>("view_buttons"),
                    () => onRecallAction(SelectedDizi.Guid),
                    () => onMapListAction(SelectedDizi.Guid, 0),
                    () => onMapListAction(SelectedDizi.Guid, 1),
                    () => onDiziForgetAction(SelectedDizi.Guid),
                    () => onDiziBuyBackAction(SelectedDizi.Guid),
                    () => onDiziReturnAction(SelectedDizi.Guid),
                    () => onChallengeAction?.Invoke()
                );
                ButtonsView.SetMode(View_Buttons.Modes.Idle);
            }

            public void Set(string diziGuid)
            {
                var dizi = Game.World.Faction.GetDizi(diziGuid);
                SelectedDizi = dizi;
                ActivityUpdate(diziGuid);
            }

            public void ActivityUpdate(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                View_Buttons.Modes mode;
                switch (SelectedDizi.Activity)
                {
                    case DiziActivities.None:
                        mode = View_Buttons.Modes.None;
                        break;
                    case DiziActivities.Lost:
                        mode = View_Buttons.Modes.Lost;
                        break;
                    case DiziActivities.Idle:
                        mode = View_Buttons.Modes.Idle;
                        break;
                    case DiziActivities.Adventure:
                    {
                        var activity = Adventure.GetActivity(guid);
                        mode = activity.State switch
                        {
                            AdventureActivity.States.Progress => View_Buttons.Modes.Adventure,
                            AdventureActivity.States.Returning => View_Buttons.Modes.Returning,
                            AdventureActivity.States.Waiting => View_Buttons.Modes.Waiting,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        break;
                    }
                    case DiziActivities.Battle:
                        mode = View_Buttons.Modes.None;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ButtonsView.SetMode(mode);
                AdventureLogUpdate(SelectedDizi.Guid);
            }

            public void AdventureLogUpdate(string guid)
            {
                if (SelectedDizi == null) return;
                if (guid != SelectedDizi.Guid) return;
                var logs = GetLog();
                if(logs == null) return;
                Scroll_advLog.SetList(logs.Count);
            }

            private IView OnLogSet(int index, View view)
            {
                var log = new Prefab_Log(view);
                var storyLog = GetLog();
                var fragment = storyLog[storyLog.Count - index - 1];
                if (fragment.Reward != null)
                    log.SetReward(fragment.Reward);
                else log.SetMessage(fragment.Message);
                log.Display(true);
                return view;
            }

            private IReadOnlyList<ActivityFragment> GetLog()
            {
                return SelectedDizi.Activity switch
                {
                    DiziActivities.None => null,
                    DiziActivities.Lost => null,
                    DiziActivities.Idle => Idle.GetFragments(SelectedDizi.Guid),
                    DiziActivities.Adventure => Adventure.GetFragments(SelectedDizi.Guid),
                    DiziActivities.Battle => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            private void OnAdvMsgReset(IView v) => v.GameObject.SetActive(false);

            private Dizi SelectedDizi { get; set; }

            private class View_Buttons : UiBase
            {
                public enum Modes
                {
                    None = 0,
                    Idle,
                    Adventure,
                    Returning,
                    Waiting,
                    Lost
                }

                private Button Btn_callback { get; }
                private Button Btn_selectAdvMap { get; }
                private Button Btn_selectProMap { get; }
                private Button Btn_forgetDizi { get; }
                private Button Btn_buybackDizi { get; }
                private Button Btn_returnDizi { get; }
                private Button Btn_challenge { get; }

                public View_Buttons(IView v,
                    Action onRecallAction,
                    Action onAdvMapAction,
                    Action onProductionMapAction,
                    Action onDiziForgetAction,
                    Action onDiziBuyBackAction,
                    Action onDiziReturnAction,
                    Action onChallengeAction) : base(v, true)
                {
                    Btn_callback = v.GetObject<Button>("btn_callback");
                    Btn_callback.OnClickAdd(() =>
                    {
                        onRecallAction?.Invoke();
                        SetMode(Modes.Returning); //Temporary
                    });
                    Btn_selectAdvMap = v.GetObject<Button>("btn_selectAdvMap");
                    Btn_selectAdvMap.OnClickAdd(() => onAdvMapAction?.Invoke());
                    Btn_selectProMap = v.GetObject<Button>("btn_selectProMap");
                    Btn_selectProMap.OnClickAdd(() => onProductionMapAction?.Invoke());
                    Btn_forgetDizi = v.GetObject<Button>("btn_forgetDizi");
                    Btn_forgetDizi.OnClickAdd(onDiziForgetAction);
                    Btn_buybackDizi = v.GetObject<Button>("btn_buybackDizi");
                    Btn_buybackDizi.OnClickAdd(onDiziBuyBackAction);
                    Btn_returnDizi = v.GetObject<Button>("btn_returnDizi");
                    Btn_returnDizi.OnClickAdd(onDiziReturnAction);
                    Btn_challenge = v.GetObject<Button>("btn_challenge");
                    Btn_challenge.OnClickAdd(onChallengeAction);
                }

                public void SetMode(Modes mode)
                {
                    var faction = Game.World.Faction;
                    DisplayButton(Btn_challenge, faction is { IsChallenging: true } &&
                                                 mode == Modes.Idle);
                    //DisplayButton(Btn_challenge, mode == Modes.Idle);
                    DisplayButton(Btn_callback, mode == Modes.Adventure);
                    DisplayButton(Btn_selectAdvMap, mode == Modes.Idle);
                    DisplayButton(Btn_selectProMap, mode == Modes.Idle);
                    DisplayButton(Btn_forgetDizi, mode == Modes.Lost);
                    DisplayButton(Btn_buybackDizi, mode == Modes.Lost);
                    DisplayButton(Btn_returnDizi, mode == Modes.Waiting);
                    void DisplayButton(Button button, bool display) => button.gameObject.SetActive(display);
                }

            }

            private class Prefab_Log : UiBase
            {
                const int RowHeight = 60;
                private View_TextHandler TextHandlerView { get; }
                private View_Reward RewardView { get; }

                public Prefab_Log(IView v) : base(v, true)
                {
                    TextHandlerView = new View_TextHandler(v.GetObject<View>("view_textHandler"), this);
                    RewardView = new View_Reward(v.GetObject<View>("view_reward"), this);
                }

                public void SetMessage(string message)
                {
                    TextHandlerView.LogMessage(message);
                    TextHandlerView.Display(true);
                    RewardView.Display(false);
                }

                public void SetReward(IGameReward reward)
                {
                    TextHandlerView.Display(false);
                    var list = reward.AllItems.GroupBy(o => (o.Type, o.Id, o.Name), o => o)
                        .Select(group => (group.Key.Name, group.Count())).ToList();
                    list.Add(("包裹", reward.Packages.Length));
                    RewardView.SetViewReward(0, list.ToArray());
                    RewardView.Display(true);
                }

                private void SetHeight(int height) => View.RectTransform.SetHeight(height);

                private class View_TextHandler : UiBase
                {
                    private const int OneLine = 24;
                    private Text Text_time { get; }
                    private Text Text_message { get; }
                    private Prefab_Log Prefab { get; }

                    public View_TextHandler(IView v, Prefab_Log prefab) : base(v, true)
                    {
                        Prefab = prefab;
                        Text_message = v.GetObject<Text>("text_message");
                    }

                    public void LogMessage(string message)
                    {
                        var line = message.Length / OneLine;
                        Text_message.text = message;
                        Prefab.SetHeight((1 + line) * RowHeight);
                    }

                    public void ResetUi() => Text_message.text = string.Empty;
                }

                private class View_Reward : UiBase
                {
                    private Element_Prefab ExpElement { get; }
                    private Element_Prefab RewardElement1 { get; }
                    private Element_Prefab RewardElement2 { get; }
                    private Element_Prefab RewardElement3 { get; }
                    private Element_Prefab RewardElement4 { get; }
                    private Element_Prefab RewardElement5 { get; }
                    private Element_Prefab RewardElement6 { get; }
                    private Element_Prefab RewardElement7 { get; }
                    private Element_Prefab RewardElement8 { get; }
                    private Element_Prefab RewardElement9 { get; }
                    private Element_Prefab RewardElement10 { get; }
                    private Element_Prefab RewardElement11 { get; }
                    private Element_Prefab RewardElement12 { get; }
                    private Element_Prefab RewardElement13 { get; }
                    private List<Element_Prefab> AllRewardElement { get; }
                    private Prefab_Log Prefab { get; }

                    public View_Reward(IView v, Prefab_Log prefab) : base(v, true)
                    {
                        Prefab = prefab;
                        ExpElement = new Element_Prefab(v.GetObject<View>("element_exp"));
                        RewardElement1 = new Element_Prefab(v.GetObject<View>("element_reward1"));
                        RewardElement2 = new Element_Prefab(v.GetObject<View>("element_reward2"));
                        RewardElement3 = new Element_Prefab(v.GetObject<View>("element_reward3"));
                        RewardElement4 = new Element_Prefab(v.GetObject<View>("element_reward4"));
                        RewardElement5 = new Element_Prefab(v.GetObject<View>("element_reward5"));
                        RewardElement6 = new Element_Prefab(v.GetObject<View>("element_reward6"));
                        RewardElement7 = new Element_Prefab(v.GetObject<View>("element_reward7"));
                        RewardElement8 = new Element_Prefab(v.GetObject<View>("element_reward8"));
                        RewardElement9 = new Element_Prefab(v.GetObject<View>("element_reward9"));
                        RewardElement10 = new Element_Prefab(v.GetObject<View>("element_reward10"));
                        RewardElement11 = new Element_Prefab(v.GetObject<View>("element_reward11"));
                        RewardElement12 = new Element_Prefab(v.GetObject<View>("element_reward12"));
                        RewardElement13 = new Element_Prefab(v.GetObject<View>("element_reward13"));
                        AllRewardElement = new List<Element_Prefab>()
                        {
                            RewardElement1,
                            RewardElement2,
                            RewardElement3,
                            RewardElement4,
                            RewardElement5,
                            RewardElement6,
                            RewardElement7,
                            RewardElement8,
                            RewardElement9,
                            RewardElement10,
                            RewardElement11,
                            RewardElement12,
                            RewardElement13
                        };
                    }

                    public void SetViewReward(int exp, (string itemName, int value)[] items)
                    {
                        ExpElement.Set(exp);
                        var totalElements = items.Length;
                        totalElements += exp > 0 ? 1 : 0;
                        var rows = totalElements switch
                        {
                            > 9 => 3,
                            > 4 => 2,
                            _ => 1
                        };
                        SetPrefabSize(rows);
                        for (var i = 0; i < AllRewardElement.Count; i++)
                        {
                            var element = AllRewardElement[i];
                            if (i >= items.Length)
                            {
                                element.Hide();
                                continue;
                            }

                            var (title, value) = items[i];
                            element.Set(value, title);
                        }
                    }

                    private void SetPrefabSize(int rows)
                    {
                        Prefab.SetHeight(rows * RowHeight);
                    }

                    private class Element_Prefab : UiBase
                    {
                        private Text Text_title { get; }
                        private Text Text_value { get; }

                        public Element_Prefab(IView v) : base(v, true)
                        {
                            Text_title = v.GetObject<Text>("text_title");
                            Text_value = v.GetObject<Text>("text_value");
                        }

                        public void Set(int value, string title = null)
                        {
                            Display(value > 0);
                            Text_value.text = value.ToString();
                            if (title != null) Text_title.text = title;
                        }

                        public void Hide() => Display(false);
                    }
                }

            }
        }

        private class Page_main_adventureMaps : UiBase
        {
            private ScrollRect Scroll_mapSelector { get; }
            private ListViewUi<Prefab_map> MapView { get; }
            private Button Btn_refersh { get; }
            private Button Btn_action { get; }
            private Button Btn_cancel { get; }

            public Page_main_adventureMaps(IView v,
                Action onRefreshAction,
                Action<int> onAdvStartAction
            ) : base(v, true)
            {
                v.ResetRectToZero();
                Scroll_mapSelector = v.GetObject<ScrollRect>("scroll_mapSelector");
                MapView = new ListViewUi<Prefab_map>(v.GetObject<View>("prefab_map"), Scroll_mapSelector);
                Btn_refersh = v.GetObject<Button>("btn_refresh");
                Btn_refersh.OnClickAdd(onRefreshAction);
                Btn_action = v.GetObject<Button>("btn_action");
                Btn_action.OnClickAdd(() =>
                {
                    onAdvStartAction?.Invoke(SelectedIndex);
                    Display(false);
                });
                Btn_cancel = v.GetObject<Button>("btn_cancel");
                Btn_cancel.OnClickAdd(() => Display(false));
                SetSelectionIndex();
            }

            //-99 = 没有选择任何东西
            private int SelectedIndex { get; set; } = -99;

            //如果选择<=-1将不能按
            private void SetSelectionIndex(int index = -99)
            {
                SelectedIndex = index;
                Btn_action.interactable = SelectedIndex > -99;
            }

            public void ListMap(IAutoAdvMap[] maps)
            {
                MapView.ClearList(ui => ui.Destroy());
                for (var i = 0; i < maps.Length; i++)
                {
                    var map = maps[i];
                    var ui = MapView.Instance(v => new Prefab_map(v, map, SetSelected));
                    ui.Set(map.Name, map.About, map.ActionLingCost);
                    ui.SetMapImage(map.Image);
                }

                SetSelectionIndex();
            }

            private void SetSelected(int mapId)
            {
                for (var i = 0; i < MapView.List.Count; i++)
                {
                    var ui = MapView.List[i];
                    var selected = ui.Map.Id == mapId;
                    ui.SetSelected(selected);
                    if (selected) SetSelectionIndex(mapId);
                }
            }

            private class Prefab_map : UiBase
            {
                private Image Img_mapIco { get; }
                private Text Text_mapTitle { get; }
                private Text Text_about { get; }
                private Image Img_costIco { get; }
                private Text Text_cost { get; }
                private Image Img_selected { get; }
                private Button Btn_Map { get; }
                public IAutoAdvMap Map { get; }

                public Prefab_map(IView v, IAutoAdvMap map, Action<int> onClickAction) : base(v, true)
                {
                    Map = map;
                    Img_mapIco = v.GetObject<Image>("img_mapIco");
                    Text_mapTitle = v.GetObject<Text>("text_mapTitle");
                    Text_about = v.GetObject<Text>("text_about");
                    Img_costIco = v.GetObject<Image>("img_costIco");
                    Text_cost = v.GetObject<Text>("text_cost");
                    Img_selected = v.GetObject<Image>("img_selected");
                    Btn_Map = v.GetObject<Button>("btn_map");
                    Btn_Map.OnClickAdd(() => onClickAction(map.Id));
                }

                public void SetSelected(bool isSelected) => Img_selected.gameObject.SetActive(isSelected);
                public void SetMapImage(Sprite mapImg) => Img_mapIco.sprite = mapImg;
                public void SetCost(Sprite costImg) => Img_costIco.sprite = costImg;

                public void Set(string title, string about, int advCost)
                {
                    Text_mapTitle.text = title;
                    Text_about.text = about;
                    Text_cost.text = advCost.ToString();
                }
            }
        }

        private class Page_main_diziList : UiBase
        {
            private ScrollRect Scroll_dizi { get; }
            private ListViewUi<DiziPrefab> DiziList { get; }
            private Element Elm_all { get; }
            private Element Elm_idle { get; }
            private Element Elm_production { get; }
            private Element Elm_adventure { get; }
            private Element Elm_lost { get; }
            private event Action<Dizi> OnDiziSelected;
            private string SelectedDiziGuid { get; set; }
            private Element[] AllFilter { get; }

            //cache 当前弟子列表
            private Dizi[] CurrentList { get; set; }

            public Page_main_diziList(IView v, Action<Dizi> onDiziClicked) : base(v, true)
            {
                v.ResetRectToZero();
                OnDiziSelected = onDiziClicked;
                Scroll_dizi = v.GetObject<ScrollRect>("scroll_dizi");
                DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"), Scroll_dizi);
                Elm_all = new Element(v.GetObject<View>("element_all"), () =>
                {
                    SetFilterSelected(Elm_all);
                    UpdateList(GetAllDizi());
                });
                Elm_idle = new Element(v.GetObject<View>("element_idle"), () =>
                {
                    SetFilterSelected(Elm_idle);
                    UpdateList(GetIdleDizi());
                });
                Elm_production = new Element(v.GetObject<View>("element_production"), () =>
                {
                    SetFilterSelected(Elm_production);
                    UpdateList(GetProductionDizi());
                });
                Elm_adventure = new Element(v.GetObject<View>("element_adventure"), () =>
                {
                    SetFilterSelected(Elm_adventure);
                    UpdateList(GetAdventureDizi());
                });
                Elm_lost = new Element(v.GetObject<View>("element_lost"), () =>
                {
                    SetFilterSelected(Elm_lost);
                    UpdateList(GetLostDizi());
                });
                AllFilter = new[]
                {
                    Elm_all,
                    Elm_idle,
                    Elm_adventure,
                    Elm_production,
                    Elm_lost
                };
            }

            private Dizi[] GetLostDizi() => Game.World.Faction.DiziList
                .Where(d => d.Activity == DiziActivities.Lost)
                .ToArray();

            private Dizi[] GetProductionDizi() => Game.World.Faction.DiziList
                .Where(d => d.Activity == DiziActivities.Adventure && WorldState.Adventure.GetActivity(d.Guid).AdvType == AdventureActivity.AdvTypes.Production)
                .ToArray();

            private Dizi[] GetAdventureDizi() => Game.World.Faction.DiziList
                .Where(d => d.Activity == DiziActivities.Adventure && WorldState.Adventure.GetActivity(d.Guid).AdvType == AdventureActivity.AdvTypes.Adventure)
                .ToArray();

            private Dizi[] GetAllDizi() => Game.World.Faction.DiziList.ToArray();

            private Dizi[] GetIdleDizi() => Game.World.Faction.DiziList
                .Where(d => d.Activity == DiziActivities.Idle)
                .ToArray();

            private void SetFilterSelected(Element element)
            {
                foreach (var e in AllFilter) e.SetSelected(e == element);
            }

            public void SetElements()
            {
                var faction = Game.World.Faction;
                var list = faction.DiziList.ToList();
                Elm_all.SetValue(list.Count, faction.MaxDizi);
                Elm_idle.SetValue(GetIdleDizi().Length, faction.MaxDizi);
                Elm_production.SetValue(GetProductionDizi().Length, faction.MaxDizi);
                Elm_adventure.SetValue(GetAdventureDizi().Length, faction.MaxDizi);
                Elm_lost.SetValue(GetLostDizi().Length, faction.MaxDizi);
            }

            public void UpdateList(Dizi[] list = null)
            {
                if (list == null)
                    list = Game.World.Faction.DiziList.ToArray();
                CurrentList = list;
                ClearList();
                for (var i = 0; i < list.Length; i++)
                {
                    var dizi = list[i];
                    var guid = dizi.Guid;
                    var index = i;
                    var ui = DiziList.Instance(v => new DiziPrefab(v, () =>
                    {
                        OnDiziSelected?.Invoke(CurrentList[index]);
                        SetSelected(guid);
                    }));
                    ui.Init(dizi);
                }
            }

            private void SetSelected(string diziGuid)
            {
                SelectedDiziGuid = Game.World.Faction.GetDizi(diziGuid)?.Guid
                                   ?? string.Empty; //如果弟子不在了,是不可选中的
                foreach (var ui in DiziList.List)
                    ui.SetSelected(ui.DiziGuid == SelectedDiziGuid);
            }

            private void ClearList()
            {
                DiziList.ClearList(ui => ui.Destroy());
                SelectedDiziGuid = null;
            }

            private class DiziPrefab : UiBase
            {
                private Image Img_avatar { get; }
                private Text Text_name { get; }
                private Button Btn_dizi { get; }
                private Image Img_select { get; }
                public string DiziGuid { get; private set; }

                public DiziPrefab(IView v, Action onClickAction) : base(v, true)
                {
                    Img_avatar = v.GetObject<Image>("img_avatar");
                    Text_name = v.GetObject<Text>("text_name");
                    Btn_dizi = v.GetObject<Button>("btn_dizi");
                    Img_select = v.GetObject<Image>("img_select");
                    Btn_dizi.OnClickAdd(onClickAction);
                }

                public void SetIcon(Sprite ico) => Img_avatar.sprite = ico;
                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);

                public void Init(Dizi dizi)
                {
                    Text_name.text = dizi.Name;
                    DiziGuid = dizi.Guid;
                    Text_name.color = Game.GetColorFromGrade(dizi.Grade);
                    SetSelected(false);
                    Display(true);
                }
            }

            private class Element : UiBase
            {
                private Text Text_value { get; }
                private Text Text_max { get; }
                private Button Btn_filter { get; }
                private Image Img_select { get; }

                public Element(IView v, Action onFilterAction) : base(v, true)
                {
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                    Btn_filter = v.GetObject<Button>("btn_filter");
                    Btn_filter.OnClickAdd(onFilterAction);
                    Img_select = v.GetObject<Image>("img_select");
                }

                public void SetValue(int value, int max)
                {
                    Text_value.text = value.ToString();
                    Text_max.text = max.ToString();
                }

                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
            }
        }

        private class Page_main_challengeStageSelector : UiBase
        {
            private ScrollRect scroll_challenge { get; }
            private ListViewUi<ChallengePrefab> ChallengeList { get; }
            private Button btn_challenge { get; }
            private Button btn_cancel { get; }

            public Page_main_challengeStageSelector(IView v, Action<int> onChallengeAction) : base(v, false)
            {
                v.ResetRectToZero();
                scroll_challenge = v.GetObject<ScrollRect>("scroll_challenge");
                ChallengeList =
                    new ListViewUi<ChallengePrefab>(v.GetObject<View>("prefab_challenge"), scroll_challenge);
                btn_challenge = v.GetObject<Button>("btn_challenge");
                btn_challenge.OnClickAdd(() => onChallengeAction?.Invoke(SelectedNpcIndex));
                btn_cancel = v.GetObject<Button>("btn_cancel");
                btn_cancel.OnClickAdd(() => Display(false));
            }

            private int SelectedNpcIndex { get; set; }

            public void Set(IChallengeStage stage)
            {
                ChallengeList.ClearList(c => c.Destroy());
                if (stage == null) return;
                var faction = Game.World.Faction;
                var npcs = stage.GetChallengeNpcs(faction.ChallengeStageProgress);
                for (var i = 0; i < npcs.Length; i++)
                {
                    var index = i;
                    var n = npcs[i];
                    var ui = ChallengeList.Instance(v => new ChallengePrefab(v, () => SetSelected(index)));
                    ui.SetNpc(n.Name, stage.Name, n.Level, n.Icon, n.IsBoss);
                    if (i == 0)
                    {
                        //预选第一个
                        SelectedNpcIndex = index;
                        ui.SetSelected(true);
                    }
                }
            }

            private void SetSelected(int index)
            {
                SelectedNpcIndex = index;
                for (var i = 0; i < ChallengeList.List.Count; i++)
                {
                    var ui = ChallengeList.List[i];
                    ui.SetSelected(index == i);
                }
            }

            private class ChallengePrefab : UiBase
            {
                private Image Img_select { get; }
                private Image Img_npcIco { get; }
                private Image Img_IsBoss { get; }
                private Text Text_npcLevel { get; }
                private Button Btn_npc { get; }
                private Text Text_name { get; }
                private Text Text_faction { get; }

                public ChallengePrefab(IView v, Action onSelectAction) : base(v, true)
                {
                    Img_select = v.GetObject<Image>("img_select");
                    Img_npcIco = v.GetObject<Image>("img_npcIco");
                    Img_IsBoss = v.GetObject<Image>("img_isBoss");
                    Text_npcLevel = v.GetObject<Text>("text_npcLevel");
                    Text_name = v.GetObject<Text>("text_name");
                    Text_faction = v.GetObject<Text>("text_faction");
                    Btn_npc = v.GetObject<Button>("btn_npc");
                    Btn_npc.OnClickAdd(onSelectAction);
                }

                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);

                public void SetNpc(string name, string faction, int level, Sprite icon, bool isBoss)
                {
                    Img_npcIco.sprite = icon;
                    Text_name.text = name;
                    Text_faction.text = faction;
                    Text_npcLevel.text = level.ToString();
                    Img_IsBoss.gameObject.SetActive(isBoss);
                    Img_select.gameObject.SetActive(false);
                }
            }
        }
    }
}