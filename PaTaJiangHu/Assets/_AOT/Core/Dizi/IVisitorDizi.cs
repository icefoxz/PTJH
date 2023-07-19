using AOT.Utls;
using UnityEngine;

namespace AOT.Core.Dizi
{
    public interface IVisitorDizi 
    {
        ColorGrade Grade { get; }
        int Buy { get; }
        Vector2Int Sell { get; }
    }
}