# MinikubeSystemLauncher

.NET Framework 4.8 の学習・ローカル開発用コンソールアプリです。

`minikube` でローカル Kubernetes クラスターを起動し、その上で `kubectl` / `helm` / YAML / コンテナイメージを試すための小さなランチャーです。

このプロジェクトでは、役割を次の 2 つに分けています。

```text
SystemMinikubeHost
  minikube 本体を操作する側
  クラスターの起動、停止、削除、addon 有効化、イメージ操作、kubeconfig 出力を行う

UserKubeClient
  起動済み Kubernetes を使う側
  kubectl / helm を使って Pod、Deployment、Service、Ingress、YAML、ログ確認などを行う
```

---

## まず知っておく用語

### minikube

ローカル PC 上に Kubernetes クラスターを作るためのツールです。

通常の Kubernetes は複数台のサーバー上で動かすことが多いですが、`minikube` を使うと 1 台の Windows PC 上で Kubernetes の基本操作を確認し、ローカル開発の動作確認にも使えます。

このプロジェクトでは、主に次の操作を行います。

```text
minikube start
  ローカル Kubernetes クラスターを起動する

minikube stop
  クラスターを停止する

minikube delete
  クラスターを削除して作り直せる状態にする

minikube status
  クラスターの状態を確認する

minikube addons
  Dashboard、Ingress、metrics-server などの追加機能を管理する

minikube image
  minikube 内で使うコンテナイメージを扱う
```

### Kubernetes

コンテナを動かすための基盤です。

Docker でコンテナを 1 個ずつ起動する代わりに、Kubernetes では「どのイメージを、何個、どの設定で、どのネットワーク名で動かすか」を YAML で定義します。

ローカル開発では、次のような使い方をします。

```text
自作アプリをコンテナイメージ化する
  ↓
minikube にイメージを入れる
  ↓
Deployment / Service / Ingress の YAML を適用する
  ↓
Pod の状態やログを見る
```

### kubectl

Kubernetes を操作するためのコマンドです。

このプロジェクトでは `UserKubeClient` から `kubectl` を呼び出します。

よく使う例です。

```cmd
kubectl get nodes -o wide
kubectl get pods -A
kubectl get deploy -A
kubectl get svc -A
kubectl get ingress -A
kubectl apply -f deployment.yaml
kubectl delete -f deployment.yaml
kubectl logs <pod-name>
kubectl describe pod <pod-name>
kubectl port-forward svc/sample-app 8080:80
```

### kubeconfig

`kubectl` や `helm` が、どの Kubernetes クラスターへ接続するかを知るための設定ファイルです。

このプロジェクトでは、`SystemMinikubeHost` が minikube 用の kubeconfig を作り、`UserKubeClient` がその kubeconfig を使います。

```text
SystemMinikubeHost 側
  SystemKubeConfigPath
  minikube 操作用の kubeconfig

UserKubeClient 側
  KubeConfigPath
  kubectl / helm 操作用の kubeconfig
```

`SystemMinikubeHost` の `3. kubeconfig 出力のみ` は、minikube 側の kubeconfig を `UserKubeClient` 用に flatten して出力します。

### Helm

Kubernetes 用のパッケージ管理ツールです。

単純なアプリなら YAML だけでも十分ですが、複数の Deployment / Service / ConfigMap / Secret / Job などをまとめて管理したい場合、Helm chart が便利です。

例です。

```text
YAML
  自作アプリや小さな検証に向いている

Helm
  Temporal、Prometheus、Grafana、Ingress Controller など、構成要素が多いものに向いている
```

このプロジェクトの `UserKubeClient` では、次の操作を用意しています。

```text
Helm一覧: helm list -A
helm 任意コマンドを実行
```

### minikube addon

minikube に追加機能を入れる仕組みです。

このプロジェクトで扱う主な addon は次の通りです。

```text
dashboard
  ブラウザで Kubernetes の状態を見るための画面を追加する

ingress
  Ingress リソースを使って HTTP ルーティングを試せるようにする

metrics-server
  CPU / メモリ使用量を取得できるようにする
  kubectl top nodes / kubectl top pods などで使う

registry
  クラスター内レジストリを試すための addon
  ローカルイメージを使うだけなら minikube image load / build でもよい

default-storageclass / storage-provisioner
  PVC / StorageClass の確認に使う
  データを持つ Pod や StatefulSet の検証で使う
```

`16. おすすめaddonまとめて有効化` では、次をまとめて有効化します。

```text
dashboard
ingress
metrics-server
default-storageclass
storage-provisioner
```

---

## プロジェクト構成

```text
MinikubeSystemLauncher
  MinikubeSystemLauncher.sln
  build.ps1
  README.md

  src
    SystemMinikubeHost
      SystemMinikubeHost.csproj
      Program.cs
      App.config
      FodyWeavers.xml

    UserKubeClient
      UserKubeClient.csproj
      Program.cs
      App.config
      FodyWeavers.xml

  samples
    local-dev
      sample-app.yaml
```

### SystemMinikubeHost

minikube クラスターを操作するアプリです。

主な役割です。

```text
minikube start
minikube stop
minikube delete
minikube status
minikube addons list
minikube addons enable ...
minikube dashboard --url
minikube image ls / load / build / rm
kubectl config view --minify --flatten --raw
Hyper-V 有効化コマンド
ログ表示
パス設定表示
```

### UserKubeClient

起動済み Kubernetes を使うアプリです。

主な役割です。

```text
kubectl get nodes / pods / deploy / svc / ingress / events
kubectl apply -f
kubectl delete -f
kubectl logs
kubectl describe pod
kubectl port-forward
helm list -A
helm 任意コマンド
KUBECONFIG 設定済み cmd.exe を開く
ログ表示
パス設定表示
```

---

## 必要なもの

Windows 上で使う想定です。

```text
Visual Studio 2019 / 2022
.NET Framework 4.8 targeting pack
minikube
kubectl
helm  ※ Helm を使う場合
```

driver によって追加で必要なものが変わります。

```text
hyperv
  Windows Pro / Enterprise の Hyper-V 環境

docker
  Docker Desktop または Docker Engine

virtualbox
  VirtualBox
  BIOS / UEFI の VT-x / AMD-V 有効化
```

---

## minikube driver の選び方

`SystemMinikubeHost` の `App.config` にある `MinikubeDriver` で指定します。

```xml
<add key="MinikubeDriver" value="hyperv" />
```

このプロジェクトの既定値は `hyperv` です。

### hyperv

Windows の Hyper-V 上に minikube を作る方式です。

```text
向いている場合
  Windows Pro / Enterprise を使っている
  Hyper-V driver を試したい
  Windows 標準寄りの仮想化で試したい

注意点
  Windows Home では Hyper-V driver が使えない、または機能が不足する場合がある
  Hyper-V 機能の有効化後に再起動が必要になることがある
```

### docker

Docker Desktop / Docker Engine 上に minikube を作る方式です。

```text
向いている場合
  Docker Desktop をすでに使っている
  Windows Home で minikube を試したい
  Docker ベースでローカル開発したい

注意点
  Docker Desktop が起動していないと失敗する
  Docker Desktop の設定や WSL2 側の状態に影響される
```

### virtualbox

VirtualBox の VM 上に minikube を作る方式です。

```text
向いている場合
  Docker Desktop を使わずに VM で試したい
  VirtualBox の確認も兼ねたい

注意点
  BIOS / UEFI で VT-x / AMD-V が有効である必要がある
  Hyper-V と競合する環境では追加調整が必要になることがある
```

設定例です。

```xml
<!-- Hyper-V を使う -->
<add key="MinikubeDriver" value="hyperv" />

<!-- Docker Desktop を使う -->
<add key="MinikubeDriver" value="docker" />

<!-- VirtualBox を使う -->
<add key="MinikubeDriver" value="virtualbox" />
```

---

## App.config の設定

このプロジェクトでは、パスを `App.config` にフルパスで書きます。

### SystemMinikubeHost

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

各キーの意味です。

```text
WorkDirectory
  学習・ローカル開発用ファイルを置く親フォルダ

MinikubeHomeDirectory
  minikube の MINIKUBE_HOME
  minikube のキャッシュやプロファイルがここに置かれる

SystemKubeConfigPath
  SystemMinikubeHost が minikube 操作に使う kubeconfig

LogsDirectory
  Serilog のログ出力先

ExportedKubeConfigPath
  UserKubeClient が使う kubeconfig の出力先

MinikubeExePath
  minikube.exe の場所

KubectlExePath
  kubectl.exe の場所

MinikubeDriver
  hyperv / docker / virtualbox など

LogRetentionDays
  ログ保持日数
```

### UserKubeClient

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

各キーの意味です。

```text
KubeConfigPath
  kubectl / helm が使う kubeconfig
  SystemMinikubeHost の ExportedKubeConfigPath と同じ値にする

LogsDirectory
  SystemMinikubeHost が出したログを見る場所

KubectlExePath
  kubectl.exe の場所

HelmExePath
  helm.exe の場所
```

ツールの場所が分からない場合は、Windows のコマンドプロンプトで確認します。

```cmd
where minikube
where kubectl
where helm
```

Chocolatey で入れている場合は、次のような場所になることがあります。

```xml
<add key="MinikubeExePath" value="C:\ProgramData\chocolatey\bin\minikube.exe" />
<add key="KubectlExePath" value="C:\ProgramData\chocolatey\bin\kubectl.exe" />
<add key="HelmExePath" value="C:\ProgramData\chocolatey\bin\helm.exe" />
```

---

## ビルド

Visual Studio 2019 / 2022 で `MinikubeSystemLauncher.sln` を開いてビルドします。

PowerShell でビルドする場合は、Windows 上で次を実行します。

```powershell
.\build.ps1
```

出力先の例です。

```text
out\SystemMinikubeHost\SystemMinikubeHost.exe
out\UserKubeClient\UserKubeClient.exe
```

`SystemMinikubeHost` と `UserKubeClient` には `Costura.Fody` / `Fody` を追加しています。ビルド時に NuGet 由来の参照 DLL を exe に埋め込むためです。

`App.config` は設定ファイルなので、実行時は exe と同じ場所に配置してください。

---

## 基本の使い方

### 1. SystemMinikubeHost を起動する

管理者コマンドプロンプトで実行する例です。

```cmd
SystemMinikubeHost.exe
```

メニューが表示されます。

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

### 2. minikube を起動する

`1. 起動` を選びます。

内部では概ね次を実行します。

```cmd
minikube start --driver=<MinikubeDriver> --profile=minikube
minikube status --profile=minikube
kubectl --kubeconfig "<SystemKubeConfigPath>" get nodes -o wide
kubectl --kubeconfig "<SystemKubeConfigPath>" config view --minify --flatten --raw > "<ExportedKubeConfigPath>"
```

`minikube start` が失敗した場合は、後続の `status`、`kubectl get nodes`、`kubeconfig 出力` は実行しません。

詳細ログを見たい場合は `22. 詳細起動` を選びます。

```cmd
minikube start --driver=<MinikubeDriver> --profile=minikube --alsologtostderr -v=2
```

通常の起動では `--alsologtostderr -v=2` を付けないので、minikube の内部ログは少なめです。

### 3. addon を有効化する

学習・ローカル開発向けにまとめて有効化する場合は、`16. おすすめaddonまとめて有効化` を選びます。

有効化後、`9. addon一覧` で状態を確認できます。

Dashboard をブラウザで見たい場合は、`11. dashboard URL表示` を選びます。

`minikube dashboard --url` は URL 表示用のプロキシを維持するため、終了しない場合があります。終了するときは `Ctrl+C` を押します。

### 4. UserKubeClient を起動する

`SystemMinikubeHost` で kubeconfig を出力したあと、`UserKubeClient` を起動します。

```cmd
UserKubeClient.exe
```

メニューが表示されます。

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

まずは次を確認します。

```text
1. 状態確認
2. Pod一覧
9. Deployment一覧
10. Service一覧
12. Event一覧
```

---

## ローカル開発の流れ

### 1. アプリをコンテナイメージにする

自作アプリに Dockerfile を置き、イメージを作ります。

`SystemMinikubeHost` の `19. イメージビルド` を使うと、概ね次を実行します。

```cmd
minikube image build -t sample-app:dev C:\Users\taro\src\sample-app --profile=minikube
```

既に Docker イメージや tar ファイルがある場合は、`18. イメージ読み込み` を使います。

```cmd
minikube image load sample-app:dev --profile=minikube
minikube image load C:\Users\taro\images\sample-app-dev.tar --profile=minikube
```

minikube 内のイメージを確認する場合は、`17. イメージ一覧` を使います。

```cmd
minikube image ls --profile=minikube
```

### 2. YAML を用意する

サンプルとして `samples\local-dev\sample-app.yaml` を入れています。

内容は、次の 3 種類です。

```text
Deployment
  sample-app:dev というイメージから Pod を起動する

Service
  Pod への安定した接続先を作る

Ingress
  sample-app.local というホスト名で Service へルーティングする
```

ローカルイメージを使う場合は、YAML のこの指定が重要です。

```yaml
image: sample-app:dev
imagePullPolicy: Never
```

`imagePullPolicy: Never` は、外部レジストリから pull せず、minikube 内にあるイメージを使う指定です。

### 3. YAML を適用する

`UserKubeClient` の `13. YAML適用` を使います。

概ね次を実行します。

```cmd
kubectl apply -f samples\local-dev\sample-app.yaml
```

確認します。

```cmd
kubectl get pods -A
kubectl get deploy -A
kubectl get svc -A
kubectl get ingress -A
```

Pod のログを見る場合です。

```cmd
kubectl logs <pod-name>
```

Pod の状態を詳しく見る場合です。

```cmd
kubectl describe pod <pod-name>
```

ローカル PC から Service に接続したい場合は、port-forward を使います。

```cmd
kubectl port-forward svc/sample-app 8080:80
```

ブラウザで開く例です。

```text
http://localhost:8080
```

---

## コンソールアプリと Web API の載せ方

### コンソールアプリ

短時間で終わるバッチ処理なら、Kubernetes の `Job` が向いています。

```text
例
  データ取り込み
  一括変換
  検証用の単発処理
```

### Web API / Web アプリ

起動し続けるアプリなら、`Deployment + Service` が基本です。

```text
Deployment
  Pod を起動し続ける

Service
  Pod への接続先を作る

Ingress
  HTTP のルーティングを作る
```

この README のサンプル YAML は、Web API / Web アプリ向けの最小例です。

---

## SystemMinikubeHost のコマンド一覧

メニューではなく引数付きでも実行できます。

```cmd
SystemMinikubeHost.exe start
SystemMinikubeHost.exe start-verbose
SystemMinikubeHost.exe status
SystemMinikubeHost.exe export-kubeconfig
SystemMinikubeHost.exe stop
SystemMinikubeHost.exe delete
SystemMinikubeHost.exe enable-hyperv
SystemMinikubeHost.exe addons-list
SystemMinikubeHost.exe dashboard-enable
SystemMinikubeHost.exe dashboard-url
SystemMinikubeHost.exe ingress-enable
SystemMinikubeHost.exe metrics-server-enable
SystemMinikubeHost.exe registry-enable
SystemMinikubeHost.exe storage-enable
SystemMinikubeHost.exe addons-enable-recommended
SystemMinikubeHost.exe image-ls
SystemMinikubeHost.exe image-load sample-app:dev
SystemMinikubeHost.exe image-load C:\Users\taro\images\sample-app-dev.tar
SystemMinikubeHost.exe image-build sample-app:dev C:\Users\taro\src\sample-app
SystemMinikubeHost.exe image-rm sample-app:dev
SystemMinikubeHost.exe dev-help
SystemMinikubeHost.exe logs
SystemMinikubeHost.exe paths
```

---

## UserKubeClient のコマンド一覧

メニューではなく引数付きでも実行できます。

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

## stop と delete の使い分け

### stop

作業を終えるだけなら `stop` を使います。

```cmd
minikube stop --profile=minikube
```

クラスターの状態や addon、読み込んだイメージは基本的に残ります。

次回また `start` すると再開できます。

### delete

環境を作り直したい場合は `delete` を使います。

```cmd
minikube delete --profile=minikube
```

削除すると、クラスター内の Pod、Deployment、Service、Ingress、addon の状態、minikube 内に入れたイメージなどは消えます。

次のような場合に使います。

```text
driver を変えたい
addon 周りを最初からやり直したい
Storage / PVC を含めて初期化したい
minikube の状態が分からなくなった
```

---

## ログ

`SystemMinikubeHost` は Serilog でコンソールログと日次ファイルログを出します。

外部コマンドの標準出力・標準エラーも、コンソールに表示しながらログファイルへ記録します。

例です。

```text
C:\Users\taro\.minikube-system-learning\logs\system-20260627.log
```

ログ保持日数は `LogRetentionDays` で指定します。

```xml
<add key="LogRetentionDays" value="7" />
```

通常の `1. 起動` では minikube の詳細デバッグログを出しません。

詳しいログが必要なときだけ `22. 詳細起動` または次の引数付き実行を使います。

```cmd
SystemMinikubeHost.exe start-verbose
```

---

## よくある確認ポイント

### minikube が起動しているか

`SystemMinikubeHost` の `2. 状態確認` を使います。

正常な例です。

```text
host: Running
kubelet: Running
apiserver: Running
kubeconfig: Configured
```

`kubectl get nodes -o wide` で `Ready` が出れば、Kubernetes ノードとして動いています。

```text
NAME       STATUS   ROLES
minikube   Ready    control-plane
```

### kubeconfig がない

先に `SystemMinikubeHost` の `1. 起動` を成功させます。

すでに minikube が起動済みなら、`3. kubeconfig 出力のみ` を実行します。

### Docker driver で起動できない

Docker Desktop が起動しているか確認します。

```cmd
docker version
docker info
```

### VirtualBox driver で VT-x / AMD-V のエラーが出る

BIOS / UEFI で CPU 仮想化支援機能を有効にします。

Intel CPU では、次のような名前で表示されることがあります。

```text
Intel Virtualization Technology
VT-x
Virtualization Technology
```

AMD CPU では、次のような名前で表示されることがあります。

```text
SVM Mode
AMD-V
Secure Virtual Machine
```

Windows から確認する場合は、タスクマネージャーの「パフォーマンス」→「CPU」→「仮想化」を見ます。

### Hyper-V driver で失敗する

Windows Home では Hyper-V driver に必要な機能が不足することがあります。

その場合は、`MinikubeDriver` を `docker` または `virtualbox` に変更して試します。

```xml
<add key="MinikubeDriver" value="docker" />
```

---

## 文字化け対策

両アプリは起動時にコンソール入出力を UTF-8 に寄せます。

外部コマンド実行時も、概ね次の形で `cmd.exe` を呼び出します。

```cmd
cmd /d /s /c "chcp 65001 > nul & <command>"
```

さらに、`ProcessStartInfo.StandardOutputEncoding` と `StandardErrorEncoding` も UTF-8 にしています。

Windows 側の一部メッセージや外部ツールの出力では、環境によって完全には揃わない場合があります。

---

## 学習・ローカル開発のおすすめ順

最初はこの順番で試すと分かりやすいです。

```text
1. SystemMinikubeHost の App.config を自分の環境に合わせる
2. UserKubeClient の App.config を自分の環境に合わせる
3. SystemMinikubeHost.exe を起動する
4. 1. 起動 を実行する
5. 2. 状態確認 を実行する
6. 16. おすすめaddonまとめて有効化 を実行する
7. UserKubeClient.exe を起動する
8. 1. 状態確認 / 2. Pod一覧 を見る
9. sample-app.yaml を読み、自分のアプリ用に書き換える
10. minikube image build または image load を試す
11. kubectl apply -f でアプリを起動する
12. logs / describe / port-forward を試す
```
