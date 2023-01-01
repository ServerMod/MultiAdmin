using System;

namespace MultiAdmin.Features
{
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
			return "";
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
			Server.Write(
				$"{nameof(MultiAdmin)} v{Program.MaVersion} (https://github.com/ServerMod/MultiAdmin/)\nReleased under MIT License Copyright © Grover 2021",
				ConsoleColor.DarkMagenta);
		}

		public override string GetFeatureDescription()
		{
			return $"Prints {nameof(MultiAdmin)} license and version information";
		}

		public override string GetFeatureName()
		{
			return "MultiAdminInfo";
		}
	}
}
