using System.Collections.Generic;
using Visual.BaseUi;

namespace Visual.BattleUi
{
    public class TempoSliderController : PrefabController<TempoSliderUi>
    {
        private Dictionary<int,TempoSliderUi> Sliders { get; set; }
        public void Init()
        {
            Sliders = new Dictionary<int, TempoSliderUi>();
            BaseInit(false);
        }
        public void Add(int combatId,string text)
        {
            AddUi(s =>
            {
                s.Init(text);
                Sliders.Add(combatId, s);
                s.Show();
            });
        }

        public void UpdateSlider(IEnumerable<(int combatId,float value)> updateList)
        {
            foreach (var(combatId,value) in updateList) 
                Sliders[combatId].UpdateSlider(value);
        }
        public override void ResetUi()
        {
            Sliders.Clear();
            RemoveList();
        }
    }
}