using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;

namespace Paradise.WebServices.LegacyServices {
	public class UserAccount {
		public int Cmid { get; set; }
		public string EmailAddress { get; set; }
		public string Password { get; set; }
		public ChannelType Channel { get; set; }
		public string Locale { get; set; }
		public TutorialStepType TutorialStep { get; set; }
	}
}
