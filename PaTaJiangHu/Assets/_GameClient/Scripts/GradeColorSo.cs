using System;
using Server.Configs.Characters;
using Server.Controllers;
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

    public Color GetColor(ColorGrade grade)
    {
        return grade switch
        {
            ColorGrade.F => F,
            ColorGrade.E => E,
            ColorGrade.D => D,
            ColorGrade.C => C,
            ColorGrade.B => B,
            ColorGrade.A => A,
            ColorGrade.S => S,
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
        };
    }
}