using System;
using System.Runtime.InteropServices;

namespace MultiAdmin.NativeExitSignal
{
	public class WinExitSignal : IExitSignal
	{
		public event EventHandler? Exit;

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

		// A delegate type to be used as the handler routine
		// for SetConsoleCtrlHandler.
		public delegate bool HandlerRoutine(CtrlTypes ctrlType);

		// An enumerated type for the control messages
		// sent to the handler routine.
		public enum CtrlTypes
		{
			CtrlCEvent = 0,
			CtrlBreakEvent = 1,
			CtrlCloseEvent = 2,
			CtrlLogoffEvent = 5,
			CtrlShutdownEvent = 6
		}

		/// <summary>
		/// Need this as a member variable to avoid it being garbage collected.
		/// </summary>
		private readonly HandlerRoutine mHr;

		public WinExitSignal()
		{
			mHr = ConsoleCtrlCheck;

			SetConsoleCtrlHandler(mHr, true);
		}

		/// <summary>
		/// Handle the ctrl types
		/// </summary>
		/// <param name="ctrlType"></param>
		/// <returns></returns>
		private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
		{
			switch (ctrlType)
			{
				case CtrlTypes.CtrlCEvent:
				case CtrlTypes.CtrlBreakEvent:
				case CtrlTypes.CtrlCloseEvent:
				case CtrlTypes.CtrlLogoffEvent:
				case CtrlTypes.CtrlShutdownEvent:
					Exit?.Invoke(this, EventArgs.Empty);
					break;
			}

			return true;
		}
	}
}
