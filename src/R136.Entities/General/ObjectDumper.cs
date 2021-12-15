using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace R136.Entities.General
{
	public class ObjectDumper
	{
		public static string Null { get; set; } = "null";
		public static string ClassMarker { get; set; } = "{ }";
		public static string EnumerableMarker { get; set; } = "...";
		public static string BidirectionalReferenceMarker { get; set; } = "**";
		public static bool WriteClassType { get; set; } = true;
		public static bool WriteBidirectionalReferences { get; set; } = true;

		private int level;
		private readonly int indentSize;
		private readonly StringBuilder stringBuilder;
		private readonly List<int> hashListOfFoundElements;

		private ObjectDumper(int indentSize)
		{
			this.indentSize = indentSize;
			this.stringBuilder = new StringBuilder();
			this.hashListOfFoundElements = new List<int>();
		}

		public static string Dump(object? element)
			=> Dump(element, 2);

		public static string Dump(object? element, int indentSize)
			=> (new ObjectDumper(indentSize)).DumpElement(element);

		private string DumpElement(object? element)
		{
			if (element == null || element is ValueType || element is string)
				Write(FormatValue(element));

			else
			{
				var objectType = element.GetType();
				if (!typeof(IEnumerable).IsAssignableFrom(objectType))
				{
					if (WriteClassType)
						Write($"{{{objectType.FullName}}}");

					this.hashListOfFoundElements.Add(element.GetHashCode());
					this.level++;
				}

				if (element is IEnumerable enumerableElement)
					WriteEnumerableElement(enumerableElement);

				else
					WriteElement(element);

				if (!typeof(IEnumerable).IsAssignableFrom(objectType))
					this.level--;
			}

			return this.stringBuilder.ToString().Trim();
		}

		private void WriteElement(object element)
		{
			MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
			foreach (var memberInfo in members)
			{
				var fieldInfo = memberInfo as FieldInfo;
				var propertyInfo = memberInfo as PropertyInfo;

				if (fieldInfo == null && propertyInfo == null)
					continue;

				var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo!.PropertyType;
				object? value;
				try
				{
					value = fieldInfo != null
														? fieldInfo.GetValue(element)
														: propertyInfo!.GetValue(element, null);
				}
				catch (Exception)
				{
					continue;
				}

				if (type.IsValueType || type == typeof(string))
					Write($"{memberInfo.Name}: {FormatValue(value)}");

				else
				{
					bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
					Write($"{memberInfo.Name}: {(isEnumerable ? EnumerableMarker : ClassMarker)}");

					bool alreadyTouched = !isEnumerable && AlreadyTouched(value);
					this.level++;

					if (!alreadyTouched)
						DumpElement(value);

					else if (WriteBidirectionalReferences)
						Write($"{{{value!.GetType().FullName}}} {BidirectionalReferenceMarker}");

					this.level--;
				}
			}
		}

		private void WriteEnumerableElement(IEnumerable enumerableElement)
		{
			foreach (object item in enumerableElement)
			{
				if (item is IEnumerable && !(item is string))
				{
					this.level++;
					DumpElement(item);
					this.level--;
				}
				else
				{
					if (!AlreadyTouched(item))
						DumpElement(item);

					else if (WriteBidirectionalReferences)
						Write($"{{{item.GetType().FullName}}} {BidirectionalReferenceMarker}");
				}
			}
		}

		private bool AlreadyTouched(object? value)
		{
			if (value == null)
				return false;

			var hash = value.GetHashCode();
			for (int i = 0; i < this.hashListOfFoundElements.Count; i++)
			{
				if (this.hashListOfFoundElements[i] == hash)
					return true;
			}

			return false;
		}

		private void Write(string value)
			=> this.stringBuilder.AppendLine(new string(' ', this.level * this.indentSize) + value);

		private static string FormatValue(object? o)
			=> o == null ? Null : o switch
			{
				DateTime time => time.ToShortDateString(),
				string s => s,
				char c when c == '\0' => string.Empty,
				ValueType => o.ToString() ?? string.Empty,
				IEnumerable => EnumerableMarker,
				_ => ClassMarker
			};
	}
}