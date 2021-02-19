using R136.Entities.General;
using R136.Interfaces;

namespace R136.Entities.Global
{
	public class Configuration
	{
		public int LifePoints { get; set; } = 20;
		public int? LampPoints { get; set; } = 60;
		public int? MaxInventory { get; set; } = 10;
		public RoomID[] GnuRoamingRooms { get; set; } = new RoomID[] { RoomID.GnuCave, RoomID.RockCave, RoomID.Emptiness, RoomID.Sandbank, RoomID.OilCave };
		public RoomID[] PaperRoute { get; set; } = new RoomID[] { RoomID.CapitalPCave, RoomID.ACave, RoomID.PCave, RoomID.ICave, RoomID.ECave, RoomID.RCave };
		public InputSpecs YesNoInputSpecs { get; set; } = new InputSpecs(1, "JjNn");
		public string YesInput { get; set; } = "j";
		public bool Immortal { get; set; } = false;
		public bool FreezeAnimates { get; set; } = false;
		public bool AutoReleaseItems { get; set; } = false;
	}
}
