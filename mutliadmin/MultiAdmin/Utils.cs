using System;

namespace MultiAdmin.MultiAdmin
{
	internal class Utils
	{
		public static string GetDate()
		{
			return DateTime.Now.ToString("yyyy-MM-dd_HH_mm");
		}
	}
}