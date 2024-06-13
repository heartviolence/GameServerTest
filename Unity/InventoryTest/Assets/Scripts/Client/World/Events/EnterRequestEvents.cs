
using System;

public struct EnterRequestSendEvent
{

}

public struct EnterRequestReceivedEvent
{
    public Guid guestId;
}

public struct EnterRequestAcceptedEvent
{
    public string serverAddress;
    public Guid hostId;
}