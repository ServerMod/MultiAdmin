using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class MultiAdminInfo : Feature, IEventServerPreStart, ICommand
    {
        private String config;

        public MultiAdminInfo(Server server) : base(server)
        {
        }

        public override void Init()
        {
        }

        public override void OnConfigReload()
        { 
        }

        public void PrintInfo()
        {
            Server.Write("MultiAdmin for SCP: Secret Laboratory made by Courtney (Grover_c13), who is not a girl.", ConsoleColor.DarkMagenta);
            Server.Write("Heavily modified but based off LocalAdmin by Hubert Moszka.", ConsoleColor.DarkMagenta);
            Server.Write("You can request LocalAdmin source code at moszka.hubert@gmail.com.", ConsoleColor.DarkMagenta);
            Server.Write("You can find MultiAdmin source code at https://github.com/Grover-c13/MultiAdmin/.", ConsoleColor.DarkMagenta);
            Server.Write("Released under CC-BY-SA 4.0", ConsoleColor.DarkMagenta);
            Server.Write("Do you have too much money? appreciate multiadmin/servermod? consider sending me some!", ConsoleColor.Gray);
            Server.Write("Paypal: Grover.c13@gmail.com", ConsoleColor.Gray);
        }

        public override string GetFeatureDescription()
        {
            return "Prints the license/author information";
        }

        public override string GetFeatureName()
        {
            return "MutliAdminInfo";
        }


        public void OnServerPreStart()
        {
            PrintInfo();
        }

        public void OnCall(string[] args)
        {
            PrintInfo();
        }

        public string GetCommand()
        {
            return "INFO";
        }

        public bool PassToGame()
        {
            return false;
        }

        public string GetCommandDescription()
        {
            return "Prints license and author information.";
        }


        public string GetUsage()
        {
            return "";
        }
    }
}
