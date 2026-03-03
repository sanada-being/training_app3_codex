# 非同期処理方針

## 対象
- CSV I/O（読み込み・書き込み）

## 実装内容
- `CsvDataStore`に以下の非同期メソッドを追加
  - `ReadProductsAsync` / `ReadInventoriesAsync` / `ReadSalesAsync`
  - `WriteProductsAsync` / `WriteInventoriesAsync` / `WriteSalesAsync`
- いずれも `Task.Run` + `CancellationToken` に対応し、呼び出し側でUIスレッドをブロックしない利用が可能。

## キャンセル評価
- CSV処理はデータ量により待ち時間が増えるため、キャンセル要求を受け付ける価値がある。
- `CancellationToken`を受け取り、実行前に `ThrowIfCancellationRequested` を行う設計とした。

## 検証
- NUnit
  - 非同期読み込み成功
  - キャンセル時に `OperationCanceledException` 系が発生
- ビルド
  - `dotnet build "Sales Management App/Sales Management App.slnx"`

