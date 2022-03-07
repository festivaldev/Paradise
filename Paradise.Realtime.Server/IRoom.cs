namespace Paradise.Realtime.Server {
	public interface IRoom<TPeer> where TPeer : Peer {
		void Join(TPeer peer);
		void Leave(TPeer peer);
	}
}
