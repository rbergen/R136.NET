using R136.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	class InternalCommandProcessor : CommandProcessor
	{
		public override Result Execute(CommandID id, string name, string? parameters, Player player)
			=> id switch
			{
				CommandID.ConfigGet => ExecuteConfigGet(parameters),
				CommandID.ConfigSet => ExecuteConfigSet(parameters),
				_ => Result.Error()
			};

		private Result ExecuteConfigSet(string? parameters) => throw new NotImplementedException();
		private Result ExecuteConfigGet(string? parameters) => throw new NotImplementedException();
	}
}
