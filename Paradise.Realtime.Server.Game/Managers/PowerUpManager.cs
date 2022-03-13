using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class PowerUpManager {
		private static readonly ILog Log = LogManager.GetLogger(typeof(PowerUpManager));

		private List<TimeSpan> _respawnTimesOriginal;
		private List<TimeSpan> _respawnTimes;
		private List<int> _respawning;

		private readonly BaseGameRoom Room;

		public PowerUpManager(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public bool IsLoaded => _respawnTimesOriginal != null;
		public List<int> Respawning => _respawning;

		public void Load(List<ushort> respawnTimes) {
			var length = respawnTimes.Count;
			_respawnTimesOriginal = new List<TimeSpan>(length);
			_respawnTimes = new List<TimeSpan>(length);
			_respawning = new List<int>(length);

			for (int i = 0; i < length; i++) {
				var time = TimeSpan.FromSeconds(respawnTimes[i]);

				_respawnTimesOriginal.Add(time);
				_respawnTimes.Add(TimeSpan.FromSeconds(0));
			}
		}

		public void PickUp(GamePeer peer, int pickupId, PickupItemType type, byte value) {
			if (pickupId < 0 || pickupId > _respawnTimesOriginal.Count - 1) {
				Log.Warn($"Unknown power-up with ID: {pickupId}");
				return;
			}

			/* TODO: Check if the thing is respawning before doing anything. */
			_respawnTimes[pickupId] = _respawnTimesOriginal[pickupId];
			_respawning.Add(pickupId);

			foreach (var otherPeer in Room.Peers)
				otherPeer.Events.Room.SendPowerUpPicked(pickupId, 1);

			switch (type) {
				case PickupItemType.Health:
					var originalHp = peer.Actor.Info.Health;
					peer.Actor.Info.Health = (byte)Math.Min(200, peer.Actor.Info.Health + value);
					//peer.IncrementPowerUp(type, peer.Actor.Info.Health - originalHp);
					break;
				case PickupItemType.Armor:
					var originalAp = peer.Actor.Info.ArmorPoints;
					peer.Actor.Info.ArmorPoints = (byte)Math.Min(200, peer.Actor.Info.ArmorPoints + value);
					//peer.IncrementPowerUp(type, peer.Actor.Info.ArmorPoints - originalAp);
					break;
			}
		}

		public void Update() {
			for (int i = 0; i < _respawning.Count; i++) {
				var time = _respawnTimes[_respawning[i]];
				var newTime = time.Subtract(TimeSpan.FromSeconds(Room.Loop.DeltaTime));
				if (newTime.TotalMilliseconds <= 0) {
					_respawnTimes[_respawning[i]] = TimeSpan.FromSeconds(0);
					foreach (var otherPeer in Room.Peers)
						otherPeer.Events.Room.SendPowerUpPicked(_respawning[i], 0);

					//s_log.Debug($"Respawned power-up with ID: {_respawning[i]}");
					_respawning.RemoveAt(i);
				} else {
					_respawnTimes[_respawning[i]] = newTime;
				}
			}
		}
	}
}
