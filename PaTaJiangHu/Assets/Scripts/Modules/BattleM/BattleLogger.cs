using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Color = System.Drawing.Color;

namespace BattleM
{
    public class BattleLogger : MonoBehaviour
    {
        private IEnumerable<CombatUnit> CombatUnits { get; set; }
        public void NextRound(IEnumerable<CombatUnit> combatUnits,FightRoundRecord rec)
        {
            CombatUnits = combatUnits;
            var units = combatUnits.Select(u => new UnitRecord(u)).ToList();
            RoundLog(rec, units);
            FightLog(rec);
        }


        private void RoundLog(FightRoundRecord rec,IEnumerable<UnitRecord> units)
        {
            var round = rec.Round.Sb()
                .Insert(0, "【回合").Append("】")
                .Color(Color.HotPink)
                .AppendLine(rec.IsFightEnd ? "【战斗结束】" : string.Empty);
            foreach (var unit in units.OrderBy(c=>c.Breath))
            {
                round.AppendLine($"{unit.Name}[位:{unit.Position}]{StatusLog(unit)}".Sb().Color(Color.White)
                    .ToString());
            }

            Debug.Log(round);
        }

        private void FightLog(FightRoundRecord rec)
        {
            foreach (var fragment in rec.Records)
            {
                var index = $"{fragment.Index.Sb().Color(Color.White)}.";
                var sb = new StringBuilder(index);
                fragment.On<ConsumeRecord>(r =>
                    {
                        switch (r)
                        {
                            case ConsumeRecord<IForceForm> force:
                                ForceFormConsumeLog(sb, force);
                                return;
                            case ConsumeRecord<IDodgeForm> dodge:
                                DodgeFormConsumeLog(sb, dodge);
                                return;
                            case ConsumeRecord<ICombatForm> combat:
                                CombatFormConsumeLog(sb, combat);
                                return;
                            default:
                                ConsumeLog(sb, r);
                                return;
                        }
                    })
                    .On<PositionRecord>(r => PositionRecordLog(r, sb))
                    .On<AttackRecord>(r => AttactRecordLog(r, sb))
                    .On<DodgeRecord>(r => DodgeRecordLog(r, sb))
                    .On<ParryRecord>(r => ParryRecordLog(r, sb))
                    .On<EventRecord>(r => EventRecordLog(r, sb));
            }
        }

        private static void DodgeRecordLog(DodgeRecord dod, StringBuilder sb)
        {
            string DodgeLog(IDodgeForm a) => FormLog(a) +
                                             $"身({a.Dodge})内({a.Mp})气({a.Qi}))".Sb().Color(Color.Coral);

            if (!dod.DodgeFormula.IsSuccess)
                sb.AppendLine($"闪避失败!闪避值[{dod.DodgeFormula.Finalize}]随机[{dod.DodgeFormula.RandomValue}]");
            else sb.Append("闪避成功！".Sb().Color(Color.Yellow));
            sb.Append(dod.Unit.Sb().Color(Color.LightBlue));
            sb.AppendLine(DodgeLog(dod.Form).Sb().Color(Color.LightBlue).ToString());
            sb.Append(dod.DodgeFormula.Sb().Color(Color.DarkTurquoise));
            Debug.Log(sb);
        }

        private static void ParryRecordLog(ParryRecord par, StringBuilder sb)
        {
            string ParryLog(IParryForm a) => $"【{a.Name}】".Sb().Color(Color.LightSeaGreen).ToString() +
                                             $"架({a.Parry})内({a.Mp})气({a.Qi})硬:己({a.OffBusy})".Sb().Color(Color.Coral);

            if (!par.ParryFormula.IsSuccess)
                sb.AppendLine($"招架失败!招架值[{par.ParryFormula.Finalize}]随机[{par.ParryFormula.RandomValue}]");
            else sb.Append("招架成功！".Sb().Color(Color.Yellow));
            sb.Append(UnitNamePos(par.Unit).Sb().Color(Color.LightGreen));
            sb.AppendLine(ParryLog(par.Form).Sb().Color(Color.LightBlue).ToString());
            sb.Append(par.ParryFormula.Sb().Color(Color.Yellow));
            Debug.Log(sb);
        }

        private static void PositionRecordLog(PositionRecord pos, StringBuilder sb)
        {
            var text = UnitNamePos(pos.Unit).Sb().Color(Color.White)
                .AppendLine($"移位->【{pos.NewPos.Sb().Color(Color.GreenYellow)}】")
                .Append(UnitNamePos(pos.Target).Sb().Color(Color.LightBlue));
            sb.Append(text);
            Debug.Log(sb);
        }

        private static string UnitNamePos(IUnitRecord u) => $"{u.Name}[息({u.Breath})位({u.Position})]";

        private static void AttactRecordLog(AttackRecord att, StringBuilder sb)
        {
            string AttackLog(ICombatForm a) => FormLog(a) +
                                                $"内({a.Mp})气({a.Qi})硬:己({a.OffBusy})敌({a.TarBusy})".Sb().Color(Color.Coral);

            var u = att.Unit;
            if (att.Type == FightFragment.Types.Fling) sb.Append("【投掷暗器】".Sb().Color(Color.Red));
            sb.Append($"{u.Name}伤害: 力({u.Strength})".Sb().Color(Color.Coral));
            sb.AppendLine($"【{AttackLog(att.Form)}】".Sb().Color(Color.LightBlue).ToString());
            sb.Append($"{att.DamageFormula})".Sb().Color(Color.Coral)
            );
            Debug.Log(sb);
        }

        private void EventRecordLog(EventRecord e, StringBuilder sb)
        {
            switch (e.Type)
            {
                case FightFragment.Types.TryEscape:
                    sb.Append($"{e.Unit.Name}【尝试逃走...】{StatusLog(e.Unit)}".Sb().Color(Color.Yellow));
                    break;
                case FightFragment.Types.Escaped:
                    sb.Append($"{e.Unit.Name}【逃走了！】{StatusLog(e.Unit)}".Sb().Color(Color.GreenYellow));
                    break;
                case FightFragment.Types.Death:
                    sb.Append($"{e.Unit.Name}【死亡！】{StatusLog(e.Unit)}".Sb().Color(Color.Red));
                    break;
                case FightFragment.Types.Exhausted:
                    sb.Append($"{e.Unit.Name}【倒地失去意识！】{StatusLog(e.Unit)}".Sb().Color(Color.DodgerBlue));
                    break;
                case FightFragment.Types.SwitchTarget:
                    sb.Append($"{e.Unit.Name}【换攻击目标.】".Sb().Color(Color.Orange));
                    break;
                case FightFragment.Types.None:
                case FightFragment.Types.Consume:
                case FightFragment.Types.Attack:
                case FightFragment.Types.Parry:
                case FightFragment.Types.Dodge:
                case FightFragment.Types.Position:
                case FightFragment.Types.Fling:
                case FightFragment.Types.Wait:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Log(sb);
        }

        private void ConsumeLog(StringBuilder sb, ConsumeRecord consume)
        {
            if (consume.Type == FightFragment.Types.Wait)
            {
                sb.Append($"【{consume.UnitName}】等待,".Sb().Color(Color.Cyan).ToString() +
                          StatusLog(consume.Before).Sb().Color(Color.Aquamarine) + "->");
                sb.Append(SimpleStatusLog(consume.Before, consume.After, Color.GreenYellow));
            }
            else
            {
                sb.Append($"【{consume.UnitName}】消耗,".Sb().Color(Color.IndianRed).ToString() +
                          StatusLog(consume.Before).Sb().Color(Color.Aquamarine) + "->");
                sb.Append(SimpleStatusLog(consume.Before, consume.After,
                    consume.After.Hp.Value == 0 || consume.After.Tp.Value == 0
                        ? Color.Red
                        : Color.IndianRed)).Bold();
            }

            Debug.Log(sb);
        }

        private void CombatFormConsumeLog(StringBuilder sb, ConsumeRecord<ICombatForm> combat)
        {
            var unit = CombatUnits.Single(u => u.CombatId == combat.CombatId);
            sb.Append(($"{combat.UnitName}攻击招式," + FormLog(combat.Form)).Sb().Color(Color.Orange)
                .ToString());
            sb.Append($"气:({combat.Form.Qi})内:({combat.Form.Mp})".Sb().Color(Color.Yellow));

            sb.AppendLine(ArmedKindLog(unit.Equipment.Armed).Sb().Color(Color.DarkOrange).ToString());
            sb.Append(
                $"己硬直({combat.Form.OffBusy.Sb().Bold().Color(Color.DarkOrange)})|敌硬直({combat.Form.TarBusy.Sb().Bold().Color(Color.Orange)})");
            sb.Append(
                (
                    $"气{SimpleStat(combat.Before.Tp)},内{SimpleStat(combat.Before.Mp)}->" +
                    $"气{SimpleStat(combat.After.Tp)},内{SimpleStat(combat.After.Mp)}"
                ).Sb().Color(Color.DeepSkyBlue));
            Debug.Log(sb);
        }

        private void DodgeFormConsumeLog(StringBuilder sb, ConsumeRecord<IDodgeForm> dodge)
        {
            sb.AppendLine(($"{dodge.UnitName}身法," + FormLog(dodge.Form)).Sb().Color(Color.GreenYellow)
                .ToString());
            sb.Append(
                (
                    $"气{SimpleStat(dodge.Before.Tp)},内{SimpleStat(dodge.Before.Mp)}->" +
                    $"气{SimpleStat(dodge.After.Tp)},内{SimpleStat(dodge.After.Mp)}"
                ).Sb().Color(Color.DeepSkyBlue));
            Debug.Log(sb);
        }

        private static void ForceFormConsumeLog(StringBuilder sb, ConsumeRecord<IForceForm> force)
        {
            sb.AppendLine(($"{force.UnitName}调息," + FormLog(force.Form)).Sb().Color(Color.LightPink)
                .ToString());
            sb.Append($"{force.Before}->{force.After}".Sb().Color(Color.DeepSkyBlue));
            Debug.Log(sb);
        }

        private static string FormLog<T>(T f) where T : IBreathNode, ISkillForm => $"【{f.Name}({f.Breath})】".Sb().Color(Color.Cornsilk).ToString();

        private string ArmedKindLog(Way.Armed armedKind)
        {
            return armedKind switch
            {
                Way.Armed.Unarmed => "空手",
                Way.Armed.Sword => "剑法",
                Way.Armed.Fling => "暗器",
                Way.Armed.Blade => "刀法",
                Way.Armed.Stick => "棍型",
                Way.Armed.Whip => "鞭法",
                Way.Armed.Short => "暗器",
                _ => throw new ArgumentOutOfRangeException(nameof(armedKind), armedKind, null)
            };
        }

        private string SimpleStat(IConditionValue c) => $"[{c.Value}/{c.Max}]";
        private string StatusLog(IStatusRecord s) => $"[息:{s.Breath}]血{SimpleStat(s.Hp)},气{SimpleStat(s.Tp)},内{SimpleStat(s.Mp)}";

        private string SimpleStatusLog(IStatusRecord b, IStatusRecord s, Color color)
        {
            var sb = $"[息:{s.Breath}]".Sb();
            if (b.Hp.Value != s.Hp.Value) sb.Append($"血{SimpleStat(s.Hp)} ");
            if (b.Tp.Value != s.Tp.Value) sb.Append($"气{SimpleStat(s.Tp)} ");
            if (b.Mp.Value != s.Mp.Value) sb.Append($"内{SimpleStat(s.Mp)} ");
            return sb.Color(color).ToString();
        }
    }

    public static class DebugExtension
    {
        public static StringBuilder Sb(this object obj) => new(obj.ToString());
        public static StringBuilder Bold(this StringBuilder text)
        {
            text.Insert(0, "<b>");
            text.Append("</b>");
            return text;
        }
        public static StringBuilder Color(this StringBuilder text,Color color)
        {
            //int r = (int)color.r, g = (int)color.g, b = (int)color.b;
            //var colorString = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            var colorText = $"<color={HexConverter(color)}>";
            text.Insert(0, colorText);
            text.Append("</color>");
            
            return text;
        }
        private static string HexConverter(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }
}