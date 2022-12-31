using System;
using HotFix_Project.Serialization.LitJson;
using Utls;

namespace HotFix_Project.Serialization;

internal static class LJson
{
    public static T ToObject<T>(string json,bool notNullable = false)
    {
        try
        {
            return JsonMapper.ToObject<T>(json);
        }
        catch (Exception e)
        {
            if(notNullable)
            {
                Console.WriteLine(e);
                throw;
            }
            return default;
        }
    }
    public static string ToJson(object obj) => JsonMapper.ToJson(obj);
}

internal static class ObjectBagExtension
{
    public static T Map<T>(this ObjectBag bag, int index) where T : class => LJson.ToObject<T>(bag.Bag[index]);
}