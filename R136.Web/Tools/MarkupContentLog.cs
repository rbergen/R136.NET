using Markdig;
using Microsoft.AspNetCore.Components;
using R136.Interfaces;
using R136.Web.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace R136.Web.Tools
{
	public class MarkupContentLog : IEnumerable<ContentBlock>
	{
		private const int DefaultMaxBlockCount = 10;
		
		private readonly List<ContentBlock> _blocks = new List<ContentBlock>();
		private readonly int _maxBlockCount;

		public MarkupContentLog(int maxBlockCount)
			=> _maxBlockCount = maxBlockCount;

		public MarkupContentLog() : this(DefaultMaxBlockCount) { }

		public bool IsTrimmed { get; private set; } = false;

		public IEnumerator<ContentBlock> GetEnumerator()
			=> _blocks.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable)_blocks).GetEnumerator();

		public void Add(ContentBlockType type, ResultCode resultCode, IEnumerable<string>? texts)
			=> AddBlock(type, resultCode, texts);

		public void Add(ContentBlockType type, IEnumerable<string>? texts)
			=> AddBlock(type, null, texts);

		public void Add(ContentBlockType type, ResultCode resultCode, string text)
			=> AddBlock(type, resultCode, new string[] { text });

		public void Add(ContentBlockType type, string text)
			=> AddBlock(type, null, new string[] { text });

		public void AddRaw(ContentBlockType type, string text)
		{
			_blocks.Add(new ContentBlock(type, null, (MarkupString)text));
			Trim();
		}

		private void AddBlock(ContentBlockType type, ResultCode? resultCode, IEnumerable<string>? texts)
		{
			if (texts == null || !texts.Any())
				return;

			_blocks.Add(new ContentBlock(type, resultCode, texts.ToMarkupString()));

		}

		public bool IsEmpty
			=> _blocks.Count == 0;

		public bool IsSingleBlock
			=> _blocks.Count == 1;

		public ContentBlock LastBlock
			=> _blocks[^1];

		public IEnumerable<ContentBlock> AllButLast()
		{
			return _blocks.DropLast(1);
		}

		public int Count
			=> _blocks.Count;

		private void Trim()
		{
			while (_blocks.Count > _maxBlockCount)
			{
				_blocks.RemoveAt(0);
				IsTrimmed = true;
			}
		}

		public ContentBlock this[int index]
			=> _blocks[index];
}

	public class ContentBlock
	{
		public ContentBlockType Type { get; private set; }
		public ResultCode? ResultCode { get; private set; }
		public MarkupString Content { get; private set; }

		public ContentBlock(ContentBlockType type, MarkupString content) : this(type, null, content) { }

		public ContentBlock(ContentBlockType type, ResultCode? resultCode, MarkupString content)
			=> (Type, ResultCode, Content) = (type, resultCode, content);
	}

	public enum ContentBlockType
	{
		StartMessage,
		RoomStatus,
		AnimateStatus,
		Input,
		RunResult
	}
}

#nullable restore