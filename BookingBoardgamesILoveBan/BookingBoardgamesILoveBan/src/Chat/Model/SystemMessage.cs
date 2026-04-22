using System;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;

public class SystemMessage : Message
{
    public string MessageContent { get; set; }

    private const int SystemUserIdentifier = 0;

    public SystemMessage(int id, int conversationId, DateTime sentAt, string content)
        : base(id, conversationId, SystemUserIdentifier, SystemUserIdentifier, sentAt, content, MessageType.MessageSystem)
    {
        MessageContent = content;
    }
}