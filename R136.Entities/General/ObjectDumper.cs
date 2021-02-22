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

    private int _level;
    private readonly int _indentSize;
    private readonly StringBuilder _stringBuilder;
    private readonly List<int> _hashListOfFoundElements;

    private ObjectDumper(int indentSize)
    {
      _indentSize = indentSize;
      _stringBuilder = new StringBuilder();
      _hashListOfFoundElements = new List<int>();
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
          
          _hashListOfFoundElements.Add(element.GetHashCode());
          _level++;
        }

				if (element is IEnumerable enumerableElement)
					WriteEnumerableElement(enumerableElement);

				else
					WriteElement(element);

				if (!typeof(IEnumerable).IsAssignableFrom(objectType))
          _level--;
      }

      return _stringBuilder.ToString();
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
					var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
					Write($"{memberInfo.Name}: {(isEnumerable ? EnumerableMarker : ClassMarker)}");

					var alreadyTouched = !isEnumerable && AlreadyTouched(value);
					_level++;

					if (!alreadyTouched)
						DumpElement(value);

					else if (WriteBidirectionalReferences)
						Write($"{{{value!.GetType().FullName}}} {BidirectionalReferenceMarker}");

					_level--;
				}
			}
		}

		private void WriteEnumerableElement(IEnumerable enumerableElement)
		{
			foreach (object item in enumerableElement)
			{
				if (item is IEnumerable && !(item is string))
				{
					_level++;
					DumpElement(item);
					_level--;
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
      for (var i = 0; i < _hashListOfFoundElements.Count; i++)
      {
        if (_hashListOfFoundElements[i] == hash)
          return true;
      }

      return false;
    }

    private void Write(string value)
      => _stringBuilder.AppendLine(new string(' ', _level * _indentSize) + value);

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