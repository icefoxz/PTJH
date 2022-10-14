using Server.Controllers.Adventures;
using Server;
using System;
using System.Collections.Generic;
using UnityEngine;


public class TestDizi : MonoBehaviour
{
    public void OpenRecruitWindow()
    {
        Game.MessagingManager.Invoke(EventString.Test_DiziRecruit, null);
    }

    public void OpenStaminaWindow()
    {
        Game.MessagingManager.Invoke(EventString.Test_StaminaWindow, null);
    }
}