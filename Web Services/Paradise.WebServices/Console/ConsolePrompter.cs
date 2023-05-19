using System;
using System.Collections.Generic;

namespace Paradise.WebServices {
	internal static class ConsolePrompter {
		private static List<List<char>> InputHistory = new List<List<char>>();

		private static void ClearLine(string promptText) {
			Console.CursorVisible = false;

			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth - 1));

			RewritePromptText(promptText);
			Console.CursorVisible = true;
		}

		private static void RemoveCurrentChar() {
			Console.CursorVisible = false;
			Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
			Console.Write(" ");
			Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
			Console.CursorVisible = true;
		}

		private static void RewritePromptText(string promptText) {
			Console.CursorVisible = false;

			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(promptText);

			Console.CursorVisible = true;
		}

		private static void RewriteInput(List<char> input, int inputPosition) {
			Console.CursorVisible = false;
			var cursorPosition = Console.CursorLeft;

			for (var i = inputPosition; i < input.Count; i++) {
				Console.Write(input[i]);
			}

			for (var i = 0; i < 3; i++) {
				Console.Write(" ");
			}

			Console.CursorLeft = cursorPosition;
			Console.CursorVisible = true;
		}

		public static string Prompt(string promptText) {
			List<char> input = new List<char>();
			int inputPosition = 0;
			int inputHistoryPosition = InputHistory.Count;

			Console.Write(promptText);

			ConsoleKeyInfo key;

			do {
				key = Console.ReadKey(true);

				if (Console.CursorLeft < promptText.Length) {
					RewritePromptText(promptText);
				}

				switch (key.Key) {
					case ConsoleKey.Tab:
					case ConsoleKey.LeftArrow:
						if (inputPosition > 0) {
							inputPosition--;
							Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
						}
						break;
					case ConsoleKey.RightArrow:
						if (inputPosition < input.Count) {
							inputPosition++;
							Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
						}
						break;
					case ConsoleKey.UpArrow:
						if (inputHistoryPosition > 0) {
							inputHistoryPosition -= 1;

							ClearLine(promptText);

							input = new List<char>(InputHistory[inputHistoryPosition]);
							inputPosition = input.Count;

							Console.Write(string.Concat(input));
						}
						break;
					case ConsoleKey.DownArrow:
						if (inputHistoryPosition < InputHistory.Count - 1) {
							inputHistoryPosition += 1;

							ClearLine(promptText);

							input = new List<char>(InputHistory[inputHistoryPosition]);
							inputPosition = input.Count;

							Console.Write(string.Concat(input));
						} else if (inputHistoryPosition != InputHistory.Count) {
							inputHistoryPosition = InputHistory.Count;

							ClearLine(promptText);
							Console.SetCursorPosition(promptText.Length, Console.CursorTop);

							input = new List<char>();
							inputPosition = 0;
						}
						break;
					case ConsoleKey.Backspace:
						if (inputPosition > 0) {
							inputPosition--;
							input.RemoveAt(inputPosition);

							RemoveCurrentChar();

							if (inputPosition < input.Count - 1) {
								RewriteInput(input, inputPosition);
							}
						}

						break;
					case ConsoleKey.Enter:
						if (Console.KeyAvailable) {
							input.Insert(inputPosition++, '\n');
						}

						break;
					default:
						if (!char.IsControl(key.KeyChar)) {
							input.Insert(inputPosition++, key.KeyChar);

							Console.Write(key.KeyChar);

							if (inputPosition < input.Count - 1) {
								RewriteInput(input, inputPosition);
							}
						}

						break;
				}
			} while (!(key.Key == ConsoleKey.Enter && Console.KeyAvailable == false));

			if (!string.IsNullOrWhiteSpace(string.Concat(input))) {
				InputHistory.Add(input);
			}

			Console.WriteLine();

			return string.Concat(input);
		}
	}
}
