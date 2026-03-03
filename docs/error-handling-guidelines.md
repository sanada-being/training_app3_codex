# 共通バリデーション/エラーハンドリング規約

## 例外分類
- `DomainValidationException`
  - 入力値や業務ルール違反
  - 画面表示: `入力エラー`
  - ログレベル: `WARN`
- `ApplicationOperationException`
  - 外部連携・処理手順などの業務処理失敗
  - 画面表示: `業務エラー`
  - ログレベル: `ERROR`
- その他の `Exception`
  - 想定外の障害
  - 画面表示: `システムエラー`
  - ログレベル: `ERROR`

## バリデーション共通部品
- `ValidationGuard` を利用して以下を統一する。
  - 必須チェック（`RequireNotEmpty`）
  - 正数チェック（`RequirePositive`）
  - 非負数チェック（`RequireNonNegative`）
  - 期間妥当性（`RequireDateRange`）

## UIメッセージ規約
- タイトルは例外分類に合わせて固定する。
- ユーザー向け文言は「入力ミスか」「業務上処理不可か」「想定外障害か」を判別できる内容にする。
- 想定外障害では詳細技術情報を画面に出さず、共通メッセージを表示する。

