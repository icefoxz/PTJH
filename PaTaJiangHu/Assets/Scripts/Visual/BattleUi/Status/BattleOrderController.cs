using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visual.BaseUi;

namespace Visual.BattleUi
{
    public interface IBattleOrderController
    {
        void Init();
        void UpdateOrder(IList<(int combatId, string title, int breathes)> list);
        void UpdateSize();
        void ResetUi();
    }

    public class BattleOrderController : PrefabController<BattleOrderAvatarUi>, IBattleOrderController
    {
        [SerializeField] private float _smallSize = 0.6f;

        public void Init()
        {
            BaseInit(false);
        }

        private void AddUi(int combatId, string title)
        {
            AddUi(ui =>
            {
                ui.Set(title);
                ui.transform.SetAsFirstSibling();
                ui.Init();
            });
        }

        public void UpdateOrder(IList<(int combatId,string title, int breathes)> list)
        {
            RemoveList();
            foreach (var (combatId, title, _) in list.OrderBy(l => l.breathes)) AddUi(combatId, title);
            UpdateSize();
        }

        public void UpdateSize()
        {
            for (var i = 0; i < List.Count; i++)
            {
                var ui = List[i];
                ui.SetSize(_smallSize);
            }
            List[0].EnlargeAnim();
        }

        public override void ResetUi()
        {
            RemoveList();
        }
    }
}