# Temporal on minikube

このサンプルは、minikube 上に Temporal Server と Temporal Web UI を入れ、Windows / Visual Studio 側から .NET Worker を接続して Workflow を実行するためのものです。

最初は Worker を Kubernetes Pod にしません。Temporal 本体だけを minikube に置き、Worker と Workflow 起動は Windows 側から行うと、動きが追いやすいです。

## 構成

```text
namespace: temporal

PostgreSQL
  Temporal の永続化先

Temporal Server
  frontend / history / matching / worker

Temporal Web UI
  Workflow の状態を見る画面

TemporalWorkerSample
  .NET 8 の Worker / Workflow 起動サンプル
```

## ファイル

```text
samples/temporal
  values-postgresql.yaml
  values-temporal.yaml
  install-temporal.ps1
  uninstall-temporal.ps1
  status-temporal.ps1
  port-forward-temporal-ui.ps1
  port-forward-temporal-frontend.ps1
  run-worker-local.ps1
  start-workflow-local.ps1

  TemporalWorkerSample
    TemporalWorkerSample.csproj
    Program.cs
    Dockerfile

  k8s
    worker-deployment.yaml
```

## 1. minikube を起動する

先に `SystemMinikubeHost` の `1. 起動` を成功させます。

その後、`UserKubeClient` の `1. 状態確認` と `2. Pod一覧` で Kubernetes に接続できることを確認します。

## 2. addon を確認する

Temporal の PostgreSQL では PVC を使います。`default-storageclass` と `storage-provisioner` が有効になっていると進めやすいです。

`SystemMinikubeHost` の `16. おすすめaddonまとめて有効化` を実行しておくと、storage 系 addon も有効化されます。

## 3. Temporal をインストールする

Helm を使います。

```powershell
cd samples\temporal
.\install-temporal.ps1
```

このスクリプトは次を行います。

```text
1. temporal namespace を作る
2. Bitnami PostgreSQL chart を入れる
3. Temporal Helm chart を入れる
4. frontend / web の起動を待つ
```

状態確認です。

```powershell
.\status-temporal.ps1
```

## 4. Temporal Web UI を開く

別の PowerShell で実行します。

```powershell
cd samples\temporal
.\port-forward-temporal-ui.ps1
```

ブラウザで開きます。

```text
http://localhost:8080
```

## 5. Temporal frontend を port-forward する

Worker / Client が Windows 側から Temporal に接続するため、別の PowerShell で実行します。

```powershell
cd samples\temporal
.\port-forward-temporal-frontend.ps1
```

接続先は次です。

```text
localhost:7233
```

## 6. .NET Worker を起動する

別の PowerShell で実行します。

```powershell
cd samples\temporal
.\run-worker-local.ps1
```

この Worker は次の Task Queue を待ち受けます。

```text
sample-task-queue
```

## 7. Workflow を実行する

さらに別の PowerShell で実行します。

```powershell
cd samples\temporal
.\start-workflow-local.ps1 -Name Hatsuyama
```

Worker 側に Activity のログが出て、Workflow の結果が表示されます。Web UI でも実行履歴を確認できます。

## 8. Worker を Kubernetes Pod にする場合

慣れてきたら Worker も minikube 上で動かせます。

A方式です。

```powershell
cd samples\temporal
.\build-worker-a-minikube-image.ps1
kubectl apply -f .\k8s\worker-deployment.yaml
```

B方式です。

```powershell
cd samples\temporal
.\build-worker-b-container-archive.ps1
.\load-worker-b-container-archive.ps1
kubectl apply -f .\k8s\worker-deployment.yaml
```

Pod 内 Worker は次に接続します。

```text
temporal-frontend.temporal.svc.cluster.local:7233
```

## アンインストール

```powershell
cd samples\temporal
.\uninstall-temporal.ps1
```

PostgreSQL の PVC は、誤削除を避けるため自動では消しません。完全に消したい場合は、スクリプト表示に従って PVC を削除してください。

## kubeconfig

このフォルダの PowerShell サンプルは、既定で `..\..\..\.kube\config` を `KUBECONFIG` に設定してから `kubectl` / `helm` を実行します。

別の kubeconfig を使う場合は `-KubeConfig` を指定してください。

```powershell
.\port-forward-blazor.ps1 -KubeConfig "C:\Users\Hatsuyama\Desktop\k8s\.kube\config"
```

`https://127.0.0.1:6443` に接続しようとして失敗する場合は、古い kubeconfig を参照している可能性があります。`SystemMinikubeHost` の `3. kubeconfig 出力のみ` を実行してから再試行してください。

