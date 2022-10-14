using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;

namespace HotFix_Project.Data
{
    ///// <summary>
    ///// 1维数据袋
    ///// </summary>
    //public interface IObjectBag
    //{
    //    void LoadParam(object[] args);
    //    object[] ToParam();
    //}

    //internal abstract class ObjectBag : IObjectBag
    //{
    //    public abstract void LoadParam(object[] args);
    //    public abstract object[] ToParam();
    //    public string ToJson() => Json.Serialize(ToParam());
    //    public static TBag LoadParam<TBag>(object[] objects) where TBag : class, IObjectBag, new()
    //    {
    //        var t = new TBag();
    //        t.LoadParam(objects);
    //        return t;

    //    }
    //    public static TBag ToObject<TBag>(string obText) where TBag : class, IObjectBag, new()
    //    {
    //        var objects = Json.Deserialize<object[]>(obText);
    //        return LoadParam<TBag>(objects);
    //    }

    //    public static string ToJson<TBag>(TBag bag) where TBag : class, IObjectBag, new() =>
    //        Json.Serialize(bag.ToParam());
    //}
    /// <summary>
    /// 1维数据袋
    /// </summary>
    public interface IObjectBag
    {
        void LoadParam(JToken[] args);
        JToken[] ToParam();
    }

    public abstract class ObjectBag : IObjectBag
    {
        public abstract void LoadParam(JToken[] args);
        public abstract JToken[] ToParam();
        protected T Cast<T>(JToken obj) => obj.ToObject<T>();
        public static TBag LoadParam<TBag>(JToken[] objects) where TBag : class, IObjectBag, new()
        {
            var t = new TBag();
            t.LoadParam(objects);
            return t;

        }
        public static TBag ToObject<TBag>(string obText) where TBag : class, IObjectBag, new()
        {
            var objects = Json.Deserialize<JToken[]>(obText);
            return LoadParam<TBag>(objects);
        }

        public static string ToJson<TBag>(TBag bag) where TBag : class, IObjectBag, new() =>
            Json.Serialize(bag.ToParam());

        public string ToJson() => Json.Serialize(ToParam());
    }

}
