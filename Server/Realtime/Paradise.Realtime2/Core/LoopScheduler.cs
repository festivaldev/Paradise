using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Paradise.Realtime.Core {
	public class LoopScheduler : ILoopScheduler, IDisposable {
		private static int schedulerId;
		private readonly List<ILoop> loops = new List<ILoop>();
		private readonly ManualResetEventSlim pauseHandle = new ManualResetEventSlim(true);
		private readonly Stopwatch stopwatch = new Stopwatch();
		private readonly Thread thread;
		private bool disposed;
		private float lag;
		private float load;
		private int loadTick;
		private bool started;
		public bool IsPaused => !pauseHandle.IsSet;

		public float TickInterval {
			get { return 1000 / TickRate; }
		}

		public float TickRate { get; private set; }
		public IReadOnlyCollection<ILoop> Loops => loops.AsReadOnly();

		public LoopScheduler(float tickRate) {
			if (tickRate <= 0)
				throw new ArgumentOutOfRangeException(nameof(tickRate), "Tick rate cannot be less or equal to 0.");

			thread = new Thread(DoScheduling) {
				Name = "LoopScheduler-thread-" + Interlocked.Increment(ref schedulerId)
			};

			TickRate = tickRate;
		}

		public void Dispose() {
			Dispose(true);
		}

		public void Schedule(ILoop loop) {
			ThrowIfDisposed();

			if (loop == null)
				throw new ArgumentNullException(nameof(loop));

			/* Setup loop for scheduling. */
			loop.Setup();
			var wasPaused = IsPaused;

			if (!wasPaused)
				PauseInternal();

			lock (loops) {
				if (loops.Contains(loop))
					throw new InvalidOperationException("Already scheduling the specified ILoop instance.");

				loops.Add(loop);
			}

			if (!wasPaused) {
				ResumeInternal();
			}
		}

		public bool Unschedule(ILoop loop) {
			ThrowIfDisposed();

			if (loop == null)
				throw new ArgumentNullException(nameof(loop));

			var result = false;
			loop.Teardown();
			var wasPaused = IsPaused;

			if (!wasPaused) {
				PauseInternal();
			}

			lock (loops) {
				result = loops.Remove(loop);
			}

			if (!wasPaused) {
				ResumeInternal();
			}

			return result;
		}
		public float GetLoad() {
			ThrowIfDisposed();

			return (!IsPaused ? load / Math.Max(loadTick, 1) : 0) + Loops.Count;
		}

		public void Start() {
			ThrowIfDisposed();

			if (started)
				throw new InvalidOperationException("LoopScheduler already started.");

			started = true;
			thread.Start();
		}

		public void Stop() {
			ThrowIfDisposed();

			if (!started)
				throw new InvalidOperationException("LoopScheduler not started.");

			Dispose();
		}

		public void Pause() {
			ThrowIfDisposed();
			PauseInternal();
		}

		public void Resume() {
			ThrowIfDisposed();

			lag = 0f;

			ResumeInternal();
		}

		private void DoScheduling() {
			var interval = (int)Math.Ceiling(TickInterval);
			var loadInterval = (int)Math.Ceiling(TickRate * 3);

			try {
				while (started) {
					pauseHandle.Wait();
					stopwatch.Start();

					for (var i = 0; i < loops.Count; i++) {
						loops[i].Tick();
					}

					loadTick++;
					Thread.Sleep(interval);
					/* 
                     * Stop stopwatch after Thread.Sleep to measure inaccuracy
                     * in sleep call as well.
                     */
					stopwatch.Stop();
					var elapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
					stopwatch.Restart();
					lag += elapsed;

					if (loadTick < loadInterval) {
						load += elapsed - TickInterval;
					} else {
						/* Reset load measurement every loadInterval. */
						load = 0;
						loadTick = 0;
					}

					stopwatch.Stop();
				}
			} catch (ThreadAbortException) {

			}
		}

		protected virtual void Dispose(bool disposing) {
			if (disposed)
				return;

			if (disposing) {
				started = false;
				/*
				 * Give the thread a chance to spin a couple of times to shut
				 * down gracefully then kill if it does not.
				 */
				ResumeInternal();

				if (!thread.Join(Math.Min((int)(TickInterval * 3), 300))) {
					thread.Abort();
				}

				foreach (var loop in loops) {
					loop.Teardown();
				}

				loops.Clear();
				pauseHandle.Dispose();
			}

			disposed = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PauseInternal() => pauseHandle.Reset();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResumeInternal() => pauseHandle.Set();

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void ThrowIfDisposed() {
			if (disposed)
				throw new ObjectDisposedException(null);
		}
	}
}
