namespace HotFix_Project.Views.Bases
{
    public interface IMonoUpdate
    {
        string ClassFullName { get; }
        void Update();
    }
}
