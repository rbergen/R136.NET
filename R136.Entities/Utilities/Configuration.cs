using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Utilities
{
	public class Configuration
	{
		public int LifePoints { get; set; } = 20;
		public bool Immortal { get; set; } = false;
		public int? LampPoints { get; set; } = 60;
		public bool FreezeAnimates { get; set; } = false;
		public bool AutoReleaseItems { get; set; } = false;
	}
}
