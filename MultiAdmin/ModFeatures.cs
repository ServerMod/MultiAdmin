using System;

namespace MultiAdmin
{
	[Flags]
	public enum ModFeatures
	{
		None = 0,

		CustomEvents = 1 << 0,

		All = ~(~0 << 1)
	}
}
