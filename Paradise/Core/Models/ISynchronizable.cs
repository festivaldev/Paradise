using System.Collections.Generic;

namespace Paradise.Core.Models {
	public interface ISynchronizable {
		SortedList<int, object> Changes { get; }
	}
}
