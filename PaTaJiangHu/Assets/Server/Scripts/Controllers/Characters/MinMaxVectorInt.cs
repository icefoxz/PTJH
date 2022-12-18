using System;
using UnityEngine;

namespace Server.Controllers.Characters
{
    [Serializable] internal class MinMaxVectorInt
    {
        [SerializeField]private Vector2Int _range;
        public Vector2Int Range => _range;

        public int Generate() => UnityEngine.Random.Range(Range.x, Range.y + 1);
    }
}