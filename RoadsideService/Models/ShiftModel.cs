using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsideService.Models
{
    public class ShiftModel
    {
        public int ShiftID { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool? IsHoliday { get; set; }
        public string EmployeeName { get; set; }
    }
}
