using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Global
{
	public class Facilities
	{
		public static IServiceProvider? ServiceProvider { get; set; }

		public static Random Randomizer { get; }

		public static TextsMap TextsMap { get; }

		static Facilities()
		{
			Randomizer = new Random();
			TextsMap = new TextsMap();
		}
	}
}
