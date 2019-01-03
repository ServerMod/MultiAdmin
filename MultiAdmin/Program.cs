using System;

namespace MultiAdmin
{
	public static class Program
	{
		private static Server server;

		public static void Write(string message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			if (Utils.IsProcessHandleZero) return;

			Console.ForegroundColor = color;
			message = Server.TimeStamp(message);
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
		}

		private static void OnExit(object sender, EventArgs e)
		{
			// TODO: Cleanup server on exit
			//Console.WriteLine("exit");
			//Console.ReadKey();
		}

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			server = new Server();
			server.StartServer();
		}
	}
}