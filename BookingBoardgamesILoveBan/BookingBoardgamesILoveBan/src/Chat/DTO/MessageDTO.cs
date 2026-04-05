using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookingBoardgamesILoveBan.src.Chat.DTO;

public record MessageDTO(
    int Id,
    int ConversationId,
    int SenderId,
    int ReceiverId,
    DateTime SentAt,
    string Content,
    MessageType Type,
    string ImageUrl,
    bool IsResolved,
    bool IsAccepted,
    bool IsAcceptedByBuyer,
    bool IsAcceptedBySeller,
    int RequestId,
    int PaymentId
)
{
    // this one will be used to create a dto when a new message is created in the textbox

    // these are junk used for testing purposes only but i am too afraid to delete them
    public MessageDTO(int senderId, string content, MessageType type, DateTime sentAt) : this(
    0, 0, senderId, 0, sentAt, content, type, "", false, false, false, false, -1, -1
    )
    { }
    public MessageDTO(int conversationId, string content, MessageType type) : this(
    0, conversationId, 0, 0, DateTime.Now, content, type, "", false, false, false, false, -1, -1
)
    { }

    public string GetPreview() => Type switch
    {
        MessageType.Text or MessageType.System => Content.Length > 50 ? Content[..50] : Content,
        MessageType.Image => "[Image]",
        MessageType.RentalRequest => "[Rental Request]",
        MessageType.CashAgreement => "[Cash Agreement]",
        _ => "[Attachment]"
    };
}