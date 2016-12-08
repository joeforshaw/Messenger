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
        
        public static void Send<TArgs> (IHasID subscriber, TArgs args)
        {            
            InnerSend (GetSignatureFor (subscriber), typeof (TArgs), args);
        }
        
        public static void Send (IHasID subscriber)
        {            
            InnerSend (GetSignatureFor (subscriber), null, null);
        }
        
        public static void Subscribe<TArgs> (IHasID subscriber, Action<TArgs> callback)
        {
            InnerSubscribe (subscriber, GetSignatureFor (subscriber), typeof (TArgs), args => callback ((TArgs) args));
        }
        
        public static void Subscribe (IHasID subscriber, Action callback)
        {
            InnerSubscribe (subscriber, GetSignatureFor (subscriber), null, args => callback ());
        }
        
        public static void Subscribe<TArgs> (IEnumerable<IHasID> subscribers, Action<TArgs> callback)
        {
            foreach (var subscriber in subscribers)
            {
                InnerSubscribe (subscriber, GetSignatureFor (subscriber), typeof (TArgs), args => callback ((TArgs) args));
            }
        }
        
        public static void Subscribe (IEnumerable<IHasID> subscribers, Action callback)
        {
            foreach (var subscriber in subscribers)
            {
                InnerSubscribe (subscriber, GetSignatureFor (subscriber), null, args => callback ());
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
        
        public static void Unsubscribe<TArgs> (IHasID unsubscriber)
        {
            InnerUnsubscribe (unsubscriber, GetSignatureFor (unsubscriber), typeof (TArgs));
        }

        public static void Unsubscribe (IHasID unsubscriber)
        {
            InnerUnsubscribe (unsubscriber, GetSignatureFor (unsubscriber), null);
        }
        
        public static void Unsubscribe<TArgs> (IEnumerable<IHasID> unsubscribers)
        {
            foreach (var unsubscriber in unsubscribers)
            {
                InnerUnsubscribe (unsubscriber, GetSignatureFor (unsubscriber), typeof (TArgs));
            }
        }
        
        public static void Unsubscribe (IEnumerable<IHasID> unsubscribers)
        {
            foreach (var unsubscriber in unsubscribers)
            {
                InnerUnsubscribe (unsubscriber, GetSignatureFor (unsubscriber), null);
            }
        }

        public static void Unsubscribe<TArgs> (object unsubscriber, string message)
        {
            InnerUnsubscribe (unsubscriber, message, typeof (TArgs));
        }

        public static void Unsubscribe (object unsubscriber, string message)
        {
            InnerUnsubscribe (unsubscriber, message, null);
        }

        public static void ClearAllSubscribers ()
        {
            Callbacks.Clear ();
        }

        static string GetSignatureFor (IHasID objectWithID)
        {
            return $"{objectWithID.GetType ().FullName}.{objectWithID.ID}";
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
            if (message    == null) { throw new ArgumentNullException (nameof (message)); }
            if (callback   == null) { throw new ArgumentNullException (nameof (callback)); }
            
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
            if (message    == null) { throw new ArgumentNullException (nameof (message)); }

            var key = new MessageSignature (message, argType);
            
            if (!Callbacks.ContainsKey (key)) { return; }
                        
            if (Callbacks [key].Any ())
            {
                var deleted = new List<MessageSubscription> ();
                while (Callbacks.TryRemove (key, out deleted)) { }
            }
        }
    }
    
    public interface IHasID
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
