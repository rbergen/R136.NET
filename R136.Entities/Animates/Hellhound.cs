using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Hellhound : StrikableAnimate
	{

		public static IDictionary<AnimateStatus, ICollection<string>> StatusTexts { get; set; }

		public Hellhound(IServiceProvider serviceProvider, RoomID startRoom, int strikeCount) : base(serviceProvider, startRoom, strikeCount) { }

		public override ICollection<string> ProcessStatus()
		{
			AnimateStatus textStatus = Status;

			switch (textStatus)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();
					Status = AnimateStatus.PreparingNextAttack;
					
					break;

				case AnimateStatus.PreparingNextAttack:
					if (Animate.Randomizer.Next(1) == 0)
						Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Dying:
					StatusManager?.ReleaseItem(ItemID.HoundMeat);
					Status = AnimateStatus.Done;

					break;
			}

			StatusTexts.TryGetValue(textStatus, out ICollection<string> texts);
			return texts;
		}
	}
}
