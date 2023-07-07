using UnityEngine;
using UnityEngine.UI;

namespace AOT._AOT
{
    public class Test_LoadDll : MonoBehaviour
    {
        [SerializeField] private Text _text;
        private static Text Text { get; set; }
        void Start()
        {
            Text = _text;
            LoadDll.LoadMetadataForAOTAssemblies();
            var loadDll = new LoadDll("HotUpdate");
            loadDll.StartHotReloadAssembly("Hello", "Run");
        }

        public static void SetMessage(string msg) => Text.text = msg;

    }
}