# Messenger
An easy C# publish-subscribe library.

## Usage
`Messenger` allows objects to subscribe to different message signatures and also send messages to subscribers with matching signatures.

Subscribing:
```
Messenger.Subscribe (this, "foo.bar", () => Console.WriteLine ("Foo Bar"));
```

Sending messages:
```
Messenger.Send ("foo.bar");
```

Unsubscribing:
```
Messenger.Unsubscribe (this, "foo.bar");
```

### Signatures
Message signatures are composed of a string identifer and an argument type. For an object to receive messages of a specific type, both the string identifier and argument type must match.
```
Messenger.Subscribe<ArgumentType> (this, "string identifier", args => Console.WriteLine ("Message received"));
```

It's possible to subscribe to a signature without an argument type if arguments aren't required:
```
Messenger.Subscribe (this, "string identifier", () => Console.WriteLine ("Message received"));
```

But it's important to note messages with the same string indentifier but different argument type (including no type arguments) are different signatures, so won't be received by the same subscribers.

## Example
Here's an example of an object subscribing to "foo.bar" messages:
```
using System;
using JoeForshaw.Messenger;

class SubscribeSample
{
    public static void Main (string [] args)
    {
        var subscriber = new Subscriber ();

        Messenger.Subscribe (subscriber, "foo.bar", subscriber.HandleFooBar);
        
        Messenger.Send ("foo.bar"); // Prints "Foo Bar"
    }
}

class Subscriber
{
    public void HandleFooBar ()
    {
        Console.WriteLine ("Foo Bar");
    }
}
```

## IHasID Interface
Instead of subscribing to messages using string signatures, you can pass in an object which implements the `IHasID` interface. Under the hood, a signature will be generated from the type of the object and it's ID (a required `int` property of the `IHasID` interface).

This can useful when passing around objects that need to kept in sync, such as models:
```
using System;
using JoeForshaw.Messenger;

class ModelSyncSample
{
    public static void Main (string [] args)
    {
        var model = new Model (1, "Foo");
        var copy  = new Model (1, "Foo");
        
        model.Name = "Bar";
        
        Console.WriteLine (model.Name); // "Bar"
        Console.WriteLine (copy.Name);  // "Bar"
    }
}

class Model : IHasID
{
    public int ID { get; set; }

    string _name;
    public string Name
    {
        get { return _name; }
        set
        {
            _name = value;
            
            SendUpdateMessage ();
        }
    }

    public Model (int id, string name)
    {
        ID = id;
        Name = name;
        Messenger.Subscribe<ModelUpdatedArgs> (this, this, HandleUpdateMessage);
    }
    
    public void SendUpdateMessage ()
    {
        Messenger.Send (this, new ModelUpdatedArgs { UpdatedModel = this });
    }
    
    public void HandleUpdateMessage (ModelUpdatedArgs args)
    {
        _name = args.UpdatedModel.Name;
    }
}

class ModelUpdatedArgs
{
    public Model UpdatedModel { get; set; }
}
```

## Roadmap
* Asynchronous message sends
* Subscribe return values
