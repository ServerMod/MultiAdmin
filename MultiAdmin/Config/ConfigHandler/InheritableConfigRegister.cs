using System.Collections.Generic;

namespace MultiAdmin.Config.ConfigHandler
{
	/// <summary>
	/// A <see cref="ConfigEntry"/> register. This abstract class provides a base for a config handler implementation and <see cref="InheritableConfigRegister"/> inheritance.
	/// </summary>
	public abstract class InheritableConfigRegister : ConfigRegister
	{
		/// <summary>
		/// Creates an <see cref="InheritableConfigRegister"/> with the parent <paramref name="parentConfigRegister"/> to inherit unset config values from.
		/// </summary>
		/// <param name="parentConfigRegister">The <see cref="ConfigRegister"/> to inherit unset config values from.</param>
		protected InheritableConfigRegister(ConfigRegister? parentConfigRegister = null)
		{
			ParentConfigRegister = parentConfigRegister;
		}

		/// <summary>
		/// The parent <see cref="ConfigRegister"/> to inherit from.
		/// </summary>
		public ConfigRegister? ParentConfigRegister { get; protected set; }

		/// <summary>
		/// Returns whether <paramref name="configEntry"/> should be inherited from the parent <see cref="ConfigRegister"/>.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to decide whether to inherit.</param>
		public abstract bool ShouldInheritConfigEntry(ConfigEntry configEntry);

		/// <summary>
		/// Updates the value of <paramref name="configEntry"/>.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be assigned a value.</param>
		public abstract void UpdateConfigValueInheritable(ConfigEntry configEntry);

		/// <summary>
		/// Updates the value of <paramref name="configEntry"/> from this <see cref="InheritableConfigRegister"/> if the <see cref="ParentConfigRegister"/> is null or if <seealso cref="ShouldInheritConfigEntry"/> returns false.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be assigned a value.</param>
		public override void UpdateConfigValue(ConfigEntry configEntry)
		{
			if (configEntry != null && configEntry.Inherit && ParentConfigRegister != null &&
				ShouldInheritConfigEntry(configEntry))
			{
				ParentConfigRegister.UpdateConfigValue(configEntry);
			}
			else if (configEntry != null)
			{
				UpdateConfigValueInheritable(configEntry);
			}
		}

		/// <summary>
		/// Returns an array of the hierarchy of <see cref="ConfigRegister"/>s.
		/// </summary>
		/// <param name="highestToLowest">Whether to order the returned array from highest <see cref="ConfigRegister"/> in the hierarchy to the lowest.</param>
		public ConfigRegister[] GetConfigRegisterHierarchy(bool highestToLowest = true)
		{
			List<ConfigRegister> configRegisterHierarchy = new();

			ConfigRegister configRegister = this;
			while (!configRegisterHierarchy.Contains(configRegister))
			{
				configRegisterHierarchy.Add(configRegister);

				// If there's another InheritableConfigRegister as a parent, then get the parent of that, otherwise, break the loop as there are no more parents
				if (configRegister is InheritableConfigRegister inheritableConfigRegister && inheritableConfigRegister.ParentConfigRegister != null)
				{
					configRegister = inheritableConfigRegister.ParentConfigRegister;
				}
				else
				{
					break;
				}
			}

			if (highestToLowest)
				configRegisterHierarchy.Reverse();

			return configRegisterHierarchy.ToArray();
		}
	}
}
