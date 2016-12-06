using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace JoeForshaw.Messenger
{
    public static class Messenger
    {
        static readonly MessageDispatcher Callbacks = new MessageDispatcher ();

        public static void Send<TArgs> (string message, TArgs args)
        {            
            InnerSend (message, typeof (TArgs), args);
        }

        public static void Send (string message)
        {            
            InnerSend (message, null, null);
        }
        
        public static void Send<TArgs> (IDable idable, TArgs args)
        {            
            InnerSend (GetIDableSignature (idable), typeof (TArgs), args);
        }
        
        public static void Send (IDable idable)
        {            
            InnerSend (GetIDableSignature (idable), null, null);
        }
        
        public static void Subscribe<TArgs> (IDable idable, Action<TArgs> callback)
        {
            InnerSubscribe (idable, GetIDableSignature (idable), typeof (TArgs), args => callback ((TArgs) args));
        }
        
        public static void Subscribe (IDable idable, Action callback)
        {
            InnerSubscribe (idable, GetIDableSignature (idable), null, args => callback ());
        }
        
        public static void Subscribe<TArgs> (IEnumerable<IDable> idables, Action<TArgs> callback)
        {
            foreach (var idable in idables)
            {
                InnerSubscribe (idable, GetIDableSignature (idable), typeof (TArgs), args => callback ((TArgs) args));
            }
        }
        
        public static void Subscribe (IEnumerable<IDable> idables, Action callback)
        {
            foreach (var idable in idables)
            {
                InnerSubscribe (idable, GetIDableSignature (idable), null, args => callback ());
            }
        }

        public static void Subscribe<TArgs> (object subscriber, string message, Action<TArgs> callback)
        {
            InnerSubscribe (subscriber, message, typeof (TArgs), (args) => callback ((TArgs) args));
        }

        public static void Subscribe (object subscriber, string message, Action callback)
        {
            InnerSubscribe (subscriber, message, null, (args) => callback ());
        }
        
        public static void Unsubscribe<TArgs> (IDable idable)
        {
            InnerUnsubscribe (idable, GetIDableSignature (idable), typeof (TArgs));
        }

        public static void Unsubscribe (IDable idable)
        {
            InnerUnsubscribe (idable, GetIDableSignature (idable), null);
        }
        
        public static void Unsubscribe<TArgs> (IEnumerable<IDable> idables)
        {
            foreach (var idable in idables)
            {
                InnerUnsubscribe (idable, GetIDableSignature (idable), typeof (TArgs));
            }
        }
        
        public static void Unsubscribe (IEnumerable<IDable> idables)
        {
            foreach (var idable in idables)
            {
                InnerUnsubscribe (idable, GetIDableSignature (idable), null);
            }
        }

        public static void Unsubscribe<TArgs> (object subscriber, string message)
        {
            InnerUnsubscribe (subscriber, message, typeof (TArgs));
        }

        public static void Unsubscribe (object subscriber, string message)
        {
            InnerUnsubscribe (subscriber, message, null);
        }

        public static void ClearAllSubscribers ()
        {
            Callbacks.Clear ();
        }

        static string GetIDableSignature (IDable idable)
        {
            return $"{idable.GetType ().FullName}.{idable.ID}";
        }

        static void InnerSend (string message, Type argType, object args)
        {
            if (message == null) { throw new ArgumentNullException (nameof (message)); }
            
            var key = new MessageSignature (message, argType);
            
            if (!Callbacks.ContainsKey (key)) { return; }
            
            var actions = Callbacks [key];
            
            if (actions == null || !actions.Any ()) { return; }
            
            foreach (var action in actions)
            {
                if (action.Item1.Target != null && actions.Contains (action))
                {
                    action.Item2 (args);
                }
            }
        }
        
        static void InnerSubscribe (object subscriber, string message, Type argType, Action<object> callback)
        {
            if (subscriber == null) { throw new ArgumentNullException (nameof (subscriber)); }
            
            if (message == null) { throw new ArgumentNullException (nameof (message)); }
        
            if (callback == null) { throw new ArgumentNullException (nameof (callback)); }
            
            var key = new MessageSignature (message, argType);
            
            var value = new MessageSubscription (subscriber, callback);
            
            if (Callbacks.ContainsKey (key))
            {
                Callbacks [key].Add (value);
            }
            else
            {
                Callbacks [key] = new List<MessageSubscription> { value };
            }
        }
        
        static void InnerUnsubscribe (object subscriber, string message, Type argType)
        {
            if (subscriber == null) { throw new ArgumentNullException (nameof (subscriber)); }
            
            if (message == null) { throw new ArgumentNullException (nameof (message)); }

            var key = new MessageSignature (message, argType);
            
            if (!Callbacks.ContainsKey (key)) { return; }
            
            Callbacks [key].RemoveAll (tuple => !tuple.Item1.IsAlive || tuple.Item1.Target == subscriber);
            
            if (Callbacks [key].Any ())
            {
                var deleted = new List<MessageSubscription> ();
                while (Callbacks.TryRemove (key, out deleted)) { }
            }
        }
    }
    
    public interface IDable
    {
        int ID { get; set; }
    }
    
    class MessageDispatcher : ConcurrentDictionary<MessageSignature, List<MessageSubscription>> { }
    
    class MessageSignature : Tuple<string, Type>
    {    
        public MessageSignature (string key, Type argType) : base (key, argType) { }
    }
    
    class MessageSubscription : Tuple<WeakReference, Action<object>>
    {
        public MessageSubscription (object caller, Action<object> callback) : base (new WeakReference (caller), callback) { }
    }
}
