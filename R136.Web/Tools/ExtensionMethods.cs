using Markdig;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace R136.Web.Tools
{
	public static class ExtensionMethods
	{
		private static MarkdownPipeline? _pipeline = null;
		private static readonly object _pipelineLock = new object();

		public static MarkupString ToMarkupString(this IEnumerable<string> texts)
		{
			var markdown = Markdown.ToHtml(string.Join('\n', texts), Pipeline);
			
			if (markdown.StartsWith("<p>") && markdown.LastIndexOf("<p>") == 0)
				markdown = markdown[3..^5];

			return (MarkupString)markdown;
		}

		private static MarkdownPipeline Pipeline
		{
			get
			{
				lock (_pipelineLock)
				{
					if (_pipeline == null)
						_pipeline = new MarkdownPipelineBuilder().UseSoftlineBreakAsHardlineBreak().Build();

					return _pipeline;
				}
			}
		}

		public static IEnumerable<T> DropLast<T>(this IEnumerable<T> source, int n)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			if (n < 0)
				throw new ArgumentOutOfRangeException(nameof(n),
						"Argument n should be non-negative.");

			return InternalDropLast(source, n);
		}

		private static IEnumerable<T> InternalDropLast<T>(IEnumerable<T> source, int n)
		{
			Queue<T> buffer = new Queue<T>(n + 1);

			foreach (T x in source)
			{
				buffer.Enqueue(x);

				if (buffer.Count == n + 1)
					yield return buffer.Dequeue();
			}
		}
	}
}

#nullable restore