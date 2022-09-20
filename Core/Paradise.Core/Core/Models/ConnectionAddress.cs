using System;

namespace Paradise.Core.Models {
	[Serializable]
	public class ConnectionAddress {
		public ConnectionAddress() {
		}

		public ConnectionAddress(string connection) {
			try {
				string[] array = connection.Split(new char[]
				{
					':'
				});
				this.Ipv4 = ConnectionAddress.ToInteger(array[0]);
				this.Port = ushort.Parse(array[1]);
			} catch {
			}
		}

		public ConnectionAddress(string ipAddress, ushort port) {
			this.Ipv4 = ConnectionAddress.ToInteger(ipAddress);
			this.Port = port;
		}

		public int Ipv4 { get; set; }

		public ushort Port { get; set; }

		public string ConnectionString {
			get {
				return string.Format("{0}:{1}", ConnectionAddress.ToString(this.Ipv4), this.Port);
			}
		}

		public string IpAddress {
			get {
				return ConnectionAddress.ToString(this.Ipv4);
			}
		}

		public static string ToString(int ipv4) {
			return string.Format("{0}.{1}.{2}.{3}", new object[]
			{
				ipv4 >> 24 & 255,
				ipv4 >> 16 & 255,
				ipv4 >> 8 & 255,
				ipv4 & 255
			});
		}

		public static int ToInteger(string ipAddress) {
			int num = 0;
			string[] array = ipAddress.Split(new char[]
			{
				'.'
			});
			if (array.Length == 4) {
				for (int i = 0; i < array.Length; i++) {
					num |= int.Parse(array[i]) << (3 - i) * 8;
				}
			}
			return num;
		}
	}
}
