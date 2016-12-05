using System;
using NUnit.Framework;

namespace JoeForshaw.Messenger
{
    [TestFixture]
    public class MessengerTest
    {
        [TearDown]
        public void TearDown ()
        {
            Messenger.ClearSubscribers ();
        }
        
        [Test]
        public void TestSubscribeWithoutArg ()
        {
            var subscriber = new MockSubscriber ();
            var otherSubscriber = new MockSubscriber ();
            Messenger.Subscribe (this, MockSubscriber.Signature, subscriber.HandleMessage);
            var countBefore = subscriber.MessagesReceived;
            
            Messenger.Send (MockSubscriber.Signature);
            
            Assert.AreEqual (0, countBefore);
            Assert.AreEqual (1, subscriber.MessagesReceived);
            Assert.AreEqual (0, otherSubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestSubscribeWithArg ()
        {
            var subscriber = new MockSubscriber ();
            var otherSubscriber = new MockSubscriber ();
            Messenger.Subscribe<MockArgs> (this, MockSubscriber.Signature, subscriber.HandleMessage);
            var countBefore = subscriber.MessagesReceived;
            
            Messenger.Send (MockSubscriber.Signature, new MockArgs ());
            
            Assert.AreEqual (0, countBefore);
            Assert.AreEqual (1, subscriber.MessagesReceived);
            Assert.AreEqual (0, otherSubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestSubscribeWithIDWithoutArg ()
        {
            var subscriber = new MockSubscriberWithID ();
            var otherSubscriber = new MockSubscriber ();
            Messenger.Subscribe (subscriber, subscriber.HandleMessage);
            var countBefore = subscriber.MessagesReceived;
            
            Messenger.Send (subscriber);
            
            Assert.AreEqual (0, countBefore);
            Assert.AreEqual (1, subscriber.MessagesReceived);
            Assert.AreEqual (0, otherSubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestSubscribeWithIDWithArg ()
        {
            var subscriber = new MockSubscriberWithID ();
            var otherSubscriber = new MockSubscriber ();
            Messenger.Subscribe<MockArgs> (subscriber, subscriber.HandleMessage);
            var countBefore = subscriber.MessagesReceived;
            
            Messenger.Send (subscriber, new MockArgs ());
            
            Assert.AreEqual (0, countBefore);
            Assert.AreEqual (1, subscriber.MessagesReceived);
            Assert.AreEqual (0, otherSubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithoutArg ()
        {
            var subscriber = new MockSubscriber ();
            Messenger.Subscribe (this, MockSubscriber.Signature, subscriber.HandleMessage);
            var countAfterSubscribe = subscriber.MessagesReceived;
            
            Messenger.Send (MockSubscriber.Signature);
            var countAfterSend = subscriber.MessagesReceived;
            
            Messenger.Unsubscribe (subscriber, MockSubscriber.Signature);
            Messenger.Send (MockSubscriber.Signature);
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, subscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithArg ()
        {
            var subscriber = new MockSubscriber ();
            Messenger.Subscribe<MockArgs> (this, MockSubscriber.Signature, subscriber.HandleMessage);
            var countAfterSubscribe = subscriber.MessagesReceived;
            
            Messenger.Send (MockSubscriber.Signature, new MockArgs ());
            var countAfterSend = subscriber.MessagesReceived;
            
            Messenger.Unsubscribe<MockArgs> (subscriber, MockSubscriber.Signature);
            Messenger.Send (MockSubscriber.Signature, new MockArgs ());
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, subscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithIDWithoutArg ()
        {
            var subscriber = new MockSubscriberWithID ();
            Messenger.Subscribe (subscriber, subscriber.HandleMessage);
            var countAfterSubscribe = subscriber.MessagesReceived;
            
            Messenger.Send (subscriber);
            var countAfterSend = subscriber.MessagesReceived;
            
            Messenger.Unsubscribe (subscriber);
            Messenger.Send (subscriber);
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, subscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithIDWithArg ()
        {
            var subscriber = new MockSubscriberWithID ();
            Messenger.Subscribe<MockArgs> (subscriber, subscriber.HandleMessage);
            var countAfterSubscribe = subscriber.MessagesReceived;
            
            Messenger.Send (subscriber, new MockArgs ());
            var countAfterSend = subscriber.MessagesReceived;
            
            Messenger.Unsubscribe<MockArgs> (subscriber);
            Messenger.Send (subscriber, new MockArgs ());
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, subscriber.MessagesReceived);
        }
    }
    
    class MockSubscriber
    {
        public static readonly string Signature = "signature.mock";
    
        public int MessagesReceived { get; set; }
        
        public void HandleMessage ()
        {
            MessagesReceived++;
        }
        
        public void HandleMessage<T> (T args)
        {
            MessagesReceived++;
        }
    }

    class MockSubscriberWithID : MockSubscriber, IDable
    {
        public int ID { get; set; }
    }

    class MockArgs { }
}
