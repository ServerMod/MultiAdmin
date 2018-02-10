using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class InactivityShutdown : Feature, IEventRoundStart, IEventRoundEnd, IEventTick
    {
        private Boolean waiting;
        private long roundEndTime;
        private int waitFor;

        public InactivityShutdown(Server server) : base(server)
        {
        }

        public override void Init()
        {
            roundEndTime = Utils.GetUnixTime();
        }

        public override void OnConfigReload()
        {
            waitFor = Server.ServerConfig.GetIntValue("SHUTDOWN_ONCE_EMPTY_FOR", -1);
        }

        public void OnRoundEnd()
        {
            roundEndTime = Utils.GetUnixTime();
            waiting = true;
        }


        public override string GetFeatureDescription()
        {
            return "Stops the server after a period inactivity.";
        }

        public override string GetFeatureName()
        {
            return "Stop Server once Inactive";
        }

        public void OnRoundStart()
        {
            waiting = false;
        }

        public void OnTick()
        {
            if (waitFor > 0)
            {
                long elapsed = Utils.GetUnixTime() - roundEndTime;

                if (elapsed >= waitFor)
                {
                    Server.Write("Server has been inactive for " + waitFor + " seconds, shutting down");
                    Server.StopServer();
                }
            }
        }
    }
}
