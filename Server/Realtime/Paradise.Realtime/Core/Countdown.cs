using System;

namespace Paradise.Realtime.Core {
	public class Countdown {
		private readonly FixedTimer _timer;

		public bool IsEnabled {
			get => _timer.IsEnabled;
			set => _timer.IsEnabled = value;
		}

		public int Count { get; private set; }
		public int StartCount { get; }
		public int EndCount { get; }

		public Countdown(ILoop loop, int startCount, int endCount) {
			if (startCount <= endCount)
				throw new ArgumentOutOfRangeException();

			StartCount = startCount;
			EndCount = endCount;
			_timer = new FixedTimer(loop, 1000f);
			Reset();
		}

		public event Action Completed;
		public event Action<int> Counted;

		public void Start() {
			_timer.Start();
			/* Count first value on Start. */
			DoCountdown();
		}

		public void Stop() => _timer.Stop();

		public void Reset() {
			Count = StartCount;
			_timer.Reset();
		}

		public void Restart() {
			Reset();
			Start();
		}

		public void Tick() {
			while (_timer.Tick())
				DoCountdown();
		}

		private void DoCountdown() {
			Counted?.Invoke(Count);
			var newCount = Count - 1;

			if (newCount < EndCount) {
				Stop();
				Completed?.Invoke();
			} else {
				Count = newCount;
			}
		}
	}
}
