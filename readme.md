# Kindle Viewer

- Kindle 本を閲覧するツール
- Windows PC の Kindle アプリのキャッシュ情報を利用するので、通信とかは無し

## キャッシュ XML フォーマット

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

## キャッシュカバー画像

`C:\Users\{UserName}\AppData\Local\Amazon\Kindle\Cache\covers`

- フォルダの中に、`md5(ASIN).jpg` のファイル名でカバー画像が入ってる

## 利用パッケージ

| パッケージ名     | 用途                 | URL                                         |
| ---------------- | -------------------- | ------------------------------------------- |
| ReactiveProperty | Binding 更新         | https://github.com/runceel/ReactiveProperty |
| AvalonDock       | ドッキングレイアウト | https://github.com/Dirkster99/AvalonDock    |
