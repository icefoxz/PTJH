using Models;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子动态属性
    /// </summary>
    public record DiziDynamicProps
    {
        public int Level { get; }
        public int Exp { get; }
        public int Stamina { get; }
        public int Silver { get; }
        public int Food { get; }
        public int Emotion { get; }
        public int Injury { get; }
        public int Inner { get; }

        public DiziDynamicProps(Dizi d)
        {
            Level = d.Level;
            Exp = d.Exp.Value;
            Stamina = d.Stamina.Con.Value;
            Silver = d.Silver.Value;
            Food = d.Food.Value;
            Emotion = d.Emotion.Value;
            Injury = d.Injury.Value;
            Inner = d.Inner.Value;
        }
    }
}