For English users, see readme.md

--------

# WinZoPNG

WinZoPNG は Windows 用 ZopfliPNG フロントエンドです。

ZopfliPNG は並列処理を行えませんが、このソフトを使うことでファイル単位の並列処理を行え、
いくらか短時間で処理できるほか、進捗や各ファイルの縮み具合を一覧で確認できます。

WOptiPNG に着想を得ています。
https://github.com/tp7/WOptiPNG/


## 制限事項

 * 対象のファイルを確認やバックアップなく上書きします。必要であれば事前にバックアップしてください。
 * PNG 専用です。PNG 以外のファイルはサポートしていません。必要に応じて事前に PNG に変換してください。
 * APNG/MNG も、zopflipng がサポートしていないため、サポートしていません。
 * ETA(推定終了時刻)は不正確です。開始時刻とファイルサイズの進捗から雑に計算しています。
   スリープ/休止状態の中断、動的なスレッド数の変化は考慮外です。


## 環境

| -    | 最小 | 推奨 |
| :--: | ----- | ----- |
| OS   | Windows 10 以降 x64 (*1) | 左に同じ |
| CPU  | 2コア | 4コア/スレッド以上 |
| Mem  | 4GB (*2) | 8GB以上 |
| Screen | SXGA (1280x1024) | ←以上 |

その他:
要 .Net Framework 8.0 ランタイム

入手元
: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

 * (*1) Microsoft によるサポート期限内のもの
 * (*2) メモリサイズは処理する画像ファイルの大きさに依存


## インストール方法

zip の中身を WinZoPNG 専用のディレクトリに展開します。
必要に応じてショートカットを作成します。


## アンインストール方法

設定を含めて削除する場合、一度アプリケーションを起動し、
「About」ボタンをクリックし、設定ファイルの保存先を確認してください。

アプリケーションを閉じて、インストール時に作成したディレクトリを削除します。
必要に応じて設定ファイルディレクトリを削除します。
概ね次の場所にあります：`C:\Users\%USERNAME%\AppData\Local\WinZoPNG`


## 使用方法

ファイルをドラッグアンドドロップ、または「Add Files」「Add Dir」ボタンから参照し追加します。
後述の設定（基本的にはデフォルト設定のままで問題ありません）を確認したら「Execute」ボタンを押すと縮小処理を実行します。

実行中にファイルの追加・削除はできません。

実行中はステータスバーに進捗を表示します。
例) 
19:09:41; Elapse 0:00:46; 36.45% (22/60) done; ETA 19:11:01; 8 files are Running; Remain 30 file(s); Total Reduced approx.2.17MiB (2,270,544Bytes 6.95%)

左から順に以下の項目を出力しています。

 * 現在日時(HH:MM:SS)
 * 経過時間(HH:MM:SS)
 * ファイルサイズベースの進捗率
 * 完了したファイル数/総ファイル数
 * ETA: 予想完了時刻 (翌日以降になる場合は MM/DD も表示)
 * 実行中ファイル数
 * 残ファイル数
 * 削減したデータサイズ,全体に対する削減率

なお、おまけでステータスバーをダブルクリックすると表示内容をクリップボードにコピーできます。

予想完了日時の算出においては、処理が完了したファイルの元サイズを基準に進捗率を判断し、経過時間から逆算することで総所要時間・予想完了時刻を算出しています。
同時処理数の変化、システムのスリープなどは考慮していません。
あくまで簡易的なものとお考え下さい。


## 設定について


### Threads / スレッド数

最大同時並行処理数です。

メニーコア CPU では、安定性や操作性のため、CPU が同時処理できるスレッド数よりも少し減らした数字が望ましいようです。
例えば8コア/16スレッドの CPU では、12～14 程度に設定するのが望ましいです。

非効率なため、CPU のスレッド数以上には設定できません。

なお、実行中に動的に変更可能ですが、スレッド数が減るのはファイルの処理を終えたタイミングで反映されます。


### Priority / プロセス優先度

zopflipng.exe のプロセス優先度です。
Idle を推奨します。


### Optimization levels / 最適化レベル

Default
: 推奨値: 圧縮率と処理時間のバランスに優れた設定です。

Better
: `--filters=0me` オプションを使います。Default よりは時間を要しますがより圧縮できる可能性があります。

Strong
: `--filters=01234me` オプションを使います。Better よりさらに時間がかかりますが、さらに圧縮できる可能性があります。

Insane
: `--filters=01234mepb` オプションを使います。非推奨です。非常に長い処理時間を要します。
トップページやバナー画像など何度もリクエストされるファイルのサイズをわずかでも圧縮したい、
などの特殊用途向けです。


WinZoPNG は常に `-m` オプションを併用します。


### Keep Timestamp / タイムスタンプ維持

元のファイルの作成日時、変更日時を保持します。

**注意：アクセス日時は維持されません。**


## License

MIT license.
詳細は license.txt をご覧ください。

ZopfliPNG のソースとバイナリはこちらから。

Original ZopfliPNG made by Google, Apache-2.0 license
: https://github.com/google/zopfli

ZopfliPNG-bin made by imagemin group, MIT License
: https://github.com/imagemin/zopflipng-bin

WinZoPNG made by CatStorming, MIT License
: https://github.com/CatStorming/WinZoPNG


## 開発環境

 * OS: Windows 10 Pro x64 Japanese
 * IDE: Microsoft Visual Studio Community 2022 (64 bit)
 * CPU: AMD Ryzen 7 3700X
 

## その他

マルチスレッド、ListView VirtualMode などの勉強に作りました。
試行錯誤があるので、汚く、MVC分離もできていない酷いコードです。
ただ代わりに(?)ビルドは簡単で、.Net Framework 8.0 以外の外部ライブラリは不要です。

ステータスバーの表示はダブルクリックでクリップボードにコピーできます。

ドキュメント・プログラム内での (TM) (R) 表記は省略しています。


## TODO

予定は未定

 * zopflipng.exe のプロセスを kill するハードキャンセル対応
 * zopflipng.exe の出力を表示する機能
 * 多言語対応
 * コマンドラインサポート
 * MVC 分離のリファクタリング


## 更新履歴

v1.0.1 - 細かい修正

v1.0.0 - 初公開


[EOF]
