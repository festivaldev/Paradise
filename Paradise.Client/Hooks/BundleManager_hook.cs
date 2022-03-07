using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paradise.Client {
	public static class BundleManager_hook {
		public static Action<Steamworks.MicroTxnAuthorizationResponse_t> OnMicroTxnCallback;

		public static void CallOnMicroTxnCallback() {
			//callback(new Steamworks.MicroTxnAuthorizationResponse_t() { m_bAuthorized = 1, m_ulOrderID = (ulong)DateTime.Now.Ticks });
			OnMicroTxnCallback?.Invoke(new Steamworks.MicroTxnAuthorizationResponse_t { m_bAuthorized = 1 });
		}
	}
}
