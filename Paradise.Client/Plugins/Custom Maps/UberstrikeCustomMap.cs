using UberStrike.Core.Types;

namespace Paradise.Client {
	public class UberstrikeCustomMap {
		public string Name { get; set; }
		public string FileName { get; set; }
		public int MapId { get; set; }
		public GameModeType SupportedGameModes;

		public UberstrikeCustomMap() {

		}
	}
}
