# コーディング規約 逸脱管理台帳（Issue #82）

## 1. 目的
- `CODING_CONVENTIONS.md` 適用後に残る規約逸脱を、理由と対応方針つきで継続管理する。

## 2. 回帰確認結果（2026-03-04）
- `dotnet build "Sales Management App/Sales Management App.slnx"`: 成功（0 warnings / 0 errors）
- `dotnet test tests/SalesManagementApp.Tests/SalesManagementApp.Tests.csproj`: 成功（69 passed）
- `dotnet test tests/SalesManagementApp.Tests/SalesManagementApp.Tests.csproj --filter "FullyQualifiedName~WeeklyFlowE2ETests"`: 成功（1 passed）

## 3. 適用済み規約（今回まで）
- 命名規約の主要部分を適用（Issue #79）
- 可読性・書式・using順序を適用（Issue #80）
- WinForms本体の公開メンバーにXMLドキュメントコメントを適用（Issue #81）

## 4. 逸脱一覧

| ID | 規約項目 | 対象 | 現状 | 理由 | 対応方針 |
|---|---|---|---|---|---|
| DEV-001 | クラス接尾辞規約（例: `Control`） | 画面制御クラス | `...Controller` を使用 | 本プロジェクトは MVC/MVP 寄りの責務分離方針で統一済み | 方針維持。命名例外として継続管理（`docs/coding-conventions-naming-exceptions.md`） |
| DEV-002 | 変数接頭辞規約（`F` / `v` / `w` / `C_`） | 全体 | 現代的C#スタイル（接頭辞なし）を使用 | 可読性と .NET 標準慣習を優先 | 方針維持。命名例外として継続管理（`docs/coding-conventions-naming-exceptions.md`） |
| DEV-003 | `public/protected` へのXMLコメント付与 | `src/SalesManagementApp.Core` | 未付与が 135 宣言残存 | 既存公開APIへのコメント付与は差分が大きく、今回Issueのスコープ外 | 専用Issueで段階的に対応（サービス層→状態管理→ドメイン/インフラの順） |

## 5. 補足（計測方法）
- 未付与件数（DEV-003）は次のコマンドで抽出した。
  - `rg -n "^\s*(public|protected)\b" src -g "*.cs"`
  - 上記宣言に対して、直前の非空行が `///` で始まるかを確認するスクリプトで集計
