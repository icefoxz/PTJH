using UnityEngine;

namespace Test
{
    public class TestDizi : MonoBehaviour
    {
        public void OpenRecruitWindow()
        {
            Game.MessagingManager.Send(EventString.Test_DiziRecruit, string.Empty);
        }

        public void OpenStaminaWindow()
        {
            Game.MessagingManager.Send(EventString.Test_StaminaWindow, string.Empty);
        }
    }
}