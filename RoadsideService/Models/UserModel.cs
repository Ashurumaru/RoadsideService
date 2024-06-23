using System;

namespace RoadsideService.Models
{
    public class UserModel
    {
        public static int EmployeeId { get; set; }
        public static string FirstName { get; set; }
        public static string MiddleName { get; set; }
        public  static string LastName { get; set; }
        public static string Email { get; set; }
        public static string Phone { get; set; }
        public static int RoleId { get; set; }
        public DateTime HireDate { get; set; }
        public static string Login { get; set; }
        public static string Password { get; set; }

        public static string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
