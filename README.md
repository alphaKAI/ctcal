# ctcal
Class Table Calendar

## これは何
Twinsの時間割をコンソールで見るためのツール．

## 使い方

Releaseのページでバイナリを公開しているのでそれをダウンロードし，実行権限を与えて起動すればよい．  
使い方については，`-h`や`--help`オプションを与えることで確認できる．  
また，Chromeをheadlessモードで起動し，それをSeleniumを用いて制御することによって自動化を行っているので，Chromeをインストールしておく必要がある．

## ビルド方法(自分でビルドしたい場合)

### ビルドにあたり用意するもの

* .NET Core SDK 2.2系(僕の環境は2.2.101)
* Chrome (headless modeとして使っているので)

```sh
$ git clone https://github.com/alphaKAI/ctcal
$ cd ctcal
$ zsh build-multi.sh
```

これで，Windows，Linux，macOS向けのバイナリのそれぞれがクロスコンパイルされ

- Windows用: `ctcal-single.exe`
- Linux用: `ctcal-single.linux`
- macOS用: `ctcal-single.macos`

という名前で吐き出されるので，それを実行すれば良い(他のバージョンがいらない場合は適宜`build-multi.sh`を編集して他のプラットフォームについての記述を消せば良い)

## ライセンス
ManatoolはMITライセンスのもとで配布される．

Copyright (C) 2016 Akihiro Shoji  