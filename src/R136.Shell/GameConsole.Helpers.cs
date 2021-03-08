using Microsoft.Extensions.Primitives;
using R136.Interfaces;
using R136.Shell.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace R136.Shell
{
	partial class GameConsole
	{
		private void SaveStatus(InputSpecs? inputSpecs)
		{
			if (_status == null)
				return;

			_status.InputSpecs = inputSpecs;
			_status.EngineSnapshot = _engine!.TakeSnapshot();
			_status.Texts = _texts.ToArray();

			_status.Save();
		}

		private void RestoreStatus()
		{
			if (_status?.EngineSnapshot != null && _engine != null)
				_engine.RestoreSnapshot(_status.EngineSnapshot);

			if (_status?.Texts != null)
			{
				_texts.Clear();
				foreach (string text in _status.Texts)
				{
					WritePlainText(text);
					_texts.Enqueue(text);
				}
			}

			if (_status?.Pausing ?? false)
				WaitForKey();
		}

		private void WaitForKey()
		{
			ConsoleColor color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;

			Console.Write(_languages?.GetConfigurationValue(Constants.ProceedText) ?? Constants.ProceedText);
			Console.ReadKey();

			Console.ForegroundColor = color;

			(int left, int top) = Console.GetCursorPosition();

			ClearLine(top, left + 1);

			if (_status != null)
				_status.Pausing = false;
		}

		private static void ClearLine(int row, int length)
		{
			Console.SetCursorPosition(0, row);
			Console.Write(new string(' ', length));
			Console.SetCursorPosition(0, row);
		}

		private static string GetInput(out int totalCharacters)
		{
			ConsoleColor color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;

			Console.Write(Constants.Prompt);

			string input = Console.ReadLine() ?? string.Empty;
			totalCharacters = input.Length + Constants.Prompt.Length;

			Console.ForegroundColor = color;

			return input;
		}

		private static StringValues FilterHTML(StringValues message)
		{
			if (message.Count != 1 || !((string)message).StartsWith("<table>"))
				return message;

			string messageText = message;

			messageText = Regex.Replace(messageText, @"<h\d>", "**");
			messageText = Regex.Replace(messageText, @"</h\d>", "**\n");
			messageText = Regex.Replace(messageText, @"<br />", "\n");
			messageText = Regex.Replace(messageText, @"<br/>", ", ");
			messageText = Regex.Replace(messageText, @"<tr.*?>", "\n");
			messageText = Regex.Replace(messageText, @"&nbsp;", " ");
			messageText = Regex.Replace(messageText, @"&[a-zA-Z]+?;", string.Empty);
			messageText = Regex.Replace(messageText, @"<[^>]+?>", string.Empty);

			return messageText.Split('\n');
		}

		private void WriteMessage(StringValues message)
			=> WriteText(FilterHTML(message).Append(string.Empty).ToArray());

		private void WriteMessages()
		{
			var lines = new List<string>();
			foreach (var message in _messages)
			{
				lines.AddRange(FilterHTML(message));
				lines.Add(string.Empty);
			}

			WriteText(lines.ToArray());
			_messages.Clear();
		}

		private void WriteText(StringValues texts)
		{
			if (StringValues.IsNullOrEmpty(texts))
				return;

			string plainText = texts.ToPlainText();
			WritePlainText(plainText);
			_texts.Enqueue(plainText);

			while (_texts.Count > 20)
				_texts.Dequeue();
		}

		private void WritePlainText(string plainText)
		{
			int consoleWidth = Console.WindowWidth;
			int rowCountDown = Console.WindowHeight - 1;

			foreach (string item in plainText.Split('\n'))
			{
				string plainLine = item;
				while (plainLine.Length >= consoleWidth)
				{
					int lastFittingSpace = plainLine.LastIndexOf(' ', consoleWidth - 1);
					if (lastFittingSpace <= 0)
						break;

					WritePlainLine(plainLine[..(lastFittingSpace)], ref rowCountDown);
					plainLine = plainLine[(lastFittingSpace + 1)..];
				}

				WritePlainLine(plainLine, ref rowCountDown);
			}
		}

		private void WritePlainLine(string plainLine, ref int rowCountDown)
		{
			Console.WriteLine(plainLine);
			if (--rowCountDown == 0)
			{
				WaitForKey();
				rowCountDown = Console.WindowHeight - 1;
			}
		}

		private void ShowLanguageSwitchInstructions()
		{
			var strings = new List<string>();
			var languageSections = _configuration!.GetSection(Constants.Languages).GetChildren();
			string codes = string.Join(", ", languageSections.Select(cs => cs.Key));

			foreach (var section in languageSections)
				strings.Add(section[Constants.LanguageSwitchInstructionText].Replace("{codes}", codes));

			_messages.Add(strings.ToArray());
		}
	}
}
