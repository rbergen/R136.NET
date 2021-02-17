using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.General
{
	interface INotifyTurnEnding
	{
		public Func<ICollection<string>?> TurnEndingHandler { get; }
	}
}
