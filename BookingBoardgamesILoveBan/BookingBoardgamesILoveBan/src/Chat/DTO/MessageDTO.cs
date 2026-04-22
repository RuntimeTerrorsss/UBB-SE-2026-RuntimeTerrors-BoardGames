using System;
using BookingBoardgamesILoveBan.Src.Enum;

namespace BookingBoardgamesILoveBan.Src.Chat.DTO
{
    public record MessageDataTransferObject(
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
        public string GetChatMessagePreview()
        {
            int maximumPreviewLength = 50;

            return type switch
            {
                MessageType.MessageText or MessageType.MessageSystem => content.Length > maximumPreviewLength ? content[..maximumPreviewLength] : content,
                MessageType.MessageImage => "[Image]",
                MessageType.MessageRentalRequest => "[Rental Request]",
                MessageType.MessageCashAgreement => "[Cash Agreement]",
                _ => "[Attachment]"
            };
        }
    }
}