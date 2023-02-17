using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices.Discord {
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

	internal class DiscordClient {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(DiscordClient));

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private DiscordSettings discordSettings = new DiscordSettings();
		private DiscordSocketClient discordClient;

		private Dictionary<Guid, SocketMessage> messageMap = new Dictionary<Guid, SocketMessage>();
		private Dictionary<Guid, List<string>> messageBuffer = new Dictionary<Guid, List<string>>();

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
		}

		public async void Disconnect() {
			await discordClient.SetStatusAsync(UserStatus.Offline);
		}

		private async Task OnReady() {
			await discordClient.SetActivityAsync(new Game("Paradise Web Services", ActivityType.Playing));

			CommandHandler.CommandOutput += (sender, args) => {
				if (sender is ParadiseCommand) {
					if (!messageMap.ContainsKey((sender as ParadiseCommand).CommandGuid)) return;

					messageBuffer[(sender as ParadiseCommand).CommandGuid].Add(args.Text);
				}
			};

			CommandHandler.CommandCompleted += async (sender, args) => {
				if (sender is ParadiseCommand) {
					if (!messageMap.ContainsKey((sender as ParadiseCommand).CommandGuid)) return;

					var message = messageMap[args.Guid];
					var text = string.Join("\n", messageBuffer[args.Guid]);

					await RespondToMessage($"```{text}```", message);

					messageMap.Remove(args.Guid);
					messageBuffer.Remove(args.Guid);
				}
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
				await RespondToMessage("You're not allowed to run commands.", message);
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
						PrintDiscordHelp(message);
						break;
					default: {
						var guid = Guid.NewGuid();

						messageMap[guid] = message;
						messageBuffer[guid] = new List<string>();

						CommandHandler.HandleCommand(cmdArgs[0], cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray(), guid);
						break;
					}
				}
			}
		}

		private async void PrintDiscordHelp(SocketMessage message) {
			var lines = new List<string> {
				"Available commands:\n",
				"clear\t\tClears the messages in the Bot channel.",
				"database\tControls the LiteDB database instance. (Alias: db)"
			};

			foreach (var type in CommandHandler.Commands.OrderBy(_ => _.Name).ToList()) {
				var cmd = (ParadiseCommand)Activator.CreateInstance(type, new object[] { Guid.Empty });
				lines.Add(cmd.HelpString);
			}

			var text = string.Join("\n", lines);

			await RespondToMessage($"```{text}```", message);
		}

		private async Task RespondToMessage(string text, SocketMessage message) {
			await message.Channel.SendMessageAsync(text, false, null, null, null, new MessageReference(message.Id, message.Channel.Id, (message.Channel as SocketGuildChannel).Guild.Id));
		}
	}
}
