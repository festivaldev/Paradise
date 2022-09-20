using Paradise.Core.Models.Views;
using System;
using System.IO;
using Paradise.DataCenter.Common.Entities;

namespace Paradise.Core.Serialization.Legacy
{
	public static class AuthenticateApplicationViewProxy
	{
		public static void Serialize(Stream stream, AuthenticateApplicationView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					if (instance.CommServer != null)
					{
						PhotonViewProxy.Serialize(memoryStream, instance.CommServer);
					}
					else
					{
						num |= 1;
					}
					if (instance.EncryptionInitVector != null)
					{
						StringProxy.Serialize(memoryStream, instance.EncryptionInitVector);
					}
					else
					{
						num |= 2;
					}
					if (instance.EncryptionPassPhrase != null)
					{
						StringProxy.Serialize(memoryStream, instance.EncryptionPassPhrase);
					}
					else
					{
						num |= 4;
					}
					if (instance.GameServers != null)
					{
						ListProxy<PhotonView>.Serialize(memoryStream, instance.GameServers, new ListProxy<PhotonView>.Serializer<PhotonView>(PhotonViewProxy.Serialize));
					}
					else
					{
						num |= 8;
					}
					BooleanProxy.Serialize(memoryStream, instance.IsEnabled);
					BooleanProxy.Serialize(memoryStream, instance.WarnPlayer);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static AuthenticateApplicationView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			AuthenticateApplicationView authenticateApplicationView = null;
			if (num != 0)
			{
				authenticateApplicationView = new AuthenticateApplicationView();
				if ((num & 1) != 0)
				{
					authenticateApplicationView.CommServer = PhotonViewProxy.Deserialize(bytes);
				}
				if ((num & 2) != 0)
				{
					authenticateApplicationView.EncryptionInitVector = StringProxy.Deserialize(bytes);
				}
				if ((num & 4) != 0)
				{
					authenticateApplicationView.EncryptionPassPhrase = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0)
				{
					authenticateApplicationView.GameServers = ListProxy<PhotonView>.Deserialize(bytes, new ListProxy<PhotonView>.Deserializer<PhotonView>(PhotonViewProxy.Deserialize));
				}
				authenticateApplicationView.IsEnabled = BooleanProxy.Deserialize(bytes);
				authenticateApplicationView.WarnPlayer = BooleanProxy.Deserialize(bytes);
			}
			return authenticateApplicationView;
		}
	}
}
