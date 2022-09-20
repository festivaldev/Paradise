using System;

namespace Paradise.Core.Models {
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class NonSynchronizableAttribute : Attribute {
	}
}
