# Console Job Sample

.NET 8 のコンソールアプリを Kubernetes Job として動かすサンプルです。

このサンプルでは、同じ `sample-console-job:dev` というイメージを 2 つの方法で作れます。

```text
A方式
  Dockerfile を使い、minikube image build で minikube 内にイメージを作る

B方式
  dotnet publish /t:PublishContainer で tar.gz を作り、minikube image load で取り込む
```

どちらの方式でも、最後に使う YAML は同じ `job.yaml` です。

---

## サンプル構成

```text
samples\console-job
  ConsoleJobSample
    ConsoleJobSample.csproj
    Program.cs
    Dockerfile
  job.yaml
  build-a-minikube-image.ps1
  build-b-container-archive.ps1
  load-b-container-archive.ps1
  README.md
```

---

## A方式: Dockerfile + minikube image build

`ConsoleJobSample\Dockerfile` を使って、minikube 側でコンテナイメージをビルドします。

`SystemMinikubeHost` のメニューなら、`19. イメージビルド` を使います。

```text
image tag : sample-console-job:dev
build path: C:\Users\taro\src\MinikubeSystemLauncher\samples\console-job\ConsoleJobSample
```

引数付きで実行する例です。

```cmd
SystemMinikubeHost.exe image-build sample-console-job:dev C:\Users\taro\src\MinikubeSystemLauncher\samples\console-job\ConsoleJobSample
```

PowerShell スクリプトでも実行できます。

```powershell
cd samples\console-job
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\build-a-minikube-image.ps1
```

この方式では、Dockerfile の中で `mcr.microsoft.com/dotnet/sdk:8.0` を使って `dotnet restore` / `dotnet publish` します。

---

## B方式: dotnet publish /t:PublishContainer + minikube image load

Visual Studio / .NET SDK 側でコンテナイメージのアーカイブを作り、それを minikube に読み込みます。

```powershell
cd samples\console-job
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\build-b-container-archive.ps1
```

作成されるファイルの例です。

```text
samples\console-job\artifacts\sample-console-job-dev.tar.gz
```

その後、`SystemMinikubeHost` の `18. イメージ読み込み` でこの tar.gz を指定します。

```text
C:\Users\taro\src\MinikubeSystemLauncher\samples\console-job\artifacts\sample-console-job-dev.tar.gz
```

引数付きで実行する例です。

```cmd
SystemMinikubeHost.exe image-load C:\Users\taro\src\MinikubeSystemLauncher\samples\console-job\artifacts\sample-console-job-dev.tar.gz
```

PowerShell スクリプトで直接 minikube に読み込むこともできます。

```powershell
cd samples\console-job
.\load-b-container-archive.ps1
```

この方式は Dockerfile を使いません。`.NET SDK` のコンテナ発行機能でイメージを作ります。

---

## Job を適用する

A方式またはB方式で `sample-console-job:dev` を minikube に用意したあと、`job.yaml` を適用します。

`UserKubeClient` のメニューなら、`13. YAML適用` で `job.yaml` を指定します。

```cmd
UserKubeClient.exe apply C:\Users\taro\src\MinikubeSystemLauncher\samples\console-job\job.yaml
```

---

## 実行結果を見る

Pod 名を確認します。

```cmd
kubectl get pods
```

ログを見ます。

```cmd
kubectl logs job/sample-console-job
```

`Completed` になっていれば、Job として正常終了しています。

---

## もう一度実行する

同じ Job 名は、そのまま再 apply しても再実行されません。
一度削除してから apply します。

```cmd
kubectl delete -f job.yaml
kubectl apply -f job.yaml
```

`UserKubeClient` のメニューなら、`14. YAML削除` のあとに `13. YAML適用` を実行します。
