using R136.Entities.Animates;
using R136.Entities.Global;
using R136.Interfaces;
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
				var animateInitializerMethod = ToInitializerMethod(initializer.ID);
				Type? animateType = animateInitializerMethod?.Method.DeclaringType;

				if (animateInitializerMethod == null || animateType == null)
					continue;

				if (initializer.StatusTexts != null)
					Facilities.AnimateStatusTextsMap[animateType] = (IDictionary<AnimateStatus, ICollection<string>>)initializer.StatusTexts;

				if (initializer.Virtual)
					continue;

				animates[initializer.ID] = animateInitializerMethod.Invoke(initializer);
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

		private static Func<Initializer, Animate>? ToInitializerMethod(AnimateID id) => id switch
		{
			AnimateID.HellHound => HellHound.FromInitializer,
			AnimateID.RedTroll => RedTroll.FromInitializer,
			AnimateID.Plant => Plant.FromInitializer,
			AnimateID.Gnu => Gnu.FromInitializer,
			AnimateID.Dragon => Dragon.FromInitializer,
			AnimateID.Swelling => Swelling.FromInitializer,
			AnimateID.Door => Door.FromInitializer,
			AnimateID.Voices => Voices.FromInitializer,
			AnimateID.Barbecue => Barbecue.FromInitializer,
			AnimateID.Tree => Tree.FromInitializer,
			AnimateID.GreenCrystal => GreenCrystal.FromInitializer,
			AnimateID.Computer => Computer.FromInitializer,
			AnimateID.DragonHead => DragonHead.FromInitializer,
			AnimateID.Lava => Lava.FromInitializer,
			AnimateID.Vacuum => Vacuum.FromInitializer,
			AnimateID.PaperHatch => PaperHatch.FromInitializer,
			AnimateID.SwampBase => Swamp.FromInitializer,
			AnimateID.NorthSwamp => Swamp.FromInitializer,
			AnimateID.MiddleSwamp => Swamp.FromInitializer,
			AnimateID.SouthSwamp => Swamp.FromInitializer,
			AnimateID.Mist => Mist.FromInitializer,
			AnimateID.Teleporter => Teleporter.FromInitializer,
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
}
