using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

public class DiziListViewManager
{
    private View_diziListView DiziListView { get; set; }

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

    private class View_diziListView : UiBase
    {
        private ScrollRect Scroll_diziListView { get; }
        private Prefab Dizi { get; }
        private View_topRight TopRight {get;}
        public View_diziListView(IView v) : base(v.GameObject, true)
        {
            Scroll_diziListView = v.GetObject<ScrollRect>("scroll_diziListView");
            Dizi = new Prefab(v.GetObject<View>("prefab_dizi"));
            TopRight = new View_topRight(v.GetObject<View>("view_topRight"));
        }
        private class Prefab : UiBase
        {
            private Button Btn_dizi { get; }
            private Text Text_diziName { get; }

            public Prefab(IView v) : base(v.GameObject, true)
            {
                Btn_dizi = v.GetObject<Button>("btn_dizi");
                //Btn_dizi.OnClickAdd():
                Text_diziName = v.GetObject<Text>("text_diziName");
            }
            public void GetDiziName(string name) => Text_diziName.text = name;
            
        }
        private class View_topRight : UiBase
        {
            private Text Text_value { get; }
            private Text Text_max { get; }

            public View_topRight(IView v) : base(v.GameObject, true)
            {
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
            }

            public void SetExistingDizi(int value, int max)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
            }
        }

    }
}
