using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace R136.Shell.Tools
{
	public static class ExtensionMethods
	{
		private static MarkdownPipeline? _pipeline = null;

		public static string ToPlainText(this StringValues texts)
		{
			if (texts == StringValues.Empty)
				return string.Empty;

			return Markdown.ToPlainText(string.Join('\n', texts), Pipeline);
		}

		private static MarkdownPipeline Pipeline
		{
			get
			{
				if (_pipeline == null)
					_pipeline = new MarkdownPipelineBuilder().UseSoftlineBreakAsHardlineBreak().Build();

				return _pipeline;
			}
		}
	}
}
