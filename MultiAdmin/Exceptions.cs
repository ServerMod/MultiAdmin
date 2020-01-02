using System;

namespace MultiAdmin
{
	public static class Exceptions
	{
		[Serializable]
		public class ServerNotRunningException : Exception
		{
			public ServerNotRunningException() : base("The server is not running")
			{
			}
		}

		[Serializable]
		public class ServerAlreadyRunningException : Exception
		{
			public ServerAlreadyRunningException() : base("The server is already running")
			{
			}
		}
	}
}
