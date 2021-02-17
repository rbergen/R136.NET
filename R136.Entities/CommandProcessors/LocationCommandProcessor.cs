using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	public class LocationCommandProcessor : CommandProcessor
	{
		private ICollection<Item> _items;
		private ICollection<Animate> _animates;

		public LocationCommandProcessor(ICollection<Item> items, ICollection<Animate> animates)
			=> (_items, _animates) = (items, animates);

		public override Result Execute(CommandID id, string name, string? parameters, Player player, ICollection<Item> presentItems, Animate? presentAnimate)
		{
			if (parameters != null)
				Result.Failure(Facilities.TextsMap[this, (int)TextID.CommandSyntax]?.ReplaceInAll("{command}", name));


		}

		private static ICollection<string>? GetTexts(CommandID commandId, int textId)
			=> Facilities.CommandTextsMap[commandId, textId];

		private enum TextID
		{
			CommandSyntax,
			CantGoThere
		}
	}
}
