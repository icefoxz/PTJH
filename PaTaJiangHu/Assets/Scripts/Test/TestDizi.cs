using Server.Controllers.Adventures;
using Server;
using System;
using System.Collections.Generic;
using UnityEngine;


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