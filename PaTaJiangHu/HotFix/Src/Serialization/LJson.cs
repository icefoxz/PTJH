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
    private static Type StringType => typeof(string);
    private static Type IntType => typeof(int);
    private static Type FloatType => typeof(float);
    private static Type DoubleType => typeof(double);
    private static Type CharType => typeof(char);
    private static Type LongType => typeof(long);
    private static Type BoolType => typeof(bool);

    public static T Get<T>(this ObjectBag bag, int index) 
    {
        var value = bag.GetString(index);
        var type = typeof(T);
        if (type == StringType) return (T)Convert.ChangeType(value, StringType);
        if (type == IntType) return (T)Convert.ChangeType(value, IntType);
        if (type == FloatType) return (T)Convert.ChangeType(value, FloatType);
        if (type == DoubleType) return (T)Convert.ChangeType(value, DoubleType);
        if (type == CharType) return (T)Convert.ChangeType(value, CharType);
        if (type == LongType) return (T)Convert.ChangeType(value, LongType);
        if (type == BoolType)
        {
            var b = int.Parse(value);
            return (T)Convert.ChangeType(b, BoolType);
        }
        return LJson.ToObject<T>(bag.Bag[index]);
    }
}