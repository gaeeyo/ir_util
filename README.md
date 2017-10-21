# ir_util
httpでリクエスト受けてUSB赤外線リモコンADVANCEで信号送るやつ

Google Home と IFTTT を組み合わせて、テレビや照明を操作してみたくて試作したやつ。

- USB赤外線リモコンADVANCE (ADIR01P)
  - http://bit-trade-one.co.jp/product/module/adir01p/

リモコンのデータは ADIR01P_Trns_CT_v12 で作成して、remoconData.csv にエクスポートして使う。
リモコンのデータの名前は正規表現で記述して、リクエストに一致したものが実行される。

ir_util.exe はデフォルトでは 8080 番ポートで待ち受ける。(.configで変更可)
http://localhost:8080/?q=テレビ%20つけ%20て
に対応するリモコンのデータ名は「テレビ.*つけ」などにする。

起動中に remoconData.csv を書き換えると、自動的にリロードされる。

ADIR01P_Trns_CT_v12 は何かがバグっていて、意図せずデータが上書きされてしまうことがある。
データを追加削除するためにインポートとエクスポートを繰り返したりせずエクスポート専用として割り切って、あとはテキストエディタで書き換えたほうが良い。
