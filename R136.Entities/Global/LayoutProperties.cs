using R136.Interfaces;

namespace R136.Entities.Global
{
	public class LayoutProperties
	{
		public RoomID StartRoom { get; set; } = RoomID.Forest0;
		public RoomID GreenCrystalRoom { get; set; } = RoomID.Forest4;
		public RoomID[] GnuRoamingRooms { get; set; } = new RoomID[] { RoomID.GnuCave, RoomID.RockCave, RoomID.Emptiness, RoomID.Sandbank, RoomID.OilCave };
		public RoomID[] PaperRoute { get; set; } = new RoomID[] { RoomID.CapitalPCave, RoomID.ACave, RoomID.PCave, RoomID.ICave, RoomID.ECave, RoomID.RCave };
		public InputSpecs YesNoInputSpecs { get; set; } = new InputSpecs(1, "[jJnN]", true);
		public string YesInput { get; set; } = "j";
		public InputSpecs CommandInputSpecs { get; set; } = new InputSpecs(65, "[a-zA-Z0-9 ]+", true);

		public void Load(LayoutProperties properties)
		{
			StartRoom = properties.StartRoom;
			GreenCrystalRoom = properties.GreenCrystalRoom;
			GnuRoamingRooms = properties.GnuRoamingRooms;
			PaperRoute = properties.PaperRoute;
			YesNoInputSpecs = properties.YesNoInputSpecs;
			YesInput = properties.YesInput;
			CommandInputSpecs = properties.CommandInputSpecs;
		}
	}
}