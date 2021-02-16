using R136.Entities.General;

namespace R136.Entities.Global
{
	public class Configuration
	{
		public int LifePoints { get; set; } = 20;
		public bool Immortal { get; set; } = false;
		public int? LampPoints { get; set; } = 60;
		public bool FreezeAnimates { get; set; } = false;
		public bool AutoReleaseItems { get; set; } = false;
		public RoomID[] GnuRoamingRooms { get; set; } = new RoomID[] { RoomID.GnuCave, RoomID.RockCave, RoomID.Emptiness, RoomID.Sandbank, RoomID.OilCave };
		public InputSpecs YesNoInputSpecs { get; set; } = new InputSpecs(1, "JjNn");
		public string YesInput { get; set; } = "j";
	}
}
