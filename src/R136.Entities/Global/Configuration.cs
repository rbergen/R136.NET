using R136.Entities.General;
using System;

namespace R136.Entities.Global
{
	public class Configuration : LayoutProperties, ISnapshot
	{
		private const int BinarySize = 8;

		public int LifePoints { get; set; } = 20;
		public int? LampPoints { get; set; } = 60;
		public int? MaxInventory { get; set; } = 10;
		public bool Immortal { get; set; } = false;
		public bool FreezeAnimates { get; set; } = false;
		public bool AutoPlaceItems { get; set; } = false;
		public bool AutoOpenConnections { get; set; } = false;
		public bool EnableConfigList { get; set; } = false;

		public byte[] GetBinary()
			=> new byte[BinarySize] 
			{ 
				LifePoints.ToByte(), 
				LampPoints.IntToByte(), 
				MaxInventory.IntToByte(), 
				Immortal.ToByte(), 
				FreezeAnimates.ToByte(), 
				AutoPlaceItems.ToByte(), 
				AutoOpenConnections.ToByte(), 
				EnableConfigList.ToByte() 
			};

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

		public int? SetBinary(Span<byte> value)
		{
			if (value.Length < BinarySize)
				return null;

			LifePoints = value[0];
			LampPoints = value[1].ToNullableInt();
			MaxInventory = value[2].ToNullableInt();
			Immortal = value[3].ToBool();
			FreezeAnimates = value[4].ToBool();
			AutoPlaceItems = value[5].ToBool();
			AutoOpenConnections = value[6].ToBool();
			EnableConfigList = value[7].ToBool();

			return BinarySize;
		}
	}
}
