using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class Titlebar : Feature, IEventPlayerConnect, IEventPlayerDisconnect, IEventServerStart
    {
        private int playerCount;
        private int maxPlayers;

        public Titlebar(Server server) : base(server)
        {
        }


        public override string GetFeatureDescription()
        {
            return "Updates the title bar with instance based information, such as session id and player count. (Requires servermod to function fully)";
        }

        public override string GetFeatureName()
        {
            return "Titlebar";
        }

        public override void Init()
        {
            maxPlayers = Server.ServerConfig.GetIntValue("MAX_PLAYERS", 20);
            playerCount = -1; // -1 for the "server" player, once the server starts this will increase to 0.
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

        public void OnServerStart()
        {
            UpdateTitlebar();
        }

        public void UpdateTitlebar()
        {
            var smod = "";
            if (Server.HasServerMod)
            {
                smod = "ServerMod Version " + Server.ServerModVersion;
            }
            var displayPlayerCount = playerCount;
            if (playerCount == -1) displayPlayerCount = 0;
            Console.Title = "SCP:SL MutliAdmin " + Server.MA_VERSION + " | Config: " + Server.ConfigKey + " | Session ID:" + Server.GetSessionId() + " | " + displayPlayerCount + "/" + maxPlayers + " | " + smod;
        }
    }
}
