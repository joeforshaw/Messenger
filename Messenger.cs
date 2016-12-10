using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace JoeForshaw.Messenger
{
    public static class Messenger
    {
        static readonly MessageDispatcher Callbacks = new MessageDispatcher ();

        public static void Send<T> (string message, T args)
        {            
            InnerSend (message, typeof (T), args);
        }

        public static void Send (string message)
        {            
            InnerSend (message, null, null);
        }
        
        public static void Send<T> (IHasID sender, T args)
        {            
            InnerSend (GetSignatureFor (sender), typeof (T), args);
        }
        
        public static void Send (IHasID sender)
        {            
            InnerSend (GetSignatureFor (sender), null, null);
        }
        
        public static void Subscribe<T> (object subscriber, IHasID sender, Action<T> callback)
        {
            InnerSubscribe (subscriber, GetSignatureFor (sender), typeof (T), args => callback ((T) args));
        }
        
        public static void Subscribe (object subscriber, IHasID sender, Action callback)
        {
            InnerSubscribe (subscriber, GetSignatureFor (sender), null, args => callback ());
        }
        
        public static void Subscribe<T> (object subscriber, IEnumerable<IHasID> senders, Action<T> callback)
        {
            foreach (var sender in senders)
            {
                InnerSubscribe (subscriber, GetSignatureFor (sender), typeof (T), args => callback ((T) args));
            }
        }
        
        public static void Subscribe (object subscriber, IEnumerable<IHasID> senders, Action callback)
        {
            foreach (var sender in senders)
            {
                InnerSubscribe (subscriber, GetSignatureFor (sender), null, args => callback ());
            }
        }

        public static void Subscribe<T> (object subscriber, string message, Action<T> callback)
        {
            InnerSubscribe (subscriber, message, typeof (T), (args) => callback ((T) args));
        }

        public static void Subscribe (object subscriber, string message, Action callback)
        {
            InnerSubscribe (subscriber, message, null, (args) => callback ());
        }
        
        public static void Unsubscribe (object unsubscriber, IHasID sender)
        {
            InnerUnsubscribe (unsubscriber, GetSignatureFor (sender), null);
        }
        
        public static void Unsubscribe<T> (object unsubscriber, IHasID sender)
        {
            InnerUnsubscribe (unsubscriber, GetSignatureFor (sender), typeof (T));
        }
        
        public static void Unsubscribe<T> (object unsubscriber, IEnumerable<IHasID> senders)
        {
            foreach (var sender in senders)
            {
                InnerUnsubscribe (unsubscriber, GetSignatureFor (sender), typeof (T));
            }
        }

        public static void Unsubscribe<T> (object unsubscriber, string message)
        {
            InnerUnsubscribe (unsubscriber, message, typeof (T));
        }

        public static void Unsubscribe (object unsubscriber, string message)
        {
            InnerUnsubscribe (unsubscriber, message, null);
        }

        public static void ClearAllSubscribers ()
        {
            Callbacks.Clear ();
        }

        static string GetSignatureFor (IHasID sender)
        {
            return $"{sender.GetType ().FullName}.{sender.ID}";
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
