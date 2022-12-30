using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using NameM;
using UnityEngine;
using UnityEngine.Analytics;
using Utls;

namespace Server.Controllers.Characters
{
    public interface ITestDiziController
    {
        void OnGenerateDizi(int grade);
        void OnDiziLevel(int level);
        void OnSetStamina(int currentStamina, int minutePass);
    }

    public class TestTestDiziController : ITestDiziController
    {
        private StaminaTimer StaminaTimer { get; }
        private DiziConfig Config { get; }

        internal TestTestDiziController(DiziConfig config)
        {
            Config = config;
            StaminaTimer = new GameObject(nameof(Characters.StaminaTimer)).AddComponent<StaminaTimer>();
        }
        private TestTestDiziController.Dizi TestDizi { get; set; }
        public void OnGenerateDizi(int grade)
        {
            TestDizi = GenerateDizi(grade);
            Game.MessagingManager.Send(EventString.Test_DiziGenerate, TestDizi);
        }
        public void OnDiziLevel(int level)
        {
            var leveling = Config.LevelConfig;
            var str = leveling.GetLeveledValue(LevelConfigSo.Props.Strength, TestDizi.Strength, level);
            var agi = leveling.GetLeveledValue(LevelConfigSo.Props.Agility, TestDizi.Agility, level);
            var hp = leveling.GetLeveledValue(LevelConfigSo.Props.Strength, TestDizi.MaxHp, level);
            var mp = leveling.GetLeveledValue(LevelConfigSo.Props.Strength, TestDizi.MaxMp, level);
            var dizi = TestDizi.Clone();
            dizi.SetHp(hp);
            dizi.SetMp(mp);
            dizi.SetAgility(agi);
            dizi.SetStrength(str);
            dizi.SetLevel(level);
            Game.MessagingManager.Send(EventString.Test_DiziLeveling, dizi);
        }
        public void OnSetStamina(int currentStamina, int minutePass)
        {
            var lastTicks = SysTime.UnixTicksFromNow(TimeSpan.FromMinutes(-minutePass));
            SetStamina(currentStamina, lastTicks);
        }

        private void SetStamina(int baseStamina, long lastTicks) =>
            StaminaTimer.Init(baseStamina, lastTicks, Config.StaminaGenerator);

        private Dizi GenerateDizi(int grade)
        {
            var name = GenerateName();
            var (str, agi, hp, mp, sta, inv) = Config.GradeConfigSo.GenerateFromGrade(grade: grade);
            var cap = new Capable(grade: grade, dodgeSlot: 3, combatSlot: 5, bag: inv, str, agi, hp, mp);
            return new Dizi(
                name: name, 
                strength: str, 
                agility: agi, 
                hp: hp, 
                mp: mp, 
                level: 1, 
                stamina: sta,
                gradeTitle: GradeConfigSo.GetColorTitle((GradeConfigSo.Grades)grade),
                capable: cap, condition: new Dictionary<int, int>());
        }
        private string GenerateName()
        {
            var gen = Sys.RandomBool() ? Gender.Male : Gender.Female;
            return NameGen.GenName(gen).Text;
        }

        internal class Dizi
        {
            public string Name { get; private set; }
            public int Strength { get; private set; }
            public int Agility { get; private set; }
            public int Hp { get; private set; }
            public int MaxHp { get; private set; }
            public int Mp { get; private set; }
            public int MaxMp { get; private set; }
            public int Level { get; private set; }
            public int Stamina { get; private set; }
            public string GradeTitle { get;  }
            public Capable Capable { get; private set; }
            public Dictionary<int, int> Condition { get; private set; }

            public Dizi()
            {
            }
            public Dizi(string name, GradeValue<int> strength, GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp, int level, int stamina, string gradeTitle, Capable capable, Dictionary<int, int> condition)
            {
                Name = name;
                Strength = strength.Value;
                Agility = agility.Value;
                Hp = hp.Value;
                MaxHp = hp.Value;
                Mp = mp.Value;
                MaxMp = mp.Value;
                Level = level;
                Stamina = stamina;
                Capable = capable;
                Condition = condition;
                GradeTitle = gradeTitle;
            }

            public void SetHp(int hp)
            {
                Hp = hp;
                MaxHp = hp;
            }
            public void SetMp(int mp)
            {
                Mp = mp;
                MaxMp = mp;
            }
            public void SetLevel(int level) => Level = level;
            public void SetStrength(int str) => Strength = str;
            public void SetAgility(int str) => Agility = str;

            public Dizi Clone() =>
                new(Name, Capable.Strength, Capable.Agility, Capable.Hp, Capable.Mp, Level, Stamina, GradeTitle,
                    Capable,
                    Condition.ToDictionary(c => c.Key, c => c.Value));
        }

        [Serializable]
        internal class DiziConfig
        {
            [SerializeField] private LevelConfigSo 等级配置;
            [SerializeField] private GradeConfigSo 资质配置;
            [SerializeField] private StaminaGenerateSo 体力产出配置;

            public LevelConfigSo LevelConfig => 等级配置;
            public GradeConfigSo GradeConfigSo => 资质配置;
            public StaminaGenerateSo StaminaGenerator => 体力产出配置;
        }
    }
}