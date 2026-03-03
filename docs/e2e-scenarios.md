# E2Eテストシナリオ（週次フロー）

## シナリオID
- `E2E-WEEKLY-01`

## 前提データ
- `tests/SalesManagementApp.Tests/TestData/E2E/products.csv`
- `tests/SalesManagementApp.Tests/TestData/E2E/inventory.csv`
- `tests/SalesManagementApp.Tests/TestData/E2E/sales.csv`

## 検証フロー
1. CSV取込（商品・在庫・売上）
2. 商品管理操作
   - 新規商品登録
   - 既存商品更新
3. 在庫管理操作
   - 新規商品の入荷
4. 売上登録
   - 複数商品の売上登録と在庫減算
5. 集計
   - 期間合計
   - 商品別売上
   - 週次合計
6. 要発注確認
   - 閾値在庫以下の対象確認

## 期待結果
- 売上合計、商品別、週次が手計算と一致
- 在庫連動が正しく反映
- 主要業務フローを通しで再現可能

