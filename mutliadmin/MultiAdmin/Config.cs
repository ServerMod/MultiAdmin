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
		private String config_file;

		public Config(String config_file)
		{
			this.config_file = config_file;
			Reload();
		}

		public void Reload()
		{
			if (!Directory.Exists(FileManager.AppFolder))
			{
				Directory.CreateDirectory(FileManager.AppFolder);
			}

			config = new YamlConfig(config_file);
		}
	}
}
