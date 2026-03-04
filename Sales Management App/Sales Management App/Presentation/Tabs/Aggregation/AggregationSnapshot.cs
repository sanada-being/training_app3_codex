using System;
using System.Collections.Generic;
using SalesManagementApp.Core.Application.Models;

namespace Sales_Management_App.Presentation.Tabs.Aggregation {
    internal sealed class AggregationSnapshot {
        /// <summary>
        /// 集計開始日を取得または設定します。
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 集計終了日を取得または設定します。
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 商品別集計結果を取得または設定します。
        /// </summary>
        public List<ProductSalesSummary> ProductSummaries { get; set; }

        /// <summary>
        /// 週別集計結果を取得または設定します。
        /// </summary>
        public List<WeeklySalesSummary> WeeklySummaries { get; set; }

        /// <summary>
        /// 集計期間内の総売上金額を取得または設定します。
        /// </summary>
        public int TotalSalesAmount { get; set; }
    }
}
