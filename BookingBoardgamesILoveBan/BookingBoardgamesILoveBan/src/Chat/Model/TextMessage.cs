using System;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;

public class TextMessage : Message
{
    public string Content { get; set; }
    public TextMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string content) : base(id, conversationId, senderId, receiverId, sentAt, content, MessageType.Text)
    {
        Content = content;
    }
}