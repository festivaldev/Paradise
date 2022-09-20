using Paradise.Core.Models.Views;
using System;
using System.Collections.Generic;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class UberstrikeLevelViewModel {
		public UberstrikeLevelViewModel() {
			this.Maps = new List<MapView>();
		}

		public List<MapView> Maps { get; set; }
	}
}
