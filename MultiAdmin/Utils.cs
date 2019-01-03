using System;
using System.Diagnostics;

namespace MultiAdmin
{
	internal class Utils
	{
		public static string DateTime => System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm");
		public static string UnixTime => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

		public static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;
		public static bool IsMac => Environment.OSVersion.Platform == PlatformID.MacOSX;

		public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT ||
		                                Environment.OSVersion.Platform == PlatformID.Win32S ||
		                                Environment.OSVersion.Platform == PlatformID.Win32Windows ||
		                                Environment.OSVersion.Platform == PlatformID.WinCE;

		// Skip process handle check if using Unix
		public static bool IsProcessHandleZero => !IsUnix && Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero;
	}
}