using R136.Entities.Animates;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public abstract class Animate : EntityBase
	{
		public AnimateID ID { get; }
		public RoomID CurrentRoom { get; set; }

		protected AnimateStatus Status { get; set; }

		public static IReadOnlyDictionary<AnimateID, Animate> CreateMap(ICollection<Initializer> initializers)
		{
			Dictionary<AnimateID, Animate> animates = new Dictionary<AnimateID, Animate>(initializers.Count);

			foreach (var initializer in initializers)
			{
				var createMethod = GetCreateMethod(initializer.ID);
				Type? animateType = createMethod?.Method.DeclaringType;

				if (createMethod == null || animateType == null)
					continue;

				if (initializer.StatusTexts != null)
					Facilities.AnimateStatusTextsMap[animateType] = initializer.StatusTexts.ToDictionary(pair => pair.Key, pair => (ICollection<string>)pair.Value);

				if (initializer.Virtual)
					continue;

				animates[initializer.ID] = createMethod.Invoke(initializer);
			}

			return animates;
		}

		protected Animate(AnimateID id, RoomID startRoom)
			=> (ID, CurrentRoom, Status) = (id, startRoom, AnimateStatus.Initial);

		protected ICollection<string>? GetTextsForStatus(AnimateStatus status)
			=> Facilities.AnimateStatusTextsMap[this, status];

		public virtual ICollection<string>? ProgressStatus()
		{
			AnimateStatus textStatus = Status;

			ProgressStatusInternal(textStatus);

			return GetTextsForStatus(textStatus);
		}

		protected virtual void ProgressStatusInternal(AnimateStatus status) { }

		public virtual Result ApplyItem(ItemID item) => Result.Failure();

		public virtual Result ApplyItem(Item item) => ApplyItem(item.ID);

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

	}

	abstract class StrikableAnimate : Animate
	{
		protected internal int StrikesLeft { get; protected set; }

		protected StrikableAnimate(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom)
			=> StrikesLeft = strikeCount;

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Sword)
				return Result.Error();

			if (--StrikesLeft == 0)
			{
				Status = AnimateStatus.Dying;
				return Result.Success();
			}

			return Result.Failure();
		}
	}

	public enum AnimateStatus
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
