using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Utilities
{
	public class StatusTextMapper
	{
		public IDictionary<AnimateStatus, ICollection<string>> Map { get; }

		public StatusTextMapper(IDictionary<AnimateStatus, ICollection<string>> map) => Map = map;
	}
}
