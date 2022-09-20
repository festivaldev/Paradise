using Paradise.Core.Models.Views;
using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class GameApplicationViewProxy {
		public static void Serialize(Stream stream, GameApplicationView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.CommServer != null) {
					PhotonViewProxy.Serialize(memoryStream, instance.CommServer);
				} else {
					num |= 1;
				}
				if (instance.EncryptionInitVector != null) {
					StringProxy.Serialize(memoryStream, instance.EncryptionInitVector);
				} else {
					num |= 2;
				}
				if (instance.EncryptionPassPhrase != null) {
					StringProxy.Serialize(memoryStream, instance.EncryptionPassPhrase);
				} else {
					num |= 4;
				}
				if (instance.GameServers != null) {
					ListProxy<PhotonView>.Serialize(memoryStream, instance.GameServers, new ListProxy<PhotonView>.Serializer<PhotonView>(PhotonViewProxy.Serialize));
				} else {
					num |= 8;
				}
				if (instance.SupportUrl != null) {
					StringProxy.Serialize(memoryStream, instance.SupportUrl);
				} else {
					num |= 16;
				}
				if (instance.Version != null) {
					StringProxy.Serialize(memoryStream, instance.Version);
				} else {
					num |= 32;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static GameApplicationView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			GameApplicationView gameApplicationView = new GameApplicationView();
			if ((num & 1) != 0) {
				gameApplicationView.CommServer = PhotonViewProxy.Deserialize(bytes);
			}
			if ((num & 2) != 0) {
				gameApplicationView.EncryptionInitVector = StringProxy.Deserialize(bytes);
			}
			if ((num & 4) != 0) {
				gameApplicationView.EncryptionPassPhrase = StringProxy.Deserialize(bytes);
			}
			if ((num & 8) != 0) {
				gameApplicationView.GameServers = ListProxy<PhotonView>.Deserialize(bytes, new ListProxy<PhotonView>.Deserializer<PhotonView>(PhotonViewProxy.Deserialize));
			}
			if ((num & 16) != 0) {
				gameApplicationView.SupportUrl = StringProxy.Deserialize(bytes);
			}
			if ((num & 32) != 0) {
				gameApplicationView.Version = StringProxy.Deserialize(bytes);
			}
			return gameApplicationView;
		}
	}
}
