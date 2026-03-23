# ModsTabSorter

この Beat Saber プラグインは、譜面選択メニュー画面左側にある `MODS` タブ内の表示順を、任意の並びに変更し保存できるプラグインです。

This Beat Saber plugin lets you change and save the display order of the entries inside the `MODS` tab on the left side of the song selection play menu.

## インストール (Installation)

1. [リリースページ](https://github.com/rynan4818/ModsTabSorter/releases)から `ModsTabSorter` のリリースをダウンロードします。

   Download the latest `ModsTabSorter` release from the [release page](https://github.com/rynan4818/ModsTabSorter/releases).

2. ダウンロードした zip ファイルを `Beat Saber` フォルダに解凍して、`Plugins` フォルダに `ModsTabSorter.dll` を配置してください。

   Extract the downloaded zip file into your `Beat Saber` folder and place `ModsTabSorter.dll` in the `Plugins` folder.

3. 前提として、以下の mod が導入されている必要があります。
   * `BSIPA`
   * `SiraUtil`
   * `BeatSaberMarkupLanguage`

   The following mods must already be installed:
   * `BSIPA`
   * `SiraUtil`
   * `BeatSaberMarkupLanguage`

## 使い方 (How to Use)

### タブ順の並び替え (Reordering MODS tabs)

1. Beat Saber を起動し、メインメニュー左側の `Mods Tab Sorter` ボタンを押します。

   Launch Beat Saber and press the `Mods Tab Sorter` button on the left side of the main menu.

2. 並び替え画面に、現在登録されている `MODS` タブ一覧が表示されます。

   The sorter screen will show the list of currently registered `MODS` tabs.

3. 並び替えたいタブを選択します。

   Select the tab you want to reorder.

4. `Move Up` / `Move Down` を押して順序を変更します。

   Use `Move Up` / `Move Down` to change the order.

5. 変更内容は自動で保存され、`MODS` タブ側へ反映されます。

   Your changes are saved automatically and applied to the `MODS` tab order.

### Reload ボタン (Reload button)

* `Reload` を押すと、現在登録されている `MODS` タブ一覧を読み直します。

  Press `Reload` to reload the currently registered `MODS` tab list.

* 新しく追加された `MODS` タブがある場合は、保存済み順序を保ったまま一覧へ反映されます。

  If new `MODS` tabs were added, they will be reflected in the list while keeping your saved order.

## 注意点 (Notes)

* 並び替え対象は、譜面選択時のプレイメニュー左側にある `MODS` タブ内の項目です。

  The reorder target is the set of entries inside the `MODS` tab on the left side of the song selection play menu.

* Beat Saber 本体の標準タブや、BSML を使わずに作られた独自 UI には作用しません。

  It does not affect Beat Saber’s built-in tabs or custom UIs that do not use BSML `GameplaySetup`.

* Beat Saber や BSML のバージョン更新により、`GameplaySetup` の内部実装やライフサイクルが変わることがあります。その場合はプラグイン側の追従修正が必要になります。

  Beat Saber or BSML updates may change the internal implementation or lifecycle of `GameplaySetup`, and the plugin may need to be updated accordingly.
