# CODING_CONVENTIONS.md

## 1. 目的
本規約は、Gaia Cloud プロジェクトのソースコードの可読性・保守性・一貫性を担保することを目的とする。

## 2. 命名規約

### 2.1 ファイル命名
- ファイル名とクラス名は一致させる。
- 1ファイルに複数クラスがある場合は、メインクラス名をファイル名にする。
- 単語は不必要に省略しない（必要最小限の語数にする）。
- 無理に英訳せず、伝わりやすさを優先してローマ字（基本はヘボン式）も許容する。

### 2.2 接尾辞
| 対象 | 接尾辞 |
|---|---|
| フォームクラス | `Form`, `Dialog` |
| グリッドクラス | `Grid` |
| ビュークラス | `View` |
| コントローラクラス | `Control` |
| ドキュメントクラス | `Document` |
| 構造体クラス（モデル） | `Record` |
| 定義クラス | `Define` |
| 環境クラス | `Environment` |
| クエリクラス | `Query` |
| エクセル出力クラス | `Excel` |
| テストクラス | `Test` |
| 列挙型 | `Enum` |

### 2.3 接頭辞
| 対象 | 接頭辞 | 例 | 備考 |
|---|---|---|---|
| グローバル変数（クラス変数, static変数） | `G_` | `G_AppEnv` | `public` なグローバル変数は非推奨 |
| フィールド（インスタンス変数, メンバ変数） | `F` | `FTreeView` | `private` / `protected` のみ |
| プロパティ | なし | `public int PKikCD { get; set; }` | `F` を付けない |
| メソッド引数 | `v` | `int vKikCD` | `sender`, `e` は例外的に接頭辞不要 |
| ローカル変数 | `w` | `string wMessage` | `for` のループ変数は `i`, `j` 等を許容 |
| 定数 | `C_` | `C_Col_Mesho` | 各単語先頭は大文字、`_` 適宜可 |
| クラス名 | なし |  |  |
| インターフェース | `I` | `IService` |  |

## 3. 関数設計・命名

### 3.1 単一責務
- 1つの関数の目的は1つだけにする。
- 複数処理を1関数に詰め込まない。

### 3.2 命名
- 関数名は大文字で始める。
  - 良い例: `GetCount`, `UpdateMesho`
  - 悪い例: `getCount`, `updateMesho`
- 原則「動詞 + 名詞」の英語で命名する。
- 直感的で意味が明確な語を使う。
  - 悪い例: `Check`（対象や戻り値が曖昧）
  - 悪い例: `CheckHierarchy`, `InvestigateNode`
- 日本語のほうが明確ならローマ字を許容する。
  - 良い例: `HankakuToZenkaku`
  - 悪い例: `Insatsu`, `Teisei`
- 単語区切りは大文字（PascalCase）を使い、原則 `_` は使わない。
  - 悪い例: `Change_view_mode`
- `bool` を返す判定系関数は `Is`, `Has`, `Can`, `Exists` などを先頭に付ける。
- 拡張版関数に連番や `Ex` は使わず、意味のある名前にする。
  - 悪い例: `GetTokenFromCSV2`, `CopyFileEx`

## 4. 可読性・書式

### 4.1 スペース
- 代入・演算子の前後に半角スペースを入れる。
  - 良い例: `i = 0`
  - 悪い例: `i=0`
- カンマ・コロン・セミコロンの後ろに半角スペースを入れる（前には入れない）。
  - 良い例: `FunctionA(Object vAObj, int vMode)`
  - 悪い例: `FunctionA(Object vAObj,int vMode)`

### 4.2 省略・英単語
- 不必要な省略はしない。
  - 良い例: `wTankaFlag`, `DispToRec`, `ConvertForDB`
  - 悪い例: `wFlag`, `wFlg`, `Disp2Rec`, `Convert4DB`
- 単語途中の不自然な大文字は使わない。
  - 悪い例: `UpDate`, `DownLoad`
  - 良い例: `Update`, `Download`

### 4.3 コメントの履歴記述
- 修正者名・日付のみのコメントは残さない（履歴はGitで管理）。
  - 悪い例: `2014/04/01 Add by XXX`

### 4.4 文字列連結
- ループ内など大量連結・長さ不定の連結は `StringBuilder` を使う。
- 固定文（例: メッセージボックス）などは `+` 連結でも可。

### 4.5 ネスト抑制
- 早期 `return` でネストを浅くする。
- 例外的条件・前提NG条件を先に判定して抜ける。

### 4.6 `this` の扱い
- プロパティ: クラス内参照時に `this.Property` 推奨。
- フィールド: 接頭辞 `F` で識別できるため `this` は任意。
- メソッド: `this` は任意。

## 5. `using` の順序
- 並び順は以下とする。
  1. .NET標準
  2. サードパーティ（DevExpress / JSON / ProtoBuf 等）
- 各グループ内はアルファベット順。

例:
```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Moq;
using NUnit.Framework;
```

## 6. テスト規約
- テストクラスの関数名は日本語で記述する（テスト一覧の可読性向上のため）。
- テストクラス以外で、日本語の関数名・変数名を使用してはならない。

## 7. フォームイベントの使い分け
- `Form.Shown`
  - 画面表示時にデータ読み込みなど、時間のかかる処理に推奨
- `Form.Load`
  - 上記以外のコントロール初期化等に使用

## 8. コメント規約

### 8.1 XMLドキュメントコメント
- クラス外から見える `public` / `protected` メンバーには、XML形式コメントを必須とする。

例:
```csharp
/// <summary>
/// オブジェクトを指定したファイルにJSON形式でシリアライズします。
/// </summary>
/// <param name="vObject">シリアライズ対象のオブジェクト。</param>
/// <param name="vFilePath">出力するファイルパス。既に存在したら上書きします。</param>
/// <returns>書き込んだバイト数。0なら書き込み失敗です。</returns>
public static int Serialize(object vObject, string vFilePath)
```

### 8.2 実装コメントの方針
- ソースを見れば自明な説明コメントは不要。
- 「なぜこの実装か」が伝わらない箇所にのみ記述する。
- 例:
  - 処理を削除した理由（再追加防止のため）
  - 一斉修正で一部だけ未修正にした理由（修正漏れ誤認防止のため）

### 8.3 一時コメント
- 実装途中で push する場合は、未完了箇所を明示する。
  - `Todo`: 未実装で追加が必要な箇所
  - `Hack`: 実装済みだが改善が必要な箇所
  - `Undone`: 使用禁止（`Todo` を使う）

## 9. 禁止・推奨まとめ
- 禁止:
  - 曖昧な命名（例: `Check` 単体）
  - 連番・`Ex` による関数名拡張
  - テスト以外での日本語識別子
  - 修正者名/日付だけのコメント
  - `Undone` コメント
- 推奨:
  - 単一責務関数
  - 早期 return
  - 不必要な省略の回避
  - 意味が伝わる命名
  - XMLドキュメントコメント（`public` / `protected`）
