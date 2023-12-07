using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Paradise.Realtime.Core {
	public class Loop : ILoop {
		private readonly Action<Exception> _exceptionHandler;
		private readonly Action _handler;
		private readonly ConcurrentQueue<Action> _workQueue;
		private Stopwatch _stopwatch;

		public Loop(Action handler, Action<Exception> exceptionHandler) {
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
			_exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
			_stopwatch = new Stopwatch();
			_workQueue = new ConcurrentQueue<Action>();
		}

		public float Time { get; private set; }
		public float DeltaTime { get; private set; }

		public void Setup() {
			_stopwatch.Restart();
			Time = 0f;
			DeltaTime = 0f;
		}

		public void Teardown() {
			/* Space. */
		}

		public void Tick() {
			DoActions();
			DoUpdate();
			DoTime();
		}

		public void Enqueue(Action action) {
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_workQueue.Enqueue(action);
		}

		/* Execute all the actions enqueued. */
		private void DoActions() {
			while (!_workQueue.IsEmpty) {
				if (!_workQueue.TryDequeue(out var action))
					continue;

				try {
					action();
				} catch (ThreadAbortException) {
					throw;
				} catch (Exception e) {
					_exceptionHandler(e);
				}
			}
		}

		/* Do time calculations. */
		private void DoTime() {
			_stopwatch.Stop();
			var elapsed = (float)_stopwatch.Elapsed.TotalMilliseconds;
			_stopwatch.Restart();
			Time += elapsed;
			DeltaTime = elapsed;
		}

		/* Call the handler. */
		private void DoUpdate() {
			/*
             * Pass control to the user code and if it throws an exception,
             * pass it back to user exception handler.
             */
			try {
				_handler();
			} catch (ThreadAbortException) {
				throw;
			} catch (Exception e) {
				_exceptionHandler(e);
			}
		}
	}
}
