# 開発者向けセットアップ手順

## 前提
- Windows
- .NET SDK 8.x
- Git

## セットアップ
1. リポジトリをクローンする。
2. 依存解決とビルドを実行する。
   - `dotnet build "Sales Management App/Sales Management App.slnx"`
3. テストを実行する。
   - `dotnet test tests/SalesManagementApp.Tests/SalesManagementApp.Tests.csproj`

## 開発フロー（本リポジトリ運用）
1. `origin/develop` から `feature/*` ブランチを作成。
2. 実装・テスト追加。
3. ローカルで build/test 成功を確認。
4. PR作成（日本語）し、`build-and-test` の通過後にマージ。

## テスト構成
- 単体テスト: `tests/SalesManagementApp.Tests`
- E2Eテスト: `tests/SalesManagementApp.Tests/E2E`
- テストデータ: `tests/SalesManagementApp.Tests/TestData`

## 主要ドキュメント
- 要件/設計: `docs/requirements.md`, `docs/basic-design.md`, `docs/data-design.md`
- アーキテクチャ: `docs/architecture.md`
- TDD方針: `docs/tdd-guidelines.md`
- エラー/検証規約: `docs/error-handling-guidelines.md`
- データ保護: `docs/data-protection.md`

