using System.Collections.Generic;

namespace Paradise.Realtime.Server {
	public interface IRoom<TPeer> where TPeer : BasePeer {
		IReadOnlyList<TPeer> Peers { get; }
		void Join(TPeer peer);
		void Leave(TPeer peer);
	}
}
