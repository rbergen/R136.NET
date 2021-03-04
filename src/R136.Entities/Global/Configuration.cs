namespace R136.Entities.Global
{
	public class Configuration : LayoutProperties
	{
		public int LifePoints { get; set; } = 20;
		public int? LampPoints { get; set; } = 60;
		public int? MaxInventory { get; set; } = 10;
		public bool Immortal { get; set; } = false;
		public bool FreezeAnimates { get; set; } = false;
		public bool AutoPlaceItems { get; set; } = false;
		public bool AutoOpenConnections { get; set; } = false;
		public bool EnableConfigList { get; set; } = false;

		public void Load(Configuration configuration)
		{
			LifePoints = configuration.LifePoints;
			LampPoints = configuration.LampPoints;
			MaxInventory = configuration.MaxInventory;
			Immortal = configuration.Immortal;
			FreezeAnimates = configuration.FreezeAnimates;
			AutoPlaceItems = configuration.AutoPlaceItems;
			AutoOpenConnections = configuration.AutoOpenConnections;
			EnableConfigList = configuration.EnableConfigList;
		}

	}
}
