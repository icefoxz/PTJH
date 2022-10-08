using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace Data.Views
{
    /// <summary>
    /// 1维数据袋
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectBag<T> where T : class, IObjectBag<T>, new()
    {
        void LoadParam(object[] args);
        object[] ToParam();
    }

    public abstract class ObjectBag<T> : IObjectBag<T> where T : class, IObjectBag<T>, new()
    {
        public abstract void LoadParam(object[] args);
        public abstract object[] ToParam();

        public static TBag ToObject<TBag>(string obText) where TBag : class, IObjectBag<TBag>, new()
        {
            var objects = JsonMapper.ToObject<object[]>(obText);
            var t = new TBag();
            t.LoadParam(objects);
            return t;
        }
        public static string ToJson<TBag>(TBag bag) where TBag : class, IObjectBag<TBag>, new()
        {
            return JsonMapper.ToJson(bag.ToParam());
        }
    }

    public class ViewBag : ObjectBag<ViewBag>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override void LoadParam(object[] args)
        {
            var list = args.ToList();
            Id = (int)list[0];
            Name = (string)list[1];
        }

        public override object[] ToParam() => new object[] { Id, Name };
    }

    public static class TestViewBag
    {
        public static List<ViewBag> GetBags(int amt)
        {
            var list = new List<ViewBag>();
            for (var i = 0; i < amt; i++)
            {
                 var bag = new ViewBag { Id = i, Name = $"Bag.{i}" };
                 list.Add(bag);
            }
            return list;
        }
        public static List<object[]> GetObjects(int amt)
        {
            var list = new List<object[]>();
            for (var i = 0; i < amt; i++)
            {
                 var bag = new ViewBag { Id = i, Name = $"Bag.{i}" };
                 list.Add(bag.ToParam());
            }
            return list;
        }
        public static List<string> GetJsons(int amt)
        {
            var list = new List<string>();
            for (var i = 0; i < amt; i++)
            {
                 var bag = new ViewBag { Id = i, Name = $"Bag.{i}" };
                 list.Add(ViewBag.ToJson(bag));
            }
            return list;
        }
    }
}
