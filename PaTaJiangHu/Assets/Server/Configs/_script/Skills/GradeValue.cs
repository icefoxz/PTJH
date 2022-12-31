namespace Server.Configs._script.Skills
{
    /// <summary>
    /// 品级值, 主要声明该值为什么品级
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct GradeValue<T> 
    {
        public T Value { get; set; }
        public int Grade { get; set; }

        public GradeValue(T value, int grade)
        {
            Value = value;
            Grade = grade;
        }
    }
}