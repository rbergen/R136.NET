using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public abstract class Living
	{
		public Room CurrentRoom { get; }

		public Living(Room startRoom) 
			=> CurrentRoom = startRoom;
	}

	public abstract class StrikableMonster : Living 
	{
		public int StrikesLeft { get; }

		public StrikableMonster(Room startRoom, int strikeCount) : base(startRoom) 
			=> StrikesLeft = strikeCount;
	}
}
