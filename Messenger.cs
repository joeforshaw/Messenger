using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("JoeForshaw.Messenger.Tests")]

namespace JoeForshaw.Messenger
{
    public static class Messenger
    {
        static readonly MessageDispatcher Callbacks = new MessageDispatcher ();

        // Subscribe
        
        public static void Subscribe<T> (object subscriber, IHasID sender, Action<T> callback)
        {
            PerformSubscribe (subscriber, GetSignatureFor (sender), typeof (T), args => callback ((T) args));
        }
        
        public static void Subscribe (object subscriber, IHasID sender, Action callback)
        {
            PerformSubscribe (subscriber, GetSignatureFor (sender), null, args => callback ());
        }
        
        public static void Subscribe<T> (object subscriber, IEnumerable<IHasID> senders, Action<T> callback)
        {
            foreach (var sender in senders)
            {
                PerformSubscribe (subscriber, GetSignatureFor (sender), typeof (T), args => callback ((T) args));
            }
        }
        
        public static void Subscribe (object subscriber, IEnumerable<IHasID> senders, Action callback)
        {
            foreach (var sender in senders)
            {
                PerformSubscribe (subscriber, GetSignatureFor (sender), null, args => callback ());
            }
        }

        public static void Subscribe<T> (object subscriber, string identifier, Action<T> callback)
        {
            PerformSubscribe (subscriber, identifier, typeof (T), (args) => callback ((T) args));
        }

        public static void Subscribe (object subscriber, string identifier, Action callback)
        {
            PerformSubscribe (subscriber, identifier, null, (args) => callback ());
        }
        
        // Send

        public static void Send<T> (string identifier, T args)
        {            
            PerformSend (identifier, typeof (T), args);
        }

        public static void Send (string identifier)
        {            
            PerformSend (identifier, null, null);
        }
        
        public static void Send<T> (IHasID sender, T args)
        {            
            PerformSend (GetSignatureFor (sender), typeof (T), args);
        }
        
        public static void Send (IHasID sender)
        {            
            PerformSend (GetSignatureFor (sender), null, null);
        }
        
        // Unsubscribe
        
        public static void Unsubscribe<T> (object unsubscriber, string identifier)
        {
            PerformUnsubscribe (unsubscriber, identifier, typeof (T));
        }

        public static void Unsubscribe (object unsubscriber, string identifier)
        {
            PerformUnsubscribe (unsubscriber, identifier, null);
        }
                
        public static void Unsubscribe<T> (object unsubscriber, IHasID sender)
        {
            PerformUnsubscribe (unsubscriber, GetSignatureFor (sender), typeof (T));
        }
        
        public static void Unsubscribe (object unsubscriber, IHasID sender)
        {
            PerformUnsubscribe (unsubscriber, GetSignatureFor (sender), null);
        }
        
        public static void Unsubscribe<T> (object unsubscriber, IEnumerable<IHasID> senders)
        {
            foreach (var sender in senders)
            {
                PerformUnsubscribe (unsubscriber, GetSignatureFor (sender), typeof (T));
            }
        }
        
        public static void Unsubscribe (object unsubscriber, IEnumerable<IHasID> senders)
        {
            foreach (var sender in senders)
            {
                PerformUnsubscribe (unsubscriber, GetSignatureFor (sender), null);
            }
        }

        // Internal methods

        internal static void ClearAllSubscribers ()
        {
            Callbacks.Clear ();
        }
        
        // Private methods

        static string GetSignatureFor (IHasID sender)
        {
            return $"{sender.GetType ().FullName}.{sender.ID}";
        }

        static void PerformSend (string identifier, Type argType, object args)
        {
            if (identifier == null) { throw new ArgumentNullException (nameof (identifier)); }
            
            var key = new MessageSignature (identifier, argType);
            
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
        
        static void PerformSubscribe (object subscriber, string identifier, Type argType, Action<object> callback)
        {
            if (subscriber == null) { throw new ArgumentNullException (nameof (subscriber)); }
            if (identifier == null) { throw new ArgumentNullException (nameof (identifier)); }
            if (callback   == null) { throw new ArgumentNullException (nameof (callback)); }
            
            var key = new MessageSignature (identifier, argType);
            
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
        
        static void PerformUnsubscribe (object subscriber, string identifier, Type argType)
        {
            if (subscriber == null) { throw new ArgumentNullException (nameof (subscriber)); }
            if (identifier == null) { throw new ArgumentNullException (nameof (identifier)); }

            var key = new MessageSignature (identifier, argType);
            
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
