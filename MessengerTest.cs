using NUnit.Framework;

namespace JoeForshaw.Messenger.Tests
{
    [TestFixture]
    public class MessengerTest
    {
        [TearDown]
        public void TearDown ()
        {
            Messenger.ClearAllSubscribers ();
        }
        
        [Test]
        public void TestSubscribeWithoutArg ()
        {
            var subscriber      = new MockSubscriber ();
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
            var subscriber      = new MockSubscriber ();
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
            var subscriber = new MockSubscriber ();
            var sender     = new MockSenderWithID ();
            Messenger.Subscribe (subscriber, sender, subscriber.HandleMessage);
            var countBefore = subscriber.MessagesReceived;
            
            Messenger.Send (sender);
            
            Assert.AreEqual (0, countBefore);
            Assert.AreEqual (1, subscriber.MessagesReceived);
        }
        
        [Test]
        public void TestSubscribeWithIDWithArg ()
        {
            var subscriber = new MockSubscriber ();
            var sender     = new MockSenderWithID ();
            Messenger.Subscribe<MockArgs> (subscriber, sender, subscriber.HandleMessage);
            var countBefore = subscriber.MessagesReceived;
            
            Messenger.Send (sender, new MockArgs ());
            
            Assert.AreEqual (0, countBefore);
            Assert.AreEqual (1, subscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithoutArg ()
        {
            var unsubscriber = new MockSubscriber ();
            Messenger.Subscribe (this, MockSubscriber.Signature, unsubscriber.HandleMessage);
            var countAfterSubscribe = unsubscriber.MessagesReceived;
            
            Messenger.Send (MockSubscriber.Signature);
            var countAfterSend = unsubscriber.MessagesReceived;
            
            Messenger.Unsubscribe (unsubscriber, MockSubscriber.Signature);
            Messenger.Send (MockSubscriber.Signature);
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, unsubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithArg ()
        {
            var unsubscriber = new MockSubscriber ();
            Messenger.Subscribe<MockArgs> (this, MockSubscriber.Signature, unsubscriber.HandleMessage);
            var countAfterSubscribe = unsubscriber.MessagesReceived;
            
            Messenger.Send (MockSubscriber.Signature, new MockArgs ());
            var countAfterSend = unsubscriber.MessagesReceived;
            
            Messenger.Unsubscribe<MockArgs> (unsubscriber, MockSubscriber.Signature);
            Messenger.Send (MockSubscriber.Signature, new MockArgs ());
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, unsubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithIDWithoutArg ()
        {
            var unsubscriber = new MockSubscriber ();
            var sender       = new MockSenderWithID ();
            Messenger.Subscribe (unsubscriber, sender, unsubscriber.HandleMessage);
            var countAfterSubscribe = unsubscriber.MessagesReceived;
            
            Messenger.Send (sender);
            var countAfterSend = unsubscriber.MessagesReceived;
            
            Messenger.Unsubscribe (unsubscriber, sender);
            Messenger.Send (sender);
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, unsubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestUnsubscribeWithIDWithArg ()
        {
            var unsubscriber = new MockSubscriber ();
            var sender       = new MockSenderWithID ();
            Messenger.Subscribe<MockArgs> (unsubscriber, sender, unsubscriber.HandleMessage);
            var countAfterSubscribe = unsubscriber.MessagesReceived;
            
            Messenger.Send (sender, new MockArgs ());
            var countAfterSend = unsubscriber.MessagesReceived;
            
            Messenger.Unsubscribe<MockArgs> (unsubscriber, sender);
            Messenger.Send (sender, new MockArgs ());
            
            Assert.AreEqual (0, countAfterSubscribe);
            Assert.AreEqual (1, countAfterSend);
            Assert.AreEqual (1, unsubscriber.MessagesReceived);
        }
        
        [Test]
        public void TestSubscribersOfDifferentSignaturesDoNotReceiveSameMessages ()
        {
            var firstSubscriber  = new MockSubscriber ();
            var secondSubscriber = new MockSubscriber ();
            
            Messenger.Subscribe (firstSubscriber,  MockSubscriber.Signature,       firstSubscriber.HandleMessage);
            Messenger.Subscribe (secondSubscriber, MockSubscriber.Signature + "2", secondSubscriber.HandleMessage);
            Messenger.Send (MockSubscriber.Signature);
            Messenger.Send (MockSubscriber.Signature);
            Messenger.Send (MockSubscriber.Signature + "2");
            
            Assert.AreEqual (firstSubscriber.MessagesReceived,  2);
            Assert.AreEqual (secondSubscriber.MessagesReceived, 1);
        }
    }
    
    class MockSubscriber
    {
        public static readonly string Signature = "signature.mock";
    
        public int MessagesReceived { get; private set; }
        
        public void HandleMessage ()
        {
            MessagesReceived++;
        }
        
        public void HandleMessage<T> (T args)
        {
            MessagesReceived++;
        }
    }

    class MockSenderWithID : MockSubscriber, IHasID
    {
        public int ID { get; set; }
    }

    class MockArgs { }
}
