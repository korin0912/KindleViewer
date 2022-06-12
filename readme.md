# Kindle Viewer

Kindle 本を閲覧するツール

## 動作環境

- Windows PC
- [.NET Runtime 6.0.x](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

### 利用前の準備

- 最初の一回だけ
  - Kindle アプリを起動し、スクロールしてカバー画像をすべて表示させる
    - PC 上に所持している本やそのカバー画像をキャッシュさせることが目的
- 新しい本を買った場合
  - Kindle アプリを起動し、新しい本を表示させる

<!-- ## 操作方法

- 一覧
  - 本をダブルクリックでリーダータブが表示されて、本が読める
- リーダー
  - タブに Kindle Cloud Reader が表示されるので、操作方法はそれに準拠
    - 初回は Amazon の認証が求められる。(アプリ側でID/パスワードは覚えることはなく
    - ) -->

---

## 開発メモ

### キャッシュ XML フォーマット

`C:\Users\{UserName}\AppData\Local\Amazon\Kindle\Cache\KindleSyncMetadataCache.xml`

```
<response>
    <add_update_list>
        <meta_data>
            <ASIN>ASIN</ASIN>
            <title pronunciation="タイトル読み仮名">タイトル</title>
            <authors>
                <author pronunciation="著者読み仮名">著者</author>
            </authors>
            <publishers>
                <publisher>出版社</publisher>
            </publishers>
            <publication_date>発刊日</publication_date>
            <purchase_date>購入日</purchase_date>
            <textbook_type>TextBook Type</textbook_type>
            <cde_contenttype>CDE Content Type</cde_contenttype>
            <content_type>Content Type</content_type>
            <origins></origins>
        </meta_data>
```

- 発刊日と購入日のフォーマットは `yyyy-MM-ddTHH:mm:ss+zzz`
- 購入日が無いデータは無効なデータ

### キャッシュカバー画像

`C:\Users\{UserName}\AppData\Local\Amazon\Kindle\Cache\covers`

- フォルダの中に、`md5(ASIN).jpg` のファイル名でカバー画像が入ってる

### リーダー

- 本を読むのは、Kindle Cloud Reader で行うので、URL に ASIN を渡して WebView で表示される
  - `https://read.amazon.co.jp/manga/{ASIN}?language={LANG}`
- アプリ側で ID/パスワード管理はしないで、WebView 側で Amazon の認証にまかせる

### 利用パッケージ

| パッケージ名                                                                        | 用途                 |
| ----------------------------------------------------------------------------------- | -------------------- |
| [ReactiveProperty](https://github.com/runceel/ReactiveProperty)                     | Binding 更新         |
| [AvalonDock](https://github.com/Dirkster99/AvalonDock)                              | ドッキングレイアウト |
| [Microsoft.Web.WebView2](https://docs.microsoft.com/ja-jp/microsoft-edge/webview2/) | WEB ページ表示       |
