# リリース判定チェックリスト

## 1. 品質ゲート
- [x] CI `build-and-test` が最新PRで成功
- [x] `dotnet build` が成功
- [x] `dotnet test` が成功

## 2. テスト証跡
- [x] 単体テスト結果がある
- [x] E2Eシナリオ定義がある（`docs/e2e-scenarios.md`）
- [x] E2E実施結果がある（`docs/e2e-test-report.md`）
- [x] 重大不具合がクローズ済み

## 3. 運用資料
- [x] 利用者向け手順書がある（`docs/user-operation-manual.md`）
- [x] 開発者向け手順書がある（`docs/developer-setup.md`）
- [x] データ保護/復旧手順がある（`docs/data-protection.md`）
- [x] 既知制約が明記されている（`docs/known-limitations.md`）

## 4. 引き継ぎ確認
- [x] 必要ドキュメントへのリンクがREADMEに整理されている
- [x] 主要コマンド（build/test）がREADMEに明記されている

## リリース判定
- 判定: `Ready`
- 判定日: 2026-03-03

