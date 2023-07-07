using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameClient.System
{
    public class Preloader : MonoBehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Scrollbar _scrollbar;

        private Text MessageText => _messageText;
        private Scrollbar Scrollbar => _scrollbar;


        public void SetMessage(string text) => MessageText.text = text;

        public void SetFinish()
        {
            SetMessage("加载完毕!");
            _scrollbar.size = 1f;
            StartCoroutine(CloseAfterASecond());

            IEnumerator CloseAfterASecond()
            {
                yield return new WaitForSeconds(1);
                gameObject.SetActive(false);
            }
        }

        public void SetProgress(float ratio)
        {
            gameObject.SetActive(true);
            SetMessage($"加载中...{(int)(ratio * 100)}%");
            _scrollbar.size = ratio;
        }
    }
}
