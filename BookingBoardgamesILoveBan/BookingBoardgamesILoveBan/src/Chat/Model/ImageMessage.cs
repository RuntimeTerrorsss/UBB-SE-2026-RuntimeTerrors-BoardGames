using System;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;

public class ImageMessage : Message
{
    public string ImageUrl { get; set; }
    public ImageMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string imageUrl) : base(id, conversationId, senderId, receiverId, sentAt, "[Image]", MessageType.Image)
    {
        ImageUrl = imageUrl;
    }
}