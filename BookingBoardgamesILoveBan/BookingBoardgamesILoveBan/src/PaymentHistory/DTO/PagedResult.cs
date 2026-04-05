using System.Collections.Generic;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.DTO
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => TotalCount == 0 ? 0 : (TotalCount - 1) / PageSize + 1;
    }
}
