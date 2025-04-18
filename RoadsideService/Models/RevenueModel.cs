﻿using System;

namespace RoadsideService.Models
{
    public class RevenueModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string CustomerName { get; set; }
    }
}