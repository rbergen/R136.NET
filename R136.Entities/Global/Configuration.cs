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
		public bool LogToConsole { get; set; } = false;
		public bool EnableConfigList { get; set; } = false;
	}
}
