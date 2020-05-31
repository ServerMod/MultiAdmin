using System;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class InactivityShutdown : Feature, IEventRoundStart, IEventWaitingForPlayers, IEventTick
	{
		private DateTime queueStartTime;
		private int waitFor;
		private bool waiting;

		public InactivityShutdown(Server server) : base(server)
		{
		}

		public void OnWaitingForPlayers()
		{
			queueStartTime = DateTime.Now;
			waiting = true;
		}

		public void OnRoundStart()
		{
			waiting = false;
		}

		public void OnTick()
		{
			if (waitFor > 0 && waiting)
			{
				int elapsed = (DateTime.Now - queueStartTime).Seconds;

				if (elapsed >= waitFor)
				{
					Server.Write("Server has been inactive for " + waitFor + " seconds, shutting down");
					Server.StopServer();
				}
			}
		}

		public override void Init()
		{
			queueStartTime = DateTime.Now;
		}

		public override void OnConfigReload()
		{
			waitFor = Server.ServerConfig.ShutdownWhenEmptyFor.Value;
		}


		public override string GetFeatureDescription()
		{
			return "Stops the server after a period inactivity [Requires ServerMod]";
		}

		public override string GetFeatureName()
		{
			return "Stop Server When Inactive";
		}
	}
}
