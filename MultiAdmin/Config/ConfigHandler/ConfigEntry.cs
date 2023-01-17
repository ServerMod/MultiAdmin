using System;

namespace MultiAdmin.Config.ConfigHandler
{
	/// <summary>
	/// A base <see cref="ConfigEntry"/> for storing config values. This can be registered to a <see cref="ConfigRegister"/> to get config values automatically.
	/// </summary>
	public abstract class ConfigEntry
	{
		/// <summary>
		/// The key to read from the config file.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// The type of the value of the <see cref="ConfigEntry"/>.
		/// </summary>
		public abstract Type ValueType { get; }

		/// <summary>
		/// The value of the <see cref="ConfigEntry"/>.
		/// </summary>
		public abstract object? ObjectValue { get; set; }

		/// <summary>
		/// The default value of the <see cref="ConfigEntry"/>.
		/// </summary>
		public abstract object? ObjectDefault { get; set; }

		/// <summary>
		/// Whether to inherit this config value from the <see cref="ConfigRegister"/>'s parent <see cref="ConfigRegister"/>s if they support value inheritance.
		/// </summary>
		public bool Inherit { get; }

		/// <summary>
		/// The name of the <see cref="ConfigEntry"/>.
		/// </summary>
		public string? Name { get; }

		/// <summary>
		/// The description of the <see cref="ConfigEntry"/>.
		/// </summary>
		public string? Description { get; }

		/// <summary>
		/// Creates a basic <see cref="ConfigEntry"/> with no values and indication for whether to inherit the value.
		/// </summary>
		public ConfigEntry(string key, bool inherit = true, string? name = null, string? description = null)
		{
			Key = key;

			Inherit = inherit;

			Name = name;
			Description = description;
		}

		/// <summary>
		/// Creates a basic <see cref="ConfigEntry"/> with no values.
		/// </summary>
		public ConfigEntry(string key, string? name = null, string? description = null) : this(key, true, name,
			description)
		{
		}
	}

	/// <inheritdoc />
	/// <summary>
	/// A generic <see cref="ConfigEntry{T}" /> for storing config values. This can be registered to a <see cref="ConfigEntry{T}" /> to get config values automatically.
	/// </summary>
	public class ConfigEntry<T> : ConfigEntry
	{
		public override Type ValueType => typeof(T);

		/// <summary>
		/// The typed value of the <see cref="ConfigEntry{T}"/>.
		/// </summary>
		public T Value { get; set; }

		/// <summary>
		/// The typed default value of the <see cref="ConfigEntry{T}"/>.
		/// </summary>
		public T Default { get; set; }

		public override object? ObjectValue
		{
			get => Value;
			set => Value = (T)value!;
		}

		public override object? ObjectDefault
		{
			get => Default;
			set => Default = (T)value!;
		}

		/// <inheritdoc />
		/// <summary>
		/// Creates a <see cref="ConfigEntry{T}" /> with the provided type, default value, and indication for whether to inherit the value.
		/// </summary>
		public ConfigEntry(string key, T defaultValue, bool inherit = true, string? name = null,
			string? description = null) : base(key, inherit, name, description)
		{
			Value = defaultValue;
			Default = defaultValue;
		}

		/// <inheritdoc />
		/// <summary>
		/// Creates a <see cref="ConfigEntry{T}" /> with the provided type and default value.
		/// </summary>
		public ConfigEntry(string key, T defaultValue, string? name = null, string? description = null) : this(
			key, defaultValue, true, name, description)
		{
		}
	}
}
