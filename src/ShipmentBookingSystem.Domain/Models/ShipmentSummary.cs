using System;
using System.Collections.Generic;
using System.Text;

namespace ShipmentBookingSystem.Domain.Models
{
    public sealed class ShipmentSummary
    {
        public int CustormerID { get; set; }
        public int ShipmentsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ProductSummary> Products { get; set; }
    }

    public sealed class ProductSummary
    {
        public string ProductCode { get; set; }
        public int TotalQuantity { get; set; }
    }

}
