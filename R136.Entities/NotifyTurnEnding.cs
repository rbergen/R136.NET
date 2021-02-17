using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	interface INotifyTurnEnding
	{
		public Func<ICollection<string>?> TurnEndingHandler { get; }
	}
}
