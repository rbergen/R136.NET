using R136.Entities.General;
using System;
using System.Threading;

namespace R136.Shell
{
	class Animation
	{
		private const int SpiderDropHeight = 13;
		private const string SpiderThreadSection = "               █               ";
		private const string ClearSpiderSection = "                               ";

		private const int LetterSpaceWidth = 6;
		private const string ClearLeftLetterSection = "            ";
		private const string ClearRightLetterSection = "                    ";

		private readonly Blocks _blocks = new();

		public void Run()
		{
			int screenRightX = Console.WindowWidth - 1;

			ConsoleColor foreColor = Console.ForegroundColor;
			ConsoleColor backColor = Console.BackgroundColor;

			Console.CursorVisible = false;
			Console.BackgroundColor = ConsoleColor.Gray;
			Console.Clear();

			int spiderX = (screenRightX - _blocks.GetWidth(Block.Spider)) / 2;

			LowerSpider(spiderX);

			int leftLetterFinalX = screenRightX / 2 - _blocks.GetWidth(Block.LetterR) - LetterSpaceWidth;

			int screenMiddleX = screenRightX / 2;
			if ((screenRightX % 2) == 0)
				screenMiddleX--;

			SwoopInLetters(screenMiddleX, leftLetterFinalX);

			Thread.Sleep(2000);

			SwoopInDigits(screenMiddleX, spiderX, leftLetterFinalX);

			Thread.Sleep(2500);

			Console.CursorVisible = true;
			Console.ResetColor();
			Console.ForegroundColor = foreColor;
			Console.BackgroundColor = backColor;
			Console.Clear();
		}

		private void LowerSpider(int column)
		{
			int spiderBottomY = _blocks.BlockRowCount - 1;
			int spiderRightX = _blocks.GetWidth(Block.Spider) - 1;

			// Introduce the spider from the top of the screen
			for (int dropIndex = _blocks.BlockRowCount; dropIndex > 0; dropIndex--)
			{
				foreach (var position in Enum.GetValues<BlockPosition>())
				{
					WriteBlock(column, 0, ConsoleColor.Black, _blocks[Block.Spider, position], 0, dropIndex - 1, spiderRightX, spiderBottomY);

					Thread.Sleep(50);
				}
			}

			// Lower it to its final place, leaving a silk thread
			for (int dropIndex = 0; dropIndex < SpiderDropHeight; dropIndex++)
			{
				WriteSection(column, dropIndex, ConsoleColor.Black, SpiderThreadSection);

				foreach (var position in Enum.GetValues<BlockPosition>())
				{
					WriteBlock(column, dropIndex + 1, ConsoleColor.Black, _blocks[Block.Spider, position]);

					Thread.Sleep(50);
				}
			}
		}

		private void SwoopInLetters(int screenMiddleX, int leftLetterFinalX)
		{
			int letterWidth = _blocks.GetWidth(Block.LetterR);
			int letterRightX = _blocks.GetWidth(Block.LetterR) - 1;
			int letterBottomY = _blocks.BlockRowCount - 1;

			int screenRightX = Console.WindowWidth - 1;

			// Introduce letters from either side of the screen
			for (int i = 0; i < letterWidth; i++)
			{
				WriteBlock(0, 2, ConsoleColor.Black, _blocks[Block.LetterR], letterRightX - i, 0, letterRightX, letterBottomY);
				WriteBlock(screenRightX - i, 2, ConsoleColor.Black, _blocks[Block.LetterP], 0, 0, i, letterBottomY);
			}

			int rightLetterFirstX = screenRightX - letterWidth;
			int flowIndex = 0;

			// Bring the letters to the center
			for (; flowIndex < leftLetterFinalX; flowIndex++)
			{
				WriteBlock(flowIndex, 2, ConsoleColor.Black, _blocks[Block.Space]);
				WriteBlock(flowIndex + 1, 2, ConsoleColor.Black, _blocks[Block.LetterR]);

				WriteBlock(rightLetterFirstX - flowIndex, 2, ConsoleColor.Black, _blocks[Block.LetterP]);
				WriteBlock(screenRightX - flowIndex, 2, ConsoleColor.Black, _blocks[Block.Space]);
			}

			// If the silk thread is just left of center due to screen width, bump the P one more place to the left
			if ((screenRightX % 2) == 0)
			{
				WriteBlock(rightLetterFirstX - flowIndex, 2, ConsoleColor.Black, _blocks[Block.LetterP]);
				WriteBlock(screenRightX - flowIndex, 2, ConsoleColor.Black, _blocks[Block.Space]);
			}

			// Cut the silk thread to create the letter I
			WriteSection(screenMiddleX, 1, ConsoleColor.Black, "▀");
			WriteSection(screenMiddleX, 1 + _blocks.BlockRowCount, ConsoleColor.Black, "▄");
		}

		private void SwoopInDigits(int screenMiddleX, int spiderX, int leftLetterfinalX)
		{
			int screenRightX = Console.WindowWidth - 1;
			int screenBottomY = Console.WindowHeight - 1;
			int letterWidth = _blocks.GetWidth(Block.LetterR);
			int digitsWidth = _blocks.GetWidth(Block.Digits);

			int lettersTopY = (screenBottomY - _blocks.BlockRowCount) / 2;
			if (lettersTopY < 2)
				lettersTopY = 2;

			// clear everything except for the R
			WriteSection(screenMiddleX, 0, ConsoleColor.Black, " ");
			WriteSection(screenMiddleX, 1, ConsoleColor.Black, " ");

			for (int i = 2; i < 2 + _blocks.BlockRowCount; i++)
				WriteSection(screenMiddleX, i, ConsoleColor.Black, ClearRightLetterSection);

			for (int i = 2 + _blocks.BlockRowCount; i < SpiderDropHeight; i++)
				WriteSection(screenMiddleX, i, ConsoleColor.Black, " ");

			for (int i = SpiderDropHeight; i < SpiderDropHeight + _blocks.BlockRowCount; i++)
				WriteSection(spiderX, i, ConsoleColor.Black, ClearSpiderSection);

			// Lower the R to the vertical middle of the screen
			for (int i = 2; i < lettersTopY; i++)
			{
				WriteBlock(leftLetterfinalX, i, ConsoleColor.Black, _blocks[Block.LetterR, BlockPosition.Lower]);

				Thread.Sleep(5);

				WriteSection(leftLetterfinalX, i, ConsoleColor.Black, ClearLeftLetterSection);
				WriteBlock(leftLetterfinalX, i + 1, ConsoleColor.Black, _blocks[Block.LetterR, BlockPosition.Upper]);

				Thread.Sleep(5);
			}

			int digitsFinalX = leftLetterfinalX + letterWidth + 3;
			int digitsBottomY = _blocks.BlockRowCount - 1;

			// Introduce the digits from the right-hand side of the screen
			for (int i = 0; i < digitsWidth; i++)
				WriteBlock(screenRightX - i, lettersTopY, ConsoleColor.Red, _blocks[Block.Digits], 0, 0, i, digitsBottomY);

			// Bring the digits to the center
			for (int i = screenRightX - digitsWidth; i >= digitsFinalX; i--)
			{
				WriteBlock(i, lettersTopY, ConsoleColor.Red, _blocks[Block.Digits]);
				WriteBlock(i + digitsWidth, lettersTopY, ConsoleColor.Red, _blocks[Block.Space]);
			}
		}

		private static void WriteSection(int x, int y, ConsoleColor color, string sectionString)
		{
			Console.ForegroundColor = color;
			Console.SetCursorPosition(x, y);
			Console.Write(sectionString);
		}

		private static void WriteBlock(int x, int y, ConsoleColor color, string[] blockStrings)
		{
			Console.ForegroundColor = color;

			foreach (string rowString in blockStrings)
			{
				Console.SetCursorPosition(x, y++);
				Console.Write(rowString);
			}
		}

		private static void WriteBlock(int x, int y, ConsoleColor color, string[] blockStrings, int leftx, int topy, int rightx, int bottomy)
		{
			Console.ForegroundColor = color;

			for (int i = topy; i <= bottomy; i++)
			{
				Console.SetCursorPosition(x, y + i - topy);
				Console.Write(blockStrings[i][leftx..(rightx + 1)]);
			}
		}

		private enum BlockPosition : byte
		{
			Upper = 0,
			Lower = 1
		}

		private enum Block : byte
		{
			Spider = 0,
			LetterR = 1,
			LetterP = 2,
			Space = 3,
			Digits = 4
		}

		private class Blocks
		{
			public readonly int BlockRowCount;
			private readonly string[][][] _strings;

			public Blocks()
			{
				_strings = new string[Enum.GetValues<Block>().Length][][];

				int blockPositionCount = Enum.GetValues<BlockPosition>().Length;

				_strings.Set(Block.Spider, new string[blockPositionCount][]);
				_strings.Set(Block.LetterR, new string[blockPositionCount][]);
				_strings.Set(Block.LetterP, new string[1][]);
				_strings.Set(Block.Space, new string[1][]);
				_strings.Set(Block.Digits, new string[1][]);


				_strings.Set(Block.Spider, BlockPosition.Upper, new string[]
				{
					"  ▄▄▄        ▄ █ ▄        ▄▄▄  ",
					" ▄▀  ▀▀▄▄  ▄▀ ▀█▀ ▀▄  ▄▄▀▀  ▀▄ ",
					"▄█ ▄▄▄▄  ███ ▀ ▄ ▀ ███  ▄▄▄▄ █▄",
					"   █   ▀▀████▄███▄████▀▀   █   ",
					" ▄█    ▄▄▄▀█████████▀▄▄▄    █▄ ",
					"   ▄▀▀▀                 ▀▀▀▄   ",
					"  █                         █  ",
					" ▀▀                         ▀▀ ",
				});

				_strings.Set(Block.Spider, BlockPosition.Lower, new string[]
				{
					"               █               ",
					"  █▀▀▄▄     ▄▀▄█▄▀▄     ▄▄▀▀█  ",
					" █     ▀▀▄▄█ ▄ ▀ ▄ █▄▄▀▀     █ ",
					"▀▀ █▀▀▀▄▄███▄ ▄█▄ ▄███▄▄▀▀▀█ ▀▀",
					"  ▄▀     ▀███████████▀     ▀▄  ",
					" ▀▀ ▄▄▄▀▀▀ ▀▀▀▀▀▀▀▀▀ ▀▀▀▄▄▄ ▀▀ ",
					"  ▄▀                       ▀▄  ",
					" ▄█                         █▄ "
				});

				_strings.Set(Block.LetterR, BlockPosition.Upper, new string[]
				{
					"█▀▀▀▀▀▀▀▀▀▀▄",
					"█          █",
					"█          █",
					"█▄▄▄▄▄▄▄▄▄▄▀",
					"█     ▀▄    ",
					"█       ▀▄  ",
					"█         ▀▄",
					"            "
				});

				_strings.Set(Block.LetterR, BlockPosition.Lower, new string[]
				{
					"▄▄▄▄▄▄▄▄▄▄▄ ",
					"█          █",
					"█          █",
					"█          █",
					"█▀▀▀▀▀█▀▀▀▀ ",
					"█      ▀▄   ",
					"█        ▀▄ ",
					"▀          ▀"
				});

				_strings.Get(Block.LetterP)[0] = new string[]
				{
					"█▀▀▀▀▀▀▀▀▀▀▄",
					"█          █",
					"█          █",
					"█▄▄▄▄▄▄▄▄▄▄▀",
					"█           ",
					"█           ",
					"█           ",
					"            "
				};

				_strings.Get(Block.Space)[0] = new string[]
				{
					" ",
					" ",
					" ",
					" ",
					" ",
					" ",
					" ",
					" "
				};

				_strings.Get(Block.Digits)[0] = new string[]
				{
					"▄█   ▄▀▀▀▀▀▄   ▄▀▀▀▀▀▄",
					" █         █   █      ",
					" █         █   █      ",
					" █    ▀▀▀▀▀▄   █▀▀▀▀▀▄",
					" █         █   █     █",
					" █         █   █     █",
					" █   ▀▄▄▄▄▄▀   ▀▄▄▄▄▄▀",
					"                      "
				};

				BlockRowCount = _strings[0][0].Length;
			}

			public string[] this[Block block, BlockPosition position]
				=> _strings.Get(block, _strings.Get(block).Length == 1 ? 0 : position);

			public string[] this[Block block]
				=> _strings.Get(block)[0];

			public int GetWidth(Block block)
				=> _strings.Get(block)[0][0].Length;
		}
	}
}
