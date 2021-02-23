using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace R136.Entities.CommandProcessors
{
	class InternalCommandProcessor : CommandProcessor
	{
		private const int Default = 0;

		public override Result Execute(CommandID id, string name, string? parameters, Player player)
			=> id switch
				{
					CommandID.ConfigGet => ExecuteConfigGet(parameters),
					CommandID.ConfigSet => ExecuteConfigSet(parameters),
					CommandID.ConfigList => ExecuteConfigList(parameters),
					_ => Result.Error()
				};

		private static Result ExecuteConfigList(string? parameters)
			=> parameters == null && Facilities.Configuration.EnableConfigList
			? Result.Success(GetPublicPropertyNames<Configuration>())
			: Result.Error(Facilities.CommandTextsMap[CommandID.ConfigList, Default]);

		private Result ExecuteConfigSet(string? parameters)
		{
			if (parameters == null)
				return Result.Success();

			var terms = parameters.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
			if (terms.Length != 2)
				return Result.Success();

			var propertyName = terms[0];
			var propertyValue = terms[1].Trim();

			try
			{
				var property = Facilities.Configuration.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (property == null)
					return Result.Success();

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

				return Result.Success(GetTexts(property.Name, oldValue, newValue));
			}
			catch (Exception ex)
			{
				Facilities.LogLine(this, $"Exception while setting {propertyName} to \"{propertyValue}\": {ex}");
			}

			return Result.Success();
		}

		private Result ExecuteConfigGet(string? parameters)
		{
			if (parameters == null)
				return Result.Success();

			var terms = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (terms.Length != 1)
				return Result.Success();

			var propertyName = terms[0];

			try
			{
				var property = Facilities.Configuration.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (property == null)
					return Result.Success();

				return Result.Success(GetTexts(property.Name, property.GetValue(Facilities.Configuration)));
			}
			catch (Exception ex)
			{
				Facilities.LogLine(this, $"Exception while getting {propertyName}: {ex}");
			}

			return Result.Success();
		}

		private static bool SetValue<T>(PropertyInfo property, T value)
		{
			try
			{
				property.SetValue(Facilities.Configuration, value);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private static ICollection<string>? GetTexts(string propertyName, object? value)
			=> Facilities.CommandTextsMap[CommandID.ConfigGet, Default]
			.ReplaceInAll("{setting}", propertyName)
			.ReplaceInAll("{value}", ObjectDumper.Dump(value));

		private static ICollection<string>? GetTexts(string propertyName, object? oldValue, object? newValue)
			=> Facilities.CommandTextsMap[CommandID.ConfigSet, Default]
			.ReplaceInAll("{setting}", propertyName)
			.ReplaceInAll("{oldvalue}", ObjectDumper.Dump(oldValue)[..^1])
			.ReplaceInAll("{newvalue}", ObjectDumper.Dump(newValue));

		public static string[] GetPublicPropertyNames<TType>()
			=> typeof(TType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(property => property.CanWrite && property.CanRead)
			.Select(p => p.Name).ToArray();
	}
}
