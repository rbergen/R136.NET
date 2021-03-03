using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using R136.Interfaces;
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

		private readonly List<ContentBlock> _blocks = new();
		public int MaxBlockCount { get; set; } = DefaultMaxBlockCount;
		public int SaveBlockCount { get; set; } = DefaultSaveBlockCount;

		public bool IsTrimmed { get; private set; } = false;

		public IEnumerator<ContentBlock> GetEnumerator()
			=> _blocks.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable)_blocks).GetEnumerator();

		public void Add(ContentBlockType type, ResultCode resultCode, StringValues texts)
			=> AddBlock(type, resultCode, texts);

		public void Add(ContentBlockType type, StringValues texts)
			=> AddBlock(type, null, texts);

		public void AddRaw(ContentBlockType type, StringValues text)
		{
			_blocks.Add(new ContentBlock()
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

			_blocks.Add(new ContentBlock()
			{
				Type = type,
				ResultCode = resultCode,
				Text = texts.ToMarkupString()
			});

			Trim();
		}

		public bool IsEmpty
			=> _blocks.Count == 0;

		public bool IsSingleBlock
			=> _blocks.Count == 1;

		public ContentBlock LastBlock
			=> _blocks[^1];

		public IEnumerable<ContentBlock> AllButLast()
			=> _blocks.DropLast(1);

		public int Count
			=> _blocks.Count;

		private void Trim()
		{
			while (_blocks.Count > MaxBlockCount)
			{
				_blocks.RemoveAt(0);
				IsTrimmed = true;
			}
		}

		public class Snapshot
		{
			public bool IsTrimmed { get; set; }
			public ContentBlock[]? ContentBlocks { get; set; }
		}

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
			=> new()
			{
				ContentBlocks = _blocks.TakeLast(SaveBlockCount).ToArray(),
				IsTrimmed = IsTrimmed || _blocks.Count > SaveBlockCount
			};

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			_blocks.Clear();

			if (snapshot.ContentBlocks != null)
				_blocks.AddRange(snapshot.ContentBlocks);
			
			IsTrimmed = snapshot.IsTrimmed;

			return true;
		}

		public ContentBlock this[int index]
			=> _blocks[index];
}

	public class ContentBlock
	{
		public ContentBlockType Type { get; set; }
		public ResultCode? ResultCode { get; set; }
		public string? Text { get; set; }

		[JsonIgnore]
		public MarkupString Content => (MarkupString)(Text ?? string.Empty);
	}

	public enum ContentBlockType
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