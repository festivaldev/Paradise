using System;

namespace Paradise.Core.Models {
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class SyncronizedPropertyAttribute : Attribute {
		public SyncronizedPropertyAttribute(int id) {
			this.ID = id;
		}

		public int ID { get; private set; }
	}
}
