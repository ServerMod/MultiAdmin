using System;

namespace MultiAdmin
{
	public static class Exceptions
	{
		public class ServerNotRunningException : Exception
		{
			public ServerNotRunningException() : base("The server is not running")
			{
			}
		}

		public class ServerAlreadyRunningException : Exception
		{
			public ServerAlreadyRunningException() : base("The server is already running")
			{
			}
		}
	}
}
