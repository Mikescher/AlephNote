using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows;

namespace AlephNote.Repository
{
	class SynchronizationDispatcher
	{
		public enum DispatcherMode { AppDispatcher, CustomDispatcher }

		private class DispatchItem
		{
			public volatile bool Executed = false;
			public Action Action;
		}

		private class SwitchModeObject : IDisposable
		{
			private readonly DispatcherMode oldMode;
			private readonly DispatcherMode newMode;
			private readonly SynchronizationDispatcher owner;

			public SwitchModeObject(SynchronizationDispatcher d, DispatcherMode m)
			{
				oldMode = d.mode;
				owner = d;
				newMode = m;

				d.mode = m;
			}

			public void Dispose()
			{
				owner.mode = oldMode;
				if (newMode == DispatcherMode.CustomDispatcher) owner.Work();
			}
		}

		private readonly ConcurrentQueue<DispatchItem> queue = new ConcurrentQueue<DispatchItem>();
		private DispatcherMode mode = DispatcherMode.AppDispatcher;

		public void Invoke(Action a)
		{
			if (mode == DispatcherMode.AppDispatcher)
			{
				Application.Current.Dispatcher.Invoke(a);
			}
			else if (mode == DispatcherMode.CustomDispatcher)
			{
				var i = new DispatchItem { Action = a };
				queue.Enqueue(i);
				while (!i.Executed) Thread.Sleep(0);
			}
		}

		public void BeginInvoke(Action a)
		{
			if (mode == DispatcherMode.AppDispatcher)
			{
				Application.Current.Dispatcher.BeginInvoke(a);
			}
			else if (mode == DispatcherMode.CustomDispatcher)
			{
				var i = new DispatchItem { Action = a };
				queue.Enqueue(i);
			}
		}

		public IDisposable EnableCustomDispatcher()
		{
			return new SwitchModeObject(this, DispatcherMode.CustomDispatcher);
		}

		public void Work()
		{
			if (mode == DispatcherMode.CustomDispatcher)
			{
				DispatchItem item;
				while (queue.TryDequeue(out item))
				{
					try
					{
						item.Action();
						item.Executed = true;
					}
					catch (Exception e)
					{
						App.Logger.Error("Dispatcher", "A dispatched method call threw an exception", e);
					}
				}
			}
		}
	}
}
