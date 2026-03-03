# 在庫算出方式（追加課題対応）

## 1. 変更概要
- 旧方式: `InventoryService` 内で在庫数を直接 `+/-` する方式。
- 新方式: `InventoryStockCalculator` に算出責務を分離し、入荷/出庫の在庫計算を専用クラスで実施する方式。

## 2. 差分
| 観点 | 旧方式 | 新方式 |
|---|---|---|
| 算出責務 | `InventoryService` に内包 | `InventoryStockCalculator` に分離 |
| 入荷計算 | `target.Stock += quantity` | `CalculateAfterInbound` |
| 出庫計算 | `target.Stock -= quantity` | `CalculateAfterOutbound` |
| 異常系 | サービス内に散在 | 計算クラスで集中検証 |
| 上限チェック | 明示なし | `checked` でオーバーフロー検知 |

## 3. 新方式のルール
- 入荷: `currentStock + inboundQuantity`
  - 在庫数は0以上
  - 入荷数量は1以上
  - 計算結果が `int` 上限を超える場合はエラー
- 出庫: `currentStock - outboundQuantity`
  - 在庫数は0以上
  - 出庫数量は1以上
  - 在庫不足はエラー

## 4. 既存データの取り扱い
- `inventory.csv` のスキーマ変更はなし（移行不要）。
- 既存の在庫残数はそのまま読み込める。
- 実行時の算出ルールのみ新方式を適用する。

## 5. 実装ポイント
- `src/SalesManagementApp.Core/Application/Services/InventoryStockCalculator.cs`
- `src/SalesManagementApp.Core/Application/Services/InventoryService.cs`
- `tests/SalesManagementApp.Tests/Inventory/InventoryStockCalculatorTests.cs`
