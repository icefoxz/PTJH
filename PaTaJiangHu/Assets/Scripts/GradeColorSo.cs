using System;
using Server.Configs.Characters;
using UnityEngine;

[CreateAssetMenu(fileName = "GradeColorSo", menuName = "配置/品阶颜色")]
internal class GradeColorSo : ScriptableObject
{
    [SerializeField] private Color 白;
    [SerializeField] private Color 绿;
    [SerializeField] private Color 篮;
    [SerializeField] private Color 紫;
    [SerializeField] private Color 橙;
    [SerializeField] private Color 红;
    [SerializeField] private Color 金;

    private Color F => 白; //"白",
    private Color E => 绿; //"绿",
    private Color D => 篮; //"篮",
    private Color C => 紫; //"紫",
    private Color B => 橙; //"橙",
    private Color A => 红; //"红",
    private Color S => 金; //"金",

    public Color GetColor(GradeConfigSo.Grades grade)
    {
        return grade switch
        {
            GradeConfigSo.Grades.F => F,
            GradeConfigSo.Grades.E => E,
            GradeConfigSo.Grades.D => D,
            GradeConfigSo.Grades.C => C,
            GradeConfigSo.Grades.B => B,
            GradeConfigSo.Grades.A => A,
            GradeConfigSo.Grades.S => S,
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
        };
    }
}