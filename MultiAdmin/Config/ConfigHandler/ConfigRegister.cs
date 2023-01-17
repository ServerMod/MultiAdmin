using System.Collections.Generic;

namespace MultiAdmin.Config.ConfigHandler
{
	/// <summary>
	/// A <see cref="ConfigEntry"/> register. This abstract class provides a base for a config handler implementation.
	/// </summary>
	public abstract class ConfigRegister
	{
		/// <summary>
		/// A list of registered <see cref="ConfigEntry"/>s.
		/// </summary>
		protected readonly List<ConfigEntry> registeredConfigs = new();

		/// <summary>
		/// Returns an array of registered <see cref="ConfigEntry"/>s.
		/// </summary>
		public ConfigEntry[] GetRegisteredConfigs()
		{
			return registeredConfigs.ToArray();
		}

		/// <summary>
		/// Returns the first <see cref="ConfigEntry"/> with a key matching <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key of the <see cref="ConfigEntry"/> to retrieve.</param>
		public ConfigEntry? GetRegisteredConfig(string key)
		{
			if (string.IsNullOrEmpty(key))
				return null;

			key = key.ToLower();

			foreach (ConfigEntry registeredConfig in registeredConfigs)
			{
				if (key == registeredConfig.Key.ToLower())
					return registeredConfig;
			}

			return null;
		}

		/// <summary>
		/// Registers <paramref name="configEntry"/> into the <see cref="ConfigRegister"/> to be assigned a value.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be registered.</param>
		/// <param name="updateValue">Whether to update the value of the config after registration.</param>
		public void RegisterConfig(ConfigEntry configEntry, bool updateValue = true)
		{
			if (string.IsNullOrEmpty(configEntry.Key))
				return;

			registeredConfigs.Add(configEntry);

			if (updateValue)
				UpdateConfigValue(configEntry);
		}

		/// <summary>
		/// Registers <paramref name="configEntries"/> into the <see cref="ConfigRegister"/> to be assigned values.
		/// </summary>
		/// <param name="configEntries">The <see cref="ConfigEntry"/>s to be registered.</param>
		/// <param name="updateValue">Whether to update the value of the config after registration.</param>
		public void RegisterConfigs(ConfigEntry[] configEntries, bool updateValue = true)
		{
			foreach (ConfigEntry configEntry in configEntries)
			{
				RegisterConfig(configEntry, updateValue);
			}
		}

		/// <summary>
		/// Un-registers <paramref name="configEntry"/> from the <see cref="ConfigRegister"/>.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be un-registered.</param>
		public void UnRegisterConfig(ConfigEntry configEntry)
		{
			if (string.IsNullOrEmpty(configEntry.Key))
				return;

			registeredConfigs.Remove(configEntry);
		}

		/// <summary>
		/// Un-registers the <see cref="ConfigEntry"/> linked to the given <paramref name="key"/> from the <see cref="ConfigRegister"/>.
		/// </summary>
		/// <param name="key">The key of the <see cref="ConfigEntry"/> to be un-registered.</param>
		public void UnRegisterConfig(string key)
		{
			ConfigEntry? entry = GetRegisteredConfig(key);
			if (entry != null)
				UnRegisterConfig(entry);
		}

		/// <summary>
		/// Un-registers <paramref name="configEntries"/> from the <see cref="ConfigRegister"/>.
		/// </summary>
		/// <param name="configEntries">The <see cref="ConfigEntry"/>s to be un-registered.</param>
		public void UnRegisterConfigs(params ConfigEntry[] configEntries)
		{
			foreach (ConfigEntry configEntry in configEntries)
			{
				UnRegisterConfig(configEntry);
			}
		}

		/// <summary>
		/// Un-registers the <see cref="ConfigEntry"/>s linked to the given <paramref name="keys"/> from the <see cref="ConfigRegister"/>.
		/// </summary>
		/// <param name="keys">The keys of the <see cref="ConfigEntry"/>s to be un-registered.</param>
		public void UnRegisterConfigs(params string[] keys)
		{
			foreach (string key in keys)
			{
				UnRegisterConfig(key);
			}
		}

		/// <summary>
		/// Un-registers all registered <see cref="ConfigEntry"/>s from the <see cref="ConfigRegister"/>.
		/// </summary>
		public void UnRegisterConfigs()
		{
			foreach (ConfigEntry configEntry in registeredConfigs)
			{
				UnRegisterConfig(configEntry);
			}
		}

		/// <summary>
		/// Updates the value of <paramref name="configEntry"/>.
		/// </summary>
		/// <param name="configEntry">The <see cref="ConfigEntry"/> to be assigned a value.</param>
		public abstract void UpdateConfigValue(ConfigEntry configEntry);

		/// <summary>
		/// Updates the values of the <paramref name="configEntries"/>.
		/// </summary>
		/// <param name="configEntries">The <see cref="ConfigEntry"/>s to be assigned values.</param>
		public void UpdateConfigValues(params ConfigEntry[] configEntries)
		{
			foreach (ConfigEntry configEntry in configEntries)
			{
				UpdateConfigValue(configEntry);
			}
		}

		/// <summary>
		/// Updates the values of the registered <see cref="ConfigEntry"/>s.
		/// </summary>
		public void UpdateRegisteredConfigValues()
		{
			foreach (ConfigEntry registeredConfig in registeredConfigs)
			{
				UpdateConfigValue(registeredConfig);
			}
		}
	}
}
