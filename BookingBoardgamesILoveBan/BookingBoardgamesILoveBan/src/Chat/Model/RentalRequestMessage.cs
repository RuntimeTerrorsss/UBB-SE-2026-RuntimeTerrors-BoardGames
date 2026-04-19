using System;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;

public class RentalRequestMessage : Message
{
    public int RentalRequestId { get; set; }
    public bool IsRequestResolved { get; set; }
    public bool IsRequestAccepted { get; set; }
    public string RequestContent { get; set; }

    public RentalRequestMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string content,
        int requestId, bool isResolved, bool isAccepted = false) : base(id, conversationId, senderId, receiverId, sentAt, content, MessageType.MessageRentalRequest)
    {
        RequestContent = content;
        RentalRequestId = requestId;
        this.IsRequestResolved = isResolved;
        this.IsRequestAccepted = isAccepted;
    }
}
