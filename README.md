# MinikubeSystemLauncher

学習用の .NET Framework 4.8 コンソールアプリです。

目的は、`minikube` を操作する側と、起動済み Kubernetes を `kubectl` / `helm` で使う側を分けて、まず動きのイメージを掴むことです。driver は App.config の `MinikubeDriver` で明示します。既定値は `hyperv` です。

`MinikubeDriver` は `hyperv` / `docker` / `virtualbox` などに変更できます。Windows Pro / Enterprise で Hyper-V を使う想定なら `hyperv`、Docker Desktop を使うなら `docker`、VirtualBox を使うなら `virtualbox` を指定します。

この版では、**タスクスケジューラ登録、Windows Service、AdminSetup、UserStart は入れていません**。

また、パス指定はすべて `App.config` に **フルパスで明示**します。実行中にパスを推測したり、切り替えたりする機能は入れていません。


---

## UIについて

この版では、UI は自作の軽量 TUI ヘルパーで構成しています。あわせて、配布しやすくするために Costura.Fody / Fody を NuGet パッケージとして追加しています。

主な見た目の変更は次の通りです。

```text
- 起動時のタイトルをシンプルなバナー表示
- メニューは説明を増やしすぎず、カテゴリごとに縦横ミックスで表示
- メニューを Cluster / Addons / Images / Utility などにグループ化
- 添付コードに近いシンプルな見た目をベースに、横並びも混ぜて表示
- 色は派手にせず、白 / グレー中心の落ち着いた表示
- 実行コマンドは見やすく表示
- Serilog のコンソール出力を有効化し、色付きの INFO / WARN ログも表示
```

処理内容やコマンド仕様は変えず、コンソール表示だけを見やすくしています。UI 表示には追加の UI ライブラリを使っていません。

---

## プロジェクト構成

```text
MinikubeSystemLauncher
  src
    SystemMinikubeHost
      SystemMinikubeHost.csproj
      Program.cs
      App.config

    UserKubeClient
      UserKubeClient.csproj
      Program.cs
      App.config

  samples
    local-dev
      sample-app.yaml

  MinikubeSystemLauncher.sln
  build.ps1
  README.md
```

---

## 1. SystemMinikubeHost

`SystemMinikubeHost.exe` は、minikube 側を操作する学習用コンソールアプリです。

やることは次の通りです。

```text
SystemMinikubeHost
  minikube start を cmd /c 経由で実行する
  minikube stop を cmd /c 経由で実行する
  minikube delete を cmd /c 経由で実行する
  minikube status を cmd /c 経由で実行する
  minikube addons list を cmd /c 経由で実行する
  minikube addons enable dashboard を cmd /c 経由で実行する
  minikube addons enable ingress を cmd /c 経由で実行する
  minikube addons enable metrics-server を cmd /c 経由で実行する
  minikube addons enable registry を cmd /c 経由で実行する
  minikube addons enable default-storageclass / storage-provisioner を cmd /c 経由で実行する
  よく使う addon をまとめて有効化する
  minikube dashboard --url を cmd /c 経由で実行する
  minikube image ls / load / build / rm を cmd /c 経由で実行する
  ローカル開発用のコマンド例を表示する
  kubectl config view --flatten --raw で kubeconfig を出力する
  Hyper-V 有効化コマンドを cmd /c 経由で実行する
  Serilog ログを出力する
```

引数なしで起動すると番号付きメニューが表示されます。

```text
========================================
SystemMinikubeHost メニュー
========================================

  Cluster
  [ 1] 起動: minikube start + kubeconfig 出力
  [22] 詳細起動: minikube start --alsologtostderr -v=2
  [ 2] 状態確認: minikube status / kubectl get nodes
  [ 3] kubeconfig 出力のみ          [ 7] 停止: minikube stop
  [ 8] 削除: minikube delete

  Addons
  [ 9] addon一覧: minikube addons list
  [10] dashboard addon有効化: minikube addons enable dashboard
  [11] dashboard URL表示: minikube dashboard --url
  [12] ingress addon有効化: minikube addons enable ingress
  [13] metrics-server addon有効化: minikube addons enable metrics-server
  [14] registry addon有効化: minikube addons enable registry
  [15] storage系 addon有効化: default-storageclass / storage-provisioner
  [16] おすすめaddonまとめて有効化

  Images
  [17] イメージ一覧: minikube image ls
  [18] イメージ読み込み: minikube image load <imageまたはtar>
  [19] イメージビルド: minikube image build -t <image> <path>
  [20] イメージ削除: minikube image rm <image>

  Utility
  [ 4] Hyper-V 有効化コマンドを実行
  [ 5] 最新ログを表示              [ 6] 現在のパス設定を表示
  [21] ローカル開発コマンド例を表示

  [0] 終了
```

アプリ起動時点では、管理者権限や SYSTEM 権限でないことを理由に終了しません。

ただし、minikube / Hyper-V / addon / image など権限が必要なジョブを標準ユーザーで選んだ場合は、コンソールに警告を出してジョブを実行せず、メニューに戻ります。

```text
標準ユーザーで起動:
  アプリ自体は起動する
  メニューも表示する
  権限が必要なジョブを選ぶと警告する

管理者で起動:
  学習用として minikube + Hyper-V 操作を試しやすい

SYSTEMで起動:
  SYSTEM実行に近い形で確認できる
```

---

## 2. UserKubeClient

`UserKubeClient.exe` は、`kubectl` / `helm` を使うための補助コンソールアプリです。

やることは次の通りです。

```text
UserKubeClient
  App.config で指定された kubeconfig を使って kubectl を実行する
  App.config で指定された kubeconfig を使って helm を実行する
  Deployment / Service / Ingress / Event を一覧表示する
  YAML を apply / delete する
  Pod の logs / describe を実行する
  port-forward を開始する
  KUBECONFIG 設定済みの cmd.exe を開く
  SystemMinikubeHost が出したログを確認する
```

引数なしで起動すると番号付きメニューが表示されます。

```text
========================================
UserKubeClient メニュー
========================================

  Observe
  [ 1] 状態確認: kubectl get nodes -o wide
  [ 2] Pod一覧: kubectl get pods -A
  [ 9] Deployment一覧: kubectl get deploy -A
  [10] Service一覧: kubectl get svc -A
  [11] Ingress一覧: kubectl get ingress -A
  [12] Event一覧: kubectl get events -A

  Develop
  [13] YAML適用: kubectl apply -f <file>
  [14] YAML削除: kubectl delete -f <file>
  [15] Podログ表示: kubectl logs
  [16] Pod詳細表示: kubectl describe pod
  [17] port-forward: kubectl port-forward

  Tools
  [ 3] kubectl 任意コマンドを実行
  [ 4] Helm一覧: helm list -A
  [ 5] helm 任意コマンドを実行
  [ 6] KUBECONFIG設定済みの cmd.exe を開く
  [ 7] 最新ログを表示              [ 8] 現在のパス設定を表示

  [0] 終了
```

`UserKubeClient` は minikube を起動しません。SYSTEM処理も呼びません。あくまで、既に出力済みの kubeconfig を使って Kubernetes を利用する側です。

---

## 明示パス設定

この版では、パスを自動推測しません。

設定例では次の固定パスを使っています。

```text
C:\Users\taro\.minikube-system-learning\
  home\     MINIKUBE_HOME
  kube\     SystemMinikubeHost 側の KUBECONFIG
  logs\     Serilog ログ

C:\Users\taro\.kube\config
  kubectl / helm が参照する kubeconfig
```

このサンプルでは `taro` という固定パスを使っています。別の配置にしたい場合は、`App.config` の値を手で直接書き換えてください。アプリ側にパス切替機能はありません。

---

## SystemMinikubeHost の App.config

`src\SystemMinikubeHost\App.config` の例です。

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="WorkDirectory" value="C:\Users\taro\.minikube-system-learning" />
    <add key="MinikubeHomeDirectory" value="C:\Users\taro\.minikube-system-learning\home" />
    <add key="SystemKubeConfigPath" value="C:\Users\taro\.minikube-system-learning\kube\config" />
    <add key="LogsDirectory" value="C:\Users\taro\.minikube-system-learning\logs" />
    <add key="ExportedKubeConfigPath" value="C:\Users\taro\.kube\config" />

    <add key="MinikubeExePath" value="C:\Program Files\Kubernetes\Minikube\minikube.exe" />
    <add key="KubectlExePath" value="C:\Program Files\Kubernetes\kubectl.exe" />
    <add key="MinikubeDriver" value="hyperv" />

    <add key="LogRetentionDays" value="7" />
  </appSettings>
</configuration>
```

`MinikubeDriver` は既定で `hyperv` です。環境に合わせて `docker` や `virtualbox` に変更できます。

`SystemKubeConfigPath` は `SystemMinikubeHost` が minikube 操作用に使う kubeconfig です。

`ExportedKubeConfigPath` は、`kubectl config view --minify --flatten --raw` の出力先です。

---

## UserKubeClient の App.config

`src\UserKubeClient\App.config` の例です。

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="KubeConfigPath" value="C:\Users\taro\.kube\config" />
    <add key="LogsDirectory" value="C:\Users\taro\.minikube-system-learning\logs" />

    <add key="KubectlExePath" value="C:\Program Files\Kubernetes\kubectl.exe" />
    <add key="HelmExePath" value="C:\Program Files\Helm\helm.exe" />
  </appSettings>
</configuration>
```

`SystemMinikubeHost` の `ExportedKubeConfigPath` と、`UserKubeClient` の `KubeConfigPath` は同じパスにしてください。

---

## minikube driver の選び方

`src\SystemMinikubeHost\App.config` の `MinikubeDriver` で、`minikube start --driver=...` に渡す driver を切り替えます。

```xml
<add key="MinikubeDriver" value="hyperv" />
```

このプロジェクトの既定値は `hyperv` です。

```text
hyperv
  Windows の Hyper-V 上に minikube を作る方式です。
  Windows Pro / Enterprise で Hyper-V を使う学習環境に向いています。
  Windows Home では Hyper-V driver が使えない、または追加機能が不足する場合があります。

docker
  Docker Engine / Docker Desktop 上に minikube を作る方式です。
  Docker Desktop をすでに使っている場合は扱いやすいです。
  Docker Desktop が起動していないと失敗します。

virtualbox
  VirtualBox の VM 上に minikube を作る方式です。
  Docker Desktop を使わずに VM で試したい場合の候補です。
  BIOS / UEFI 側で VT-x / AMD-V が有効である必要があります。
```

例です。

```xml
<!-- Hyper-V を使う -->
<add key="MinikubeDriver" value="hyperv" />

<!-- Docker Desktop を使う -->
<add key="MinikubeDriver" value="docker" />

<!-- VirtualBox を使う -->
<add key="MinikubeDriver" value="virtualbox" />
```

---

## minikube / kubectl / helm の場所

このプロジェクトでは、ツールの場所も `App.config` に明示します。

既定例:

```xml
<add key="MinikubeExePath" value="C:\Program Files\Kubernetes\Minikube\minikube.exe" />
<add key="KubectlExePath" value="C:\Program Files\Kubernetes\kubectl.exe" />
<add key="MinikubeDriver" value="hyperv" />
<add key="HelmExePath" value="C:\Program Files\Helm\helm.exe" />
```

実環境で違う場所にある場合は、`where` で確認して config を直してください。

```cmd
where minikube
where kubectl
where helm
```

Chocolatey などで入れている場合は、例えば次のような場所になることがあります。

```xml
<add key="MinikubeExePath" value="C:\ProgramData\chocolatey\bin\minikube.exe" />
<add key="KubectlExePath" value="C:\ProgramData\chocolatey\bin\kubectl.exe" />
<add key="HelmExePath" value="C:\ProgramData\chocolatey\bin\helm.exe" />
```

---

## 単体 exe 配布について

`SystemMinikubeHost` と `UserKubeClient` の両方に、依存 DLL を exe に埋め込むための `Costura.Fody` / `Fody` を追加しています。

追加している NuGet パッケージは次の通りです。

```xml
<PackageReference Include="Fody" Version="6.9.3" PrivateAssets="all" IncludeAssets="runtime; build; native; contentfiles; analyzers" />
<PackageReference Include="Costura.Fody" Version="6.2.0" PrivateAssets="all" IncludeAssets="runtime; build; native; contentfiles; analyzers" />
```

各プロジェクトには次の `FodyWeavers.xml` も置いています。

```xml
<Weavers>
  <Costura />
</Weavers>
```

これにより、ビルド時に NuGet 由来の参照 DLL が exe のリソースとして埋め込まれます。`App.config` は設定ファイルなので、必要に応じて exe と同じ場所に配置してください。

---

## ビルド

Visual Studio 2019 / 2022 で `MinikubeSystemLauncher.sln` を開いてビルドしてください。

PowerShell で試す場合は、Windows 上で次を実行します。

```powershell
.\build.ps1
```

出力先の例:

```text
out\SystemMinikubeHost\SystemMinikubeHost.exe
out\UserKubeClient\UserKubeClient.exe
```

---

## 使い方

### 1. SystemMinikubeHost を起動する

学習用に管理者コマンドプロンプトで実行する例です。

```cmd
SystemMinikubeHost.exe
```

メニューから `1. 起動` を選ぶと、内部では概ね次の処理を `cmd /c` 経由で実行します。通常起動では minikube の詳細デバッグログを出さないため、コンソール出力は少なめです。

```cmd
minikube start --driver=<MinikubeDriver> --profile=minikube
kubectl --kubeconfig "C:\Users\taro\.minikube-system-learning\kube\config" config view --minify --flatten --raw > "C:\Users\taro\.kube\config"
```

調査用に詳細ログを見たい場合は、メニューから `22. 詳細起動` を選びます。内部では次のように `--alsologtostderr -v=2` を付けます。

```cmd
minikube start --driver=<MinikubeDriver> --profile=minikube --alsologtostderr -v=2
```

引数付きでも実行できます。

```cmd
SystemMinikubeHost.exe start
SystemMinikubeHost.exe start-verbose
```

メニューから `7. 停止` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube stop --profile=minikube
```

これは minikube の VM / クラスターを停止します。状態は残るので、あとで `1. 起動` を選ぶと再開できます。

メニューから `8. 削除` を選ぶと、確認入力のあとに概ね次を `cmd /c` 経由で実行します。

```cmd
minikube delete --profile=minikube
```

削除後、`ExportedKubeConfigPath` に出力済みの kubeconfig が残っている場合は、削除するかどうかを追加で確認します。

メニューから `9. addon一覧` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube addons list --profile=minikube
```

メニューから `10. dashboard addon有効化` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube addons enable dashboard --profile=minikube
```

メニューから `11. dashboard URL表示` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube dashboard --url --profile=minikube
```

`minikube dashboard --url` は URL 表示用のプロキシを維持するため、コマンドが終了しない場合があります。コンソール上に URL が表示されたら、終了したいときは `Ctrl+C` を押してください。

メニューから `12. ingress addon有効化` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube addons enable ingress --profile=minikube
```

メニューから `13. metrics-server addon有効化` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube addons enable metrics-server --profile=minikube
```

メニューから `14. registry addon有効化` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube addons enable registry --profile=minikube
```

`registry` は、クラスター内レジストリを使う学習をしたい場合に便利です。単にローカルイメージを使うだけなら、`minikube image load` でも十分です。

メニューから `15. storage系 addon有効化` を選ぶと、内部では概ね次を順番に `cmd /c` 経由で実行します。

```cmd
minikube addons enable default-storageclass --profile=minikube
minikube addons enable storage-provisioner --profile=minikube
```

これは PVC / StorageClass の学習用です。環境によっては最初から有効になっていることがあります。

メニューから `16. おすすめaddonまとめて有効化` を選ぶと、次の addon をまとめて有効化します。

```text
dashboard
ingress
metrics-server
default-storageclass
storage-provisioner
```

メニューから `17. イメージ一覧` を選ぶと、内部では概ね次を `cmd /c` 経由で実行します。

```cmd
minikube image ls --profile=minikube
```

メニューから `18. イメージ読み込み` を選ぶと、ローカル Docker にあるイメージ名、または Docker image tar のパスを入力して、概ね次を実行します。

```cmd
minikube image load sample-app:dev --profile=minikube
minikube image load C:\Users\taro\images\sample-app-dev.tar --profile=minikube
```

メニューから `19. イメージビルド` を選ぶと、イメージタグと Dockerfile があるフォルダを入力して、概ね次を実行します。

```cmd
minikube image build -t sample-app:dev C:\Users\taro\src\sample-app --profile=minikube
```

メニューから `20. イメージ削除` を選ぶと、概ね次を実行します。

```cmd
minikube image rm sample-app:dev --profile=minikube
```

メニューから `21. ローカル開発コマンド例を表示` を選ぶと、`image build/load` から `kubectl apply` までの流れを表示します。

### 2. ローカル開発の例

ローカル開発では、まず `SystemMinikubeHost` 側で minikube にイメージを入れます。

```cmd
SystemMinikubeHost.exe image-build sample-app:dev C:\Users\taro\src\sample-app
```

または、既にローカル Docker にあるイメージや tar ファイルを読み込みます。

```cmd
SystemMinikubeHost.exe image-load sample-app:dev
SystemMinikubeHost.exe image-load C:\Users\taro\images\sample-app-dev.tar
```

その後、`UserKubeClient` 側で YAML を適用します。

```cmd
UserKubeClient.exe apply samples\local-dev\sample-app.yaml
UserKubeClient.exe pods
UserKubeClient.exe services
UserKubeClient.exe ingress
```

ローカルイメージを使う YAML では、次の指定が重要です。

```yaml
image: sample-app:dev
imagePullPolicy: Never
```

サンプルとして `samples\local-dev\sample-app.yaml` を入れています。これは `sample-app:dev` を使う Deployment / Service / Ingress の最小例です。アプリの待受ポートが `8080` 以外の場合は、`containerPort` と Service の `targetPort` を変更してください。

Ingress を使う場合は、`ingress` addon を有効化したうえで、必要に応じて `sample-app.local` の名前解決を自分の環境に合わせて設定してください。

### 3. UserKubeClient を起動する

```cmd
UserKubeClient.exe
```

メニューから `1. 状態確認` や `2. Pod一覧` を選べます。ローカル開発では、`13. YAML 適用`、`15. Podログ表示`、`16. Pod詳細表示`、`17. port-forward` もよく使います。

---

## 引数付き実行も可能

メニューではなく、直接コマンド指定もできます。

```cmd
SystemMinikubeHost.exe start
SystemMinikubeHost.exe status
SystemMinikubeHost.exe export-kubeconfig
SystemMinikubeHost.exe enable-hyperv
SystemMinikubeHost.exe addons-list
SystemMinikubeHost.exe dashboard-enable
SystemMinikubeHost.exe ingress-enable
SystemMinikubeHost.exe metrics-server-enable
SystemMinikubeHost.exe registry-enable
SystemMinikubeHost.exe storage-enable
SystemMinikubeHost.exe addons-enable-recommended
SystemMinikubeHost.exe dashboard-url
SystemMinikubeHost.exe image-ls
SystemMinikubeHost.exe image-load sample-app:dev
SystemMinikubeHost.exe image-load C:\Users\taro\images\sample-app-dev.tar
SystemMinikubeHost.exe image-build sample-app:dev C:\Users\taro\src\sample-app
SystemMinikubeHost.exe image-rm sample-app:dev
SystemMinikubeHost.exe dev-help
SystemMinikubeHost.exe logs
SystemMinikubeHost.exe paths
```

```cmd
UserKubeClient.exe status
UserKubeClient.exe pods
UserKubeClient.exe deployments
UserKubeClient.exe services
UserKubeClient.exe ingress
UserKubeClient.exe events
UserKubeClient.exe apply C:\Users\taro\src\sample-app\k8s\deployment.yaml
UserKubeClient.exe delete-file C:\Users\taro\src\sample-app\k8s\deployment.yaml
UserKubeClient.exe logs-pod sample-app-xxxxx default
UserKubeClient.exe describe-pod sample-app-xxxxx default
UserKubeClient.exe port-forward svc/sample-app 8080:80 default
UserKubeClient.exe kubectl get pods -A
UserKubeClient.exe helm list -A
UserKubeClient.exe shell
UserKubeClient.exe logs
UserKubeClient.exe paths
```

---

## 起動失敗時の挙動

`SystemMinikubeHost` の `1. 起動` は、`minikube start` が成功した場合だけ、続けて `minikube status`、`kubectl get nodes`、`kubeconfig 出力` を実行します。

`minikube start` が失敗した場合は、そこで処理を止めます。VM や kubeconfig が存在しない状態で後続の `kubectl` を実行すると、原因と関係ないエラーが増えて分かりにくくなるためです。

```text
minikube start 失敗
  -> status / kubectl get nodes / kubeconfig 出力は実行しない

minikube status 失敗
  -> kubectl get nodes は実行しない

SystemMinikubeHost 側 kubeconfig が存在しない
  -> kubeconfig 出力は実行しない
```

VirtualBox driver で次のエラーが出る場合は、BIOS/UEFI 側で CPU 仮想化支援機能が無効です。

```text
This computer doesn't have VT-X/AMD-v enabled.
Enabling it in the BIOS is mandatory.
```

Intel CPU では `Intel Virtualization Technology` / `VT-x`、AMD CPU では `SVM Mode` / `AMD-V` という名前で表示されることが多いです。

Windows から確認する場合は、タスクマネージャーの「パフォーマンス」→「CPU」→「仮想化」、または `systeminfo` の `Virtualization Enabled In Firmware` を確認します。


---

## メニュー実行後の挙動

引数なしで起動したメニュー画面では、番号選択後に処理を実行し、結果を表示したあと Enter キー待ちになります。

```text
Enter キーでメニューへ戻ります。
```

Enter を押すとメニューに戻ります。処理中に例外が発生した場合も、アプリ全体は終了せず、警告を表示してメニューへ戻ります。

引数付きで `SystemMinikubeHost.exe start` や `UserKubeClient.exe status` のように実行した場合は、従来通りそのコマンドだけ実行して終了します。

---

## ログ

`SystemMinikubeHost` は Serilog でコンソールログと日次ファイルログを出します。

`cmd /c` で実行したコマンドの標準出力・標準エラーは、コンソールにも表示しながら Serilog にも記録します。Serilog のコンソール sink を有効にしているため、`[INF]` / `[WRN]` などの色付きログも表示されます。

通常の `1. 起動` は `--alsologtostderr -v=2` を付けません。minikube の内部ログを詳しく見たい場合だけ、`22. 詳細起動` または `SystemMinikubeHost.exe start-verbose` を使います。

文字化けを減らすため、両アプリは起動時に `Console.OutputEncoding` / `Console.InputEncoding` を UTF-8 に寄せています。また、外部コマンド実行時は `cmd /d /s /c "chcp 65001 > nul & ..."` でコードページを UTF-8 に変更し、`ProcessStartInfo.StandardOutputEncoding` / `StandardErrorEncoding` も UTF-8 にしています。

例:

```text
C:\Users\taro\.minikube-system-learning\logs\system-20260627.log
```

`LogRetentionDays` の既定値は `7` です。

```xml
<add key="LogRetentionDays" value="7" />
```

---

## 入れていないもの

このプロジェクトには、以下は入れていません。

```text
AdminSetup
UserStart
タスクスケジューラ登録
schtasks /Run
Schedule.Service COM
Windows Service
C:\ProgramData\MinikubeSystem
実行中のパス切替機能
```

---

## 注釈

このプロジェクトは学習用です。

実際の運用で、SYSTEM と標準ユーザーの境界、ACL、タスクスケジューラ、サービス化などをどう設計するかは別途検討してください。このREADMEでは、まずコンソール上で `cmd /c` ジョブを選び、状態やログを確認しながら動きを理解することを優先しています。
