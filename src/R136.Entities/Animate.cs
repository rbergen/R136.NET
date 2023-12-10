using Microsoft.Extensions.Primitives;
using R136.Entities.Animates;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public abstract class Animate : EntityBase, ISnappable<Animate.Snapshot>
	{
		public AnimateID ID { get; private set; }
		public RoomID CurrentRoom { get; set; }
		public bool IsTriggered { get; private set; }

		protected AnimateStatus Status { get; set; }

		public static IReadOnlyDictionary<AnimateID, Animate> UpdateOrCreateMap(IReadOnlyDictionary<AnimateID, Animate>? sourceMap, ICollection<Initializer> initializers)
		{
			SnapshotContainer? snapshot = null;

			if (sourceMap != null)
			{
				snapshot = new();
				TakeSnapshots(snapshot, sourceMap);
			}

			Dictionary<AnimateID, Animate> animates = new(initializers.Count);

			foreach (var initializer in initializers)
			{
				var createMethod = GetCreateMethod(initializer.ID);
				Type? animateType = createMethod?.Method.DeclaringType;

				if (createMethod == null || animateType == null)
					continue;

				if (initializer.StatusTexts != null)
					Facilities.AnimateStatusTextsMap[animateType] = initializer.StatusTexts.ToDictionary(pair => pair.Key, pair => (StringValues)pair.Value);

				if (initializer.Virtual)
					continue;

				animates[initializer.ID] = createMethod.Invoke(initializer);
			}

			if (snapshot != null)
				RestoreSnapshots(snapshot, animates);

			return animates;
		}

		public static void TakeSnapshots(ISnapshotContainer container, IReadOnlyDictionary<AnimateID, Animate> animates)
			=> container.Animates = animates.Values
				.Select(animate => animate.TakeSnapshot())
				.ToArray();

		public static bool RestoreSnapshots(ISnapshotContainer container, IReadOnlyDictionary<AnimateID, Animate> animates)
		{
			if (container.Animates == null)
				return false;

			bool result = true;

			foreach (var animate in container.Animates)
				result &= animates[animate.ID].RestoreSnapshot(animate);

			return result;
		}

		protected Animate(AnimateID id, RoomID startRoom)
			=> (ID, CurrentRoom, Status) = (id, startRoom, AnimateStatus.Initial);

		protected StringValues GetTextsForStatus(AnimateStatus status)
			=> Facilities.AnimateStatusTextsMap[this, status];

		public virtual StringValues ProgressStatus()
		{
			AnimateStatus textStatus = Status;

			ProgressStatusInternal(textStatus);

			return GetTextsForStatus(textStatus);
		}

		protected virtual void ProgressStatusInternal(AnimateStatus status) { }

		public virtual Result ApplyItem(ItemID item)
			=> Result.Failure();

		public virtual Result ApplyItem(Item item)
			=> ApplyItem(item.ID);

		public virtual void ResetTrigger()
			=> IsTriggered = false;

		protected virtual void Trigger()
			=> IsTriggered = true;

		private static Func<Initializer, Animate>? GetCreateMethod(AnimateID id) => id switch
		{
			AnimateID.HellHound => HellHound.Create,
			AnimateID.RedTroll => RedTroll.Create,
			AnimateID.Plant => Plant.Create,
			AnimateID.Gnu => Gnu.Create,
			AnimateID.Dragon => Dragon.Create,
			AnimateID.Swelling => Swelling.Create,
			AnimateID.Door => Door.Create,
			AnimateID.Voices => Voices.Create,
			AnimateID.Barbecue => Barbecue.Create,
			AnimateID.Tree => Tree.Create,
			AnimateID.GreenCrystal => GreenCrystal.Create,
			AnimateID.Computer => Computer.Create,
			AnimateID.DragonHead => DragonHead.Create,
			AnimateID.Lava => Lava.Create,
			AnimateID.Vacuum => Vacuum.Create,
			AnimateID.PaperHatch => PaperHatch.Create,
			AnimateID.SwampBase => Swamp.Create,
			AnimateID.NorthSwamp => Swamp.Create,
			AnimateID.MiddleSwamp => Swamp.Create,
			AnimateID.SouthSwamp => Swamp.Create,
			AnimateID.Mist => Mist.Create,
			AnimateID.Teleporter => Teleporter.Create,
			_ => null
		};

		public virtual Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			snapshot ??= new Snapshot();
			snapshot.ID = ID;
			snapshot.Room = CurrentRoom;
			snapshot.Status = Status;
			snapshot.IsTriggered = IsTriggered;

			return snapshot;
		}

		public virtual bool RestoreSnapshot(Snapshot state)
		{
			if (state.ID != ID)
				return false;

			CurrentRoom = state.Room;
			Status = state.Status;
			IsTriggered = state.IsTriggered;
			return true;
		}

		public class Initializer
		{
			public AnimateID ID { get; set; }
			public RoomID StartRoom { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public int StrikeCount { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool Virtual { get; set; }
			public Dictionary<AnimateStatus, string[]>? StatusTexts { get; set; }
		}

		public class Snapshot : ISnapshot
		{
			private const int BytesBaseSize = 4;

			public AnimateID ID { get; set; }
			public RoomID Room { get; set; }
			public AnimateStatus Status { get; set; }
			public int StrikesLeft { get; set; }
			public bool IsTriggered { get; set; }

			public void AddBytesTo(List<byte> bytes)
			{
				ID.AddByteTo(bytes);
				Room.AddByteTo(bytes);
				Status.AddByteTo(bytes);
				IsTriggered.AddByteTo(bytes);
				StrikesLeft.AddBytesTo(bytes);
			}

			public int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				if (bytes.Length <= BytesBaseSize)
					return null;

				var span = bytes.Span;

				ID = span[0].To<AnimateID>();
				Room = span[1].To<RoomID>();
				Status = span[2].To<AnimateStatus>();
				IsTriggered = span[3].ToBool();

				int? bytesRead;

				(StrikesLeft, bytesRead) = bytes[BytesBaseSize..].ToInt();

				return bytesRead != null ? BytesBaseSize + bytesRead : null;
			}
		}

		public interface ISnapshotContainer
		{
			Snapshot[]? Animates { get; set; }
		}

		private class SnapshotContainer : ISnapshotContainer
		{
			public Snapshot[]? Animates { get; set; }
		}
	}

	abstract class StrikableAnimate : Animate, ISnappable<Animate.Snapshot>
	{
		protected internal int StrikesLeft { get; protected set; }

		protected StrikableAnimate(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom)
			=> StrikesLeft = strikeCount;

		public bool IsSeriouslyInjured => StrikesLeft == 1;

		public bool IsDead => StrikesLeft == 0;

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Sword)
				return Result.Error();

			StrikesLeft--;

			if (IsDead)
			{
				Status = AnimateStatus.Dying;
				Trigger();
				return Result.Success();
			}

			return Result.Failure();
		}

		public override Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			snapshot = base.TakeSnapshot(snapshot);
			snapshot.StrikesLeft = StrikesLeft;
			return snapshot;
		}

		public override bool RestoreSnapshot(Snapshot state)
		{
			if (!base.RestoreSnapshot(state))
				return false;

			StrikesLeft = state.StrikesLeft;
			return true;
		}
	}

	public enum AnimateStatus : byte
	{
		Initial,
		PreparingFirstAttack,
		Attack,
		PreparingNextAttack,
		Dying,
		SelfInjury,
		FirstStep,
		FirstWait,
		SecondStep,
		SecondWait,
		Operating,
		Done
	}
}
