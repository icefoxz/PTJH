using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using Utls;

namespace Server.Configs.BattleSimulation
{
    [CreateAssetMenu(fileName = "模拟战斗文本配置", menuName = "配置/简易战斗/文本配置")]
    internal class BattleSimulatorMessageSo : ScriptableObject
    {
        [Header("{0}=进攻者,{1}=反馈者,{2}=攻击部位")]
        [SerializeField] private string 进攻前摇默认 = "{0}不慌不忙一个起手式,";
        [SerializeField] private string 进攻执行默认 = "突然身形一变直驱{1}的{2}";
        [SerializeField] private string 反馈前摇默认 = "早已谋定应变招式";
        [SerializeField] private string 反馈执行默认 = "轻松的接下{0}的攻击";
        [SerializeField] private string 伤害描述默认 = "{1}遭受重伤,似乎生命岌岌可危了!";
        [SerializeField] private string[] 随机攻击部位;
        [SerializeField] private RoundTextGenerator 回合文本;

        private RoundTextGenerator RoundTextGen => 回合文本;

        private string PreAttackDefault => 进攻前摇默认;
        private string AttackDefault => 进攻执行默认;
        private string PreRespondDefault => 反馈前摇默认;
        private string RespondDefault => 反馈执行默认;
        private string DamageDefault => 伤害描述默认;
        private string[] AttackArea => 随机攻击部位;
        //private string PreAttack { get; } = "{0}不慌不忙一个起手式,";
        //private string Attack { get; } = "突然身形一变直驱{1}的{2}";
        //private string PreRespond { get; } = "早已谋定应变招式";
        //private string Respond { get; } = "轻松的接下{0}的攻击";
        //private string Damage { get; } = "{1}遭受重伤,似乎生命岌岌可危了!";

        public string[] GetSimulationMessages(int round, bool isPlayerWin, ISimCombat player, ISimCombat enemy, int playerRemaining)
        {
            string GetRoundText(ISimCombat op, ISimCombat tar, bool isOpWin)
            {
                return  ResolveCharacterName(ResolveWithDefault(PreAttackDefault, 
                           RoundTextGen.GetPreAttack(op, tar, isOpWin)), op, tar)
                       + ResolveCharacterName(ResolveWithDefault(AttackDefault, 
                           RoundTextGen.GetAttack(op, tar, isOpWin)), op, tar)
                       + ResolveCharacterName(ResolveWithDefault(PreRespondDefault, 
                           RoundTextGen.GetPreRespond(op, tar, isOpWin)), op, tar)
                       + ResolveCharacterName(ResolveWithDefault(RespondDefault, 
                           RoundTextGen.GetRespond(op, tar, isOpWin)), op, tar)
                       + ResolveCharacterName(ResolveWithDefault(DamageDefault, 
                           RoundTextGen.GetDamage(op, tar, isOpWin)), op, tar);
            }

            string ResolveWithDefault(string defaultText, IReadOnlyCollection<string> array) =>
                array.Count == 0 ? defaultText : array.RandomPick();
            string ResolveCharacterName(string text, ISimCombat offender, ISimCombat defender) =>
                string.Format(text, offender.Name, defender.Name, AttackArea.RandomPick());

            return new[]
            {
                GetRoundText(player, enemy, isPlayerWin),
                GetRoundText(enemy, player, !isPlayerWin)
            };
        }
        [Serializable]private class RoundTextGenerator
        {
            [SerializeField]private TextCondition[] 进攻前摇;
            [SerializeField]private TextCondition[] 进攻执行;
            [SerializeField]private TextCondition[] 反馈前摇;
            [SerializeField]private TextCondition[] 反馈执行;
            [SerializeField]private TextCondition[] 伤害描述;

            private TextCondition[] PreAttack => 进攻前摇;
            private TextCondition[] Attack => 进攻执行;
            private TextCondition[] PreRespond => 反馈前摇;
            private TextCondition[] Respond => 反馈执行;
            private TextCondition[] Damage => 伤害描述;

            public string[] GetPreAttack(ISimCombat op, ISimCombat tar, bool isOpWin) =>
                GetFilter(PreAttack, op, tar, isOpWin);
            public string[] GetAttack(ISimCombat op, ISimCombat tar, bool isOpWin) =>
                GetFilter(Attack, op, tar, isOpWin);
            public string[] GetPreRespond(ISimCombat op, ISimCombat tar, bool isOpWin) =>
                GetFilter(PreRespond, op, tar, isOpWin);
            public string[] GetRespond(ISimCombat op, ISimCombat tar, bool isOpWin) =>
                GetFilter(Respond, op, tar, isOpWin);
            public string[] GetDamage(ISimCombat op, ISimCombat tar, bool isOpWin) =>
                GetFilter(Damage, op, tar, isOpWin);
            private string[] GetFilter(TextCondition[] array, ISimCombat op, ISimCombat tar, bool isOpWin) =>
                array.Where(o => o.IsInTerm(op, tar, isOpWin)).SelectMany(o => o.Messages).ToArray();
        }
        [Serializable]private class TextCondition
        {
            [SerializeField] private string _name;
            [SerializeField] private PercentageDiff[] 条件;
            private PercentageDiff[] Clauses => 条件;
            [SerializeField] private string[] 文本;
            public string[] Messages => 文本;

            public bool IsInTerm(ISimCombat op, ISimCombat tar, bool isOpWin) =>
                Clauses.All(c => c.IsInTerm(op, tar, isOpWin));
        }
        [Serializable]private class PercentageDiff
        {
            private const char PercentText = '%';

            private enum CompareTypes
            {
                [InspectorName("战力强于")]PowGreater,
                [InspectorName("战力弱于")]PowLesser,
                [InspectorName("攻击强于")]OffGreater,
                [InspectorName("攻击弱于")] OffLesser,
                [InspectorName("血量大于")]DefGreater,
                [InspectorName("血量小于")]DefLesser,
                [InspectorName("力量大于")]StrGreater,
                [InspectorName("力量小于")]StrLesser,
                [InspectorName("敏捷大于")]AgiGreater,
                [InspectorName("敏捷小于")]AgiLesser,
                [InspectorName("战胜")]Winner,
                [InspectorName("战败")]Loser,
            }

            private bool RenameElement()
            {
                if (Compare == CompareTypes.Winner)
                {
                    _name = "进攻者是战胜者";
                    return true;
                }
                if (Compare == CompareTypes.Loser)
                {
                    _name = "进攻者是战败者";
                    return true;
                }
                if(Range is {Min: 0, Max: 0 })
                {
                    _name = string.Empty;
                    return true;
                }
                _name = $"进攻者{GetText(Compare)}{TermText()}";
                return true;
            }

            private string TermText()
            {
                var min = Range.Min.ToString()+PercentText;
                var max = Range.Max.ToString()+PercentText;
                return $":{min}~{max}{ResolveInverse()}";

                string ResolveInverse() => Inverse ?"(范围外)":string.Empty;
            }

            private string GetText(CompareTypes compare) => compare switch
            {
                CompareTypes.PowGreater => "战力强于",
                CompareTypes.PowLesser => "战力弱于",
                CompareTypes.OffGreater => "攻击强于",
                CompareTypes.OffLesser => "攻击弱于",
                CompareTypes.DefGreater => "血量大于",
                CompareTypes.DefLesser => "血量小于",
                CompareTypes.StrGreater => "力量大于",
                CompareTypes.StrLesser => "力量小于",
                CompareTypes.AgiGreater => "敏捷大于",
                CompareTypes.AgiLesser => "敏捷小于",
                CompareTypes.Winner => "胜利",
                CompareTypes.Loser => "失败",
                _ => string.Empty
            };

            [ConditionalField(true, nameof(RenameElement))][SerializeField][ReadOnly] private string _name;
            [SerializeField] private CompareTypes 判断类型;
            [ConditionalField(nameof(判断类型), true, CompareTypes.Winner, CompareTypes.Loser)] 
            [SerializeField] private bool 范围外;
            [ConditionalField(nameof(判断类型), true, CompareTypes.Winner, CompareTypes.Loser)]
            [SerializeField] private MinMaxInt 范围;
            private CompareTypes Compare => 判断类型;
            private MinMaxInt Range => 范围;
            private bool Inverse => 范围外;

            public bool IsInTerm(ISimCombat op,ISimCombat tar,bool isOpWin)
            {
                var diff = 0;
                switch (Compare)
                {
                    case CompareTypes.PowGreater:
                        diff = GetDiff(op.Power, tar.Power);
                        break;
                    case CompareTypes.PowLesser:
                        diff = GetDiff(tar.Power, op.Power);
                        break;
                    case CompareTypes.OffGreater:
                        diff = GetDiff(op.Offend, tar.Offend);
                        break;
                    case CompareTypes.OffLesser:
                        diff = GetDiff(tar.Offend, op.Offend);
                        break;
                    case CompareTypes.DefGreater:
                        diff = GetDiff(op.Defend, tar.Defend);
                        break;
                    case CompareTypes.DefLesser:
                        diff = GetDiff(tar.Defend, op.Defend);
                        break;
                    case CompareTypes.StrGreater:
                        diff = GetDiff((int)op.Strength, (int)tar.Strength);
                        break;
                    case CompareTypes.StrLesser:
                        diff = GetDiff((int)tar.Strength, (int)op.Strength);
                        break;
                    case CompareTypes.AgiGreater:
                        diff = GetDiff((int)op.Agility, (int)tar.Agility);
                        break;
                    case CompareTypes.AgiLesser:
                        diff = GetDiff((int)tar.Agility, (int)op.Agility);
                        break;
                    case CompareTypes.Winner:
                        return isOpWin;
                    case CompareTypes.Loser:
                        return !isOpWin;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var min = Range.Min;
                var max = Range.Max;
                var isInRange = diff < max && diff >= min;
                return Inverse ? !isInRange : isInRange;
            }

            private int GetDiff(int max, int value)
            {
                var diff = max - value;
                if (diff <= 0) return 0;
                return diff * 100 / max;
            }
        }
    }
}