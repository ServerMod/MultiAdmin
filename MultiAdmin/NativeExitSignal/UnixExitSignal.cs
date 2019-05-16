using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace MultiAdmin.NativeExitSignal
{
	public class UnixExitSignal : IExitSignal
	{
		public event EventHandler Exit;

		private readonly UnixSignal[] signals = {new UnixSignal(Signum.SIGTERM), new UnixSignal(Signum.SIGINT), new UnixSignal(Signum.SIGUSR1)};

		public readonly Thread exitSignalThread;

		public UnixExitSignal()
		{
			exitSignalThread = new Thread(() =>
			{
				// blocking call to wait for any kill signal
				UnixSignal.WaitAny(signals, -1);

				Exit?.Invoke(this, EventArgs.Empty);
			});

			RunListener();
		}

		public void RunListener()
		{
			exitSignalThread.Start();
		}
	}
}
