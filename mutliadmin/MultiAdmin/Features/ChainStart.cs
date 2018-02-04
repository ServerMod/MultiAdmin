using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
    class ChainStart : Feature, IEventServerStart
    {
        private String config;

        public ChainStart(Server server) : base(server)
        {
        }

        public override void Init()
        {
        }

        public override string GetFeatureDescription()
        {
            return "Automatically starts the next server after the first one is done loading.";
        }

        public override string GetFeatureName()
        {
            return "ChainStart";
        }


        public void OnServerStart()
        {
            if (!(String.IsNullOrWhiteSpace(Server.ConfigChain) || Server.ConfigChain.Trim().Equals("\"\"")))
            {
                Server.Write("Starting next with chained config:" + Server.ConfigChain);
                Server.NewInstance(Server.ConfigChain);
            }
        }
    }
}
