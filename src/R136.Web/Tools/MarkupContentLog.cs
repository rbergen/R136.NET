using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using R136.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

#nullable enable

namespace R136.Web.Tools
{
	public class MarkupContentLog : IEnumerable<ContentBlock>, ISnappable<MarkupContentLog.Snapshot>
	{
		private const int DefaultMaxBlockCount = 100;
		private const int DefaultSaveBlockCount = 20;

		private readonly List<ContentBlock> blocks = new();
		public int MaxBlockCount { get; set; } = DefaultMaxBlockCount;
		public int SaveBlockCount { get; set; } = DefaultSaveBlockCount;
		public bool IsTrimmed { get; private set; } = false;

		public IEnumerator<ContentBlock> GetEnumerator()
			=> this.blocks.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable)this.blocks).GetEnumerator();

		public void Add(ContentBlockType type, ResultCode resultCode, StringValues texts)
			=> AddBlock(type, resultCode, texts);

		public void Add(ContentBlockType type, StringValues texts)
			=> AddBlock(type, null, texts);

		public void AddRaw(ContentBlockType type, StringValues text)
		{
			this.blocks.Add(new()
			{
				Type = type,
				Text = text
			});

			Trim();
		}

		private void AddBlock(ContentBlockType type, ResultCode? resultCode, StringValues texts)
		{
			if (texts == StringValues.Empty)
				return;

			this.blocks.Add(new()
			{
				Type = type,
				ResultCode = resultCode,
				Text = texts.ToMarkupString()
			});

			Trim();
		}

		public int Count
			=> this.blocks.Count;

		private void Trim()
		{
			while (this.blocks.Count > MaxBlockCount)
			{
				this.blocks.RemoveAt(0);
				IsTrimmed = true;
			}
		}

		public class Snapshot : ISnapshot
		{
			private const int BytesBaseSize = 1;

			public bool IsTrimmed { get; set; }
			public ContentBlock[]? ContentBlocks { get; set; }

			public void AddBytes(List<byte> bytes)
			{
				if (ContentBlocks == null)
				{
					IsTrimmed.AddByte(bytes);
					ContentBlocks.AddSnapshotsBytes(bytes);
					return;
				}

				List<ContentBlock> blocks = new(ContentBlocks
					.Reverse()
					.SkipWhile(block => block.Type == ContentBlockType.LanguageSwitch)
					.TakeWhile(block => block.Type != ContentBlockType.Input)
					.Reverse()
				);

				if (ContentBlocks.Length > 0 && ContentBlocks[^1].Type == ContentBlockType.LanguageSwitch)
					blocks.Add(ContentBlocks[^1]);

				(IsTrimmed || blocks.FirstOrDefault() != ContentBlocks.FirstOrDefault()).AddByte(bytes);
				blocks.ToArray().AddSnapshotsBytes(bytes);
			}

			public int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				if (bytes.Length <= BytesBaseSize)
					return null;

				IsTrimmed = bytes.Span[0].ToBool();

				int? bytesRead;
				(ContentBlocks, bytesRead) = bytes[BytesBaseSize..].ToSnapshotArrayOf<ContentBlock>();

				return bytesRead != null ? bytesRead + BytesBaseSize : null;
			}
		}

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
			=> new()
			{
				ContentBlocks = this.blocks.TakeLast(SaveBlockCount).ToArray(),
				IsTrimmed = IsTrimmed || this.blocks.Count > SaveBlockCount
			};

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			this.blocks.Clear();

			if (snapshot.ContentBlocks != null)
				this.blocks.AddRange(snapshot.ContentBlocks);

			IsTrimmed = snapshot.IsTrimmed;

			return true;
		}

		public ContentBlock this[int index]
			=> this.blocks[index];
	}

	public class ContentBlock : ISnapshot
	{
		private const int BytesBaseSize = 2;

		public ContentBlockType Type { get; set; }
		public ResultCode? ResultCode { get; set; }
		public string? Text { get; set; }

		[JsonIgnore]
		public MarkupString Content => (MarkupString)(Text ?? string.Empty);

		public void AddBytes(List<byte> bytes)
		{
			Type.AddByte(bytes);
			ResultCode.AddEnumByte(bytes);
			Text.AddTextBytes(bytes);
		}

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			if (bytes.Length <= BytesBaseSize)
				return null;

			var span = bytes.Span;

			Type = span[0].To<ContentBlockType>();
			ResultCode = span[1].ToNullable<ResultCode>();

			int? readBytes;
			(Text, readBytes) = bytes[BytesBaseSize..].ToText();

			return readBytes != null ? readBytes + BytesBaseSize : null;
		}
	}

	public enum ContentBlockType : byte
	{
		StartMessage,
		RoomStatus,
		AnimateStatus,
		Input,
		RunResult,
		LanguageSwitch
	}
}

#nullable restore