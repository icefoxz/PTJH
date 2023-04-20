﻿using System;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Models;
using Server.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_View_ConPropsMgr : MainPageBase
    {
        private View_ConProps View_conProps { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Top;
        protected override string ViewName => "demo_view_conProps";
        protected override bool IsDynamicPixel => true;

        public Demo_View_ConPropsMgr(Demo_v1Agent uiAgent) : base(uiAgent) { }

        private static Color GetBuffColor(bool isDebuff)=> Game.Controllers.Get<DiziController>().BuffColor(isDebuff);
        protected override void Build(IView view)
        {
            View_conProps = new View_ConProps(view);
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => View_conProps.Update(bag.Get<string>(0)));
            Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag => View_conProps.Update(bag.Get<string>(0)));
            Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate, bag => View_conProps.Update(bag.Get<string>(0)));
        }
        public override void Show() => View_conProps.Display(true);
        public override void Hide() => View_conProps.Display(false);

        public void Set(Dizi dizi) => View_conProps.Set(dizi.Guid);

        private class View_ConProps : UiBase
        {
            public enum Props { Strength, Agility, Hp, Mp }
            private Element_prop Strength { get; }
            private Element_prop Agility { get; }
            private Element_prop Hp { get; }
            private Element_prop Mp { get; }
            public View_ConProps(IView v) : base(v, true)
            {
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
                var s = Server.Configs.Characters.DiziProps.Strength;
                var a = Server.Configs.Characters.DiziProps.Agility;
                var h = Server.Configs.Characters.DiziProps.Hp;
                var m = Server.Configs.Characters.DiziProps.Mp;
                SetProps(Props.Strength, dizi.Capable.Strength.Value, 0, dizi.WeaponPower, dizi.GetPropStateAddon(s));
                SetProps(Props.Agility, dizi.GetLeveledValue(a), 0, 0, dizi.GetPropStateAddon(a));
                SetProps(Props.Hp, dizi.GetLeveledValue(h), 0, dizi.ArmorPower, dizi.GetPropStateAddon(h));
                SetProps(Props.Mp, dizi.GetLeveledValue(m), 0, 0, dizi.GetPropStateAddon(m));
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
    }
}
