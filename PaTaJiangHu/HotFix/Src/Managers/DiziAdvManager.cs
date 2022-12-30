using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

public class DiziAdvManager
{
    private View_diziAdv DiziAdv { get; set; }
    public void Init()
    {
        InitUi();
        RegEvents();
    }

    private void RegEvents()
    {
        
    }

    private void InitUi()
    {
        
    }

    private class View_diziAdv : UiBase
    {
        private Button Btn_Switch { get; }
        private Element_con Food { get; }
        private Element_con State { get; }
        private Element_con Silver { get; }
        private View_advLayout AdvLayoutView { get; }
        public View_diziAdv(IView v, Action onSwitchAction) : base(v.GameObject, true)
        {
            Btn_Switch = v.GetObject<Button>("btn_switch");
            Btn_Switch.OnClickAdd(() =>
            {
                onSwitchAction?.Invoke();
            });
            Food = new Element_con(v.GetObject<View>("element_conFood"));
            State = new Element_con(v.GetObject<View>("element_conState"));
            Silver = new Element_con(v.GetObject<View>("element_contSilver"));
            AdvLayoutView = new View_advLayout(v.GetObject<View>("view_advLayout"));
        }
        
        private class Element_con : UiBase
        {
            private Scrollbar Scrbar_condition { get; }
            private Text Text_value { get; }
            private Text Text_max { get; }
            private Text Text_title { get; }
            public Element_con(IView v) : base(v.GameObject, true)
            {
                Scrbar_condition = v.GetObject<Scrollbar>("scrbar_condition");
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
                Text_title = v.GetObject<Text>("text_title");
            }
            public void Set(int value, int max, string title)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
                Text_title.text = title;
            }
        }
        private class View_advLayout : UiBase
        {
            private ScrollRect Scroll_advLog { get; }
            private Prefab Prefab_Log { get; }
            private Button Btn_recall { get; }
            private Image Img_costIco { get; }
            private Text Text_cost { get; }
            private Element_equit Slot0 { get; }
            private Element_equit Slot1 { get; }
            private Element_equit Slot2 { get; }
            private Button Btn_advStart { get; }

            public View_advLayout(IView v/*, Action onRecallAction, Action onAdvStartAction*/) : base(v.GameObject, true)
            {
                Scroll_advLog = v.GetObject<ScrollRect>("scroll_advLog");
                Prefab_Log = new Prefab(v.GetObject<View>("prefab_log"));
                Btn_recall = v.GetObject<Button>("btn_recall");
                ///Btn_recall.OnClickAdd(() =>
                ///{
                ///    onRecallAction?.Invoke();
                ///});
                Img_costIco = v.GetObject<Image>("img_costIco");
                Text_cost = v.GetObject<Text>("text_cost");
                Slot0 = new Element_equit(v.GetObject<View>("element_equitSlot0"));
                Slot1 = new Element_equit(v.GetObject<View>("element_equitSlot1"));
                Slot2 = new Element_equit(v.GetObject<View>("element_equitSlot2"));
                Btn_advStart = v.GetObject<Button>("btn_advStart");
                ///Btn_advStart.OnClickAdd(() =>
                ///{
                ///    onAdvStartAction?.Invoke();
                ///});
            }
            public void Set(Sprite icon, int cost)
            {
                Img_costIco.sprite = icon;
                Text_cost.text = cost.ToString();
            }
            private class Prefab : UiBase
            {
                private Text Prefab_Log { get; }
                public Prefab(IView v) :base(v.GameObject, false)
                {
                    Prefab_Log = v.GetObject<Text>("prefab_log");
                }
                public void LogMessage(string message) => Prefab_Log.text = message;
            }

            private class Element_equit : UiBase
            {
                private Scrollbar Scrbar_item { get; }
                private Image Img_ico { get; }
                private Text Text_min { get; }
                private Text Text_sec { get; }
                
                public Element_equit(IView v) : base(v.GameObject, true)
                {
                    Scrbar_item = v.GetObject<Scrollbar>("scrbar_item");
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_min = v.GetObject<Text>("text_min");
                    Text_sec = v.GetObject<Text>("text_sec");
                }
                public void SetIcon(Sprite icon) => Img_ico.sprite = icon;
                public void SetTimer(int min, int sec)
                {
                    Text_min.text = min.ToString();
                    Text_sec.text = sec.ToString();
                }
            }
        }
    }
}
