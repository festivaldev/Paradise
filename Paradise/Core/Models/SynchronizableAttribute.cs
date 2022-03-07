using System;

namespace Paradise.Core.Models {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class SynchronizableAttribute : Attribute {
	}
}
