using System;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class MultiAdminInfo : Feature, IEventServerPreStart, ICommand
	{
		public MultiAdminInfo(Server server) : base(server)
		{
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
			return GetFeatureDescription();
		}

		public string GetUsage()
		{
			return string.Empty;
		}

		public void OnServerPreStart()
		{
			PrintInfo();
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
		}

		public void PrintInfo()
		{
			Server.Write("MultiAdmin (https://github.com/Grover-c13/MultiAdmin/)", ConsoleColor.DarkMagenta);
			Server.Write("Released under CC-BY-SA 4.0", ConsoleColor.DarkMagenta);
		}

		public override string GetFeatureDescription()
		{
			return "Prints MultiAdmin license information";
		}

		public override string GetFeatureName()
		{
			return "MultiAdminInfo";
		}
	}
}