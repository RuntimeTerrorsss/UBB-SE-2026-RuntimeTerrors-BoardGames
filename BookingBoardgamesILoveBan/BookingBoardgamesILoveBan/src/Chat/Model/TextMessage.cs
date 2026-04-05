using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Model;
using System;

public class TextMessage : Message
{
    public string Content { get; set; }
    public TextMessage(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string content):
        base(id, conversationId, senderId, receiverId, sentAt, content, MessageType.Text)
    {
        Content = content;
    }
}