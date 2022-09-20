using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class RegisterClientApplicationViewModel {
		public ApplicationRegistrationResult Result { get; set; }

		public ICollection<int> ItemsAttributed { get; set; }
	}
}
