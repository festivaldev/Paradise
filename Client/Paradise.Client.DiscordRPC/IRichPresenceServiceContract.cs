using System;
using System.Xml.Serialization;

namespace Paradise {
	public partial class RichPresenceSerializable {
		[XmlElement]
		public bool ClearPresence;

		[XmlElement]
		public string State;

		[XmlElement]
		public string Details;

		[XmlElement]
		public ButtonSerializable[] Buttons;

		[XmlElement]
		public TimestampsSerializable Timestamps;

		[XmlElement]
		public AssetsSerializable Assets;

		[XmlElement]
		public PartySerializable Party;

		[XmlElement]
		public SecretsSerializable Secrets;
	}

	public partial class ButtonSerializable {
		[XmlElement]
		public string Label;

		[XmlElement]
		public string Url;
	}

	public partial class TimestampsSerializable {
		[XmlElement]
		public DateTime? Start;

		[XmlElement]
		public ulong? StartUnixMilliseconds;

		[XmlElement]
		public DateTime? End;

		[XmlElement]
		public ulong? EndUnixMilliseconds;
	}

	public partial class AssetsSerializable {
		[XmlElement]
		public string LargeImageKey;

		[XmlElement]
		public string LargeImageText;

		[XmlElement]
		public string SmallImageKey;

		[XmlElement]
		public string SmallImageText;
	}

	public partial class PartySerializable {
		[XmlElement]
		public int Max;

		[XmlElement]
		public int Privacy;

		[XmlElement]
		public int Size;
	}

	public partial class SecretsSerializable {
		[XmlElement]
		public string JoinSecret;

		[XmlElement]
		public string MatchSecret;

		[XmlElement]
		public string SpectateSecret;
	}
}