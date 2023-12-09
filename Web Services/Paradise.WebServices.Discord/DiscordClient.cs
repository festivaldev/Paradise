using Cmune.DataCenter.Common.Entities;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UberStrike.Core.Models;
using UberStrike.Core.Types;
using static Paradise.TcpSocket;

namespace Paradise.WebServices.Discord {
	enum GAME_FLAGS {
		None = 0x0,
		LowGravity = 0x1,
		NoArmor = 0x2,
		QuickSwitch = 0x4,
		MeleeOnly = 0x8
	}

	internal class DiscordClient {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(DiscordClient));

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private readonly DiscordSettings discordSettings = new DiscordSettings();
		private DiscordSocketClient discordClient;

		private DiscordWebhookClient chatClient;
		private DiscordWebhookClient playerAnnouncementClient;
		private DiscordWebhookClient gameAnnouncementClient;
		private DiscordWebhookClient errorLogClient;

		private readonly Dictionary<Guid, SocketMessage> messageMap = new Dictionary<Guid, SocketMessage>();
		private readonly Dictionary<Guid, List<string>> messageBuffer = new Dictionary<Guid, List<string>>();

		public DiscordClient() {
			XmlSerializer serializer = new XmlSerializer(typeof(DiscordSettings));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(CurrentDirectory, "Paradise.Settings.Discord.xml")), new XmlReaderSettings { IgnoreComments = true })) {
					discordSettings = (DiscordSettings)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				Log.Error("There was an error parsing the Discord settings file.", e);
			}
		}

		public async void Connect() {
			if (discordClient != null || !discordSettings.EnableDiscordIntegration) return;

			if (!string.IsNullOrWhiteSpace(discordSettings.Token)) {
				discordClient = new DiscordSocketClient(new DiscordSocketConfig {
					AlwaysDownloadUsers = true,
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

			if (discordSettings.ChatIntegration && !string.IsNullOrWhiteSpace(discordSettings.ChatWebHookUrl)) {
				chatClient = new DiscordWebhookClient(discordSettings.ChatWebHookUrl);
			}

			if ((discordSettings.PlayerJoinAnnouncements || discordSettings.PlayerLeaveAnnouncements) && !string.IsNullOrWhiteSpace(discordSettings.PlayerAnnouncementWebHookUrl)) {
				playerAnnouncementClient = new DiscordWebhookClient(discordSettings.PlayerAnnouncementWebHookUrl);
			}

			if ((discordSettings.RoomOpenAnnouncements || discordSettings.RoomCloseAnnouncements) && !string.IsNullOrWhiteSpace(discordSettings.GameAnnouncementWebHookUrl)) {
				gameAnnouncementClient = new DiscordWebhookClient(discordSettings.GameAnnouncementWebHookUrl);
			}

			if (discordSettings.ErrorLog && !string.IsNullOrWhiteSpace(discordSettings.ErrorLogWebHookUrl)) {
				errorLogClient = new DiscordWebhookClient(discordSettings.ErrorLogWebHookUrl);
			}
		}

		public async void Disconnect() {
			await discordClient.SetStatusAsync(UserStatus.Offline);
		}

		public async Task SendLobbyChatMessage(SocketChatMessage message) {
			if (!discordSettings.ChatIntegration) return;
			if (discordSettings.ChatChannelId <= 0) return;

			var discordUser = GetDiscordUserFromCmid(message.Cmid);

			string username = null, avatarUrl = null;

			if (discordUser != null) {
				var guild = discordClient.GetGuild(discordSettings.GuildId);
				var user = guild.GetUser(discordUser.DiscordUserId);

				if (user != null) {
					username = user.Nickname;
					avatarUrl = user.GetAvatarUrl();
				}
			}

			var embed = new EmbedBuilder {
				Description = message.Message,
				Footer = new EmbedFooterBuilder {
					Text = "UberStrike Lobby Chat",
					IconUrl = discordClient.CurrentUser.GetAvatarUrl()
				}
			};

			await chatClient.SendMessageAsync(
				username: username ?? message.Name,
				avatarUrl: avatarUrl ?? null,
				embeds: new List<Embed> {
					embed.Build()
				}
			);
		}

		public async Task SendPlayerJoinMessage(CommActorInfo player) {
			if (!discordSettings.PlayerJoinAnnouncements) return;
			if (discordSettings.AnnouncementBlacklist.Contains(player.Cmid.ToString())) return;

			var discordUser = GetDiscordUserFromCmid(player.Cmid);
			var steamMember = DatabaseClient.SteamMembers.FindOne(_ => _.Cmid == player.Cmid);

			var embed = new EmbedBuilder {
				Title = "Player connected",
				Description = $"{player.PlayerName} has joined the server.",
				Color = Color.Green
			};

			embed.AddField("CMID", player.Cmid);
			embed.AddField("SteamID64", steamMember.SteamId, true);
			embed.AddField("Machine ID", steamMember.MachineId);
			embed.AddField("Rank", player.AccessLevel.ToString(), true);

			await playerAnnouncementClient.SendMessageAsync(
				username: "UberStrike",
				embeds: new List<Embed> {
					embed.Build()
				}
			);
		}

		public async Task SendPlayerLeftMessage(CommActorInfo player) {
			if (!discordSettings.PlayerLeaveAnnouncements) return;
			if (discordSettings.AnnouncementBlacklist.Contains(player.Cmid.ToString())) return;

			var discordUser = GetDiscordUserFromCmid(player.Cmid);
			var steamMember = DatabaseClient.SteamMembers.FindOne(_ => _.Cmid == player.Cmid);

			var embed = new EmbedBuilder {
				Title = "Player disconnected",
				Description = $"{player.PlayerName} has left the server.",
				Color = Color.Red
			};

			await playerAnnouncementClient.SendMessageAsync(
				username: "UberStrike",
				embeds: new List<Embed> {
					embed.Build()
				}
			);
		}

		public async Task SendGameRoomCreatedMessage(GameRoomData metadata) {
			if (!discordSettings.RoomOpenAnnouncements) return;

			var embed = new EmbedBuilder {
				Title = "Game Room created",
				Color = Color.Default
			};

			embed.AddField("Room Name", metadata.Name);
			embed.AddField("Map", GetNameForMapID(metadata.MapID), true);
			embed.AddField("Gamemode", GetGamemodeName(metadata.GameMode), true);
			embed.AddField("Player Limit", metadata.PlayerLimit, true);
			embed.AddField("Time Limit", $"{metadata.TimeLimit / 60} min", true);
			embed.AddField("Kill/Round Limit", metadata.KillLimit, true);
			embed.AddField("Game Modifiers", GetGameFlags(metadata.GameFlags));
			embed.AddField("Requires Password", metadata.IsPasswordProtected ? "Yes" : "No", true);
			embed.AddField("Minimum Level", metadata.LevelMin > 0 ? metadata.LevelMin.ToString() : "None", true);
			embed.AddField("Maximum Level", metadata.LevelMax > 0 ? metadata.LevelMax.ToString() : "None", true);
			embed.AddField("Join this game", $"`uberstrike://connect/{metadata.Server.ConnectionString}/{metadata.Number}`");

			embed.WithImageUrl($"https://static.paradise.festival.tf/images/maps/{GetImageNameForMapID(metadata.MapID)}.jpg");
			embed.WithFooter($"Room ID: {metadata.Number}");

			await gameAnnouncementClient.SendMessageAsync(
				username: "UberStrike",
				embeds: new List<Embed> {
					embed.Build()
				}
			);
		}

		public async Task SendGameRoomDestroyedMessage(GameRoomData metadata) {
			if (!discordSettings.RoomCloseAnnouncements) return;

			var embed = new EmbedBuilder {
				Title = "Game Room closed",
				Color = Color.Default
			};

			embed.AddField("Room Name", metadata.Name);
			embed.AddField("Map", GetNameForMapID(metadata.MapID), true);
			embed.AddField("Gamemode", GetGamemodeName(metadata.GameMode), true);

			embed.WithFooter($"Room ID: {metadata.Number}");

			await gameAnnouncementClient.SendMessageAsync(
				username: "UberStrike",
				embeds: new List<Embed> {
					embed.Build()
				}
			);
		}

		public async Task LogError(Exception error) {
			if (!discordSettings.ErrorLog) return;

			await errorLogClient.SendMessageAsync(
				username: "UberStrike",
				text: $"```{error.GetType()}: {error.Message}\r\n{error.StackTrace}```"
			);
		}

		public async Task LogError(RealtimeError error) {
			if (!discordSettings.ErrorLog) return;

			await errorLogClient.SendMessageAsync(
				username: "UberStrike",
				text: $"```{error.ExceptionType}: {error.Message}\r\n{error.StackTrace}```"
			);
		}

		public bool IsMemberLinked(int cmid) {
			return DatabaseClient.DiscordUsers.FindOne(_ => _.Cmid == cmid && _.DiscordUserId != 0) != null;
		}

		public string BeginLinkMember(int cmid) {
			if (IsMemberLinked(cmid)) return null;

			if (DatabaseClient.DiscordUsers.FindOne(_ => _.Cmid == cmid && _.DiscordUserId == 0) is var link && link != null) {
				return link.Nonce;
			}

			var rand = new Random((int)DateTime.UtcNow.Ticks);

			var nonce = string.Empty;

			for (int i = 0; i < 6; i++) {
				nonce += rand.Next(0, 16).ToString("x");
			}

			DatabaseClient.DiscordUsers.Insert(new DiscordUser {
				Cmid = cmid,
				Nonce = nonce
			});

			return nonce;
		}

		public DiscordUser GetDiscordUserFromCmid(int cmid) {
			return DatabaseClient.DiscordUsers.FindOne(_ => _.Cmid == cmid && _.DiscordUserId != 0 && _.Nonce == null);
		}

		public DiscordUser GetDiscordUserFromDiscordId(ulong discordUserId) {
			return DatabaseClient.DiscordUsers.FindOne(_ => _.Cmid != 0 && _.DiscordUserId == discordUserId && _.Nonce == null);
		}

		#region Callbacks
		private async Task OnReady() {
			await discordClient.SetActivityAsync(new Game("Paradise Web Services", ActivityType.Playing));
		}

#pragma warning disable CS1998
		private async Task OnLog(LogMessage message) {
			Log.Debug($"[Discord] {message}");
		}
#pragma warning restore CS1998

		private async Task OnMessageReceived(SocketMessage message) {
			if (message.Author.IsBot || message.Author.IsWebhook) return;

			if (message.Channel is IDMChannel) {
				var discordUser = GetDiscordUserFromDiscordId(message.Author.Id);

				if (discordUser != null) {
					await message.Channel.SendMessageAsync("Your Discord profile has already been linked to UberStrike.");
					return;
				}

				discordUser = DatabaseClient.DiscordUsers.FindOne(_ => _.Nonce == message.CleanContent);

				if (discordUser == null) {
					await message.Channel.SendMessageAsync("Your Discord profile could not be linked to UberStrike.\nPlease make sure to enter a valid link code.");
					return;
				}

				discordUser.DiscordUserId = message.Author.Id;
				discordUser.Nonce = null;

				DatabaseClient.DiscordUsers.DeleteMany(_ => _.Cmid == discordUser.Cmid);
				DatabaseClient.DiscordUsers.Insert(discordUser);

				await message.Channel.SendMessageAsync("Your Discord profile has been successfully linked to UberStrike!");
			} else if (message.Channel is ITextChannel) {
				if (message.Channel.Id == discordSettings.CommandChannelId && message.CleanContent.StartsWith("?") && message.CleanContent.Length > 1) {
					var discordUser = GetDiscordUserFromDiscordId(message.Author.Id);

					if (discordUser == null) {
						await RespondToMessage(message, "Please link your Discord profile to UberStrike using `?link` in the ingame Lobby chat in order to execute commands.");
						return;
					}

					var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == discordUser.Cmid);

					if (publicProfile.AccessLevel == MemberAccessLevel.Default) {
						//await RespondToMessage(message, "You're not allowed to run commands.");
						return;
					}

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
						default:
							CommandHandler.HandleUserCommand(cmdArgs.First(), cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray(), publicProfile, default, null,
								async (ParadiseCommand invoker, bool success, string error) => {
									if (success && string.IsNullOrWhiteSpace(error)) {
										await RespondToMessage(message, $"```{invoker.Output}```");
									} else {
										await RespondToMessage(message, $"```{error}```");
									}
								});

							break;
					}
				} else if (message.Channel.Id == discordSettings.ChatChannelId) {
					var discordUser = GetDiscordUserFromDiscordId(message.Author.Id);

					if (discordUser == null) {
						await RespondToMessage(message, "Your message could not be delivered to the Lobby chat.\nPlease link your Discord profile to UberStrike using `?link` in the ingame Lobby chat.");
						return;
					}

					await ParadiseService.Instance.SocketServer.SendToCommServer(PacketType.ChatMessage, new TcpSocket.SocketChatMessage {
						Cmid = discordUser.Cmid,
						Name = $"[Discord] {message.Author.Username}",
						Message = message.CleanContent
					});
				}
			}
		}
		#endregion

		private async void PrintDiscordHelp(SocketMessage message) {
			var lines = new List<string> {
				"Available commands:\n",
				"clear\t\tClears the messages in the Bot channel.",
				"help\t\tShows this help text. (Alias: h)"
			};

			foreach (var type in CommandHandler.Commands.OrderBy(_ => _.Name).ToList()) {
				var cmd = (ParadiseCommand)Activator.CreateInstance(type, new object[] { Guid.Empty });
				lines.Add(cmd.HelpString);
			}

			var text = string.Join("\r\n", lines);

			await RespondToMessage(message, $"```{text}```");
		}

		private async Task RespondToMessage(SocketMessage message, string text) {
			await message.Channel.SendMessageAsync(text, messageReference: new MessageReference(message.Id, message.Channel.Id, (message.Channel as SocketGuildChannel).Guild.Id));
		}

		private string GetNameForMapID(int mapID) {
			switch (mapID) {
				case 3: return "Apex Twin";
				case 4: return "Aqualab Research Hub";
				case 5: return "Catalyst";
				case 6: return "CuberSpace";
				case 7: return "CuberStrike";
				case 8: return "Fort Winter";
				case 9: return "Ghost Island";
				case 10: return "Gideon's Tower";
				case 11: return "Monkey Island 2";
				case 12: return "Lost Paradise 2";
				case 13: return "Sky Garden";
				case 14: return "SuperPRISM Reactor";
				case 15: return "Temple of the Raven";
				case 16: return "The Hangar";
				case 17: return "The Warehouse";
				case 18: return "Danger Zone";
				case 64: return "Space City";
				case 65: return "Spaceport Alpha";
				case 66: return "UberZone";
			}

			return null;
		}

		private string GetImageNameForMapID(int mapID) {
			switch (mapID) {
				case 3: return "ApexTwin";
				case 4: return "AqualabResearchHub";
				case 5: return "Catalyst";
				case 6: return "Cuberspace";
				case 7: return "CuberStrike";
				case 8: return "FortWinter";
				case 9: return "GhostIsland";
				case 10: return "GideonsTower";
				case 11: return "MonkeyIsland";
				case 12: return "LostParadise2";
				case 13: return "SkyGarden";
				case 14: return "SuperPRISMReactor";
				case 15: return "TempleOfTheRaven";
				case 16: return "TheHangar";
				case 17: return "TheWarehouse";
				case 18: return "Volley";
				case 64: return "SpaceCity";
				case 65: return "SpacePortAlpha";
				case 66: return "UberZone";
			}

			return null;
		}

		private string GetGamemodeName(GameModeType gameMode) {
			switch (gameMode) {
				case GameModeType.DeathMatch: return "Deathmatch";
				case GameModeType.TeamDeathMatch: return "Team Deathmatch";
				case GameModeType.EliminationMode: return "Team Elimination";
			}

			return null;
		}

		private string GetGameFlags(int gameFlags) {
			return string.Join(", ", Enum.GetValues(typeof(GAME_FLAGS))
			.Cast<GAME_FLAGS>()
			.Where(e => (gameFlags & (int)e) != 0)
			.Select(e => {
				switch (e) {
					case GAME_FLAGS.None: return "None";
					case GAME_FLAGS.LowGravity: return "Low Gravity";
					case GAME_FLAGS.NoArmor: return "No Armor";
					case GAME_FLAGS.QuickSwitch: return "Quick Switch";
					case GAME_FLAGS.MeleeOnly: return "Melee Only";
				}

				return null;
			}));
		}
	}
}
