using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


namespace R136.Entities
{
	public abstract class Animate
	{
		protected static Random Randomizer { get; }
		private static IDictionary<AnimateStatus, ICollection<string>> StatusTexts { get; set; }
		
		public RoomID CurrentRoom { get; }

		private IStatusManager _statusManager = null;
		private IServiceProvider _serviceProvider;
		protected AnimateStatus Status { get; set; }

		static Animate() => Randomizer = new Random();

		public static IDictionary<AnimateID, Animate> FromInitializers(IServiceProvider serviceProvider, ICollection<Initializer> initializers)
		{
			Dictionary<AnimateID, Animate> animates = new Dictionary<AnimateID, Animate>(initializers.Count);

			foreach (var initializer in initializers)
			{
				Type animateType = AnimateTypeMap.FromID(initializer.ID);

				if (initializer.StatusTexts != null)
					animateType.GetProperty(nameof(StatusTexts))?.SetValue(null, (IDictionary<AnimateStatus, ICollection<string>>)initializer.StatusTexts);

				if (initializer.Virtual)
					continue;

				animates[initializer.ID] = animateType.IsAssignableTo(typeof(StrikableAnimate))
					? (Animate)Activator.CreateInstance(animateType, serviceProvider, initializer.StartRoom, initializer.StrikeCount)
					: (Animate)Activator.CreateInstance(animateType, serviceProvider, initializer.StartRoom);
			}

			return animates;
		}

		public Animate(IServiceProvider serviceProvider, RoomID startRoom) 
			=> (_serviceProvider, CurrentRoom, Status) = (serviceProvider, startRoom, AnimateStatus.Initial);

		public abstract ICollection<string> ProcessStatus();

		public abstract bool Used(ItemID item);

		protected IStatusManager StatusManager
		{
			get
			{
				if (_statusManager == null && _serviceProvider != null)
				{
					_statusManager = _serviceProvider.GetService<IStatusManager>();
				}

				return _statusManager;
			}
		}

		public class Initializer
		{
			public AnimateID ID { get; set; }
			public RoomID StartRoom { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public int StrikeCount { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool Virtual { get; set; }
			public Dictionary<AnimateStatus, string[]> StatusTexts { get; set; }
		}
	}

	public abstract class StrikableAnimate : Animate
	{
		public int StrikesLeft { get; protected set; }

		public StrikableAnimate(IServiceProvider serviceProvider, RoomID startRoom, int strikeCount) : base(serviceProvider, startRoom)
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

	public static class AnimateTypeMap
	{
		private static readonly Dictionary<AnimateID, Type> _map = new Dictionary<AnimateID, Type>(Enum.GetValues(typeof(AnimateID)).Length) 
		{
			[AnimateID.HellHound] = typeof(Animates.Hellhound),
		};

		public static Type FromID(AnimateID id) => _map[id];
	}

}
