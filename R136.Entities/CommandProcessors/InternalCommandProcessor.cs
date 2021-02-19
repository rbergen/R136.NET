using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	class InternalCommandProcessor : CommandProcessor
	{
		private const int Default = 0;

		public override Result Execute(CommandID id, string name, string? parameters, Player player)
			=> parameters == null
				? Result.Success()
				: id switch
				{
					CommandID.ConfigGet => ExecuteConfigGet(parameters),
					CommandID.ConfigSet => ExecuteConfigSet(parameters),
					_ => Result.Error()
				};

		private static Result ExecuteConfigSet(string parameters)
		{
			var terms = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (terms.Length != 2)
				return Result.Success();

			var propertyName = terms[0];

			var property = Facilities.Configuration.GetType().GetProperty(propertyName);
			if (property == null)
				return Result.Success();

			var propertyValue = terms[1];

			try
			{
				var oldValue = property.GetValue(Facilities.Configuration);

				if (property.PropertyType == typeof(bool))
					SetValue(property, bool.Parse(propertyValue));

				else if (property.PropertyType == typeof(int))
					SetValue(property, int.Parse(propertyValue));

				else if (property.PropertyType == typeof(string))
					SetValue(property, propertyValue);

				else if (property.PropertyType == typeof(int?))
					SetValue(property, propertyValue == ObjectDumper.Null ? null : (int?)int.Parse(propertyValue));

				var newValue = property.GetValue(Facilities.Configuration);

				return Result.Success(GetTexts(propertyName, oldValue, newValue));
			}
			catch { }

			return Result.Success();
		}

		private static Result ExecuteConfigGet(string parameters)
		{
			var terms = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (terms.Length != 1)
				return Result.Success();

			var propertyName = terms[0];

			var property = Facilities.Configuration.GetType().GetProperty(propertyName);
			if (property == null)
				return Result.Success();

			try
			{
				return Result.Success(GetTexts(propertyName, property.GetValue(Facilities.Configuration)));
			}
			catch { }

			return Result.Success();
		}

		private static void SetValue<T>(PropertyInfo property, T value)
		{
			try
			{
				property.SetValue(Facilities.Configuration, value);
			}
			catch { }
		}

		private static ICollection<string>? GetTexts(string propertyName, object? value)
			=> Facilities.CommandTextsMap[CommandID.ConfigGet, Default]
			.ReplaceInAll("{setting}", propertyName)
			.ReplaceInAll("{value}", ObjectDumper.Dump(value));

		private static ICollection<string>? GetTexts(string propertyName, object? oldValue, object? newValue)
			=> Facilities.CommandTextsMap[CommandID.ConfigGet, Default]
			.ReplaceInAll("{setting}", propertyName)
			.ReplaceInAll("{oldvalue}", ObjectDumper.Dump(oldValue))
			.ReplaceInAll("{newvalue}", ObjectDumper.Dump(newValue));
	}
}
