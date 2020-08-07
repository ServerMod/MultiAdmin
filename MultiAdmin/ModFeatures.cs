using System;

namespace MultiAdmin
{
	[Flags]
	public enum ModFeatures
	{
		None = 0,

		// Ex. Feature1 = 1 << 0,
		// Feature2 = 1 << 1,

		All = ~(~0 << 0) // All = ~(~0 << 2)
	}
}
