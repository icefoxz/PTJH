using System;

namespace Server.Configs.Adventures
{
    public interface IAdjustment
    {
        public enum Types
        {
            Stamina = 0,
            Silver = 1,
            Food = 2,
            Emotion = 3,
            Injury = 4,
            Inner = 5,
            Exp = 6,
        }

        string Set(Types type, int value, bool percentage);
    }

    public static class AdjustmentExtension
    {
        private const string StaminaText = "体力";
        private const string SilverText = "银两";
        private const string FoodText = "食物";
        private const string EmotionText = "精神";
        private const string InjuryText = "外伤";
        private const string InnerText = "内伤";
        private const string ExpText = "经验";
        public static string GetText(this IAdjustment.Types type)
        {
            return type switch
            {
                IAdjustment.Types.Stamina => StaminaText,
                IAdjustment.Types.Silver => SilverText,
                IAdjustment.Types.Food => FoodText,
                IAdjustment.Types.Emotion => EmotionText,
                IAdjustment.Types.Injury => InjuryText,
                IAdjustment.Types.Inner => InnerText,
                IAdjustment.Types.Exp => ExpText,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}