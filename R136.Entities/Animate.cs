using R136.Entities.Animates;
using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public abstract class Animate : EntityBase
	{
		public AnimateID ID { get; }
		public RoomID CurrentRoom { get; set; }

		protected AnimateStatus Status { get; set; }

		public static IReadOnlyDictionary<AnimateID, Animate> FromInitializers(ICollection<Initializer> initializers)
		{
			Dictionary<AnimateID, Animate> animates = new Dictionary<AnimateID, Animate>(initializers.Count);

			foreach (var initializer in initializers)
			{
				var animateType = ToType(initializer.ID);

				if (animateType == null)
					continue;

				if (initializer.StatusTexts != null)
					Facilities.AnimateStatusTextsMap[animateType] = (IDictionary<AnimateStatus, ICollection<string>>)initializer.StatusTexts;

				if (initializer.Virtual)
					continue;

				Animate? animate = null;

				if (animateType.IsAssignableTo(typeof(StrikableAnimate)) && animateType.GetConstructor(new Type[] { typeof(RoomID), typeof(int) }) != null)
					animate = (Animate?)Activator.CreateInstance(animateType, initializer.StartRoom, initializer.StrikeCount);

				else if (animateType.GetConstructor(new Type[] { typeof(RoomID) }) != null)
					animate = (Animate?)Activator.CreateInstance(animateType, initializer.StartRoom);

				if (animate != null)
					animates[initializer.ID] = animate;
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

		public virtual Result Used(ItemID item) => Result.Failure();

		public virtual Result Used(Item item) => Used(item.ID);

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

		private static Type? ToType(AnimateID id) => id switch
		{
			AnimateID.HellHound => typeof(HellHound),
			AnimateID.RedTroll => typeof(RedTroll),
			AnimateID.Plant => typeof(Plant),
			AnimateID.Gnu => typeof(Gnu),
			AnimateID.Dragon => typeof(Dragon),
			AnimateID.Swelling => typeof(Swelling),
			AnimateID.Door => typeof(Door),
			AnimateID.Voices => typeof(Voices),
			AnimateID.Barbecue => typeof(Barbecue),
			AnimateID.Tree => typeof(Tree),
			AnimateID.GreenCrystal => typeof(GreenCrystal),
			AnimateID.Computer => typeof(Computer),
			AnimateID.DragonHead => typeof(DragonHead),
			AnimateID.Lava => typeof(Lava),
			AnimateID.Vacuum => typeof(Vacuum),
			AnimateID.PaperHatch => typeof(PaperHatch),
			AnimateID.SwampBase => typeof(Swamp),
			AnimateID.NorthSwamp => typeof(Swamp),
			AnimateID.MiddleSwamp => typeof(Swamp),
			AnimateID.SouthSwamp => typeof(Swamp),
			AnimateID.Mist => typeof(Mist),
			AnimateID.Teleporter => typeof(Teleporter),
			_ => null
		};
	}

	abstract class StrikableAnimate : Animate
	{
		protected internal int StrikesLeft { get; protected set; }

		protected StrikableAnimate(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom)
			=> StrikesLeft = strikeCount;

		public override Result Used(ItemID item)
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

	public enum AnimateID
	{
		HellHound = 0,
		RedTroll = 1,
		Plant = 2,
		Gnu = 3,
		Dragon = 4,
		Swelling = 5,
		Door = 6,
		Voices = 7,
		Barbecue = 8,
		Tree = 9,
		GreenCrystal = 10,
		Computer = 11,
		DragonHead = 12,
		Lava = 13,
		Vacuum = 14,
		PaperHatch = 15,
		NorthSwamp = 16,
		MiddleSwamp = 17,
		SouthSwamp = 18,
		Mist = 19,
		Teleporter = 20,
		SwampBase = 21
	}
}
