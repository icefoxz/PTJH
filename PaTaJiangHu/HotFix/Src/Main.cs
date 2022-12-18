namespace HotFix_Project
{
    /// <summary>
    /// 热更新主调用入口
    /// </summary>
    internal class Main
    {
        public static App App { get; private set; }
        public static void Run()
        {
            App = new App();
            App.Init();
        }
        public static void RunTest()
        {
            App = new App();
            App.InitTest();
        }
    }
}
