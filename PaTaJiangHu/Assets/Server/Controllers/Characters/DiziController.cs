using System;
using System.Collections.Generic;
using System.Linq;
using NameM;
using UnityEngine;
using UnityEngine.Analytics;
using Utls;

namespace Server.Controllers.Characters
{
    public interface IDiziController
    {
        void OnGenerateDizi(int grade);
        void OnDiziLevel(int level);
        void OnSetStamina(int currentStamina, int minutePass);
    }

    public class DiziController : IDiziController
    {
        private StaminaTimer StaminaTimer { get; }
        private DiziConfig Config { get; }

        internal DiziController(DiziConfig config)
        {
            Config = config;
            StaminaTimer = new GameObject(nameof(Characters.StaminaTimer)).AddComponent<StaminaTimer>();
        }
        private DiziController.Dizi TestDizi { get; set; }
        public void OnGenerateDizi(int grade)
        {
            TestDizi = GenerateDizi(grade);
            Game.MessagingManager.Invoke(EventString.Test_DiziGenerate, TestDizi);
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
            Game.MessagingManager.Invoke(EventString.Test_DiziLeveling, dizi);
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
            var cap = new Capable(grade: grade, dodgeSlot: 3, martialSlot: 5, inventorySlot: inv);
            return new Dizi(
                name: name, 
                strength: str, 
                agility: agi, 
                hp: hp, 
                mp: mp, 
                level: 1, 
                stamina: sta,
                capable: cap, condition: new Dictionary<int, int>());
        }
        private string GenerateName()
        {
            var gen = Sys.RandomBool() ? Gender.Male : Gender.Female;
            return NameGen.GenName(gen).Text;
        }

        internal class Dizi //: IDizi
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
            public Capable Capable { get; private set; }
            public Dictionary<int, int> Condition { get; private set; }

            public Dizi(string name, int strength, int agility, int hp, int mp, int level, int stamina, Capable capable, Dictionary<int, int> condition)
            {
                Name = name;
                Strength = strength;
                Agility = agility;
                Hp = hp;
                MaxHp = hp;
                Mp = mp;
                MaxMp = mp;
                Level = level;
                Stamina = stamina;
                Capable = capable;
                Condition = condition;
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
                new(Name, Strength, Agility, Hp, Mp, Level, Stamina, Capable,
                    Condition.ToDictionary(c => c.Key, c => c.Value));
        }
        internal class Capable 
        {
            /// <summary>
            /// 品级
            /// </summary>
            public int Grade { get; }
            /// <summary>
            /// 轻功格
            /// </summary>
            public int DodgeSlot { get; }
            /// <summary>
            /// 武功格
            /// </summary>
            public int MartialSlot { get; }
            /// <summary>
            /// 背包格
            /// </summary>
            public int InventorySlot { get; }

            public Capable(int grade, int dodgeSlot, int martialSlot, int inventorySlot)
            {
                Grade = grade;
                DodgeSlot = dodgeSlot;
                MartialSlot = martialSlot;
                InventorySlot = inventorySlot;
            }
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