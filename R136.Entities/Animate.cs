using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R136.Entities.Utilities;
using R136.Entities.Animates;
using System.Reflection;

namespace R136.Entities
{
	public abstract class Animate : EntityBase
	{
		public AnimateID ID { get; }
		public RoomID CurrentRoom { get; set; }

		protected AnimateStatus Status { get; set; }

		private StatusTextMapper? StatusTexts { get; } = null;

		public static IDictionary<AnimateID, Animate> FromInitializers(ICollection<Initializer> initializers)
		{
			Dictionary<AnimateID, Animate> animates = new Dictionary<AnimateID, Animate>(initializers.Count);

			foreach (var initializer in initializers)
			{
				Type? animateType = ToType(initializer.ID);

				if (animateType == null)
					continue;

				if (initializer.StatusTexts != null)
				{
					PropertyInfo? statusTextsProperty = animateType.GetProperty(nameof(StatusTexts), typeof(StatusTextMapper));
					
					if (statusTextsProperty != null && statusTextsProperty.CanRead && statusTextsProperty.CanWrite
						&& statusTextsProperty.GetGetMethod()!.IsStatic && statusTextsProperty.GetValue(null) == null)
					{
						statusTextsProperty.SetValue(null, new StatusTextMapper((IDictionary<AnimateStatus, ICollection<string>>)initializer.StatusTexts));
					}
				}

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

		public Animate(AnimateID id, RoomID startRoom, StatusTextMapper? statusTexts) 
			=> (ID, CurrentRoom, StatusTexts, Status) = (id, startRoom, statusTexts, AnimateStatus.Initial);

		public Animate(AnimateID id, RoomID startRoom)
			=> (ID, CurrentRoom, Status) = (id, startRoom, AnimateStatus.Initial);

		protected ICollection<string>? GetTextsForStatus(AnimateStatus status)
		{
			ICollection<string>? texts = null;
			StatusTexts?.Map.TryGetValue(status, out texts);
			return texts;
		}

		public virtual ICollection<string>? ProcessStatus()
		{
			AnimateStatus textStatus = Status;

			ProcessStatusInternal(textStatus);

			return GetTextsForStatus(textStatus);
		}

		public virtual void ProcessStatusInternal(AnimateStatus status) { }

		public virtual bool Used(ItemID item) => false;

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

	public abstract class StrikableAnimate : Animate
	{
		public int StrikesLeft { get; protected set; }

		public StrikableAnimate(AnimateID id, RoomID startRoom, int strikeCount, StatusTextMapper? textMapper) : base(id, startRoom, textMapper)
			=> StrikesLeft = strikeCount;

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Sword)
				return false;

			if (--StrikesLeft == 0)
			{
				Status = AnimateStatus.Dying;
				return true;
			}

			return false;
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
		HellHound			=  0,
		RedTroll			=  1,
		Plant					=  2,
		Gnu						=	 3,
		Dragon				=  4,
		Swelling			=  5,
		Door					=  6,
		Voices				=  7,
		Barbecue			=  8,
		Tree					=  9,
		GreenCrystal	= 10,
		Computer			= 11,
		DragonHead		= 12,
		Lava					= 13,
		Vacuum				= 14,
		PaperHatch		=	15,
		NorthSwamp		= 16,
		MiddleSwamp		= 17,
		SouthSwamp		= 18,
		Mist					= 19,
		Teleporter		= 20,
		SwampBase			= 21
	}
}
