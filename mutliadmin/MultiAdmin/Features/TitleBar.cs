using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class Titlebar : Feature, IEventPlayerConnect, IEventPlayerDisconnect
    {
        private int playerCount;
        private int maxPlayers;

        public Titlebar(Server server) : base(server)
        {
        }


        public override string GetFeatureDescription()
        {
            return "Updates the title bar with instance based information, such as session id and player count.";
        }

        public override string GetFeatureName()
        {
            return "Titlebar";
        }

        public override void Init()
        {
            maxPlayers = Server.ServerConfig.GetIntValue("MAX_PLAYERS", 20);
            playerCount = -1; // -1 for the "server" player
            UpdateTitlebar();
        }

        public void OnPlayerConnect(String name)
        {
            playerCount++;
            UpdateTitlebar();
        }

        public void OnPlayerDisconnect(String name)
        {
            playerCount--;
            UpdateTitlebar();
        }

        public void UpdateTitlebar()
        {
            Console.Title = "SCP:SL MutliAdmin | Config: " + Server.ConfigKey + " | Session ID:" + Server.GetSessionId() + " | " + playerCount + "/" + maxPlayers;
        }
    }
}
