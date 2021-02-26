using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.Items
{
	public class Flashlight : Item, ICompound<Item>, INotifyTurnEnding, ISnappable<Flashlight.Snapshot>
	{
		private int? _lampPoints;
		private int? _lampPointsFromConfig;

		public bool IsOn { get; private set; }

		public ICollection<Item> Components { get; private set; }
		public StringValues CombineTexts
			=> Facilities.ItemTextsMap[ID, TextType.Combine];

#pragma warning disable IDE0060 // Remove unused parameter
		public static Flashlight Create
			(
			Initializer initializer, 
			IReadOnlyDictionary<AnimateID, Animate> animates, 
			IReadOnlyDictionary<ItemID, Item> items
			)
#pragma warning restore IDE0060 // Remove unused parameter
			=> new Flashlight
			(
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
			=> (IsOn, _lampPoints, _lampPointsFromConfig, Components)
			= (false, null, null, components.Select(itemID => itemID == id ? this : items[itemID]).ToArray());

		public int? LampPoints
		{
			get
			{
				if (_lampPointsFromConfig != Facilities.Configuration.LampPoints)
				{
					_lampPoints = _lampPointsFromConfig = Facilities.Configuration.LampPoints;
				}

				return _lampPoints!.Value;
			}

			private set
			{
				_lampPoints = value;
			}
		}

		public Item Self => this;

		private StringValues GetTexts(TextID id) => Facilities.TextsMap[this, (int)id];

		public override Result Use()
		{
			if (IsOn)
			{
				IsOn = false;

				var isDark = StatusManager?.IsDark ?? true;

				return Result.Success(GetTexts(isDark ? TextID.LightOffInDark : TextID.LightOff));
			}

			if (LampPoints == null || LampPoints > 0)
			{
				IsOn = true;
				return Result.Success(GetTexts(TextID.LightOn));
			}

			return Result.Failure(GetTexts(TextID.NeedBatteries));
		}

		public Result Combine(Item first, Item second)
		{
			return !Components.Contains(first) || !Components.Contains(second) || first == second 
				? Result.Failure()
				:	Result.Success(CombineTexts);
		}

		public StringValues TurnEnding()
		{
			if (!IsOn)
				return StringValues.Empty;

			if (LampPoints != null && LampPoints > 0)
				LampPoints--;

			if (LampPoints == 10)
				return Facilities.TextsMap[this, (int)TextID.BatteriesLow];

			if (LampPoints == 0)
			{
				IsOn = false;
				return Facilities.TextsMap[this, (int)TextID.BatteriesEmpty];
			}

			return StringValues.Empty;

		}

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (snapshot == null)
				snapshot = new Snapshot();

			base.TakeSnapshot(snapshot);
			snapshot.LampPoints = _lampPoints;
			snapshot.LampPointsFromConfig = _lampPointsFromConfig;
			snapshot.IsOn = IsOn;
			
			return snapshot;
		}
		
		public bool RestoreSnapshot(Snapshot snapshot)
		{
			if (!base.RestoreSnapshot(snapshot))
				return false;

			_lampPoints = snapshot.LampPoints;
			_lampPointsFromConfig = snapshot.LampPointsFromConfig;
			IsOn = snapshot.IsOn;

			return true;
		}

		public new class Snapshot : Item.Snapshot
		{
			public int? LampPoints { get; set; }
			public int? LampPointsFromConfig { get; set; }
			public bool IsOn { get; set; }
		}

		private enum TextID
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
