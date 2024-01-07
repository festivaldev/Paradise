using System;
using System.IO;
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

		private const int STD_INPUT_HANDLE = -10;
		private const int STD_OUTPUT_HANDLE = -11;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
											   [MarshalAs(UnmanagedType.U4)] uint access,
											   [MarshalAs(UnmanagedType.U4)] FileShare share,
																				 IntPtr securityAttributes,
											   [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
											   [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
																				 IntPtr templateFile);

#if DEBUG
		private const uint GENERIC_WRITE = 0x40000000;
		private const uint GENERIC_READ = 0x80000000;
#endif

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
		#endregion

		#region Disable QuickEdit
		private const uint ENABLE_MOUSE_INPUT = 0x0010;
		private const uint ENABLE_QUICK_EDIT = 0x0040;
		#endregion

		[DllImport("kernel32.dll")]
		private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);



		private static bool HasConsole = false;


		public static void CreateConsole() {
			if (!HasConsole) {
				HasConsole = true;
				AllocConsole();

				var stdinHandle = GetStdHandle(STD_INPUT_HANDLE);

				GetConsoleMode(stdinHandle, out uint inConsoleMode);
				inConsoleMode &= ~ENABLE_QUICK_EDIT;
				inConsoleMode &= ~ENABLE_MOUSE_INPUT;
				SetConsoleMode(stdinHandle, inConsoleMode);

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
		}

		public static void DestroyConsole() {
			ShowWindow(GetConsoleWindow(), SW_HIDE);
		}

		public static void PrintConsoleHeader() {
			Console.WriteLine($"Paradise Web Services [Version {typeof(Program).Assembly.GetName().Version}]");
			Console.WriteLine("(c) 2017, 2022-2023 Team FESTIVAL. All rights reserved." + Environment.NewLine);
		}

		public static void PrintConsoleHeaderSubtitle() {
			Console.WriteLine("\r\nType \"help\" (or \"h\") to see available commands.");
		}
	}
}