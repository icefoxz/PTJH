using System.Collections;
using System.Collections.Generic;
using AOT.Core.Systems;
using GameClient.SoScripts;
using GameClient.System;
using UnityEngine;

namespace GameClient
{
    public class TestGame : MonoBehaviour
    {
        [SerializeField]private ConfigSo _config;
        void Start()
        {
            var game = gameObject.AddComponent<Game>();
            game.InitTest(_config.Config);
            Debug.Log("TestGame Start");
        }
    }
}
