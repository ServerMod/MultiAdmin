using System;
using System.Diagnostics;
using System.Linq;

namespace MultiAdmin.MultiAdmin
{
	public static class Program
	{
		private static string configKey;
		private static string configChain;
		private static Server server;

		public static void Write(string message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			if (!Utils.SkipProcessHandle() && Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero) return;

			Console.ForegroundColor = color;
			message = Server.TimeStamp(message);
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
		}

		public static bool StartHandleConfigs(string[] args)
		{
			bool hasServerToStart = false;
			if (args.Length > 0)
			{
				configKey = args[0];
				Write("Starting this instance with config directory:" + configKey);
				// chain the rest
				string[] newArgs = args.Skip(1).Take(args.Length - 1).ToArray();
				configChain = "\"" + string.Join("\" \"", newArgs).Trim() + "\"";
			}
			else
			{
				// Either there is no "servers" folder or it is empty, and starting a normal server
				hasServerToStart = true;
				Write("Using default server mode", ConsoleColor.Green);
			}

			if (!hasServerToStart)
				Write("All servers are set to manual start! You should have at least one config that auto starts.",
					ConsoleColor.Red);

			return hasServerToStart;
		}

		private static void OnExit(object sender, EventArgs e)
		{
			Console.WriteLine("exit");
			Console.ReadKey();
		}

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			configChain = string.Empty;
			if (StartHandleConfigs(args))
				server = new Server();
			else
				Console.ReadKey();
		}
	}
}