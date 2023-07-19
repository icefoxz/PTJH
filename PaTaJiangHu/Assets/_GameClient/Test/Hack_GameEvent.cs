using AOT.Core;
using GameClient.Models;
using GameClient.System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameClient.Test
{
    internal class Hack_GameEvent : MonoBehaviour
    {
        private Recruiter Recruiter => Game.World.Recruiter;

        [Button(ButtonSizes.Small, Name = "弟子到访事件")]
        [GUIColor("cyan")]
        public void VisitorCome() => Recruiter.NewVisitor();
    }
}