using System;

namespace MultiAdmin.NativeExitSignal
{
	public interface IExitSignal
	{
		event EventHandler? Exit;
	}
}
