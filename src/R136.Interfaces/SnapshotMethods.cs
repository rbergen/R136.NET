using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace R136.Interfaces
{
	public static class SnapshotMethods
	{
		private const int SizeBlockLength = 5;

		private static bool AddPresenceByte<TValue>(this TValue? value, List<byte> bytes)
		{
			if (value == null)
			{
				Presence.Null.AddByte(bytes);
				return false;
			}

			if ((value is ICollection collection && collection.Count == 0) || (value is string text && text.Length == 0))
			{
				Presence.Empty.AddByte(bytes);
				return false;
			}

			Presence.Present.AddByte(bytes);
			
			return true;
		}

		private static Presence ReadPresenceByte(this ReadOnlyMemory<byte> bytes)
			=> bytes.Length == 0 ? Presence.Error : bytes.Span[0].To<Presence>();

		private static (Presence presence, int count, int bytesRead) ReadSizeBlock(this ReadOnlyMemory<byte> bytes)
			=> bytes.ReadPresenceByte() switch
			{
				Presence.Null => (Presence.Null, 0, 1),
				Presence.Empty => (Presence.Empty, 0, 1),
				Presence.Present => bytes.Length < SizeBlockLength ? (Presence.Error, 0, 0) : (Presence.Present, BitConverter.ToInt32(bytes[1..SizeBlockLength].Span), SizeBlockLength),
				_ => (Presence.Error, 0, 0)
			};

		public static void AddByte<TEnum>(this TEnum value, List<byte> bytes) where TEnum : Enum
			=> bytes.Add(Convert.ToByte(value));

		public static TEnum To<TEnum>(this byte value) where TEnum : Enum
			=> (TEnum)Enum.ToObject(typeof(TEnum), value);

		public static void AddEnumByte<TEnum>(this TEnum? value, List<byte> bytes) where TEnum : struct, Enum
			=> bytes.Add(value != null ? Convert.ToByte(value.Value) : (byte)Presence.Null);

		public static TEnum? ToNullable<TEnum>(this byte value) where TEnum : struct, Enum
			=> value == (byte)Presence.Null ? null : To<TEnum>(value);

		public static void AddBytes(this int value, List<byte> bytes)
			=> bytes.AddRange(BitConverter.GetBytes(value));

		public static (int value, int? bytesRead) ToInt(this ReadOnlyMemory<byte> bytes)
			=> bytes.Length >= 4 ? (BitConverter.ToInt32(bytes[0..4].Span), 4) : (0, null);

		public static void AddIntBytes(this int? value, List<byte> bytes)
		{
			if (value.AddPresenceByte(bytes))
				bytes.AddRange(BitConverter.GetBytes(value!.Value));
		}

		public static (int? value, int? bytesRead) ToNullableInt(this ReadOnlyMemory<byte> bytes)
		{
			(Presence presence, int count, int bytesRead) = bytes.ReadSizeBlock();

			return presence switch
			{
				Presence.Null => (null, bytesRead),
				Presence.Present => (count, bytesRead),
				_ => (null, null)
			}; 
		}

		public static void AddByte(this bool value, List<byte> bytes)
			=> bytes.Add((byte)(value ? 1 : 0));

		public static bool ToBool(this byte value)
			=> value != 0;

		public static void AddTextBytes(this string? value, List<byte> bytes)
		{
			if (!value.AddPresenceByte(bytes))
				return;

			byte[] textBytes = Encoding.UTF8.GetBytes(value!);

			bytes.AddRange(BitConverter.GetBytes(textBytes.Length));
			bytes.AddRange(textBytes);
		}

		public static (string? text, int? readBytes) ToText(this ReadOnlyMemory<byte> bytes)
		{
			(Presence presence, int count, int bytesRead) = bytes.ReadSizeBlock();

			switch (presence)
			{
				case Presence.Null:
					return (null, bytesRead);

				case Presence.Empty:
					return (string.Empty, bytesRead);

				case Presence.Present:
					int totalBytes = bytesRead + count;

					if (count > 0 && bytes.Length >= totalBytes)
					{
						try
						{
							return (Encoding.UTF8.GetString(bytes[bytesRead..totalBytes].Span), totalBytes);
						}
						catch (Exception) { }
					}

					break;
			}

			return (null, null);
		}

		public static void AddSnapshotBytes<TSnapshot>(this TSnapshot? value, List<byte> bytes) where TSnapshot : class, ISnapshot
		{
			if (value.AddPresenceByte(bytes))
				value!.AddBytes(bytes);
		}

		public static (TSnapshot? snapshot, int? bytesRead) ToNullable<TSnapshot>(this ReadOnlyMemory<byte> bytes) where TSnapshot : class, ISnapshot, new()
		{
			switch (bytes.ReadPresenceByte())
			{
				case Presence.Null:
					return (null, 1);

				case Presence.Present:
					TSnapshot snapshot = new();
					int? bytesRead = snapshot.LoadBytes(bytes[1..]);

					if (bytesRead != null)
						return (snapshot, bytesRead.Value + 1);

					break;
			}

			return (null, null);
		}

		private static void AddArrayBytes<TValue>(this TValue[]? values, List<byte> bytes, Action<List<byte>, TValue> converter)
		{
			if (!values.AddPresenceByte(bytes))
				return;

			bytes.AddRange(BitConverter.GetBytes(values!.Length));

			foreach (var value in values!)
				converter(bytes, value);
		}

		private static (TValue[]? array, int? bytesRead) BytesToArray<TValue>(ReadOnlyMemory<byte> bytes, Func<ReadOnlyMemory<byte>, (TValue?, int?)> converter)
		{
			(Presence presence, int count, int? bytesRead) = bytes.ReadSizeBlock();

			switch (presence)
			{
				case Presence.Null:
					return (null, bytesRead);

				case Presence.Empty:
					return (Array.Empty<TValue>(), bytesRead);

				case Presence.Present:
					List<TValue> values = new();

					TValue? value;
					int totalBytesRead = bytesRead.Value;

					while (count-- > 0 && bytes.Length > bytesRead.Value)
					{
						bytes = bytes[bytesRead.Value..];

						(value, bytesRead) = converter(bytes);

						if (bytesRead == null)
							return (null, null);

						if (value != null)
							values.Add(value);

						totalBytesRead += bytesRead.Value;
					}

					// check if we didn't run out of bytes while reading array elements
					if (count == -1)
						return (values.ToArray(), totalBytesRead);

					break;
			}

			return (null, null);
		}

		public static void AddEnumsBytes<TEnum>(this TEnum[]? values, List<byte> bytes) where TEnum : Enum
			=> AddArrayBytes(values, bytes, (list, value) => value.AddByte(bytes));

		public static (TEnum[]? enums, int? bytesRead) ToEnumArrayOf<TEnum>(this ReadOnlyMemory<byte> bytes) where TEnum : Enum
			=> BytesToArray(bytes, bytes => (bytes.Span[0].To<TEnum>(), 1));

		public static void AddIntsBytes(this int[]? values, List<byte> bytes)
			=> AddArrayBytes(values, bytes, (list, value) => value.AddBytes(bytes));

		public static (int[]? values, int? bytesRead) ToIntArray(this ReadOnlyMemory<byte> bytes)
			=> BytesToArray(bytes, bytes => bytes.ToInt());

		public static void AddTextsBytes(this string[]? values, List<byte> bytes)
			=> AddArrayBytes(values, bytes, (list, value) => value.AddTextBytes(bytes));

		public static (string[]? texts, int? bytesRead) ToTextArray(this ReadOnlyMemory<byte> bytes)
			=> BytesToArray(bytes, bytes => ToText(bytes));

		public static void AddSnapshotsBytes<TSnapshot>(this TSnapshot[]? values, List<byte> bytes) where TSnapshot : ISnapshot
			=> AddArrayBytes(values, bytes, (list, value) => value.AddBytes(bytes));

		public static (TSnapshot[]? snapshots, int? bytesRead) ToSnapshotArrayOf<TSnapshot>(this ReadOnlyMemory<byte> values) where TSnapshot : class, ISnapshot, new()
			=> BytesToArray(values, bytes =>
			{
				TSnapshot snapshot = new();
				int? bytesRead = snapshot.LoadBytes(bytes);

				return bytesRead != null ? (snapshot, bytesRead) : (null, null);
			});

		public static TResult ProcessIntermediateResult<TResult>(this (TResult item, int? bytesRead) result, ref ReadOnlyMemory<byte> bytes, ref int totalBytesRead, ref bool abort)
		{
			if (result.bytesRead == null || bytes.Length == result.bytesRead)
			{
				abort = true;
				return result.item;
			}

			bytes = bytes[result.bytesRead.Value..];
			totalBytesRead += result.bytesRead.Value;

			return result.item;
		}

		private enum Presence : byte
		{
			Empty = 0,
			Present = 1,
			Null = 255,
			Error = 254
		}
	}
}
