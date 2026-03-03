# データ設計書（CSVスキーマ・ドメインモデル）

## 1. 保存方針
- 永続化はCSV優先とする。
- 対象ファイル:
  - `products.csv`
  - `inventory.csv`
  - `sales_yyyyMMdd.csv`
- 文字コードはUTF-8を前提とする。
- 区切り文字は`,`、ヘッダ行あり。

## 2. CSVスキーマ
### 2.1 products.csv
| 列名 | 型 | 必須 | 制約 |
|---|---|---|---|
| ProductId | string | 必須 | 一意、空文字不可 |
| ProductName | string | 必須 | 空文字不可 |
| UnitPrice | int | 必須 | 0以上 |
| Category | string | 必須 | 空文字不可 |

### 2.2 inventory.csv
| 列名 | 型 | 必須 | 制約 |
|---|---|---|---|
| StoreId | string | 必須 | 空文字不可 |
| ProductId | string | 必須 | products.csvに存在 |
| Stock | int | 必須 | 0以上 |

複合キー: `StoreId + ProductId`

### 2.3 sales_yyyyMMdd.csv
| 列名 | 型 | 必須 | 制約 |
|---|---|---|---|
| SaleDate | date | 必須 | `yyyy-MM-dd` |
| StoreId | string | 必須 | 空文字不可 |
| ProductId | string | 必須 | products.csvに存在 |
| Quantity | int | 必須 | 1以上 |

計算列（アプリ内部）:
- SalesAmount = UnitPrice × Quantity

## 3. ドメインモデル
## 3.1 Product
- ProductId: string
- ProductName: string
- UnitPrice: int
- Category: string

### 3.2 InventoryRecord
- StoreId: string
- ProductId: string
- Stock: int

### 3.3 SaleRecord
- SaleDate: DateTime
- StoreId: string
- ProductId: string
- Quantity: int
- SalesAmount: int

## 4. 整合性ルール
- 商品IDは一意。
- 在庫と売上のProductIdは必ず商品マスタに存在する。
- 在庫は負数を許可しない。
- 売上登録時は在庫不足を許可しない。
- 売上金額は単価と数量から再計算可能な値として扱う。

## 5. 異常データ方針
- 必須欠落/型不正/制約違反の行はエラーとして扱う。
- 読込時のエラーは行番号と項目名を含めて通知する。
- 一括読込時は「全件中断」を基本とする（部分成功にしない）。

## 6. バックアップ方針
- 上書き保存前にバックアップファイルを作成する。
- バックアップは `out/backup` 配下に時刻付きファイル名で保存する。
