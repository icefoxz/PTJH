using AOT._AOT.Views.Abstract;
using AOT._AOT.Views.BaseUis;
using UnityEngine;

namespace GameClient.Test
{
    public class TestAdvPlayer : MonoBehaviour
    {
        [SerializeField] private View _view;
        private IView View => _view;

        public void SetViewSize(int height)
        {
            var r = View.RectTransform.rect;
            _view.RectTransform.SetHeight(height);
        }
    }
}
