using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices {
#warning Remove default values
	[XmlRoot("ParadiseDiscordSettings")]
	public class DiscordSettings {
		[XmlElement]
		public bool EnableDiscordIntegration;

		[XmlElement]
		public string Token;

		[XmlElement]
		public ulong GuildId;

		[XmlElement]
		public ulong ChannelId;

		[XmlElement]
		public DiscordRoles Roles;
	}

	public class DiscordRoles {
		[XmlElement]
		public ulong Admin;

		[XmlElement]
		public ulong SeniorModerator;

		[XmlElement]
		public ulong SeniorQA;

		[XmlElement]
		public ulong Moderator;

		[XmlElement]
		public ulong QA;
	}

	public class DiscordClient {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(DiscordClient));

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private DiscordSettings discordSettings = new DiscordSettings();
		private DiscordSocketClient discordClient;

		public DiscordClient() {
			XmlSerializer serializer = new XmlSerializer(typeof(DiscordSettings));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(CurrentDirectory, "Paradise.Settings.Discord.xml")))) {
					discordSettings = (DiscordSettings)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				Log.Error("There was an error parsing the Discord settings file.", e);
			}
		}

		public async void Connect() {
			if (discordClient != null || !discordSettings.EnableDiscordIntegration) return;

			discordClient = new DiscordSocketClient(new DiscordSocketConfig {
				GatewayIntents =
					GatewayIntents.AllUnprivileged |
					GatewayIntents.MessageContent |
					GatewayIntents.GuildMembers
			});

			discordClient.Log += OnLog;
			discordClient.Ready += OnReady;
			discordClient.MessageReceived += OnMessageReceived;

			await discordClient.LoginAsync(TokenType.Bot, discordSettings.Token);
			await discordClient.StartAsync();

			//try {
			//	TcpClient tcpClient = new TcpClient("127.0.0.1", 5055);
			//	NetworkStream stream = tcpClient.GetStream();
			//	byte[] bytesToSend = Encoding.UTF8.GetBytes("Jonas tinkt");
			//	stream.Write(bytesToSend, 0, bytesToSend.Length);
			//	//byte[] bytesToRead = new byte[tcpClient.ReceiveBufferSize];
			//	//int bytesRead = stream.Read(bytesToRead, 0, tcpClient.ReceiveBufferSize);
			//	//string ret = Encoding.UTF8.GetString(bytesToRead, 0, bytesRead);
			//	tcpClient.Close();
			//} catch (Exception e) {
			//	Log.Error("Error while testing TCP stuff", e);
			//}
		}

		private async Task OnReady() {
			await discordClient.SetActivityAsync(new Game("Paradise Web Services", ActivityType.Playing));

			CommandHandler.CommandCallback += async (sender, args) => {
				if (args.DiscordMessage == null) return;
				var message = args.DiscordMessage;

				await message.Channel.SendMessageAsync($"```{args.CommandOutput}```", false, null, null, null, new MessageReference(message.Id, message.Channel.Id, (message.Channel as SocketGuildChannel).Guild.Id));
			};
		}

		private async Task OnLog(LogMessage message) {
			Log.Info($"[Discord] {message}");
		}

		private async Task OnMessageReceived(SocketMessage message) {
			if (message.Channel.Id != discordSettings.ChannelId) return;
			if (message.Author.IsBot || message.Author.IsWebhook) return;

			Log.Info($"[Discord] {message.Author.Username}: {message.CleanContent}");

			if ((message.Author as SocketGuildUser).Roles.FirstOrDefault(_ => _.Id == discordSettings.Roles.Admin) == null) {
				await message.Channel.SendMessageAsync($"You're not allowed to run commands.", false, null, null, null, new MessageReference(message.Id, message.Channel.Id, (message.Channel as SocketGuildChannel).Guild.Id));
				return;
			}

			if (message.CleanContent.StartsWith("?")) {
				var cmd = message.CleanContent.Substring(1);
				var cmdArgs = cmd.Split(' ').ToList();

				switch (cmdArgs[0].ToLower()) {
					case "clear":
						var messages = await message.Channel.GetMessagesAsync(100).FlattenAsync();

						while (messages.Count() > 0) {
							await (message.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
							messages = await message.Channel.GetMessagesAsync(100).FlattenAsync();
						}
						break;
					case "help":
						ConsoleHelper.PrintDiscordHelp(message);
						break;
					case "q":
					case "quit":
						// Not available in Discord ;)
						break;
					default:
						CommandHandler.HandleCommand(cmdArgs[0], cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray(), message);
						break;
				}
			}
		}
	}
}
