using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Model;
using System;

public class ImageMessage : Message
{
    public string ImageUrl { get; set; }
    public ImageMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string imageUrl) :
        base(id, conversationId, senderId, receiverId, sentAt, "[Image]", MessageType.Image)
    {
        ImageUrl = imageUrl;
    }
}