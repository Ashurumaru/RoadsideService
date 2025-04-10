using System;

namespace RoadsideService.Models
{
    public class RoomOccupancyModel
    {
        public string RoomNumber { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}