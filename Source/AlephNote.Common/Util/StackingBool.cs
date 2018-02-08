using System;

namespace AlephNote.Common.Util
{
	public class StackingBool
	{
		private class StackingBoolLock : IDisposable
		{
			private readonly StackingBool _sb;
			public StackingBoolLock(StackingBool sb) { _sb = sb; _sb._count++; }
			public void Dispose() { _sb._count--; }
		}

		private int _count = 0;
		public bool Get() => _count > 0;
		public bool Value => _count > 0;

		public IDisposable Set() => new StackingBoolLock(this);
	}
}
