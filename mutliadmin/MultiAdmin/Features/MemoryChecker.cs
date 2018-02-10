using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class MemoryChecker : Feature, IEventTick
    {
        private int lowMb;
        private int tickCount;
        public MemoryChecker(Server server) : base(server)
        {
        }

        public override void Init()
        {
            lowMb = Server.ServerConfig.GetIntValue("SHUTDOWN_LOW_MEMORY", 400);
            tickCount = 0;
        }

        public override string GetFeatureDescription()
        {
            return "Restarts the server if the working memory becomes too low";
        }

        public override string GetFeatureName()
        {
            return "Restart On Low Memory";
        }

        public void OnTick()
        {
            long workingMemory = Server.GetGameProccess().WorkingSet64 / 1048576L;
            Boolean runningFor3Mins = (Server.GetGameProccess().StartTime.AddMinutes(3.0) < DateTime.Now);
            if (workingMemory < lowMb && runningFor3Mins)
            {
                tickCount++;
            }
            else
            {
                tickCount = 0;
            }

            if (tickCount == 5)
            {
                Server.RestartServer();
            }
 
        }
    }
}
