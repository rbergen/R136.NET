using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace R136.Web.Tools
{
	public static class ExtensionMethods
	{
		private static MarkdownPipeline? _pipeline = null;
		private static readonly object _pipelineLock = new object();

		public static string ToMarkupString(this StringValues texts)
		{
			if (texts == StringValues.Empty)
				return string.Empty;

			var markdown = Markdown.ToHtml(string.Join('\n', texts), Pipeline);
			
			if (markdown.StartsWith("<p>") && markdown.LastIndexOf("<p>") == 0)
				markdown = markdown[3..^5];

			return markdown;
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

		public static IServiceCollection AddLanguageProvider(this IServiceCollection serviceCollection)
			=> serviceCollection.AddScoped<ILanguageProvider>(sp =>
				new LanguageProvider
				{
					Services = sp
				});
	}
}

#nullable restore