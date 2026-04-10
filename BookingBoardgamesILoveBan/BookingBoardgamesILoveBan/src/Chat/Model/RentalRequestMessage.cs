using System;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;

public class RentalRequestMessage : Message
{
    public int RequestId { get; set; }
    public bool IsResolved { get; set; }
    public bool IsAccepted { get; set; }
    public string Content { get; set; }

    public RentalRequestMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string content,
        int requestId, bool isResolved, bool isAccepted = false) : base(id, conversationId, senderId, receiverId, sentAt, content, MessageType.RentalRequest)
    {
        Content = content;
        RequestId = requestId;
        this.IsResolved = isResolved;
        this.IsAccepted = isAccepted;
    }
}
