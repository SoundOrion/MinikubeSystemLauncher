# Blazor Server Sample

.NET 8 の Blazor Server を Kubernetes の Deployment / Service として動かすサンプルです。

コンソールアプリの Job と違い、これはブラウザで画面を確認できます。

```text
Blazor Server
  ↓
コンテナイメージ
  ↓
Deployment
  ↓
Service
  ↓
port-forward
  ↓
http://localhost:8080
```

このサンプルでは、同じ `sample-blazor-server:dev` というイメージを 2 つの方法で作れます。

```text
A方式
  Dockerfile を使い、minikube image build で minikube 内にイメージを作る

B方式
  dotnet publish /t:PublishContainer で tar.gz を作り、minikube image load で取り込む
```

どちらの方式でも、最後に使う YAML は同じ `k8s\blazor-server.yaml` です。

---

## サンプル構成

```text
samples\blazor-server
  SampleBlazorServer
    SampleBlazorServer.csproj
    Program.cs
    Dockerfile
    Components
    wwwroot
  k8s
    blazor-server.yaml
  build-a-minikube-image.ps1
  build-b-container-archive.ps1
  load-b-container-archive.ps1
  apply-blazor.ps1
  delete-blazor.ps1
  port-forward-blazor.ps1
  README.md
```

---

## A方式: Dockerfile + minikube image build

`SampleBlazorServer\Dockerfile` を使って、minikube 側でコンテナイメージをビルドします。

`SystemMinikubeHost` のメニューなら、`19. イメージビルド` を使います。

```text
image tag : sample-blazor-server:dev
build path: C:\Users\taro\src\MinikubeSystemLauncher\samples\blazor-server\SampleBlazorServer
```

PowerShell スクリプトでも実行できます。

```powershell
cd samples\blazor-server
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\build-a-minikube-image.ps1
```

---

## B方式: dotnet publish /t:PublishContainer + minikube image load

Visual Studio / .NET SDK 側でコンテナイメージのアーカイブを作り、それを minikube に読み込みます。

```powershell
cd samples\blazor-server
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\build-b-container-archive.ps1
.\load-b-container-archive.ps1
```

作成されるファイルの例です。

```text
samples\blazor-server\artifacts\sample-blazor-server-dev.tar.gz
```

`SystemMinikubeHost` の `18. イメージ読み込み` で、この tar.gz を指定しても同じです。

---

## Kubernetes に適用する

A方式またはB方式で `sample-blazor-server:dev` を minikube に用意したあと、YAML を適用します。

`UserKubeClient` のメニューなら、`13. YAML適用` で次を指定します。

```text
samples\blazor-server\k8s\blazor-server.yaml
```

PowerShell スクリプトでも実行できます。

```powershell
cd samples\blazor-server
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\apply-blazor.ps1
```

Pod を確認します。

```cmd
kubectl get pods
kubectl get svc sample-blazor-server
```

---

## ブラウザで見る

Service を Windows 側へ port-forward します。

```powershell
cd samples\blazor-server
.\port-forward-blazor.ps1
```

ブラウザで開きます。

```text
http://localhost:8080
```

終了するときは、port-forward している PowerShell で `Ctrl+C` を押します。

---

## 削除する

```powershell
cd samples\blazor-server
.\delete-blazor.ps1
```

または `UserKubeClient` の `14. YAML削除` で `k8s\blazor-server.yaml` を指定します。
