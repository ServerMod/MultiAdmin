using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Features
{
    class ConfigReload : Feature, ICommand
    {
        Boolean pass;

        public ConfigReload(Server server) : base(server)
        {
        }

        public string GetCommand()
        {
            return "CONFIG";
        }

        public string GetCommandDescription()
        {
            return "Handles reloading the config";
        }

        public override string GetFeatureDescription()
        {
            return "Config reload will swap configs";
        }

        public override string GetFeatureName()
        {
            return "Config reload";
        }

        public string GetUsage()
        {
            return "<reload>";
        }

        public override void Init()
        {
            pass = true;
        }

        public void OnCall(string[] args)
        {
            if (args[1].Equals("reload"))
            {
                Server.SwapConfigs();
                pass = true;
            }

        }

        public bool PassToGame()
        {
            return pass;
        }
    }
}
