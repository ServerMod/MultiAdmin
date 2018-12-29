using System;
using System.IO;

namespace MultiAdmin.MultiAdmin
{
	public class OldConfigException : Exception
	{
		public OldConfigException(string message) : base(message)
		{
		}
	}


	public class Config
	{
		private readonly string configFile;
		public YamlConfig config;

		public Config(string configFile)
		{
			this.configFile = configFile;
			Reload();
		}

		public void Reload()
		{
			if (!Directory.Exists(FileManager.AppFolder)) Directory.CreateDirectory(FileManager.AppFolder);

			config = new YamlConfig(configFile);
		}
	}
}