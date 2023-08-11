using System;
using System.Linq;
using GameClient.Models;
using GameClient.Modules.NameM;
using GameClient.SoScripts.Characters;
using GameClient.SoScripts.Factions;

public class DiziGenerator
{
    private GradeConfigSo GradeConfig { get; }
    internal DiziGenerator(GradeConfigSo gradeConfig)
    {
        GradeConfig = gradeConfig;
    }
    public Dizi GenerateDizi(int grade)
    {
        var (name, gender) = NameGen.GenNameWithGender();
        var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(grade);
        var cr = GradeConfig.GetRandomConsumeResource(grade).ToDictionary<(ConsumeResources, int), ConsumeResources, int>(r => r.Item1, r => r.Item2);
        var gifted = GradeConfig.GenerateGifted(grade);
        var aptitude = GradeConfig.GenerateArmedAptitude(grade);
        //var combatSkill = GradeConfig.GenerateCombatSkill(randomGrade);
        //var forceSkill = GradeConfig.GenerateForceSkill(randomGrade);
        //var dodgeSkill = GradeConfig.GenerateDodgeSkill(randomGrade);
        var capable = new Capable(grade: grade,
            combatSlot: 2, forceSlot: 2, dodgeSlot: 2, bag: bag,
            strength: strength, agility: agility, hp: hp, mp: mp,
            food: cr[ConsumeResources.Food], wine: cr[ConsumeResources.Wine],
            herb: cr[ConsumeResources.Herb], pill: cr[ConsumeResources.Pill]);
        var guid = Guid.NewGuid().ToString();
        var dizi = new Dizi(guid: guid, name: name.Text, gender: gender, level: 1, stamina: stamina,
            capable: capable, gifted, aptitude);
        dizi.SetSkill(DiziSkill.Instance(
            (GradeConfig.GenerateCombatSkill(grade), 1),
            (GradeConfig.GenerateForceSkill(grade), 1),
            (GradeConfig.GenerateDodgeSkill(grade), 1)));
        return dizi;
    }
}