using Cmune.DataCenter.Common.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberStrike.Core.Models;

namespace Paradise.WebServices.Services {
	internal class PlayersCommand : ParadiseCommand {
		public static new string Command => "players";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Lists players.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"  list\t\tLists players that are currently online.",
			$"  list-all\tLists all known players.",
			$"  search <pattern>\t\tSearches a player by name, CMID or Steam ID."
		};

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.Moderator;

		public PlayersCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "delete": {
					if (arguments.Length < 2) {
						PrintUsageText();
						return;
					}

					try {
						var pattern = arguments[1];

						if (pattern.Length < 3) {
							WriteLine("Search pattern must contain at least 3 characters.");
							return;
						}

						var playerProfiles = DatabaseClient.PublicProfiles.FindAll().ToList().OrderBy(_ => _.Name);
						var steamMembers = DatabaseClient.SteamMembers.FindAll();
						var connectedPeers = default(List<CommActorInfo>);

						if (ParadiseServerMonitoring.CommMonitoringData.TryGetValue("peers", out var _connectedPeers)) {
							connectedPeers = (_connectedPeers as JArray).ToObject<List<CommActorInfo>>();
						}

						var players = playerProfiles.Where(_ => _.Name.ToLowerInvariant().Contains(pattern.ToLowerInvariant()) || _.Cmid.ToString().Contains(pattern));
						var steamPlayers = steamMembers.Where(_ => _.SteamId.ToString().Contains(pattern));

						if (players.Count() > 0) {
							steamPlayers = steamMembers.Where(steamMember => players.Select(player => player.Cmid).Contains(steamMember.Cmid));
						} else if (steamPlayers.Count() > 0) {
							players = playerProfiles.Where(profile => steamPlayers.Select(steamMember => steamMember.Cmid).Contains(profile.Cmid));
						}

						if (players.Count() == 0) {
							WriteLine($"Could not find any player matching \"{pattern}\"");
						} else if (players.Count() > 1) {
							WriteLine($"Found more than 1 player matching \"{pattern}\", not deleting.");
						} else {
							var player = players.First();

							DatabaseClient.SteamMembers.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.GameSessions.DeleteMany(_ => _.Cmid == player.Cmid);

							if (DatabaseClient.Clans.FindOne(_ => _.OwnerCmid == player.Cmid) is var clan && clan != null) {
								WriteLine("Player owns a clan, finding next player to move ownership to");

								if (clan.Members.FindAll(_ => _.Cmid != player.Cmid).FirstOrDefault() is var clanMember && clanMember != null) {
									WriteLine("Found new clan owner!");
									clan.OwnerCmid = clanMember.Cmid;
									clanMember.Position = GroupPosition.Leader;

									DatabaseClient.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
									DatabaseClient.Clans.Insert(clan);

									DatabaseClient.ClanMembers.DeleteMany(_ => _.Cmid == clanMember.Cmid);
									DatabaseClient.ClanMembers.Insert(clanMember);
								} else {
									WriteLine("Found no new clan owner, deleting clan");

									DatabaseClient.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
								}
							}
							DatabaseClient.ClanMembers.DeleteMany(_ => _.Cmid == player.Cmid);

							DatabaseClient.GroupInvitations.DeleteMany(_ => _.InviterCmid == player.Cmid || _.InviteeCmid == player.Cmid);
							DatabaseClient.PrivateMessages.DeleteMany(_ => _.FromCmid == player.Cmid || _.ToCmid == player.Cmid);
							DatabaseClient.ContactRequests.DeleteMany(_ => _.InitiatorCmid == player.Cmid || _.ReceiverCmid == player.Cmid);
							DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.PlayerInventoryItems.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.PlayerLoadouts.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.PlayerStatistics.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.PublicProfiles.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.ItemTransactions.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.CurrencyDeposits.DeleteMany(_ => _.Cmid == player.Cmid);
							DatabaseClient.PointDeposits.DeleteMany(_ => _.Cmid == player.Cmid);

							WriteLine("Player deleted");
						}
					} catch (Exception e) {
						Console.WriteLine(e);
					}

					break;
				}
				case "list": {
					var playerProfiles = DatabaseClient.PublicProfiles.FindAll().ToList().OrderBy(_ => _.Name);
					var steamMembers = DatabaseClient.SteamMembers.FindAll();
					var connectedPeers = default(List<CommActorInfo>);

					if (ParadiseServerMonitoring.CommMonitoringData.TryGetValue("peers", out var _connectedPeers)) {
						connectedPeers = (_connectedPeers as JArray).ToObject<List<CommActorInfo>>();
					}

					if (connectedPeers == null || connectedPeers.Count() == 0) {
						WriteLine("There are currently no players online.\n");
						return;
					}

					WriteLine($"Players currently online: {connectedPeers.Count()}\n");

					WriteLine(" ----------------------------------------------------------------------- ");
					WriteLine($"| {"Username",-18} | {"CMID",-10} | {"SteamID64",-17} | {"Rank",-15} |");
					WriteLine("|-----------------------------------------------------------------------|");

					foreach (var peer in connectedPeers) {
						var profile = playerProfiles.First(_ => _.Cmid == peer.Cmid);
						var steamMember = steamMembers.First(_ => _.Cmid == peer.Cmid);

						WriteLine($"| {profile.Name,-18} | {profile.Cmid,-10} | {steamMember.SteamId,-17} | {profile.AccessLevel,-15} |");
					}

					WriteLine(" ----------------------------------------------------------------------- ");

					break;
				}
				case "list-all": {
					var playerProfiles = DatabaseClient.PublicProfiles.FindAll().ToList().OrderBy(_ => _.Name);
					var steamMembers = DatabaseClient.SteamMembers.FindAll();
					var connectedPeers = default(List<CommActorInfo>);

					if (ParadiseServerMonitoring.CommMonitoringData.TryGetValue("peers", out var _connectedPeers)) {
						connectedPeers = (_connectedPeers as JArray).ToObject<List<CommActorInfo>>();
					}

					WriteLine($"Players currently online: {connectedPeers?.Count() ?? 0}\n");

					WriteLine(" -------------------------------------------------------------------------------- ");
					WriteLine($"| {"Username",-18} | {"CMID",-10} | {"SteamID64",-17} | {"Rank",-15} | {"Online",-6} |");
					WriteLine("|--------------------------------------------------------------------------------|");

					foreach (var profile in playerProfiles) {
						var steamMember = steamMembers.First(_ => _.Cmid == profile.Cmid);
						var peer = connectedPeers?.Find(_ => _.Cmid == profile.Cmid);

						WriteLine($"| {profile.Name,-18} | {profile.Cmid,-10} | {steamMember.SteamId,-17} | {profile.AccessLevel,-15} | {(peer == null ? "No" : "Yes"),-6} |");
					}

					WriteLine(" -------------------------------------------------------------------------------- ");

					break;
				}
				case "search": {
					if (arguments.Length < 2) {
						PrintUsageText();
						return;
					}

					try {
						var pattern = arguments[1];

						if (pattern.Length < 3) {
							WriteLine("Search pattern must contain at least 3 characters.");
							return;
						}

						var playerProfiles = DatabaseClient.PublicProfiles.FindAll().ToList().OrderBy(_ => _.Name);
						var steamMembers = DatabaseClient.SteamMembers.FindAll();
						var connectedPeers = default(List<CommActorInfo>);

						if (ParadiseServerMonitoring.CommMonitoringData.TryGetValue("peers", out var _connectedPeers)) {
							connectedPeers = (_connectedPeers as JArray).ToObject<List<CommActorInfo>>();
						}

						var players = playerProfiles.Where(_ => _.Name.ToLowerInvariant().Contains(pattern.ToLowerInvariant()) || _.Cmid.ToString().Contains(pattern));
						var steamPlayers = steamMembers.Where(_ => _.SteamId.ToString().Contains(pattern));

						if (players.Count() > 0) {
							steamPlayers = steamMembers.Where(steamMember => players.Select(player => player.Cmid).Contains(steamMember.Cmid));
						} else if (steamPlayers.Count() > 0) {
							players = playerProfiles.Where(profile => steamPlayers.Select(steamMember => steamMember.Cmid).Contains(profile.Cmid));
						}

						if (players.Count() > 0) {
							WriteLine(" -------------------------------------------------------------------------------- ");
							WriteLine($"| {"Username",-18} | {"CMID",-10} | {"SteamID64",-17} | {"Rank",-15} | {"Online",-6} |");
							WriteLine("|--------------------------------------------------------------------------------|");

							foreach (var profile in players) {
								var steamMember = steamMembers.First(_ => _.Cmid == profile.Cmid);
								var peer = connectedPeers?.Find(_ => _.Cmid == profile.Cmid);

								WriteLine($"| {profile.Name,-18} | {profile.Cmid,-10} | {steamMember.SteamId,-17} | {profile.AccessLevel,-15} | {(peer == null ? "No" : "Yes"),-6} |");
							}

							WriteLine(" -------------------------------------------------------------------------------- ");
						} else {
							WriteLine($"Could not find any player matching \"{pattern}\"");
						}
					} catch (Exception e) {
						Console.WriteLine(e);
					}

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
#pragma warning restore CS1998
	}
}
