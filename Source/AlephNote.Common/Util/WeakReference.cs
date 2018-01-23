using System;
using System.Runtime.Serialization;

namespace AlephNote.Common.Util
{
	[Serializable]
	public class WeakReference<T> : WeakReference
	{
		public WeakReference(T target)
			: base(target)
		{
		}

		public WeakReference(T target, bool trackResurrection) : base(target, trackResurrection)
		{
		}
		
		public new T Target => (T)base.Target;
	}
}
