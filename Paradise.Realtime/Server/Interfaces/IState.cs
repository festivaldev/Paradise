using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server {
	public interface IState {
		void OnEnter();
		void OnExit();
		void OnResume();
		void OnUpdate();
	}
}
