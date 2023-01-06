using System;
using System.Collections;
using System.Collections.Generic;
using Server.Configs.Adventures;
using UnityEngine;

namespace _GameClient.Models
{
    /// <summary>
    /// 历练模型
    /// </summary>
    public class DiziAdventure
    {
        private readonly List<string> _messages = new List<string>();
        public string Title { get; private set; } 
        public string CurrentPlace { get; private set; }
        public DiziBag Bag { get; set; }
        public IReadOnlyList<string> Messages => _messages;
        //DiziAdvController
        private int AdventureCoId { get; set; }
        
        public DiziAdventure(Dizi dizi)
        {
            Bag = new DiziBag(dizi.Capable.Bag);
        }

        public void StartAdventure()
        {
            AdventureCoId = Game.CoService.RunCo(AdventureRoutine());
        }

        private IEnumerator AdventureRoutine()
        {
            while (true)
            {

                yield return new WaitForSeconds(1);
            }
        }


        public void ReceiveMessage(string message) => _messages.Add(message);

        //弟子背包
        public class DiziBag
        {
            private IGameItem[] _items;
            public IGameItem[] Items => _items;

            public DiziBag(int length)
            {
                _items = new IGameItem[length];
            }

            public DiziBag(IGameItem[] items)
            {
                _items = items;
            }

        }
    }
}