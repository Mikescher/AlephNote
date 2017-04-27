using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AlephNote.WPF.BindingProxies
{
	public static class EventProxy
	{
		public static Delegate Subscribe(object target, EventInfo evt, Action d)
		{
			var handlerType = evt.EventHandlerType;
			var eventParams = handlerType.GetMethod("Invoke").GetParameters();

			//lambda: (object x0, EventArgs x1) => d()
			var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, "x"));
			var body = Expression.Call(Expression.Constant(d), d.GetType().GetMethod("Invoke"));
			var lambda = Expression.Lambda(body, parameters.ToArray());

			var evtDelegate = Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);

			evt.AddEventHandler(target, evtDelegate);

			return evtDelegate;
		}

		public static void Unsubscribe(object target, EventInfo evt, Delegate d)
		{
			evt.RemoveEventHandler(target, d);
		}
	}
}
