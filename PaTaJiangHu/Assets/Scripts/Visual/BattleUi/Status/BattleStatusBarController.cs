using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;
using Visual.BaseUi;

namespace Visual.BattleUi
{
    public interface IBattleStatusBarController
    {
        IEnumerable<CombatStatusUi> List { get; }
        void Init();
        void AddUi(int stance, int combatId, Action<CombatStatusUi> initAction);
        void ResetUi();
        void UpdateBreath(int combatId, int breath, int maxBreath);
        void UpdateStatus(int combatId, IConditionValue hp, IConditionValue tp, IConditionValue mp);
    }
    public class BattleStatusBarController : UiBase, IBattleStatusBarController
    {
        [SerializeField] protected CombatStatusUi opPrefab;
        [SerializeField] protected Transform left;
        [SerializeField] protected Transform right;
        private Dictionary<int,CombatStatusUi> _map = new Dictionary<int,CombatStatusUi>();
        public IEnumerable<CombatStatusUi> List => _map.Values;

        public void Init()
        {
            var uis = left.GetComponentsInChildren<CombatStatusUi>()
                .Concat(right.GetComponentsInChildren<CombatStatusUi>());
            foreach (var ui in uis) ui.Hide();
        }

        public void AddUi(int stance,int combatId, Action<CombatStatusUi> initAction)
        {
            var ui = Instantiate(opPrefab, stance == 0 ? left.transform : right.transform);
            ui.gameObject.SetActive(true);
            _map.Add(combatId, ui);
            initAction.Invoke(ui);
        }

        private void RemoveList()
        {
            if (_map.Count > 0) _map.ToList().ForEach(kv => DestroyObj(kv.Value));
            _map.Clear();
        }

        public override void ResetUi()
        {
            RemoveList();
        }

        public void UpdateBreath(int combatId, int breath, int maxBreath) =>
            _map[combatId].SetBreath(breath, maxBreath);

        public void UpdateStatus(int combatId, IConditionValue hp, IConditionValue tp, IConditionValue mp)
        {
            _map[combatId].UpdateStatus(hp, tp, mp);
        }
    }
}