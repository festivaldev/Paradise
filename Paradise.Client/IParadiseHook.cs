using System;

namespace Paradise.Client {
	public interface IParadiseHook {
		Type TypeToHook { get; }
		void Hook();
	}
}
