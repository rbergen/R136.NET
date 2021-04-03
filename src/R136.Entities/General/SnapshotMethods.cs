using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.General
{
	public static class SnapshotMethods
	{
		private const byte NullValue = 255;

		public static TEnum To<TEnum>(this byte value) where TEnum : Enum
			=> (TEnum)Convert.ChangeType(value, typeof(TEnum));

		public static byte ToByte<TEnum>(this TEnum value) where TEnum : Enum
			=> Convert.ToByte(value);

		public static byte EnumToByte<TEnum>(this TEnum? value) where TEnum : struct, Enum
			=> value != null ? ToByte(value.Value) : NullValue;

		public static TEnum? ToNullable<TEnum>(this byte value) where TEnum : struct, Enum
			=> value == NullValue ? null : To<TEnum>(value);

		public static byte IntToByte(this int? value)
			=> value.HasValue ? Convert.ToByte(value) : NullValue;

		public static int? ToNullableInt(this byte value)
			=> value == NullValue ? null : Convert.ToInt32(value);

		public static byte ToByte(this bool value)
			=> (byte)(value ? 1 : 0);

		public static bool ToBool(this byte value)
			=> value != 0;

		public static byte ToByte(this int value)
			=> Convert.ToByte(value);

		public static byte[] EnumsToBytes<TEnum>(this TEnum[]? values) where TEnum : Enum
		{
			if (values == null)
				return new byte[1] { NullValue };

			byte[] bytes = new byte[values.Length + 1];

			bytes[0] = Convert.ToByte(values.Length);

			int i = 1;
			foreach (TEnum value in values)
				bytes[i] = value.ToByte();

			return bytes;
		}

		public static (TEnum[]? enums, int? bytesRead) ToEnumArrayOf<TEnum>(this Span<byte> values) where TEnum : Enum
		{
			if (values.Length == 0 || values.Length < values[0] + 1)
				return (null, null);

			if (values[0] == NullValue)
				return (null, 1);

			TEnum[] result = new TEnum[values[0]];

			int i;
			for (i = 0; i < result.Length; i++)
				result[i] = values[i + 1].To<TEnum>();

			return (result, i + 1);
		}

		public static byte[] ToBytes<TSnapshot>(this TSnapshot? value) where TSnapshot : ISnapshot
		{
			if (value == null)
				return new byte[1] { NullValue };

			List<byte> result = new();
			result.Add(1);
			result.AddRange(value.GetBinary());

			return result.ToArray();
		}

		public static (TSnapshot? snapshot, int? bytesRead) ToNullable<TSnapshot>(this Span<byte> value) where TSnapshot : class, ISnapshot, new()
		{
			if (value.Length == 0)
				return (null, null);

			if (value[0] == NullValue)
				return (null, 1);

			TSnapshot snapshot = new();
			int? bytesRead = snapshot.SetBinary(value[1..]);

			return bytesRead != null ? (snapshot, bytesRead) : (null, null);
		}

		public static byte[] SnapshotsToBytes<TSnapshot>(this TSnapshot[]? values) where TSnapshot : ISnapshot
		{
			if (values == null)
				return new byte[1] { NullValue };

			if (values.Length == 0)
				return new byte[1] { 0 };

			List<byte> snapshotBytes = new();
			foreach (TSnapshot value in values)
				snapshotBytes.AddRange(value.GetBinary());

			snapshotBytes.Insert(0, Convert.ToByte(values.Length));

			return snapshotBytes.ToArray();
		}

		public static (TSnapshot[]? snapshots, int? bytesRead) ToSnapshotArrayOf<TSnapshot>(this Span<byte> values) where TSnapshot : ISnapshot, new()
		{
			if (values.Length == 0)
				return (null, null);

			if (values[0] == NullValue)
				return (null, 1);

			if (values[0] == 0)
				return (Array.Empty<TSnapshot>(), 1);

			List<TSnapshot> snapshots = new();

			TSnapshot snapshot;
			int? bytesRead = 1;
			int totalBytesRead = bytesRead.Value;

			int count = values[0];
			while (count-- > 0 && values.Length > bytesRead.Value)
			{
				values = values[bytesRead.Value..];

				snapshot = new();
				bytesRead = snapshot.SetBinary(values);
				if (bytesRead == null)
					return (null, null);

				snapshots.Add(snapshot);
				totalBytesRead += bytesRead.Value;
			}

			return count == 0 ? (snapshots.ToArray(), totalBytesRead) : (null, null);
		}

		public static byte[] TextToBytes(this string? value)
		{
			if (value == null)
				return new byte[1] { NullValue };

			List<byte> result = new();
			result.Add(1);
			result.AddRange(Encoding.UTF8.GetBytes(value));
			result.InsertRange(1, BitConverter.GetBytes(result.Count));

			return result.ToArray();
		}

		public static (string? text, int? readBytes) ToText(this Span<byte> value)
		{
			if (value.Length == 0)
				return (null, null);

			if (value[0] == NullValue)
				return (null, 1);

			if (value.Length < 5)
				return (null, null);

			int stringByteLength = BitConverter.ToInt32(value[1..5]);
			if (stringByteLength < 0 || value.Length < 5 + stringByteLength)
				return (null, null);

			value = value[5..(stringByteLength + 5)];

			try
			{
				return (Encoding.UTF8.GetString(value), stringByteLength + 5);
			}
			catch (Exception) { }

			return (null, null);
		}
	}
}
