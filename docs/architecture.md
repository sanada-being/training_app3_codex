# アーキテクチャ設計（責務分離）

## 1. レイヤ構成
- UI: WinForms（入力・表示・イベント制御）
- Core.Application: ユースケース制御、バリデーション例外
- Core.Domain: エンティティ、ドメインルール、リポジトリIF
- Core.Infrastructure: CSV実装など外部I/O実装（後続タスクで追加）

## 2. 依存方向
```text
UI -> Core.Application -> Core.Domain
UI -> Core.Infrastructure (実装注入)
Core.Domain は他レイヤに依存しない
```

## 3. プロジェクト構成
- `Sales Management App/Sales Management App` : WinFormsアプリ
- `src/SalesManagementApp.Core` : 共通ロジック
- `tests/SalesManagementApp.Tests` : NUnitテスト

## 4. 実装規約（要点）
- ドメインルールはUI側に書かない。
- データアクセスはRepositoryインターフェース経由で扱う。
- 例外は `DomainValidationException` を起点に扱う。
- 新規機能はテスト先行（Red-Green-Refactor）で実装する。
