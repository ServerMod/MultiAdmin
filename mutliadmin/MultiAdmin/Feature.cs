namespace MultiAdmin.MultiAdmin
{
	public abstract class Feature
	{
		public Feature(Server server)
		{
			Server = server;
		}

		public Server Server { get; }

		public abstract string GetFeatureDescription();
		public abstract void OnConfigReload();
		public abstract string GetFeatureName();
		public abstract void Init();
	}
}