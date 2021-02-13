using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities
{
	public class Room
	{
		public string Name { get; private set; }

		public string Description { get; private set;  }

		public Dictionary<Direction, Room> Connections { get; private set; }

		public static Dictionary<RoomID, Room> FromInitializers(RoomInitializer[] roomInitializers)
		{
			Dictionary<RoomID, Room> rooms = new Dictionary<RoomID, Room>(roomInitializers.Length);

			foreach(var initializer in roomInitializers)
			{
				rooms[initializer.ID] = new Room()
				{
					Name = initializer.Name,
					Description = initializer.Description,
				};
			}

			foreach(var initializer in roomInitializers)
			{
				rooms[initializer.ID].Connections = initializer.Connections.ToDictionary(pair => pair.Key, pair => rooms[pair.Value]);
			}

			return rooms;
		}
	}

	public enum RoomID
	{
		Bos0, Bos1, Bos2, NoordMoeras, Bos4,
		Bos5, Begraafplaats, Bos7, MiddenMoeras, OpenPlek9,
		Bos10, Bos11, OpenPlek12, MoerasPad, OpenPlek14,
		Bos15, Bos16, OpenPlek17, ZuidMoeras, Ruine,

		SlijmGrot,	ZwarteGrot,	DrugsGrot, GeileGrot, DwangbuisGrot,
		VerwaarloosdeGrot, LegeGrot26, HoofdGrot, HierogliefenGrot, StankGrot,
		TroostelozeGrot, TLGrot, KleineGrot, IJsGrot, KaktusGrot,
		StalagmietenGrot, StormGrot, MistGrot, WenteltrapGrot1, TentakelGrot,

		VuilnisGrot, EchoGrot, GeheimeGrot, VoedselGrot, GnoeGrot,
		LegeGrot45, OgenGrot, RotsGrot, Leegte, Zandbank,
		MartelGrot, LegeGrot51, VeiligeGrot, NauweRotsspleet, OlieGrot,
		LegeGrot55,	WenteltrapGrot2, SpinnenGrot, PratendeGrot, LavaPut,
		
		SkoebieGrot, RadioactieveGrot, IGrot, PGrot, AGrot,
		DodenGrot, RGrot, EGrot, WenteltrapGrot3, HoofdletterPGrot,
		VerdoemenisGrot, VacuumGrot, RodeGrot, NeonGrot, BloedGrot,
		VleermuisGrot, SlangenGrot, KwabbenGrot, GlibberGrot, TeleportGrot
	}

	public enum Direction
	{
		East,
		West,
		North,
		South,
		Up,
		Down
	}

	public class RoomInitializer
	{
		public RoomID ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public Dictionary<Direction, RoomID> Connections { get; set; }
	}
}
