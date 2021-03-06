---
title: Handling a Message
summary: Write a class to handle messages in NServiceBus.
redirects:
- nservicebus/how-do-i-handle-a-message
---

To handle a message, write a class that implements `IHandleMessages<T>` where `T` is the message type:

snippet:CreatingMessageHandler

To handle messages of all types:

 1. Set up the [message convention](/nservicebus/messaging/conventions.md) to designate which classes are messages. This example uses a namespace match.
 1. Create a handler of type `Object`. This handler will be executed for all messages that are delivered to the queue for this endpoint.

Since this class is setup to handle type `Object`, every message arriving in the queue will trigger it.

snippet:GenericMessageHandler

include: non-null-task

If using the Request-Response or Full Duplex pattern, handlers will probably do the work it needs to do, such as updating a database or calling a web service, then creating and sending a response message. See [How to Reply to a Message](/nservicebus/messaging/reply-to-a-message.md).

If handling a message in a publish-and-subscribe scenario, see [How to Publish/Subscribe to a Message](/nservicebus/messaging/publish-subscribe/).


### Mapping to name

NServiceBus will try to map incoming messages to a type using [Assembly Qualified Name](https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname.aspx). This is default behavior for sharing assemblies among endpoints. When a message cannot be mapped based on Assembly Qualified Name, NServiceBus will attempt to map to the [`FullName`](https://msdn.microsoft.com/en-us/library/system.type.fullname.aspx). The following is an example of how NServiceBus gets the type information.

```cs
string fqn = message.GetType().AssemblyQualifiedName;
string fallback = message.GetType().FullName;
```


## What happens when there are no handlers for a message?

Receiving a message for which there are no message handlers is considered an error and the received message will be forwarded to the configured error queue.

Note: This behavior was slightly different in Version 3 where the message would only end up in the error queue if running in debug mode. If not in debug mode a Version 3 endpoint would log a warning but still consider the message successfully processed and therefore moving it to the configured error queue.