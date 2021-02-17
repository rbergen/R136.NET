using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public class Command
	{

	}

	public enum CommandID
	{
		GoEast,
		GoWest,
		GoNorth,
		GoSouth,
		GoUp,
		GoDown,
		Use,
		Combine,
		Pickup,
		PutDowm,
		Inspect,
		Wait,
		End,
		Status,
		Help
	}
}
