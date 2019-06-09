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
		/// <param name="parentConfigRegister">The <see cref="InheritableConfigRegister"/> to inherit unset config values from.</param>
		protected InheritableConfigRegister(InheritableConfigRegister parentConfigRegister = null)
		{
			ParentConfigRegister = parentConfigRegister;
		}

		/// <summary>
		/// The parent <see cref="InheritableConfigRegister"/> to inherit from.
		/// </summary>
		public InheritableConfigRegister ParentConfigRegister { get; protected set; }

		/// <summary>
		/// Returns whether <paramref name="configEntry"/> should be inherited from the parent <see cref="InheritableConfigRegister"/>.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to decide whether to inherit.</param>
		public abstract bool ShouldInheritConfigEntry(ConfigEntry configEntry);

		/// <summary>
		/// Updates the value of <paramref name="configEntry"/> which could be provided by another <see cref="InheritableConfigRegister"/>.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be assigned a value.</param>
		public abstract void UpdateConfigValueInheritable(ConfigEntry configEntry);

		/// <summary>
		/// Updates the value of <paramref name="configEntry"/> from this <see cref="InheritableConfigRegister"/> if the <see cref="ParentConfigRegister"/> is null or if <seealso cref="ShouldInheritConfigEntry"/> returns true.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be assigned a value.</param>
		public override void UpdateConfigValue(ConfigEntry configEntry)
		{
			if (configEntry != null && configEntry.Inherit && ParentConfigRegister != null && ShouldInheritConfigEntry(configEntry))
			{
				ParentConfigRegister.UpdateConfigValue(configEntry);
			}
			else
			{
				UpdateConfigValueInheritable(configEntry);
			}
		}

		/// <summary>
		/// Returns an array of the hierarchy of <see cref="InheritableConfigRegister"/>s.
		/// </summary>
		/// <param name="highestToLowest">Whether to order the returned array from highest <see cref="InheritableConfigRegister"/> in the hierarchy to the lowest.</param>
		public InheritableConfigRegister[] GetConfigRegisterHierarchy(bool highestToLowest = true)
		{
			List<InheritableConfigRegister> configRegisterHierarchy = new List<InheritableConfigRegister>();

			InheritableConfigRegister configRegister = this;
			while (configRegister != null && !configRegisterHierarchy.Contains(configRegister))
			{
				configRegisterHierarchy.Add(configRegister);
				configRegister = configRegister.ParentConfigRegister;
			}

			if (highestToLowest)
				configRegisterHierarchy.Reverse();

			return configRegisterHierarchy.ToArray();
		}
	}
}
