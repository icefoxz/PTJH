using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Analytics;
using Utls;
using Random = UnityEngine.Random;

namespace NameM
{
    /// <summary>
    /// 名字生成器
    /// </summary>
    public class NameGen : MonoBehaviour
    {
        public static NameGen Instance { get; private set; }
        [SerializeField]private TextAsset _surname;
        [SerializeField]private TextAsset _mName;
        [SerializeField]private TextAsset _fName;
        private static string[] Surnames { get; set; }
        private static string[] MNames { get; set; }
        private static string[] FNames { get; set; }
        void Awake()
        {
            if (Instance != null)
                throw new NotImplementedException("Multiple init!");
            Instance = this;
            Surnames = JsonConvert.DeserializeObject<string[]>(_surname.text);
            MNames = JsonConvert.DeserializeObject<string[]>(_mName.text);
            FNames = JsonConvert.DeserializeObject<string[]>(_fName.text);
        }

        public static Name GenName() => GenName((Gender)Sys.Random.Next(2));
        public static Name GenName(Gender gender,int word = -1) =>
            gender switch
            {
                Gender.Male => GenMaleName(word),
                Gender.Female => GenFemaleName(word),
                Gender.Unknown => GenMaleName(word),
                _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
            };

        public static Name GenMaleName(int word = 0) => GenName(MNames, word);
        public static Name GenFemaleName(int word = 0) => GenName(FNames, word);

        private static Name GenName(string[] names, int word = -1)
        {
            if (word <= 0)
                word = Random.Range(1, 3);
            var surname = Surnames.RandomPick();
            var lastname = new StringBuilder();
            for (int i = 0; i < word; i++) lastname.Append(names.RandomPick());
            return new Name(surname, lastname.ToString());
        }
    }
}