using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Core.Models.Views {
	public class PhotonsClusterView {
		public int PhotonsClusterId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<PhotonView> Photons { get; set; }

		public PhotonsClusterView(int photonsClusterId, string name, string description, List<PhotonView> photons) {
			this.PhotonsClusterId = photonsClusterId;
			this.Name = name;
			this.Description = description;
			this.Photons = photons;
		}

		public PhotonsClusterView(int photonsClusterId, string name, List<PhotonView> photons) : this(photonsClusterId, name, string.Empty, photons) { }
	}
}
