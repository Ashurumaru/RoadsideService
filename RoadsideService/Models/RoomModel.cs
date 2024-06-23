using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsideService.Models
{
    public class RoomModel
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public decimal PricePerNight { get; set; }
        public string RoomType { get; set; }
        public int RoomTypeID { get; set; } 
        public int MaxOccupancy { get; set; }
        public string RoomStatus { get; set; }
        public List<ResidentModel> Residents { get; set; }
    }

}
