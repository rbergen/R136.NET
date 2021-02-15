using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Plant : StrikableAnimate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Plant(IServiceProvider serviceProvider, RoomID startRoom, int strikeCount) : base(serviceProvider, startRoom, strikeCount, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();

					Status = Randomizer.Next(2) == 0
						? AnimateStatus.PreparingNextAttack
						: AnimateStatus.Attack;

					break;

				case AnimateStatus.PreparingNextAttack:
						Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Dying:
					StatusManager?.OpenConnection(Direction.North, RoomID.SlimeCave);
					Status = AnimateStatus.Done;

					break;
			}
		}
	}
}
