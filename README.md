# training_app3_codex

新人基礎研修向けの販売管理アプリ（WinForms）です。  
商品管理、在庫管理、売上登録、売上集計を扱います。

## 開発/検証コマンド
- `dotnet build "Sales Management App/Sales Management App.slnx"`
- `dotnet test tests/SalesManagementApp.Tests/SalesManagementApp.Tests.csproj`

## 配布手順
1. Visual Studio で `Sales Management App/Sales Management App/Sales Management App.csproj` を `Release` ビルドする
2. リポジトリルートで `powershell -ExecutionPolicy Bypass -File .\scripts\Create-Distribution.ps1 -Clean` を実行する
3. `dist/SalesManagementApp` フォルダをまとめて配布する

配布先PCでは、`Sales Management App.exe`、`SalesManagementApp.Core.dll`、`Sales Management App.exe.config`、各CSVを同じフォルダに置いたまま起動する。

## 主要ドキュメント
- 利用者向け手順: `docs/user-operation-manual.md`
- 開発者向け手順: `docs/developer-setup.md`
- 既知制約/未対応: `docs/known-limitations.md`
- リリース判定チェック: `docs/release-checklist.md`
- E2Eシナリオ/結果: `docs/e2e-scenarios.md`, `docs/e2e-test-report.md`
- 在庫算出方式: `docs/inventory-calculation.md`

