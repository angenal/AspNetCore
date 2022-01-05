using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebCore
{
    /// <summary>Provides methods to register and deregister weak events. </summary>
    public static class Events
    {
        private static List<WeakEventRegistration> _registeredWeakEvents = null;
        internal static List<WeakEventRegistration> RegisteredWeakEvents
        {
            get
            {
                if (_registeredWeakEvents == null)
                {
                    lock (typeof(EventRegister))
                    {
                        if (_registeredWeakEvents == null)
                            _registeredWeakEvents = new List<WeakEventRegistration>();
                    }
                }
                return _registeredWeakEvents;
            }
        }

        /// <summary>Registers a weak event handler which is automatically deregistered after the subscriber
        /// has been garbage collected (checked on each event call). </summary>
        public static EventHandler<TArgs> Register<TSubscriber, TArgs>(
            TSubscriber subscriber,
            Action<EventHandler<TArgs>> register,
            Action<EventHandler<TArgs>> deregister,
            Action<TSubscriber, object, TArgs> handler)
            where TArgs : EventArgs
            where TSubscriber : class
        {
            Func<EventHandler<TArgs>, EventHandler<TArgs>> converter = h => h;
            var weakReference = new WeakReference(subscriber);
            EventHandler<TArgs> @delegate = null;
            @delegate = converter(
                (s, e) =>
                {
                    if (weakReference.Target is TSubscriber strongReference)
                    {
                        handler(strongReference, s, e);
                    }
                    else
                    {
                        deregister(@delegate);
                        @delegate = null;
                    }
                });
            register(@delegate);
            return @delegate;
        }

        /// <summary>Registers a weak event handler which is automatically deregistered after the subscriber
        /// has been garbage collected (checked on each event call). </summary>
        /// <param name="subscriber"></param>
        /// <param name="deregister"></param>
        /// <param name="register"></param>
        /// <param name="converter">The converter: h => (o, e) => h(o, e)</param>
        /// <param name="handler"></param>
        public static TDelegate Register<TSubscriber, TDelegate, TArgs>(
            TSubscriber subscriber,
            Action<TDelegate> register,
            Action<TDelegate> deregister,
            Func<EventHandler<TArgs>, TDelegate> converter,
            Action<TSubscriber, object, TArgs> handler)
            where TArgs : EventArgs
            where TDelegate : class
            where TSubscriber : class
        {
            var weakReference = new WeakReference(subscriber);
            TDelegate @delegate = null;
            @delegate = converter(
                (s, e) =>
                {
                    var strongReference = weakReference.Target as TSubscriber;
                    if (strongReference != null)
                        handler(strongReference, s, e);
                    else
                    {
                        deregister(@delegate);
                        @delegate = null;
                    }
                });
            register(@delegate);
            return @delegate;
        }

#if !LEGACY

        /// <summary>Adds a weak event handler to the given source object. </summary>
        /// <typeparam name="TEventSource">The type of the source object.</typeparam>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <param name="source">The source object to register the event on. </param>
        /// <param name="eventName">The event name to create the registration for.</param>
        /// <param name="handler">The delegate that handles the event.</param>
        public static void Register<TEventSource, TEventArgs>(TEventSource source, string eventName, EventHandler<TEventArgs> handler)
        {
            var eventInfo = typeof(TEventSource).GetRuntimeEvent(eventName);
            RegisteredWeakEvents.Add(new WeakEventRegistration(source, eventInfo, handler));
        }

        /// <summary>Adds a static weak event handler to a static event. </summary>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <param name="sourceType">The type of the class that contains the static event. </param>
        /// <param name="eventName">The event name to create the registration for.</param>
        /// <param name="handler">The delegate that handles the event.</param>
        public static void RegisterType<TEventArgs>(Type sourceType, string eventName, EventHandler<TEventArgs> handler)
        {
            var eventInfo = sourceType.GetRuntimeEvent(eventName);
            RegisteredWeakEvents.Add(new WeakEventRegistration(null, eventInfo, handler));
        }

        /// <summary>Adds a static weak event handler to a static event. </summary>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <typeparam name="TEventSource">The type of the class that contains the static event. </typeparam>
        /// <param name="eventName">The event name to create the registration for.</param>
        /// <param name="handler">The delegate that handles the event.</param>
        public static void Register<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> handler)
        {
            var eventInfo = typeof(TEventSource).GetRuntimeEvent(eventName);
            RegisteredWeakEvents.Add(new WeakEventRegistration(null, eventInfo, handler));
        }

        /// <summary>Removes a weak event registration from the given source object.</summary>
        /// <typeparam name="TEventSource">The type of the source object.</typeparam>
        /// <param name="source">The source object to register the event from. </param>
        /// <param name="eventName">The event name to remove the registration from.</param>
        /// <param name="handler">The handler to remove.</param>
        /// <returns>True if the event registration could be found and was removed. </returns>
        public static bool Deregister<TEventSource>(TEventSource source, string eventName, Delegate handler)
        {
            var eventInfo = typeof(TEventSource).GetRuntimeEvent(eventName);
            return DeregisterEvent(source, handler, eventInfo);
        }

        /// <summary>Removes a static weak event registration from a static event.</summary>
        /// <param name="sourceType">The type of the class that contains the static event. </param>
        /// <param name="eventName">The event name to remove the registration from.</param>
        /// <param name="handler">The handler to remove. </param>
        /// <returns>True if the event registration could be found and was removed. </returns>
        public static bool DeregisterType(Type sourceType, string eventName, Delegate handler)
        {
            var eventInfo = sourceType.GetRuntimeEvent(eventName);
            return DeregisterEvent(null, handler, eventInfo);
        }

        private static bool DeregisterEvent(object source, Delegate handler, EventInfo eventInfo)
        {
            var weakEvent = RegisteredWeakEvents.FirstOrDefault(e => e.Matches(source, eventInfo, handler));
            if (weakEvent != null)
                weakEvent.DeregisterEvent();
            return weakEvent != null;
        }

        internal class WeakEventRegistration
        {
            private static readonly MethodInfo OnEventCalledInfo =
              typeof(WeakEventRegistration).GetTypeInfo().GetDeclaredMethod("OnEventCalled");

            private EventInfo _eventInfo;
            private object _eventHandler;

            private readonly object _source;

            private readonly MethodInfo _handlerMethod;
            private readonly WeakReference<object> _handlerTarget;

            public WeakEventRegistration(object source, EventInfo eventInfo, Delegate handler)
            {
                _source = source;
                _eventInfo = eventInfo;

                _handlerMethod = handler.GetMethodInfo();
                _handlerTarget = new WeakReference<object>(handler.Target);

                var eventHandler = CreateEventHandler();
                _eventHandler = eventInfo.AddMethod.Invoke(source, new object[] { eventHandler });

                if (_eventHandler == null)
                    _eventHandler = eventHandler;
            }

            public bool Matches(object source, EventInfo eventInfo, Delegate handler)
            {
                if (source == _source && Equals(eventInfo, _eventInfo))
                {
                    object target;
                    if (_handlerTarget.TryGetTarget(out target))
                        return handler.Target == target && Equals(handler.GetMethodInfo(), _handlerMethod);
                }

                return false;
            }

            public void DeregisterEvent()
            {
                if (_eventInfo != null)
                {
                    RegisteredWeakEvents.Remove(this);

                    _eventInfo.RemoveMethod.Invoke(_source, new object[] { _eventHandler });

                    _eventHandler = null;
                    _eventInfo = null;
                }
            }

            public void OnEventCalled<T>(object sender, T args)
            {
                object instance;
                if (_handlerTarget.TryGetTarget(out instance))
                    _handlerMethod.Invoke(instance, new object[] { sender, args });
                else
                    DeregisterEvent();
            }

            private object CreateEventHandler()
            {
                Type eventType = _eventInfo.EventHandlerType;
                ParameterInfo[] parameters = eventType.GetTypeInfo()
                  .GetDeclaredMethod("Invoke")
                  .GetParameters();

                return OnEventCalledInfo
                  .MakeGenericMethod(parameters[1].ParameterType)
                  .CreateDelegate(eventType, this);
            }
        }

#else

        /// <summary>Adds a weak event handler to the given source object. </summary>
        /// <typeparam name="TEventSource">The type of the source object.</typeparam>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <param name="source">The source object to register the event on. </param>
        /// <param name="eventName">The event name to create the registration for.</param>
        /// <param name="handler">The delegate that handles the event.</param>
        public static void Register<TEventSource, TEventArgs>(TEventSource source, string eventName, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            var eventInfo = typeof(TEventSource).GetEvent(eventName);
            RegisteredWeakEvents.Add(new WeakEventRegistration(source, eventInfo, handler));
        }

        /// <summary>Adds a static weak event handler to a static event. </summary>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <param name="sourceType">The type of the class that contains the static event. </param>
        /// <param name="eventName">The event name to create the registration for.</param>
        /// <param name="handler">The delegate that handles the event.</param>
        public static void RegisterStaticWeakEvent<TEventArgs>(Type sourceType, string eventName, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            var eventInfo = sourceType.GetEvent(eventName);
            RegisteredWeakEvents.Add(new WeakEventRegistration(null, eventInfo, handler));
        }

        /// <summary>Adds a static weak event handler to a static event. </summary>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <typeparam name="TEventSource">The type of the class that contains the static event. </typeparam>
        /// <param name="eventName">The event name to create the registration for.</param>
        /// <param name="handler">The delegate that handles the event.</param>
        public static void RegisterStaticWeakEvent<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            var eventInfo = typeof(TEventSource).GetEvent(eventName);
            RegisteredWeakEvents.Add(new WeakEventRegistration(null, eventInfo, handler));
        }

        /// <summary>Removes a weak event registration from the given source object.</summary>
        /// <typeparam name="TEventSource">The type of the source object.</typeparam>
        /// <param name="source">The source object to register the event from. </param>
        /// <param name="eventName">The event name to remove the registration from.</param>
        /// <param name="handler">The handler to remove.</param>
        /// <returns>True if the event registration could be found and was removed. </returns>
        public static bool DeregisterWeakEvent<TEventSource>(TEventSource source, string eventName, Delegate handler)
        {
            var eventInfo = typeof(TEventSource).GetEvent(eventName);
            return DeregisterWeakEvent(source, handler, eventInfo);
        }

        /// <summary>Removes a static weak event registration from a static event.</summary>
        /// <param name="sourceType">The type of the class that contains the static event. </param>
        /// <param name="eventName">The event name to remove the registration from.</param>
        /// <param name="handler">The handler to remove. </param>
        /// <returns>True if the event registration could be found and was removed. </returns>
        public static bool DeregisterStaticWeakEvent(Type sourceType, string eventName, Delegate handler)
        {
            var eventInfo = sourceType.GetEvent(eventName);
            return DeregisterWeakEvent(null, handler, eventInfo);
        }

        private static bool DeregisterWeakEvent(object source, Delegate handler, EventInfo eventInfo)
        {
            var weakEvent = RegisteredWeakEvents.FirstOrDefault(e => e.Matches(source, eventInfo, handler));
            if (weakEvent != null)
                weakEvent.DeregisterEvent();
            return weakEvent != null;
        }

        internal class WeakEventRegistration
        {
            private static readonly MethodInfo _onEventCalledInfo =
              typeof(WeakEvent).GetMethod("OnEventCalled");

            private EventInfo _eventInfo;
            private object _eventHandler;

            private readonly object _source;

            private readonly MethodInfo _handlerMethod;
            private readonly WeakReference _handlerTarget;

            public WeakEventRegistration(object source, EventInfo eventInfo, Delegate handler)
            {
                _source = source;
                _eventInfo = eventInfo;

                _handlerMethod = handler.Method;
                _handlerTarget = new WeakReference(handler.Target);

                var eventHandler = CreateEventHandler();
                _eventHandler = eventInfo.GetAddMethod().Invoke(source, new object[] { eventHandler });

                if (_eventHandler == null)
                    _eventHandler = eventHandler;
            }

            public bool Matches(object source, EventInfo eventInfo, Delegate handler)
            {
                if (source == _source && Equals(eventInfo, _eventInfo))
                {
                    var target = _handlerTarget.Target;
                    if (target != null)
                        return handler.Target == target && Equals(handler.Method, _handlerMethod);
                }

                return false;
            }

            public void DeregisterEvent()
            {
                if (_eventInfo != null)
                {
                    RegisteredWeakEvents.Remove(this);

                    _eventInfo.GetRemoveMethod().Invoke(_source, new object[] { _eventHandler });

                    _eventHandler = null;
                    _eventInfo = null;
                }
            }

            public void OnEventCalled<T>(object sender, T args)
            {
                var instance = _handlerTarget.Target;
                if (instance != null)
                    _handlerMethod.Invoke(instance, new object[] { sender, args });
                else
                    DeregisterEvent();
            }

            private object CreateEventHandler()
            {
                var eventType = _eventInfo.EventHandlerType;
                return Delegate.CreateDelegate(eventType, this, _onEventCalledInfo);
            }
        }

#endif
    }

    /// <summary>Provides methods for event management. </summary>
    public class EventRegister
    {
#if !LEGACY

        /// <summary>Registers an event on the given target object. </summary>
        /// <param name="target">The target object. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="callback">The callback. </param>
        /// <returns>The registration token to deregister the event. </returns>
        public static object RegisterEvent(object target, string eventName, Action<object, object> callback)
        {
            var callbackMethodInfo = callback.GetMethodInfo();
            var eventInfo = target.GetType().GetRuntimeEvent(eventName);
            var callbackDelegate = callbackMethodInfo.CreateDelegate(eventInfo.EventHandlerType, callback.Target);
            return eventInfo.AddMethod.Invoke(target, new object[] { callbackDelegate });
        }

        /// <summary>Registers a static event on the given target object. </summary>
        /// <param name="type">The target type. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="callback">The callback. </param>
        /// <returns>The registration token to deregister the event. </returns>
        public static object RegisterStaticEvent(Type type, string eventName, Action<object, object> callback)
        {
            var callbackMethodInfo = callback.GetMethodInfo();
            var eventInfo = type.GetRuntimeEvent(eventName);
            var callbackDelegate = callbackMethodInfo.CreateDelegate(eventInfo.EventHandlerType, callback.Target);
            return eventInfo.AddMethod.Invoke(null, new object[] { callbackDelegate });
        }

        /// <summary>Deregisters an event from the target object. </summary>
        /// <param name="target">The target object. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="token">The registration token. </param>
        public static void DeregisterEvent(object target, string eventName, object token)
        {
            var eventInfo = target.GetType().GetRuntimeEvent(eventName);
            eventInfo.RemoveMethod.Invoke(target, new object[] { token });
        }

        /// <summary>Deregisters a static event from the target type. </summary>
        /// <param name="type">The target type. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="token">The registration token. </param>
        public static void DeregisterStaticEvent(Type type, string eventName, object token)
        {
            var eventInfo = type.GetRuntimeEvent(eventName);
            eventInfo.RemoveMethod.Invoke(null, new object[] { token });
        }

#else

        /// <summary>Registers an event on the given target object. </summary>
        /// <param name="target">The target object. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="callback">The callback. </param>
        /// <returns>The registration token to deregister the event. </returns>
        public static object RegisterEvent(object target, string eventName, Action<object, object> callback)
        {
            var callbackMethodInfo = callback.Method;
            var eventInfo = target.GetType().GetEvent(eventName);
            var callbackDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, callback.Target, callbackMethodInfo);
            return eventInfo.GetAddMethod().Invoke(target, new object[] { callbackDelegate });
        }

        /// <summary>Registers a static event on the given target object. </summary>
        /// <param name="type">The target type. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="callback">The callback. </param>
        /// <returns>The registration token to deregister the event. </returns>
        public static object RegisterStaticEvent(Type type, string eventName, Action<object, object> callback)
        {
            var callbackMethodInfo = callback.Method;
            var eventInfo = type.GetEvent(eventName);
            var callbackDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, callback.Target, callbackMethodInfo);
            return eventInfo.GetAddMethod().Invoke(null, new object[] { callbackDelegate });
        }

        /// <summary>Deregisters an event from the target object. </summary>
        /// <param name="target">The target object. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="token">The registration token. </param>
        public static void DeregisterEvent(object target, string eventName, object token)
        {
            var eventInfo = target.GetType().GetEvent(eventName);
            eventInfo.GetRemoveMethod().Invoke(target, new object[] { token });
        }

        /// <summary>Deregisters a static event from the target type. </summary>
        /// <param name="type">The target type. </param>
        /// <param name="eventName">The event name. </param>
        /// <param name="token">The registration token. </param>
        public static void DeregisterStaticEvent(Type type, string eventName, object token)
        {
            var eventInfo = type.GetEvent(eventName);
            eventInfo.GetRemoveMethod().Invoke(null, new object[] { token });
        }

#endif
    }
}
