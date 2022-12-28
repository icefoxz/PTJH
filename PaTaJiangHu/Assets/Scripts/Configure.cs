using Server.Controllers.Characters;
using UnityEngine;

internal class Configure : MonoBehaviour
{
    [SerializeField] private GradeConfigSo _gradeConfig;
    internal GradeConfigSo GradeConfig => _gradeConfig;
}