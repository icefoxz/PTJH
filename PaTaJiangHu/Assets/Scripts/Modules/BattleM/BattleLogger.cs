using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Color = System.Drawing.Color;

namespace BattleM
{
    public class BattleLogger : MonoBehaviour
    {
        public void NextRound(CombatRoundRecord rec)
        {
            RoundLog(rec);
            FightLog(rec);
        }

        private void RoundLog(CombatRoundRecord rec)
        {
            var round = rec.Round.Sb()
                .Insert(0, "【回合").Append("】")
                .Color(Color.HotPink)
                .AppendLine(rec.IsFightEnd ? "【战斗结束】" : string.Empty);
            foreach (var bar in rec.BreathBars)
            {
                var unit = bar.Unit;
                round.Append($"{unit.Name}[位:{unit.Position}]");
                round.Append($"息：【{bar.GetBreath()}】");
                foreach (var text in bar.Breathes.Select(breath => breath.Type switch
                         {
                             BreathRecord.Types.Busy => $"【硬：{breath.Value}】",
                             BreathRecord.Types.Charge => $"【蓄：{breath.Value}】",
                             BreathRecord.Types.Exert => $"【运：{breath.Value}】",
                             BreathRecord.Types.Attack => $"【攻：{breath.Value}】",
                             BreathRecord.Types.Placing => $"【移：{breath.Value}】",
                             _ => throw new ArgumentOutOfRangeException()
                         })) round.Append(text);
                round.Append($"{StatusLog(unit)}".Sb().Color(Color.White));
                round.AppendLine();
            }
            Debug.Log(round);
        }

        private void FightLog(CombatRoundRecord rec)
        {
            var sb = new StringBuilder();
            if (rec.SwitchTargetRec != null)
            {
                var sw = rec.SwitchTargetRec;
                sb.Append($"{sw.CombatUnit.Name}【尝试逃走...】{StatusLog(sw.CombatUnit)}".Sb().Color(Color.Yellow));
            }

            switch (rec.Major)
            {
                case CombatRoundRecord.MajorEvents.None:
                    break;
                case CombatRoundRecord.MajorEvents.Combat:
                    if (rec.AttackRec.AttackPlacing != null)
                        PositionRecordLog(rec.AttackRec.AttackPlacing);
                    AttackRecordLog(rec.AttackRec);
                    break;
                case CombatRoundRecord.MajorEvents.Recover:
                    ForceFormConsumeLog(rec.RecoverRec);
                    break;
                case CombatRoundRecord.MajorEvents.Escape:
                    EscapeRecordLog(rec.EscapeRec);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var eventRecord in rec.OtherEventRec) EventRecordLog(eventRecord);//战斗事件

            foreach (var recharge in rec.RechargeRec) RechargeLog(sb, recharge);//恢复内力
            var log = sb.ToString();
            if (!string.IsNullOrWhiteSpace(log))
                Debug.Log(sb);
        }

        private static void PositionRecordLog(PositionRecord pos)
        {
            var sb = new StringBuilder();
            var text = UnitNamePos(pos.Unit).Sb().Color(Color.White)
                .Append($"移位->【{pos.NewPos.Sb().Color(Color.GreenYellow)}】| ")
                .Append(UnitNamePos(pos.Target).Sb().Color(Color.LightBlue));
            sb.Append(text);
            Debug.Log(sb);
        }

        private static string UnitNamePos(IUnitInfo u) => $"{u.Name}[位({u.Position})]";

        private void EscapeRecordLog(EscapeRecord esc)
        {
            var sb = new StringBuilder();
            var e = esc.Escapee;
            sb.Append($"{e.Name}打算逃跑......{StatusLog(e)}{(esc.IsSuccess ? "逃跑成功！~" : "逃脱失败！")}")
                .Color(Color.GreenYellow).Bold();
            var u = esc.Attacker;
            if (u == null)
            {
                sb.AppendLine($"无人阻止，{e.Name}逃跑......").Color(Color.GreenYellow).Bold();
            }
            else
            {
                sb.Append($"{u.Name}【投掷暗器】".Sb().Color(Color.Red));
                sb.Append($"{u.Name}伤害: 力({u.Strength})".Sb().Color(Color.Coral));
                sb.AppendLine($"【{AttackLog(esc.AttackConsume.Form)}】".Sb().Color(Color.LightBlue).ToString());
                sb.Append($"{esc.DamageFormula})".Sb().Color(Color.Coral)
                );
                DodgeRecordLog(esc.DodgeFormula, esc.EscapeConsume.Form, e, sb);
                ParryRecordLog(esc.ParryFormula, esc.ParryConsume.Form, e, sb);
            }

            Debug.Log(sb);
            string AttackLog(ICombatForm a) => FormLog(a) +
                                               $"内({a.Mp})气({a.Tp})硬:己({a.OffBusy})敌({a.TarBusy})".Sb()
                                                   .Color(Color.Coral);
        }

        private static void AttackRecordLog(AttackRecord att)
        {
            var sb = new StringBuilder();

            string AttackLog(ICombatForm a) => FormLog(a) +
                                               $"内({a.Mp})气({a.Tp})硬:己({a.OffBusy})敌({a.TarBusy})".Sb()
                                                   .Color(Color.Coral);

            var u = att.Unit;
            sb.Append($"【{u.Name}】力({u.Strength})".Sb().Color(Color.Coral));
            sb.Append($"【总伤害：({att.DamageFormula.Finalize}){AttackLog(att.Form)}】".Sb().Color(Color.Orange).ToString());
            sb.Append($"{att.DamageFormula})".Sb().Color(Color.DarkOrange));
            sb.AppendLine();
            DodgeRecordLog(att.DodgeFormula, att.DodgeConsume.Form , att.Target, sb);
            if (!att.DodgeFormula.IsSuccess)
            {
                ParryRecordLog(att.ParryFormula, att.ParryConsume.Form, att.Target, sb);
                CombatFormConsumeLog(sb, att.AttackConsume, att.Unit);
                sb.AppendLine();
                ConsumeLog(sb, att.Suffer);
            }

            Debug.Log(sb);

            void CombatFormConsumeLog(StringBuilder s, ConsumeRecord<ICombatForm> combat, IUnitInfo info)
            {
                s.Append($"【{info.Name}】气:({combat.Form.Tp})内:({combat.Form.Mp})".Sb().Color(Color.Cyan));
                s.Append(ArmedKindLog(info.Equip.Armed).Sb().Color(Color.DarkOrange).ToString());
                s.Append(
                    $"己硬直({combat.Form.OffBusy.Sb().Bold().Color(Color.DarkOrange)})|敌硬直({combat.Form.TarBusy.Sb().Bold().Color(Color.Orange)})");
                ConLog(s, combat);
            }

            void ConsumeLog(StringBuilder s, ConsumeRecord consume)
            {
                s.Append($"【{consume.UnitName}】".Sb().Color(Color.Cyan));
                ConLog(s, consume);
            }

            void ConLog(StringBuilder s, ConsumeRecord consume)
            {
                s.Append(
                    (
                        $"血{SimpleStat(consume.Before.Hp)},气{SimpleStat(consume.Before.Tp)},内{SimpleStat(consume.Before.Mp)}->" +
                        $"血{SimpleStat(consume.After.Hp)},气{SimpleStat(consume.After.Tp)},内{SimpleStat(consume.After.Mp)}"
                    ).Sb().Color(Color.DeepSkyBlue));

            }
        }

        static void DodgeRecordLog(DodgeFormula formula, IDodge form, IUnitInfo tar,StringBuilder sb)
        {
            string DodgeLog(IDodge a) => FormLog(a) +
                                             $"身({a.Dodge})内({a.Mp})气({a.Tp}))".Sb().Color(Color.Coral);

            sb.Append(!formula.IsSuccess
                ? $"闪避失败!随机【{formula.RandomValue}】闪避值【{formula.Finalize}】".Sb().Color(Color.Crimson)
                : $"闪避成功！随机【{formula.RandomValue}】闪避值【{formula.Finalize}】".Sb().Color(Color.Yellow));
            sb.Append(tar.Sb().Color(Color.LightBlue));
            sb.Append(DodgeLog(form).Sb().Color(Color.LightBlue).ToString());
            sb.Append(formula.Sb().Color(Color.DarkTurquoise));
            sb.AppendLine();
        }
        static void ParryRecordLog(ParryFormula formula, IParryForm form, IUnitInfo tar,StringBuilder sb)
        {
            string ParryLog(IParryForm a) => $"【{a.Name}】".Sb().Color(Color.LightSeaGreen).ToString() +
                                             $"架({a.Parry})内({a.Mp})气({a.Tp})硬:己({a.OffBusy})".Sb().Color(Color.Coral);
            sb.Append(!formula.IsSuccess
                ? $"招架失败!随机【{formula.RandomValue}】招架值【{formula.Finalize}】".Sb().Color(Color.Crimson)
                : $"招架成功！随机【{formula.RandomValue}】招架值【{formula.Finalize}】".Sb().Color(Color.Yellow));
            sb.Append(UnitNamePos(tar).Sb().Color(Color.LightGreen));
            sb.Append(ParryLog(form).Sb().Color(Color.LightBlue).ToString());
            sb.Append(formula.Sb().Color(Color.Yellow));
            sb.AppendLine();
        }

        private void EventRecordLog(SubEventRecord e)
        {
            var sb = new StringBuilder();
            switch (e.Type)
            {
                //case SubEventRecord.EventTypes.TryEscape:
                //    sb.Append($"{e.Unit.Name}【尝试逃走...】{StatusLog(e.Status)}".Sb().Color(Color.Yellow));
                //    break;
                //case SubEventRecord.EventTypes.Escaped:
                //    sb.Append($"{e.Unit.Name}【逃走了！】{StatusLog(e.Status)}".Sb().Color(Color.GreenYellow));
                //    break;
                case SubEventRecord.EventTypes.Death:
                    sb.Append($"{e.Unit.Name}【死亡！】{StatusLog(e.Status)}".Sb().Color(Color.Red));
                    break;
                case SubEventRecord.EventTypes.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //{
            //    case FightFragment.Types.TryEscape:
            //        break;
            //    case FightFragment.Types.Escaped:
            //        break;
            //    case FightFragment.Types.Death:
            //        break;
            //    case FightFragment.Types.Exhausted:
            //        sb.Append($"{e.Unit.Name}【倒地失去意识！】{StatusLog(e.Status)}".Sb().Color(Color.DodgerBlue));
            //        break;
            //    case FightFragment.Types.SwitchTarget:
            //        sb.Append($"{e.Unit.Name}【换攻击目标.】".Sb().Color(Color.Orange));
            //        break;
            //    case FightFragment.Types.None:
            //    case FightFragment.Types.Consume:
            //    case FightFragment.Types.Attack:
            //    case FightFragment.Types.Parry:
            //    case FightFragment.Types.Dodge:
            //    case FightFragment.Types.Position:
            //    case FightFragment.Types.Fling:
            //    case FightFragment.Types.Wait:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            Debug.Log(sb);
        }

        private void RechargeLog(StringBuilder sb, RechargeRecord charge)
        {
            var consume = charge.Consume;
            //if (consume.Type == FightFragment.Types.Wait)
            //{
                sb.Append($"【{consume.UnitName}】回气({charge.Charge}),".Sb().Color(Color.Cyan).ToString() +
                          StatusLog(consume.Before).Sb().Color(Color.Aquamarine) + "->");
                sb.Append(SimpleStatusLog(consume.Before, consume.After, Color.GreenYellow));
            //}
            //else
            //{
            //    sb.Append($"【{consume.UnitName}】消耗,".Sb().Color(Color.IndianRed).ToString() +
            //              StatusLog(consume.Before).Sb().Color(Color.Aquamarine) + "->");
            //    sb.Append(SimpleStatusLog(consume.Before, consume.After,
            //        consume.After.Hp.Value == 0 || consume.After.Tp.Value == 0
            //            ? Color.Red
            //            : Color.IndianRed)).Bold();
            //}
        }

        private static void ForceFormConsumeLog(ConsumeRecord<IForce> force)
        {
            var sb = new StringBuilder();
            sb.AppendLine(($"{force.UnitName}调息," + FormLog(force.Form)).Sb().Color(Color.LightPink)
                .ToString());
            sb.Append($"{force.Before}->{force.After}".Sb().Color(Color.DeepSkyBlue));
            Debug.Log(sb);
        }

        private static string FormLog<T>(T f) where T : IBreathNode, ISkillName=> $"【{f.Name}(息：{f.Breath})】".Sb().Color(Color.Cornsilk).ToString();

        private static string ArmedKindLog(Way.Armed armedKind)
        {
            return armedKind switch
            {
                Way.Armed.Unarmed => "空手",
                Way.Armed.Sword => "剑法",
                //Way.Armed.Fling => "暗器",
                Way.Armed.Blade => "刀法",
                Way.Armed.Stick => "棍型",
                Way.Armed.Whip => "鞭法",
                Way.Armed.Short => "暗器",
                _ => throw new ArgumentOutOfRangeException(nameof(armedKind), armedKind, null)
            };
        }

        private static string SimpleStat(IConditionValue c) => $"[{c.Value}/{c.Max}]";
        private static string StatusLog(IStatusRecord s) => StatusLog(s.Breath, s.Hp, s.Tp, s.Mp);
        private static string StatusLog(int breathes, IConditionValue hp, IConditionValue tp, IConditionValue mp) => $"[息:{breathes}]血{SimpleStat(hp)},气{SimpleStat(tp)},内{SimpleStat(mp)}";

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