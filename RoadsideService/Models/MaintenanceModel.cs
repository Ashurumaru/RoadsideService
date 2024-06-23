using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsideService.Models
{
    public class MaintenanceModel
    {
        public int MaintenanceID { get; set; }
        public string RoomNumber { get; set; }
        public string EmployeeFullName { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
    }
}
