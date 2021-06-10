#if LINUX
using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace MultiAdmin.NativeExitSignal
{
	public class UnixExitSignal : IExitSignal
	{
		public event EventHandler Exit;

		private static readonly UnixSignal[] Signals = {
			new UnixSignal(Signum.SIGINT),  // CTRL + C pressed
			new UnixSignal(Signum.SIGTERM), // Sending KILL
			new UnixSignal(Signum.SIGUSR1),
			new UnixSignal(Signum.SIGUSR2),
			new UnixSignal(Signum.SIGHUP)   // Terminal is closed
		};

		public UnixExitSignal()
		{
			new Thread(() =>
			{
				// blocking call to wait for any kill signal
				UnixSignal.WaitAny(Signals, -1);

				Exit?.Invoke(this, EventArgs.Empty);
			}).Start();
		}
	}
}
#endif
