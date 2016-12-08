# Messenger
An easy C# publish-subscribe library.

## Usage
`Messenger.cs` allows objects to subscribe to different message signatures and also send messages to subscribers with matching signatures.

### Signatures
Message signatures are composed of a string identifer and the message argument type. For an object to receive messages of a specific type, both the string identifier and argument type must be exactly the same.
```
// TODO
```

It's possible to subscribe to a signature without an argument type if arguments aren't required:
```
Messenger.Subscribe (this, "joeforshaw.example", () => Console.WriteLine ("Received a 'joeforshaw.example' message"))
```

### IHasID Interface
Objects which implement the `IHasID.cs` interface must contain an `ID` int property.

The below code will subscribe to message
```
class Model : IHasID
{
  public int ID { get; set; }
}
...
var foo = new Model { ID = 1 }
var bar = new Model { ID = 2 }

```

### Roadmap
* Asynchronous message sends
