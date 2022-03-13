using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise {
    public interface ITimer {
        bool IsEnabled { get; set; }
        float Interval { get; set; }

        event Action Elapsed;

        void Start();
        void Stop();
        void Reset();
        void Restart();
        bool Tick();
    }
}
