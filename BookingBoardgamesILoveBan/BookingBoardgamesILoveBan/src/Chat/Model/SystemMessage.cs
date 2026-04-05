using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SystemMessage : Message
{
    public string Content { get; set; }
    public SystemMessage(int id, int conversationId, DateTime sentAt, string content) :
        base(id, conversationId, 0, 0, sentAt, content, MessageType.System)
    {
        Content = content;
    }
}
