﻿using Microsoft.Extensions.Primitives;
using R136.Interfaces;
using R136.Shell.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace R136.Shell
{
	partial class GameConsole
	{
		private static readonly Random _random = new();

		private long _bpsPrintDelay;
		private int _bpsPrintCount;
		private bool _bpsPrintEnabled;
		
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
				foreach (string text in string.Join("\n", _status.Texts).Split('\n').TakeLast(Console.WindowHeight))
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

			BPSPrint(_languages?.GetConfigurationValue(Constants.ProceedText) ?? Constants.ProceedText);
			Console.ReadKey();

			Console.ForegroundColor = color;

			(int left, int top) = Console.GetCursorPosition();

			ClearLine(top, left + 1);

			if (_status != null)
				_status.Pausing = false;
		}

		private void ClearLine(int row, int length)
		{
			Console.SetCursorPosition(0, row);
			BPSPrint(new string(' ', length));
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

			while (_texts.Count > 25)
				_texts.Dequeue();
		}

		private void WritePlainText(string plainText)
		{
			int rowCountDown = Console.WindowHeight - 1;

			foreach (string item in plainText.Split('\n'))
				rowCountDown = WriteWordWrappedPlainSentence(rowCountDown, item);
		}

		private int WriteWordWrappedPlainSentence(int rowCountDown, string item)
		{
			int consoleWidth = Console.WindowWidth;
			string plainLine = item;
			while (plainLine.Length >= consoleWidth)
			{
				int lastFittingSpace = plainLine.LastIndexOf(' ', consoleWidth - 1);
				if (lastFittingSpace <= 0)
					break;

				rowCountDown = WritePlainLine(plainLine[..(lastFittingSpace)], rowCountDown);
				plainLine = plainLine[(lastFittingSpace + 1)..];
			}

			return WritePlainLine(plainLine, rowCountDown);
		}

		private int WritePlainLine(string plainLine, int rowCountDown)
		{
			BPSPrint(plainLine + Console.Out.NewLine);
			if (--rowCountDown == 0)
			{
				WaitForKey();
				rowCountDown = Console.WindowHeight - 1;
			}

			return rowCountDown;
		}

		private void BPSPrint(string text)
		{
			if (!_bpsPrintEnabled || text.Length == 0)
			{
				Console.Write(text);
				return;
			}

			var stopwatch = new Stopwatch();
			int i;
			for (i = 0; i < (text.Length - _bpsPrintCount); i += _bpsPrintCount)
			{
				stopwatch.Restart();
				while (stopwatch.ElapsedTicks < _bpsPrintDelay) { }
				Console.Write(text.Substring(i, _bpsPrintCount));
			}
			stopwatch.Restart();
			while (stopwatch.ElapsedTicks < _bpsPrintDelay) { }
			Console.Write(text[i..]);
		}

		private void Initialize()
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Title = _languages?.GetConfigurationValue(Constants.TitleText) ?? Constants.TitleText;

			var bpsConfig = _configuration[Constants.BPSParam];
			if (bpsConfig == null || bpsConfig == "off")
			{
				_bpsPrintEnabled = false;
				return;
			}

			_bpsPrintEnabled = true;
			_bpsPrintCount = 1;
			_bpsPrintDelay = 0;

			int top = Console.GetCursorPosition().Top;
			Console.SetCursorPosition(0, top);
			int width = Console.WindowWidth - 1;

			Stopwatch stopwatch = Stopwatch.StartNew();
			BPSPrint(RandomString(width));
			stopwatch.Stop();
			
			Console.SetCursorPosition(0, top);
			Console.Write(new string(' ', width));
			Console.SetCursorPosition(0, top);

			if (!int.TryParse(bpsConfig, out int bpsRate) || bpsRate < Constants.BPSMinimum)
				bpsRate = Constants.BPSDefault;

			long neededTicksPerChar = 10000 * 1000 / (bpsRate / 10);
			long measuredTicksPerChar = stopwatch.ElapsedTicks / width;

			if (neededTicksPerChar > measuredTicksPerChar)
			{
				_bpsPrintCount = 1;
				_bpsPrintDelay = neededTicksPerChar - measuredTicksPerChar;
			}
			else
			{
				_bpsPrintCount = (int)(measuredTicksPerChar / neededTicksPerChar) + 1;
				_bpsPrintDelay = _bpsPrintCount * neededTicksPerChar - measuredTicksPerChar;
			}
		}

		public static string RandomString(int length)
			=> new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789", length).Select(s => s[_random.Next(s.Length)]).ToArray());

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
