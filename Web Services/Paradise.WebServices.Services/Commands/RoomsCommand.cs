using Cmune.DataCenter.Common.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberStrike.Core.Models;
using UberStrike.Core.Types;

namespace Paradise.WebServices.Services {
	internal class RoomsCommand : ParadiseCommand {
		public static new string Command => "rooms";
		public static new string[] Aliases => new string[] { };

		public override string Description => "List and manage game rooms.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"  list\t\tList all rooms that are currently open.",
			$"  close <roomId>\t\tCloses an open room if it exists."
		};

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.Moderator;

		public RoomsCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "list": {
					var rooms = ParadiseServerMonitoring.GameMonitoringData.Values
						.Select(_ => ((Dictionary<string, object>)_)["rooms"] as JArray)
						.SelectMany(_ => _)
						.Select(_ => (_ as JObject).GetValue("metadata"));

					if (rooms == null || rooms.Count() == 0) {
						WriteLine("There are currently no open rooms.\n");
						return;
					}

					WriteLine($"Rooms open: {rooms.Count()}\n");

					WriteLine($"| {"ID",-10} | {"Name",-18} | {"Gamemode",-16} | {"Map",-20} | {"Players",-7} | {"Password?",-9} | {"Time",-7} | {"Levels",-7} |");

					foreach (var room in rooms) {
						var _room = room.ToObject<GameRoomData>();

						WriteLine($"| {_room.Number,-10} | {_room.Name,-18} | {GetGamemodeName(_room.GameMode),-16} | {GetNameForMapID(_room.MapID),-20} | {$"{_room.ConnectedPlayers}/{_room.PlayerLimit}",-7} | {(_room.IsPasswordProtected ? "Yes" : "No"),-9} | {$"{_room.TimeLimit / 60} min",-7} | {$"{_room.LevelMin}/{_room.LevelMax}",-7} |");
					}

					break;
				}
				case "open": {
					break;
				}
				case "close": {
					if (arguments.Length < 2) {
						PrintUsageText();
						return;
					}

					if (!int.TryParse(arguments[1], out int roomId)) {
						WriteLine("Invalid parameter: roomId");
						return;
					}

					WriteLine($"Attempting to close room with ID {roomId}.");

					ParadiseService.Instance.SocketServer.SendToGameServers(TcpSocket.PacketType.CloseRoom, roomId);

					break;
				}
			}
		}
#pragma warning restore CS1998

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

		private string GetGamemodeName(GameModeType gameMode) {
			switch (gameMode) {
				case GameModeType.DeathMatch: return "Deathmatch";
				case GameModeType.TeamDeathMatch: return "Team Deathmatch";
				case GameModeType.EliminationMode: return "Team Elimination";
			}

			return null;
		}
	}
}
