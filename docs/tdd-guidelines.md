# TDD運用ガイド（NUnit）

## 1. 基本サイクル
1. Red: 失敗するテストを書く
2. Green: 最小実装でテストを通す
3. Refactor: 振る舞いを変えずに整理する

## 2. テスト命名規約
- 形式: `MethodName_Scenario_ExpectedResult`
- 例: `RegisterProduct_WhenProductIdIsDuplicate_ThrowsValidationException`

## 3. テスト配置規約
- `tests/SalesManagementApp.Tests/<機能名>/` に配置する
- 共通データ生成は `tests/SalesManagementApp.Tests/TestHelpers/` に配置する
- 1機能につき正常系・異常系・境界値を最低1ケースずつ作成する

## 4. 失敗テスト先行ルール
- 実装コードを書き始める前に、最低1件の失敗テストを追加する
- PR本文の確認結果で「追加した失敗テスト→修正後の成功」を記録する

## 5. バグ修正ルール
- 再発防止テストを先に追加して失敗を確認する
- 修正後にテストを成功させる

## 6. ローカル実行
```powershell
dotnet restore tests/SalesManagementApp.Tests/SalesManagementApp.Tests.csproj
dotnet test tests/SalesManagementApp.Tests/SalesManagementApp.Tests.csproj --configuration Release
```

## 7. CI実行
- Pull Request 時に GitHub Actions で build/test が自動実行される
- 失敗時はマージしない
