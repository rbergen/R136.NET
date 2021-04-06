using R136.Interfaces;
using System;
using System.Collections.Generic;

namespace R136.Entities.Global
{
	public class Configuration : LayoutProperties, ISnapshot
	{
		private const int BytesBaseSize = 5;

		public int LifePoints { get; set; } = 20;
		public int? LampPoints { get; set; } = 60;
		public int? MaxInventory { get; set; } = 10;
		public bool Immortal { get; set; } = false;
		public bool FreezeAnimates { get; set; } = false;
		public bool AutoPlaceItems { get; set; } = false;
		public bool AutoOpenConnections { get; set; } = false;
		public bool EnableConfigList { get; set; } = false;

		public void AddBytes(List<byte> bytes)
		{
			Immortal.AddByte(bytes);
			FreezeAnimates.AddByte(bytes);
			AutoPlaceItems.AddByte(bytes);
			AutoOpenConnections.AddByte(bytes);
			EnableConfigList.AddByte(bytes);
			LifePoints.AddBytes(bytes);
			LampPoints.AddIntBytes(bytes);
			MaxInventory.AddIntBytes(bytes);
		}


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

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			if (bytes.Length < BytesBaseSize)
				return null;

			var span = bytes.Span;

			Immortal = span[0].ToBool();
			FreezeAnimates = span[1].ToBool();
			AutoPlaceItems = span[2].ToBool();
			AutoOpenConnections = span[3].ToBool();
			EnableConfigList = span[4].ToBool();

			bytes = bytes[BytesBaseSize..];

			int totalBytesRead = BytesBaseSize;
			bool abort = false;

			LifePoints = bytes.ToInt().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			LampPoints = bytes.ToNullableInt().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			int? bytesRead;
			(MaxInventory, bytesRead) = bytes.ToNullableInt();

			return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
		}
	}
}
