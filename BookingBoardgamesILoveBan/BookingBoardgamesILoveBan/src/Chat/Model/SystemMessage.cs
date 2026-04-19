using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;

public class SystemMessage : Message
{
    public string MessageContent { get; set; }
    public SystemMessage(int id, int conversationId, DateTime sentAt, string content) : base(id, conversationId, 0, 0, sentAt, content, MessageType.MessageSystem)
    {
        MessageContent = content;
    }
}
