using System;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Core.Application.State;

namespace Sales_Management_App {
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
            ProductService productService,
            InventoryService inventoryService,
            InventoryHistoryService inventoryHistoryService,
            SalesService salesService,
            SalesAggregationService salesAggregationService,
            AppDataRepository appDataRepository,
            AppBootstrapper appBootstrapper) {
            ProductService = productService ?? throw new ArgumentNullException("productService");
            InventoryService = inventoryService ?? throw new ArgumentNullException("inventoryService");
            InventoryHistoryService = inventoryHistoryService ?? throw new ArgumentNullException("inventoryHistoryService");
            SalesService = salesService ?? throw new ArgumentNullException("salesService");
            SalesAggregationService = salesAggregationService ?? throw new ArgumentNullException("salesAggregationService");
            AppDataRepository = appDataRepository ?? throw new ArgumentNullException("appDataRepository");
            AppBootstrapper = appBootstrapper ?? throw new ArgumentNullException("appBootstrapper");
        }
    }
}
