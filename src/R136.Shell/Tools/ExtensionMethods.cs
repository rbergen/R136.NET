using Markdig;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace R136.Shell.Tools
{
	public static class ExtensionMethods
	{
		private static MarkdownPipeline? _pipeline = null;

		public static string ToPlainText(this StringValues texts)
		{
			if (texts == StringValues.Empty)
				return string.Empty;

			return string.Join('\n', texts.Select(text => text == string.Empty ? text : Markdown.ToPlainText(text, Pipeline).Trim()));
		}

		private static MarkdownPipeline Pipeline
		{
			get
			{
				if (_pipeline == null)
					_pipeline = new MarkdownPipelineBuilder()
						.UseSoftlineBreakAsHardlineBreak()
						.Build();

				return _pipeline;
			}
		}

		public static void AddIfNotEmpty(this IList<StringValues> list, StringValues value)
		{
			if (value != StringValues.Empty)
				list.Add(value);
		}
	}
}
