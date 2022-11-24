﻿using Server;

namespace HotFix_Project.Managers
{
    internal class GameManager
    {
        private AdventureManager AdventureManager { get; set; } = new AdventureManager();
        private DiziTestManager DiziTestManager { get; set; } = new DiziTestManager();
        private SkillTestManager SkillTestManager { get; set; } = new SkillTestManager();
        private AdvMapTestManager AdvMapTestManager { get; set; } = new AdvMapTestManager();

        public void Init()
        {
            AdventureManager.Init();
            DiziTestManager.Init();
            SkillTestManager.Init();
            AdvMapTestManager.Init();
        }
    }
}