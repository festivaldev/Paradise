using Paradise.Core.Models;
using System;
using static Paradise.TcpSocket;

namespace Paradise.WebServices.Discord {
	public class Plugin : ParadiseServicePlugin {
		private DiscordClient DiscordClient;

		public override void OnLoad() {
			DiscordClient = new DiscordClient();
		}

		public override void OnStart() {
			base.OnStart();

			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;

			DiscordClient.Connect();
			ParadiseService.Instance.SocketServer.DataReceived += OnSocketDataReceived;
		}

		public override void OnStop() {
			base.OnStop();

			DatabaseManager.DatabaseOpened -= OnDatabaseOpened;
			DatabaseManager.DatabaseClosed -= OnDatabaseClosed;

			DiscordClient.Disconnect();
			ParadiseService.Instance.SocketServer.DataReceived -= OnSocketDataReceived;
		}

		#region Socket Callbacks
		private async void OnSocketDataReceived(object sender, SocketDataReceivedEventArgs e) {
			switch (e.Type) {
				case PacketType.Error:
					await DiscordClient.SendErrorLog((RealtimeError)e.Data);
					break;
				case PacketType.ChatMessage:
					await DiscordClient.SendLobbyChatMessage((SocketChatMessage)e.Data);
					break;
				case PacketType.Command:
					var cmd = (SocketCommand)e.Data;

					switch (cmd.Command) {
						case "link":
							if (DiscordClient.IsMemberLinked(cmd.Invoker.Cmid)) {
								e.Socket.SendSync(PacketType.CommandOutput, "Your profile has already been linked to Discord.", true, e.Payload.ConversationId);
								return;
							}

							var nonce = DiscordClient.BeginLinkMember(cmd.Invoker.Cmid);
							e.Socket.SendSync(PacketType.CommandOutput, $"Your Discord link code is: {nonce}.\nPlease send a DM to the Paradise Discord bot containing this code to complete the process.", true, e.Payload.ConversationId);
							break;
						default:
							break;
					}

					break;
				case PacketType.PlayerJoined:
					await DiscordClient.SendPlayerJoinMessage((CommActorInfo)e.Data);
					break;
				case PacketType.PlayerLeft:
					await DiscordClient.SendPlayerLeftMessage((CommActorInfo)e.Data);
					break;
				case PacketType.RoomOpened:
					await DiscordClient.SendGameRoomCreatedMessage((GameRoomData)e.Data);
					break;
				case PacketType.RoomClosed:
					await DiscordClient.SendGameRoomDestroyedMessage((GameRoomData)e.Data);
					break;
			}
		}
		#endregion

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			DatabaseClient.LoadCollections();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			DatabaseClient.UnloadCollections();
		}
		#endregion
	}
}
