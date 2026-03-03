# CSV入出力仕様

## 対象ファイル
- products.csv
- inventory.csv
- sales_yyyyMMdd.csv

## 共通ルール
- UTF-8
- ヘッダー必須
- 空行は無視
- 形式不正は `DomainValidationException` を送出

## 検証項目
- 列数
- 必須項目
- 整数項目（単価・在庫・数量）
- 日付形式（`yyyy-MM-dd`）

## 実装
- `SalesManagementApp.Core.Infrastructure.Csv.CsvDataStore` に集約
