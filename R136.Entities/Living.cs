using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public abstract class Living
	{
		public RoomID CurrentRoom { get; }

		public Living(RoomID startRoom) 
			=> CurrentRoom = startRoom;
	
		public enum Status
		{
			Initial,
			PreparingFirstAttack,
			Attack,
			PreparingNextAttack,
			Dying,
			DyingSelfInjury,
			FirstStep,
			FirstWait,
			SecondStep,
			SecondWait,
			Operating,
			Done
		}
	}

	public abstract class StrikableMonster : Living 
	{
		public int StrikesLeft { get; }

		public StrikableMonster(RoomID startRoom, int strikeCount) : base(startRoom) 
			=> StrikesLeft = strikeCount;
	}

	public class LivingInitializer
	{
		string TypeName { get; set; }
		Dictionary<Living.Status, string[]> StatusTexts { get; set; }
	}
}
