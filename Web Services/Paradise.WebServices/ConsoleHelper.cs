using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Paradise.WebServices {
	public static class ConsoleHelper {
		#region Console Allocation
		[DllImport("kernel32.dll")]
		private static extern bool AllocConsole();

		[DllImport("kernel32.dll")]
		private static extern bool FreeConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

		private const int STD_OUTPUT_HANDLE = -11;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
											   [MarshalAs(UnmanagedType.U4)] uint access,
											   [MarshalAs(UnmanagedType.U4)] FileShare share,
																				 IntPtr securityAttributes,
											   [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
											   [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
																				 IntPtr templateFile);

		private const uint GENERIC_WRITE = 0x40000000;
		private const uint GENERIC_READ = 0x80000000;

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_HIDE = 0;
		private const int SW_SHOW = 5;

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
		#endregion

		#region ANSI color code support
		private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
		private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

		[DllImport("kernel32.dll")]
		private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);


		#endregion

		private static bool HasConsole = false;


		public static void CreateConsole() {
			if (!HasConsole) {
				HasConsole = true;
				AllocConsole();

				var stdoutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

#if DEBUG
				var stdoutRedirect = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE, FileShare.Write, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);

				if (stdoutRedirect != stdoutHandle) {
					SetStdHandle(STD_OUTPUT_HANDLE, stdoutRedirect);
					Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });
				}
#endif

				GetConsoleMode(stdoutHandle, out uint outConsoleMode);
				outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
				SetConsoleMode(stdoutHandle, outConsoleMode);
			} else {
				Console.Clear();
				ShowWindow(GetConsoleWindow(), SW_SHOW);
			}

			PrintConsoleHeader();
		}

		public static void DestroyConsole() {
			ShowWindow(GetConsoleWindow(), SW_HIDE);
		}

		public static void PrintConsoleHeader() {
			Console.WriteLine($"Paradise Web Services [Version {typeof(Program).Assembly.GetName().Version}]");
			Console.WriteLine("(c) 2017, 2022-2023 Team FESTIVAL. All rights reserved." + Environment.NewLine);
		}

		public static void PrintConsoleHeaderSubtitle() {
			Console.WriteLine("\nType \"help\" to see available commands.");
		}

		public static void PrintConsoleHelp() {
			Console.WriteLine("Available commands:" + Environment.NewLine);
			Console.WriteLine("clear\t\tClears the console, obviously.");
			Console.WriteLine("database\tControls the LiteDB database instance. (Alias: db)");
			Console.WriteLine("quit\t\tQuits the application (or closes the console if running in GUI mode). (Alias: q)");

			foreach (var cmd in CommandHandler.Commands.OrderBy(_ => _.Command).ToList()) {
				Console.WriteLine(cmd.HelpString);
			}
		}

		public static async void PrintDiscordHelp(SocketMessage discordMessage) {
			var message = $"Available commands:\n\n" +
						  $"clear\t\tClears the console, obviously.\n" +
						  $"database\tControls the LiteDB database instance. (Alias: db)\n";

			foreach (var cmd in CommandHandler.Commands.OrderBy(_ => _.Command).ToList()) {
				message += cmd.HelpString + Environment.NewLine;
			}

			await discordMessage.Channel.SendMessageAsync($"```{message}```", false, null, null, null, new MessageReference(discordMessage.Id, discordMessage.Channel.Id, (discordMessage.Channel as SocketGuildChannel).Guild.Id));
		}
	}
}