using System;
using HotFix_Project.Serialization.LitJson;

namespace HotFix_Project.Managers;

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