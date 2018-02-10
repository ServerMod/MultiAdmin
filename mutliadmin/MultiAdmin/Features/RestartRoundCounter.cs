using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class RestartRoundCounter : Feature, IEventRoundEnd
    {
        private int count;
        private int restartAfter;

        public RestartRoundCounter(Server server) : base(server)
        {
        }

        public override void Init()
        {
            count = 0;
        }

        public override void OnConfigReload()
        {
            restartAfter = Server.ServerConfig.GetIntValue("RESTART_EVERY_NUM_ROUNDS", -1);
        }

        public void OnRoundEnd()
        {
            if (restartAfter < 0) return;
            count++; 
            if (count > restartAfter) base.Server.SoftRestartServer();
        }


        public override string GetFeatureDescription()
        {
            return "Restarts the server after X num rounds completed.";
        }

        public override string GetFeatureName()
        {
            return "Restart After X Rounds";
        }


    }
}
