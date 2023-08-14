using System;
using System.Collections;
using AOT.Core;
using AOT.Core.Dizi;
using AOT.Core.Systems.Coroutines;
using AOT.Utls;
using GameClient.Controllers;
using GameClient.SoScripts;
using GameClient.System;
using UnityEngine;

namespace GameClient.Models
{
    public class Recruiter : ModelBase
    {
        protected override string LogPrefix { get; } = "招募器";
        private Config.Recruit Recruit => Game.Config.RecruitCfg;
        public Visitor CurrentVisitor { get; private set; }
        private RecruitController RecruitController => Game.Controllers.Get<RecruitController>();
        private ICoroutineInstance RecruiterServiceCo { get; set; } // 招募器服务协程
        private RecruiterEditor editor;
        public Recruiter()
        {
            RecruiterServiceCo = Game.CoService.RunCo(RecruiterService(), LogPrefix,"招募倒数器");
            editor = RecruiterServiceCo.AddComponent<RecruiterEditor>();
            NextRecruitTime = SysTime.UnixTicksFromNow(TimeSpan.FromSeconds(30));
        }

        public long NextRecruitTime { get; set; } // 下次招募时间

        private IEnumerator RecruiterService()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
#if UNITY_EDITOR
                if (editor.CountdownSecs <= 0)
                {
                    NewVisitor();
                    yield return null;
                }
#endif
                var timeSpan = SysTime.CompareUnixNow(NextRecruitTime);
                var countdown = (int)timeSpan.TotalSeconds;
                editor.CountdownSecs = countdown;
                if (countdown <= 0) NewVisitor();
            }
        }

        public void NewVisitor()
        {
            (IVisitorDizi visitor, Dizi dizi) = RecruitController.GenerateVisitor();
            CurrentVisitor = new Visitor(visitor, dizi);
            var interval = (long)TimeSpan.FromMinutes(Recruit.Config.VisitorIntervalMins).TotalMilliseconds;
            var now = SysTime.UnixNow;
            NextRecruitTime = now + interval; // Update NextRecruitTime
            SendEvent(EventString.Recruit_VisitorDizi);
        }

        public void RemoveVisitor()
        {
            var dizi = CurrentVisitor.Dizi;
            CurrentVisitor = null;
            dizi.Dispose();
            SendEvent(EventString.Recruit_VisitorRemove);
        }

        public class Visitor 
        {
            public IVisitorDizi Set { get; set; }
            public Dizi Dizi { get; set; }

            public Visitor(IVisitorDizi set, Dizi dizi)
            {
                Set = set;
                Dizi = dizi;
            }
        }

        private class RecruiterEditor : MonoBehaviour
        {
            public long LastRecruitTime;
            public int CountdownSecs;
        }
    }
}