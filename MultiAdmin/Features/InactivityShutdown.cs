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
			queueStartTime = DateTime.UtcNow;
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
				int elapsed = (DateTime.UtcNow - queueStartTime).Seconds;

				if (elapsed >= waitFor)
				{
					Server.Write("Server has been inactive for " + waitFor + " seconds, shutting down");
					Server.StopServer();
				}
			}
		}

		public override void Init()
		{
			queueStartTime = DateTime.UtcNow;
		}

		public override void OnConfigReload()
		{
			waitFor = Server.ServerConfig.ShutdownWhenEmptyFor;
		}


		public override string GetFeatureDescription()
		{
			return "Stops the server after a period inactivity";
		}

		public override string GetFeatureName()
		{
			return "Stop Server When Inactive";
		}
	}
}