using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;
namespace BookingBoardgamesILoveBan.Src.Chat.DTO;

public record MessageDTO(
    int id,
    int conversationId,
    int senderId,
    int receiverId,
    DateTime sentAt,
    string content,
    MessageType type,
    string imageUrl,
    bool isResolved,
    bool isAccepted,
    bool isAcceptedByBuyer,
    bool isAcceptedBySeller,
    int requestId,
    int paymentId)
{
    // this one will be used to create a dto when a new message is created in the textbox

    // these are junk used for testing purposes only but i am too afraid to delete them
    public MessageDTO(int senderId, string content, MessageType type, DateTime sentAt) : this(
    0, 0, senderId, 0, sentAt, content, type, string.Empty, false, false, false, false, -1, -1)
    {
    }
    public MessageDTO(int conversationId, string content, MessageType type) : this(
    0, conversationId, 0, 0, DateTime.Now, content, type, string.Empty, false, false, false, false, -1, -1)
    {
    }

    public string GetPreview() => type switch
    {
        MessageType.Text or MessageType.System => content.Length > 50 ? content[..50] : content,
        MessageType.Image => "[Image]",
        MessageType.RentalRequest => "[Rental Request]",
        MessageType.CashAgreement => "[Cash Agreement]",
        _ => "[Attachment]"
    };
}