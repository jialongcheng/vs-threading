﻿namespace Microsoft.VisualStudio.Threading {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// An asynchronous style countdown event.
	/// </summary>
	public class AsyncCountdownEvent {
		/// <summary>
		/// The manual reset event we use to signal all awaiters.
		/// </summary>
		private readonly AsyncManualResetEvent manualEvent;

		/// <summary>
		/// The remaining number of signals required before we can unblock waiters.
		/// </summary>
		private int remainingCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCountdownEvent"/> class.
		/// </summary>
		/// <param name="initialCount">The number of signals required to unblock awaiters.</param>
		public AsyncCountdownEvent(int initialCount) {
			Requires.Range(initialCount >= 0, "initialCount");
			this.manualEvent = new AsyncManualResetEvent(initialCount == 0);
			this.remainingCount = initialCount;
		}

		/// <summary>
		/// Returns an awaitable that executes the continuation when the countdown reaches zero.
		/// </summary>
		/// <returns>An awaitable.</returns>
		public Task WaitAsync() {
			return this.manualEvent.WaitAsync();
		}

		/// <summary>
		/// Decrements the counter by one.
		/// </summary>
		public Task SignalAsync() {
			int newCount = Interlocked.Decrement(ref this.remainingCount);
			if (newCount == 0) {
				return this.manualEvent.SetAsync();
			} else if (newCount < 0) {
				return ThreadingTools.CreateFaultedTask(new InvalidOperationException());
			} else {
				return TplExtensions.CompletedTask;
			}
		}

		/// <summary>
		/// Decrements the counter by one and returns an awaitable that executes the continuation when the countdown reaches zero.
		/// </summary>
		/// <returns>An awaitable.</returns>
		public Task SignalAndWaitAsync() {
			return Task.WhenAll(this.SignalAsync(), this.WaitAsync());
		}
	}
}