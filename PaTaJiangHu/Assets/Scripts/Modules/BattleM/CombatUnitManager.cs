using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace BattleM
{
    /// <summary>
    /// 战斗单位管理器
    /// </summary>
    public class CombatUnitManager
    {
        public enum Judgment
        {
            [InspectorName("决斗")] Duel,
            [InspectorName("切磋")] Test,
        }
        private static readonly Random Random = new Random();
        /// <summary>
        /// 战斗对手映像，key = 单位， Value = 目标
        /// </summary>
        private Dictionary<ICombatInfo, CombatUnit> AliveMap { get; } =
            new Dictionary<ICombatInfo, CombatUnit>();
        public IReadOnlyList<CombatUnit> AllUnits { get; }
        public bool IsFightEnd { get; private set; }
        public Judgment Judge { get; }
        public int WinningStance => AliveStances.Count() == 1 ? AliveStances.Single() : -1;
        public IEnumerable<int> AliveStances =>
            AliveMap.Where(u => !u.Value.IsExhausted).Select(u => u.Key.StandingPoint).Distinct();

        public CombatBuffManager BuffMgr { get; set; }

        //战斗单位的辨识Id
        private int CombatIdIndex { get; set; }
        public CombatUnitManager(IEnumerable<CombatUnit> combats,Judgment judgment)
        {
            BuffMgr = new CombatBuffManager();
            AllUnits = combats.ToList();
            Judge = judgment;
            foreach (var unit in AllUnits)
            {
                unit.SetCombatId(++CombatIdIndex);
                if (Judge == Judgment.Duel)
                {
                    unit.SetStrategy(CombatUnit.Strategies.Hazard);
                }
            }
        }
        /// <summary>
        /// 获取所有活着的单位
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CombatUnit> GetAliveCombatUnits() => AliveMap.Values;
        /// <summary>
        /// 尝试获取活着的单位
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public CombatUnit TryGetAliveCombatUnit(ICombatInfo unit) => AliveMap.TryGetValue(unit, out var tg) ? tg : null;
        /// <summary>
        /// 获取所有战斗单位(包括已死单位)
        /// </summary>
        /// <param name="combatId"></param>
        /// <returns></returns>
        public CombatUnit GetFromAllUnits(int combatId) => AllUnits.Single(u => u.CombatId == combatId);

        /// <summary>
        /// 获取被仇恨的单位(所有以目标做为进攻目标的单位)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerable<CombatUnit> GetTargetedUnits(CombatUnit target) => AliveMap.Values.Where(c => c.Target == target);
        /// <summary>
        /// 获取拥有暗器并以目标做为进攻目标的单位
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public CombatUnit GetHasThrowTargetedUnit(CombatUnit target) =>
            AliveMap.Values
                .Where(c => c.Target == target && c.Equipment.Fling?.FlingTimes > 0)
                .OrderBy(c => c.Distance(target)).FirstOrDefault();
        /// <summary>
        /// 从活着列表中移除
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveAlive(ICombatInfo unit)
        {
            AliveMap.Remove(unit);
            UpdateFightEnd();
        }
        /// <summary>
        /// 检查是否有单位死亡，并把死亡单位移除
        /// </summary>
        /// <param name="onUnitExhausted"></param>
        public void CheckExhausted(Action<CombatUnit> onUnitExhausted)
        {
            foreach (var unit in AliveMap.Values.ToArray())
            {
                if (!unit.IsExhausted) continue;
                
                onUnitExhausted(unit);
                BuffMgr.RemoveAll(unit.CombatId);
                AliveMap.Remove(unit);
            }
            UpdateFightEnd();
        }

        private void UpdateFightEnd()
        {
            if (AliveMap.Where(c => !c.Value.IsExhausted)
                    .GroupBy(u => u.Key.StandingPoint).Count() <= 1)
                IsFightEnd = true;
        }

        public void AddUnit(CombatUnit combat) => AliveMap.Add(combat, combat);
        /// <summary>
        /// 为单位安排对手
        /// </summary>
        /// <param name="op"></param>
        public void SetTargetFor(CombatUnit op)
        {
            if (AliveMap.TryGetValue(op.Target, out _))
                return;
            var target = AliveMap.Values.OrderBy(_ => Random.Next(10))
                .FirstOrDefault(u => u.StandingPoint != op.StandingPoint && !u.IsExhausted);
            if (target == null)
            {
                IsFightEnd = true;
                return;
            }
            op.ChangeTarget(target);
        }
        /// <summary>
        /// 检查是否活着
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool IsAlive(ICombatInfo unit) => AliveMap.ContainsKey(unit);
    }
}