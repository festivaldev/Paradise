using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Paradise.Realtime.Core {
	public class Loop : ILoop {
		private readonly Action<Exception> exceptionHandler;
		private readonly Action handler;
		private readonly ConcurrentQueue<Action> workQueue;
		private Stopwatch stopwatch;

		public Loop(Action handler, Action<Exception> exceptionHandler) {
			this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
			this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
			stopwatch = new Stopwatch();
			workQueue = new ConcurrentQueue<Action>();
		}

		public float Time { get; private set; }
		public float DeltaTime { get; private set; }

		public void Enqueue(Action action) {
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			workQueue.Enqueue(action);
		}

		public void Setup() {
			stopwatch.Restart();
			Time = 0f;
			DeltaTime = 0f;
		}

		public void Teardown() { }

		public void Tick() {
			ExecuteActions();
			Update();
			CalculateTime();
		}

		private void ExecuteActions() {
			while (!workQueue.IsEmpty) {
				if (!workQueue.TryDequeue(out var action))
					continue;

				try {
					action();
				} catch (ThreadAbortException) {
					throw;
				} catch (Exception e) {
					exceptionHandler(e);
				}
			}
		}

		private void Update() {
			try {
				handler();
			} catch (ThreadAbortException) {
				throw;
			} catch (Exception e) {
				exceptionHandler(e);
			}
		}

		private void CalculateTime() {
			stopwatch.Stop();
			var elapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
			stopwatch.Restart();

			Time += elapsed;
			DeltaTime = elapsed;
		}
	}
}
