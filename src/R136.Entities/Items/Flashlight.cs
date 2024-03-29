﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.Items
{
	public class Flashlight : Item, ICompound<Item>, INotifyTurnEnding, ISnappable<Flashlight.Snapshot>, ILightsource, IGameServiceProvider, IGameServiceBasedConfigurator
	{
		private int? lampPoints;
		private int? lampPointsFromConfig;

		public bool IsOn { get; private set; }
		public bool HasBatteries { get; private set; }

		public ICollection<Item> Components { get; private set; }
		public StringValues CombineTexts
			=> Facilities.ItemTextsMap[ID, TextType.Combine];

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are part of delegate interface")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Legibility")]
		public static Flashlight Create
			(
				Initializer initializer,
				IReadOnlyDictionary<AnimateID, Animate> animates,
				IReadOnlyDictionary<ItemID, Item> items
			)
			=> new(
				initializer.ID,
				initializer.Name,
				initializer.Description,
				initializer.StartRoom,
				initializer.Wearable,
				!initializer.BlockPutdown,
				items,
				initializer.Components!
			);

		private Flashlight
			(
				ItemID id,
				string name,
				string description,
				RoomID startRoom,
				bool isWearable,
				bool isPutdownAllowed,
				IReadOnlyDictionary<ItemID, Item> items,
				ICollection<ItemID> components
			) : base(id, name, description, startRoom, isWearable, isPutdownAllowed)
			=> (IsOn, HasBatteries, this.lampPoints, this.lampPointsFromConfig, Components)
			= (false, false, null, null, components.Select(itemID => itemID == id ? this : items[itemID]).ToArray());

		public int? LampPoints
		{
			get
			{
				if (this.lampPointsFromConfig != Facilities.Configuration.LampPoints)
					this.lampPoints = this.lampPointsFromConfig = Facilities.Configuration.LampPoints;

				return this.lampPoints!.Value;
			}

			private set => this.lampPoints = value;
		}

		public Item Self => this;

		private StringValues GetTexts(TextID id) => Facilities.TextsMap.Get(this, id);

		public override Result Use()
		{
			if (IsOn)
			{
				IsOn = false;

				bool isDark = Player?.IsDark ?? true;

				return Result.Success(GetTexts(isDark ? TextID.LightOffInDark : TextID.LightOff));
			}

			if (HasBatteries || LampPoints == null || LampPoints > 0)
			{
				IsOn = true;
				return Result.Success(GetTexts(TextID.LightOn));
			}

			return Result.Failure(GetTexts(TextID.NeedBatteries), true);
		}

		public Result Combine(Item first, Item second)
		{
			if (!Components.Contains(first) || !Components.Contains(second) || first == second)
				return Result.Failure();

			HasBatteries = true;
			return Result.Success(CombineTexts);
		}

		public StringValues TurnEnding()
		{
			if (!IsOn || HasBatteries)
				return StringValues.Empty;

			if (LampPoints != null && LampPoints > 0)
				LampPoints--;

			if (LampPoints == 10)
				return Facilities.TextsMap.Get(this, TextID.BatteriesLow);

			if (LampPoints == 0)
			{
				IsOn = false;
				return Facilities.TextsMap.Get(this, TextID.BatteriesEmpty);
			}

			return StringValues.Empty;

		}

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			snapshot ??= new();
			base.TakeSnapshot(snapshot);
			snapshot.LampPoints = this.lampPoints;
			snapshot.LampPointsFromConfig = this.lampPointsFromConfig;
			snapshot.IsOn = IsOn;
			snapshot.HasBatteries = HasBatteries;

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			if (!base.RestoreSnapshot(snapshot))
				return false;

			this.lampPoints = snapshot.LampPoints;
			this.lampPointsFromConfig = snapshot.LampPointsFromConfig;
			IsOn = snapshot.IsOn;
			HasBatteries = snapshot.HasBatteries;

			return true;
		}

		public void RegisterServices(IServiceCollection serviceCollection)
			=> serviceCollection.AddSingleton<ILightsource>(this);

		public void Configure(IServiceProvider serviceProvider)
			=> serviceProvider.GetService<ITurnEndingProvider>()?.RegisterListener(this);

		public new class Snapshot : Item.Snapshot
		{
			private const int BytesBaseSize = 2;

			public int? LampPoints { get; set; }
			public int? LampPointsFromConfig { get; set; }
			public bool IsOn { get; set; }
			public bool HasBatteries { get; set; }

			public override void AddBytesTo(List<byte> bytes)
			{
				base.AddBytesTo(bytes);
				IsOn.AddByteTo(bytes);
				HasBatteries.AddByteTo(bytes);
				LampPoints.AddIntBytesTo(bytes);
				LampPointsFromConfig.AddIntBytesTo(bytes);
			}

			public override int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				int? bytesRead = base.LoadBytes(bytes);

				if (bytesRead == null || bytes.Length < bytesRead.Value + BytesBaseSize)
					return null;

				int totalBytesRead = bytesRead.Value;

				var span = bytes.Span;

				IsOn = span[bytesRead.Value].ToBool();
				HasBatteries = span[bytesRead.Value + 1].ToBool();

				totalBytesRead += BytesBaseSize;
				bytes = bytes[totalBytesRead..];

				(LampPoints, bytesRead) = bytes.ToNullableInt();
				if (bytesRead == null) 
					return null;

				bytes = bytes[bytesRead.Value..];
				totalBytesRead += bytesRead.Value;

				(LampPointsFromConfig, bytesRead) = bytes.ToNullableInt();

				return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
			}
		}

		private enum TextID : byte
		{
			LightOff,
			LightOffInDark,
			LightOn,
			NeedBatteries,
			BatteriesLow,
			BatteriesEmpty
		}
	}
}
