using System;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;

namespace Sales_Management_App {
    /// <summary>
    /// メインフォームで使用する依存オブジェクトを集約します。
    /// </summary>
    internal sealed class MainFormDependencies {
        internal ProductService ProductService { get; private set; }

        internal InventoryService InventoryService { get; private set; }

        internal InventoryHistoryService InventoryHistoryService { get; private set; }

        internal SalesService SalesService { get; private set; }

        internal SalesAggregationService SalesAggregationService { get; private set; }

        internal AppDataRepository AppDataRepository { get; private set; }

        internal AppBootstrapper AppBootstrapper { get; private set; }

        internal static MainFormDependencies CreateDefault() {
            return new MainFormDependencies(
                new ProductService(),
                new InventoryService(),
                new InventoryHistoryService(),
                new SalesService(),
                new SalesAggregationService(),
                new AppDataRepository(),
                new AppBootstrapper());
        }

        private MainFormDependencies(
            ProductService vProductService,
            InventoryService vInventoryService,
            InventoryHistoryService vInventoryHistoryService,
            SalesService vSalesService,
            SalesAggregationService vSalesAggregationService,
            AppDataRepository vAppDataRepository,
            AppBootstrapper vAppBootstrapper) {
            this.ProductService = vProductService ?? throw new ArgumentNullException("productService");
            this.InventoryService = vInventoryService ?? throw new ArgumentNullException("inventoryService");
            this.InventoryHistoryService = vInventoryHistoryService ?? throw new ArgumentNullException("inventoryHistoryService");
            this.SalesService = vSalesService ?? throw new ArgumentNullException("salesService");
            this.SalesAggregationService = vSalesAggregationService ?? throw new ArgumentNullException("salesAggregationService");
            this.AppDataRepository = vAppDataRepository ?? throw new ArgumentNullException("appDataRepository");
            this.AppBootstrapper = vAppBootstrapper ?? throw new ArgumentNullException("appBootstrapper");
        }
    }
}
