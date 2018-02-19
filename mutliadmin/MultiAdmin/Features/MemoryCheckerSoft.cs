using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class MemoryCheckerSoft : Feature, IEventTick, IEventRoundEnd
    {
        private int lowMb;
        private int tickCount;
        private Boolean restart;
        private Boolean warn;
        public MemoryCheckerSoft(Server server) : base(server)
        {
        }

        public override void Init()
        {
            tickCount = 0;
            restart = false;
            warn = false;
        }

        public override string GetFeatureDescription()
        {
            return "Restarts the server if the working memory becomes too low at the end of the round";
        }

        public override string GetFeatureName()
        {
            return "Restart On Low Memory at the end of the round";
        }

        public void OnTick()
        {
            Server.GetGameProccess().Refresh();
            long workingMemory = Server.GetGameProccess().WorkingSet64 / 1048576L; // process memory in MB
            long memoryLeft = 2048 - workingMemory; // 32 bit limited to 2GB

            if (memoryLeft < lowMb)
            {
                if (!warn) Server.Write("Warning: program is running low on memory (" + memoryLeft + " MB left) the server will restart at the end of the round if it continues", ConsoleColor.Red);
                warn = true;
                tickCount++;
            }
            else
            {
                warn = false;
                tickCount = 0;
            }

            if (tickCount == 10)
            {
                restart = true;
                Server.Write("Restarting the server at end of the round due to low memory");
            }
 
        }

        public override void OnConfigReload()
        {
            lowMb = Server.ServerConfig.GetIntValue("RESTART_LOW_MEMORY_ROUNDEND", 1800);
        }

        public void OnRoundEnd()
        {
            if (restart) base.Server.SoftRestartServer();
        }
    }
}
