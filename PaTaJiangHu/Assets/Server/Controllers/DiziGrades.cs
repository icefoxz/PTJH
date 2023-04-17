using UnityEngine;

namespace Server.Controllers
{
    public enum DiziGrades
    {
        E = 0,
        D = 1,
        C = 2,
        B = 3,
        A = 4,
        S = 5,
    }

    public enum ColorGrade
    {
        [InspectorName("白")] F,
        [InspectorName("绿")] E,
        [InspectorName("篮")] D,
        [InspectorName("紫")] C,
        [InspectorName("橙")] B,
        [InspectorName("红")] A,
        [InspectorName("金")] S,
    }
}