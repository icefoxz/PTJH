using UnityEngine;

namespace AOT._AOT.Utls
{
    public class DataTable : MonoBehaviour
    {
        [SerializeField] private TextAsset _csvFile;
    

        private void Start()
        {
            var data = Csv.Read(_csvFile);
            //var dic = ReadData(data);
        }

        //private T ReadData<T>(List<Dictionary<string, object>> data) where T : IReadOnlyDictionary<int,T>
        //{
        //}
    }
}