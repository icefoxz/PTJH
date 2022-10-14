using System;
using System.Collections.Generic;
using NameM;
using UnityEngine;
using UnityEngine.Analytics;
using Utls;

namespace Server.Controllers.Characters
{
    public class DiziController
    {
        private LevelConfigSo LevelConfig { get; }
        private GradeConfigSo GradeConfigSo { get; }
        private StaminaGenerateSo StaminaGenerator { get; }
        private StaminaTimer StaminaTimer { get; }

        internal DiziController(LevelConfigSo levelConfig, GradeConfigSo gradeConfigSo,StaminaGenerateSo staminaGenerator)
        {
            LevelConfig = levelConfig;
            GradeConfigSo = gradeConfigSo;
            StaminaGenerator = staminaGenerator;
            StaminaTimer = new GameObject(nameof(Characters.StaminaTimer)).AddComponent<StaminaTimer>();
        }

        internal void SetStamina(int baseStamina, long lastTicks) =>
            StaminaTimer.Init(baseStamina, lastTicks, StaminaGenerator);

        internal Dizi GenerateDizi(int grade)
        {
            var name = GenerateName();
            var (str, agi, hp, tp, mp, sta, inv) = GradeConfigSo.GenerateFromGrade(grade: grade);
            var cap = new Capable(grade: grade, dodgeSlot: 3, martialSlot: 5, inventorySlot: inv);
            return new Dizi(
                name: name, 
                strength: str, 
                agility: agi, 
                hp: hp, 
                tp: tp, 
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
            public int Tp { get; private set; }
            public int MaxTp { get; private set; }
            public int Mp { get; private set; }
            public int MaxMp { get; private set; }
            public int Level { get; private set; }
            public int Stamina { get; private set; }
            public Capable Capable { get; private set; }
            public Dictionary<int, int> Condition { get; private set; }

            public Dizi(string name, int strength, int agility, int hp, int tp, int mp, int level, int stamina, Capable capable, Dictionary<int, int> condition)
            {
                Name = name;
                Strength = strength;
                Agility = agility;
                Hp = hp;
                MaxHp = hp;
                Tp = tp;
                MaxTp = tp;
                Mp = mp;
                MaxMp = mp;
                Level = level;
                Stamina = stamina;
                Capable = capable;
                Condition = condition;
            }
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
    }
}