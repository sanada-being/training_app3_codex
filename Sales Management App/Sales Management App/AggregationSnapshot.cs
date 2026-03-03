using System;
using System.Collections.Generic;
using SalesManagementApp.Core.Application.Models;

namespace Sales_Management_App
{
    internal class AggregationSnapshot
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ProductSalesSummary> ProductSummaries { get; set; }
        public List<WeeklySalesSummary> WeeklySummaries { get; set; }
        public int TotalSalesAmount { get; set; }
    }
}
