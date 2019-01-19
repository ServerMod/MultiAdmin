using System;
using System.Collections.Generic;
using System.IO;
//using YamlDotNet.RepresentationModel;

namespace MultiAdmin
{
	public class OldConfigException : Exception
	{
		public OldConfigException(string message) : base(message)
		{
		}
	}


	public class Config
	{
		public YamlConfig config;
		private string configFile;

		public Config(string configFile)
		{
			this.configFile = configFile;
			Reload();
		}

		public void Reload()
		{
			if (!Directory.Exists(FileManager.AppFolder))
			{
				Directory.CreateDirectory(FileManager.AppFolder);
			}

			config = new YamlConfig(configFile);
		}

		public string GetConfigFilePath()
		{
			return this.configFile;
		}
	}
}
