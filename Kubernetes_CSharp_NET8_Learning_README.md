# C# / .NET 8 で学ぶ Kubernetes / minikube 学習・ローカル開発フロー

この README は、C# / .NET 8 アプリを使って Kubernetes の基本操作を段階的に学ぶためのガイドです。
特定のプロジェクトに依存しない、独立した学習用メモとして使えます。

目標は、いきなり Helm / KEDA / Temporal などの応用から入るのではなく、まず Kubernetes の基本リソースと操作を自分で確認できるようになることです。

---

## 1. 最終的に理解したい構成

最終的には、ローカルの minikube 上に次のような構成を作れる状態を目指します。

```text
PC / ブラウザ / curl
  ↓
port-forward または Ingress
  ↓
Service
  ↓
Deployment
  ↓
Pod
  ↓
C# / .NET 8 Web API または Blazor Server
```

さらに発展すると、次の要素も追加します。

```text
ConfigMap / Secret
  アプリ設定や環境変数を渡す

Job / CronJob
  バッチ処理、DBマイグレーション、一回だけ実行する処理

PersistentVolumeClaim
  PostgreSQL などのデータ永続化

StatefulSet
  DB のように永続データを持つ Pod

Ingress
  http://sample.local のような名前でアプリにアクセスする

Helm
  複数 YAML や環境ごとの差分を整理する

k9s / Dashboard
  Pod や Deployment の状態を視覚的に確認する

Temporal / KEDA
  Worker 処理、ワークフロー、イベント駆動スケーリングを学ぶ
```

---

## 2. 最初に押さえる用語

### minikube

ローカルPC上に Kubernetes クラスターを作るためのツールです。
学習・ローカル開発では、まず minikube を使って Kubernetes の動きを確認すると分かりやすいです。

### kubectl

Kubernetes を操作するためのコマンドです。
Pod、Deployment、Service、Ingress、Job などを作成・確認・削除します。

よく使う例です。

```cmd
kubectl get nodes
kubectl get pods -A
kubectl get svc -A
kubectl describe pod <pod-name>
kubectl logs <pod-name>
kubectl apply -f app.yaml
kubectl delete -f app.yaml
```

### kubeconfig

`kubectl` がどの Kubernetes クラスターへ接続するかを知るための設定ファイルです。
接続先 API Server、証明書、認証情報、namespace などが入っています。

`kubectl` が変なクラスターを見ている場合は、まず kubeconfig を疑います。

```cmd
kubectl config current-context
kubectl config get-contexts
```

### Pod

Kubernetes でアプリが実行される最小単位です。
C# アプリのコンテナは、最終的に Pod の中で動きます。

### Deployment

Web API や Blazor Server のように、常駐して動き続けるアプリを管理するためのリソースです。
Pod が落ちた場合に作り直したり、replica 数を増やしたりできます。

常駐 Worker も Deployment で動かすことが多いです。
Web API と違い、外部から HTTP で呼ばれない Worker では Service が不要な場合があります。

### 常駐 Worker

キュー、DB、Temporal、Redis、RabbitMQ、Azure Service Bus などを見に行き、処理を待ち続けるアプリです。
.NET 8 では Worker Service テンプレートや `BackgroundService` で作ると分かりやすいです。

```cmd
dotnet new worker -n SampleWorker
```

Kubernetes 上では、Job ではなく Deployment として常駐させます。

### Service

Pod に安定したアクセス先を与えるためのリソースです。
Pod は作り直されると IP が変わるため、Service 経由でアクセスします。

### Job

一回だけ実行して終了する処理に使います。
C# コンソールアプリ、DBマイグレーション、単発バッチ処理に向いています。

### CronJob

定期実行する Job です。
夜間バッチ、定期集計、定期クリーンアップなどに使います。

### ConfigMap

アプリに渡す通常の設定値を管理します。
例: ログレベル、接続先URL、画面表示メッセージなど。

### Secret

パスワード、トークン、接続文字列など、秘密情報を扱うためのリソースです。
学習環境では見やすさ優先で扱うこともありますが、実運用では管理方法に注意します。

### Ingress

HTTP/HTTPS のルーティングを定義するリソースです。
たとえば `http://sample.local` で Service にアクセスできるようにします。
minikube では ingress addon を有効化して使います。

### Helm

Kubernetes YAML をテンプレート化して、複数のリソースをまとめてインストール・更新・削除しやすくするツールです。
YAMLをなくすものではなく、YAML管理を楽にするものです。

### k9s

ターミナル上で Kubernetes リソースを視覚的に確認できる TUI ツールです。
Pod の状態、ログ、describe、delete などを素早く確認できます。

---

## 3. 学習のおすすめ順

一気に全部やると混乱しやすいので、次の順番で進めるのがおすすめです。

```text
Step 1. minikube を起動する
Step 2. kubectl で node / pod / service を確認する
Step 3. C# コンソールアプリを Job として動かす
Step 4. C# Web API または Blazor Server を Deployment として動かす
Step 5. C# 常駐 Worker を Deployment として動かす
Step 6. Service と port-forward でアクセスする
Step 7. ConfigMap / Secret で環境変数を渡す
Step 8. Ingress で名前付きアクセスを試す
Step 9. PostgreSQL + PVC で永続化を試す
Step 10. Job / CronJob でバッチやDBマイグレーションを試す
Step 11. Helm で YAML 管理を整理する
Step 12. k9s / Dashboard で状態確認を楽にする
Step 13. Temporal / KEDA などの応用へ進む
```

---

## 4. Step 1: minikube を起動する

まずは Kubernetes クラスターを起動します。

```cmd
minikube start --driver=docker --profile=minikube
```

driver は環境によって変えます。

```text
docker
  Docker Desktop / Docker Engine 上に minikube を作る

hyperv
  Hyper-V の VM 上に minikube を作る

virtualbox
  VirtualBox の VM 上に minikube を作る
```

起動後に確認します。

```cmd
minikube status --profile=minikube
kubectl get nodes -o wide
```

期待する状態です。

```text
host: Running
kubelet: Running
apiserver: Running

NAME       STATUS   ROLES
minikube   Ready    control-plane
```

---

## 5. Step 2: kubectl の基本操作

最初に覚えるコマンドです。

```cmd
kubectl get nodes
kubectl get pods -A
kubectl get deploy -A
kubectl get svc -A
kubectl get events -A
```

Pod がうまく動かないときは、まずこの2つを見ます。

```cmd
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

`describe` はイベントやスケジューリング状態を見るために使います。
`logs` はアプリの標準出力を見るために使います。

---

## 6. Step 3: C# コンソールアプリを Job で動かす

C# コンソールアプリは Kubernetes 上では画面を表示しません。
Pod の中で実行され、`Console.WriteLine` の内容は Pod ログとして確認します。

流れです。

```text
C# コンソールアプリ
  ↓
コンテナイメージ
  ↓
Kubernetes Job
  ↓
Pod が起動
  ↓
処理して終了
  ↓
kubectl logs で確認
```

Job の YAML 例です。

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: sample-console-job
spec:
  template:
    spec:
      restartPolicy: Never
      containers:
        - name: sample-console-job
          image: sample-console-job:dev
          imagePullPolicy: Never
```

実行します。

```cmd
kubectl apply -f job.yaml
kubectl get pods
kubectl logs job/sample-console-job
```

Job が正常終了すると、Pod は `Completed` になります。
これは正常です。

```text
STATUS
Completed
```

---

## 7. コンテナイメージの作り方

C# / .NET 8 アプリを Kubernetes で動かすには、コンテナイメージが必要です。
作り方は大きく2つあります。

---

### A方式: Dockerfile + minikube image build

Dockerfile を使って、minikube 側でコンテナイメージを作る方式です。

```cmd
minikube image build -t sample-app:dev . --profile=minikube
```

特徴です。

```text
向いているケース:
  Dockerfile の基本も学びたい
  C#以外のアプリにも応用したい
  一般的なコンテナビルドの流れを覚えたい

注意点:
  Dockerfile が必要
  .NET SDK イメージを取得できる必要がある
  NuGet restore が通る必要がある
```

.NET 8 の Dockerfile 例です。

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "SampleApp.dll"]
```

コンソールアプリなら runtime イメージは `mcr.microsoft.com/dotnet/runtime:8.0` でもよいです。
Web API / Blazor Server なら `mcr.microsoft.com/dotnet/aspnet:8.0` を使います。

---

### B方式: dotnet publish /t:PublishContainer + minikube image load

.NET SDK でコンテナアーカイブを作って、minikube に読み込む方式です。
Dockerfile なしでも使えます。

```powershell
dotnet publish .\MyApp.csproj `
  -c Release `
  --os linux `
  --arch x64 `
  --self-contained false `
  /t:PublishContainer `
  -p:ContainerRepository=sample-app `
  -p:ContainerImageTag=dev `
  -p:ContainerArchiveOutputPath=.\artifacts\sample-app-dev.tar.gz
```

minikube に読み込みます。

```cmd
minikube image load .\artifacts\sample-app-dev.tar.gz --overwrite=true --profile=minikube
```

特徴です。

```text
向いているケース:
  Visual Studio / .NET SDK 中心で開発したい
  Dockerfile なしで試したい
  .NET 8 のコンテナ発行機能を使いたい

注意点:
  .NET 8 SDK が必要
  作ったアーカイブを minikube image load で読み込む必要がある
  YAML の image 名と tag を合わせる必要がある
```

---

## 8. imagePullPolicy の考え方

ローカルで作ったイメージを使う場合は、基本的に次のようにします。

```yaml
image: sample-app:dev
imagePullPolicy: Never
```

これは「外部レジストリから pull せず、minikube 内にあるイメージだけを使う」という意味です。

もし minikube 内にイメージがないと、次のような状態になります。

```text
ErrImageNeverPull
Container image "sample-app:dev" is not present with pull policy of Never
```

この場合は、先にイメージを作るか、読み込んでください。

```cmd
minikube image ls --profile=minikube
minikube image build -t sample-app:dev . --profile=minikube
minikube image load sample-app-dev.tar.gz --overwrite=true --profile=minikube
```

外部公開イメージを使う場合は、たとえばこうです。

```yaml
image: nginx:1.27
imagePullPolicy: IfNotPresent
```

---

## 9. Step 4: C# Web API / Blazor Server を Deployment で動かす

Web API や Blazor Server は、Job ではなく Deployment として動かします。
常駐アプリだからです。

```text
C# Web API / Blazor Server
  ↓
Deployment
  ↓
Pod
  ↓
Service
  ↓
port-forward
  ↓
ブラウザ / curl / Postman
```

Deployment + Service の例です。

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sample-web
spec:
  replicas: 1
  selector:
    matchLabels:
      app: sample-web
  template:
    metadata:
      labels:
        app: sample-web
    spec:
      containers:
        - name: sample-web
          image: sample-web:dev
          imagePullPolicy: Never
          ports:
            - containerPort: 8080
---
apiVersion: v1
kind: Service
metadata:
  name: sample-web
spec:
  selector:
    app: sample-web
  ports:
    - port: 80
      targetPort: 8080
```

適用します。

```cmd
kubectl apply -f web.yaml
kubectl get pods
kubectl get svc
```

アクセスします。

```cmd
kubectl port-forward svc/sample-web 8080:80
```

ブラウザで開きます。

```text
http://localhost:8080
```

### 常駐 Worker の場合

常駐 Worker は Web API とかなり同じノリで動かせます。
違いは、外部から HTTP で呼ばれるアプリではないため、基本的には Service が不要なことです。

```text
Web API / Blazor Server
  Deployment
  Service
  port-forward / Ingress
  ブラウザ / curl / Postman からアクセス

常駐 Worker
  Deployment
  Service は基本不要
  Queue / DB / Temporal / Redis / RabbitMQ / Azure Service Bus などへ接続
  kubectl logs で処理状況を確認
```

.NET 8 なら Worker Service テンプレートが分かりやすいです。

```cmd
dotnet new worker -n SampleWorker
```

`BackgroundService` で常駐処理を書きます。

```csharp
public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

Deployment の例です。

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sample-worker
spec:
  replicas: 1
  selector:
    matchLabels:
      app: sample-worker
  template:
    metadata:
      labels:
        app: sample-worker
    spec:
      containers:
        - name: sample-worker
          image: sample-worker:dev
          imagePullPolicy: Never
          env:
            - name: DOTNET_ENVIRONMENT
              value: Development
            - name: WORKER_NAME
              value: sample-worker
```

確認は logs / describe を使います。

```cmd
kubectl get pods
kubectl logs -l app=sample-worker -f
kubectl describe pod -l app=sample-worker
```

使い分けは次の通りです。

```text
Job
  一回実行して終了する処理
  例: DBマイグレーション、単発バッチ、CSV取り込み

CronJob
  定期実行する処理
  例: 夜間バッチ、定期集計、定期クリーンアップ

Deployment + Worker
  常駐して処理を待ち続ける
  例: キュー処理、Temporal Worker、メッセージ処理

Deployment + Service
  HTTP で外から呼ばれる
  例: Web API、Blazor Server、MVC
```

Temporal Worker もこの分類です。
`TEMPORAL_ADDRESS` や `TASK_QUEUE` を ConfigMap / Secret / 環境変数で渡し、Deployment として常駐させます。

---

## 10. Step 5: ConfigMap / Secret で設定を渡す

アプリ設定をコンテナイメージに焼き込まず、Kubernetes 側から渡す練習です。

ConfigMap 例です。

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: sample-config
data:
  ASPNETCORE_ENVIRONMENT: Development
  SAMPLE_MESSAGE: Hello from ConfigMap
```

Deployment から環境変数として使います。

```yaml
envFrom:
  - configMapRef:
      name: sample-config
```

Secret 例です。

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: sample-secret
type: Opaque
stringData:
  CONNECTION_STRING: Host=postgres;Database=app;Username=app;Password=password
```

Deployment から使います。

```yaml
envFrom:
  - secretRef:
      name: sample-secret
```

注意点です。

```text
ConfigMap:
  通常の設定値向け

Secret:
  秘密情報向け
  ただし学習環境では中身が見えやすい
  実運用ではSecret管理方法を別途考える
```

---

## 11. Step 6: Ingress で名前付きアクセスを試す

Ingress を使うと、Service に対して HTTP ルーティングを定義できます。

minikube では ingress addon を有効化します。

```cmd
minikube addons enable ingress --profile=minikube
```

Ingress 例です。

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: sample-web
spec:
  rules:
    - host: sample.local
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: sample-web
                port:
                  number: 80
```

ローカルPCから名前でアクセスしたい場合は、`hosts` を設定します。

```text
127.0.0.1 sample.local
```

ただし、minikube の driver や OS によってアクセス方法が変わることがあります。
最初は port-forward の方が簡単です。
Ingress は Service と HTTP ルーティングを理解してから試すのがおすすめです。

---

## 12. Step 7: PostgreSQL + PVC を試す

DB のようにデータを保持するものは、Pod を消してもデータが残るように永続化を考える必要があります。

ここで出てくるリソースです。

```text
PersistentVolumeClaim
  データ保存領域を要求する

StatefulSet
  DB のように安定した名前や永続データが必要なアプリ向け

Service
  DB 接続先を固定する
```

学習では、まず PostgreSQL を Helm chart で入れるのが簡単です。
自分で StatefulSet / PVC を書くのは、その後で十分です。

確認ポイントです。

```cmd
kubectl get pvc
kubectl get pods
kubectl describe pvc <pvc-name>
```

注意点です。

```text
minikube delete すると、基本的に中のDBデータも消える
PVC は学習環境では便利だが、永続性の範囲を過信しない
DB は Deployment より StatefulSet の方が自然
```

---

## 13. Step 8: DBマイグレーションを Job で実行する

Webアプリ起動前やデプロイ時に、DBマイグレーションを Job として実行する構成は実践的です。

```text
DB Migration Job
  ↓
PostgreSQL に接続
  ↓
マイグレーション実行
  ↓
Completed
```

注意点です。

```text
同時に複数実行されないようにする
失敗時にログで原因を追えるようにする
接続文字列は Secret から渡す
本番ではロールバック方針も考える
```

---

## 14. Step 9: CronJob で定期バッチを試す

定期実行する C# コンソールアプリは CronJob にできます。

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: sample-batch
spec:
  schedule: "*/5 * * * *"
  jobTemplate:
    spec:
      template:
        spec:
          restartPolicy: Never
          containers:
            - name: sample-batch
              image: sample-batch:dev
              imagePullPolicy: Never
```

確認します。

```cmd
kubectl get cronjob
kubectl get jobs
kubectl get pods
kubectl logs <pod-name>
```

---

## 15. Step 10: Helm を使うタイミング

Helm は最初から必須ではありません。
YAML が増えてきたら使います。

```text
YAML直書きでよい段階:
  Job 1個
  Deployment + Service 1セット
  小さいWeb API

Helmが便利になる段階:
  複数Deploymentがある
  ConfigMap / Secret / Service / Ingress が増えた
  dev / stg / prod で設定を切り替えたい
  PostgreSQL / Temporal / Grafana などを入れたい
```

Helm の考え方です。

```text
templates/*.yaml
  Kubernetes YAML のテンプレート

values.yaml
  環境ごとに変えたい値

helm install
  インストール

helm upgrade
  更新

helm uninstall
  まとめて削除
```

Helm は YAML をなくすものではなく、Kubernetes YAML をプロジェクトとして管理しやすくするものです。

---

## 16. Step 11: k9s / Dashboard で見る

### Dashboard

Webブラウザで Pod、Deployment、Service、Event などを確認できます。
minikube addon として使えるので、最初の可視化に向いています。

```cmd
minikube addons enable dashboard --profile=minikube
minikube dashboard --url --profile=minikube
```

### k9s

ターミナル上で Kubernetes の状態を視覚的に見るツールです。
Pod の状態確認、ログ、describe、削除などが素早くできます。

```cmd
k9s --kubeconfig C:\Users\<user>\.kube\config
```

学習では、次を見ると理解が進みます。

```text
Pods
Deployments
Services
Ingresses
Jobs
CronJobs
ConfigMaps
Secrets
Events
Logs
Describe
```

---

## 17. Step 12: Temporal / KEDA へ進む

基本操作ができるようになったら、Temporal や KEDA に進みます。

### Temporal

ワークフロー、リトライ、長時間処理、Worker 処理を扱うための仕組みです。
C# / .NET Worker と相性がよいです。

最初は次の順で試すのがおすすめです。

```text
1. Temporal Server を Helm で minikube に入れる
2. Web UI を port-forward で見る
3. Temporal frontend を port-forward する
4. Windows 側の C# Worker から接続する
5. Workflow を起動して Web UI で見る
6. 慣れたら Worker も Pod 化する
```

### KEDA

イベントやキューの量に応じて Pod を増減させる仕組みです。
Temporal Worker、Queue Worker、バッチ処理と相性がよいです。

ただし、KEDA は最初から入れなくてよいです。
まずは Deployment や Worker を手動で動かせるようになってからで十分です。

---

## 18. よくある状態と原因

### Pending

Pod がまだ起動していない状態です。

確認します。

```cmd
kubectl describe pod <pod-name>
```

よくある原因です。

```text
イメージがない
リソース不足
PVC が作れない
node にスケジュールできない
```

---

### ErrImageNeverPull

`imagePullPolicy: Never` なのに、minikube 内に対象イメージがありません。

対応です。

```cmd
minikube image ls --profile=minikube
minikube image build -t sample-app:dev . --profile=minikube
minikube image load sample-app-dev.tar.gz --overwrite=true --profile=minikube
```

---

### ImagePullBackOff

外部レジストリからイメージを pull できません。

よくある原因です。

```text
イメージ名が間違っている
タグが存在しない
ネットワークから外部レジストリに出られない
認証が必要
社内プロキシや証明書が必要
```

---

### CrashLoopBackOff

コンテナは起動しているが、アプリがすぐ落ちています。

確認します。

```cmd
kubectl logs <pod-name>
kubectl describe pod <pod-name>
```

よくある原因です。

```text
アプリ起動エラー
環境変数不足
接続文字列ミス
ポート設定ミス
必要ファイルがコンテナに入っていない
```

---

### port-forward できない

よくある原因です。

```text
Pod が Running ではない
Service 名が違う
ポート番号が違う
kubectl が別の kubeconfig を見ている
```

確認します。

```cmd
kubectl get pods
kubectl get svc
kubectl config current-context
```

---

## 19. 最初に覚える確認コマンド一覧

```cmd
kubectl get nodes
kubectl get pods
kubectl get pods -A
kubectl get deploy -A
kubectl get svc -A
kubectl get ingress -A
kubectl get events -A
kubectl describe pod <pod-name>
kubectl logs <pod-name>
kubectl logs job/<job-name>
kubectl apply -f app.yaml
kubectl delete -f app.yaml
kubectl port-forward svc/<service-name> 8080:80
```

minikube 側です。

```cmd
minikube status --profile=minikube
minikube addons list --profile=minikube
minikube image ls --profile=minikube
minikube image build -t sample-app:dev . --profile=minikube
minikube image load sample-app-dev.tar.gz --overwrite=true --profile=minikube
minikube stop --profile=minikube
minikube delete --profile=minikube
```

---

## 20. 学習時に気を付けること

### 1. いきなり全部やらない

Kubernetes はリソースが多いので、最初から Ingress、DB、Helm、KEDA、Temporal を同時にやると原因切り分けが難しくなります。

まずは次の順に進めます。

```text
Job
Deployment
Service
port-forward
ConfigMap / Secret
Ingress
PVC / DB
Helm
Temporal / KEDA
```

---

### 2. Pod が動かないときは describe と logs

まず見るべきものはこの2つです。

```cmd
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

`describe` で Kubernetes 側の理由を見ます。
`logs` でアプリ側の理由を見ます。

---

### 3. image 名と tag を合わせる

ビルドしたイメージ名と YAML の `image:` は一致している必要があります。

```cmd
minikube image build -t sample-web:dev .
```

```yaml
image: sample-web:dev
```

ここがズレると Pod は起動しません。

---

### 4. kubeconfig を意識する

`kubectl` がどのクラスターを見ているかは非常に重要です。

```cmd
kubectl config current-context
```

想定と違う接続先を見ていると、`127.0.0.1:6443` に接続しようとして失敗するなどの問題が起きます。

---

### 5. port-forward は Pod が Running になってから

Pod が Pending のままだと port-forward はできません。

```cmd
kubectl get pods
```

`Running` を確認してから実行します。

```cmd
kubectl port-forward svc/sample-web 8080:80
```

---

### 6. Job は Completed が正常

Job は処理して終わるものなので、`Completed` が正常終了です。
Web API や Blazor Server のように動き続けるものは Deployment を使います。

---

### 7. minikube delete はリセットボタン

`minikube delete` はクラスターを削除します。
Pod、Deployment、Service、PVC、読み込んだローカルイメージなども消える可能性があります。

普段は `stop`、作り直したいときだけ `delete` を使います。

```cmd
minikube stop --profile=minikube
minikube delete --profile=minikube
```

---

## 21. まず作るとよいサンプル一覧

学習用に作るなら、この順番がおすすめです。

```text
01-console-job
  C# コンソールアプリを Job で実行

02-webapi
  Minimal API を Deployment + Service + port-forward で実行

03-blazor-server
  Blazor Server を Deployment + Service + port-forward で表示

04-worker
  .NET Worker Service を Deployment として常駐実行

05-config-secret
  ConfigMap / Secret を環境変数として渡す

06-ingress
  sample.local で Web API / Blazor にアクセス

07-postgres
  PostgreSQL + PVC を作る

08-db-migration-job
  C# コンソールアプリで DB マイグレーションを実行

09-cronjob
  C# コンソールアプリを定期実行

10-helm-chart
  Web API / Blazor / Worker / ConfigMap / Service / Ingress を Helm 化

11-temporal
  Temporal Server + C# Worker

12-keda
  Worker をイベントに応じてスケール
```

---


## 22. 補足: MinikubeSystemLauncher を使って実行できること

この README の学習フローは、別途作成した `MinikubeSystemLauncher` を使って進めることもできます。
`SystemMinikubeHost` は minikube 側の操作、`UserKubeClient` は起動済み Kubernetes を使う操作、という分担にすると分かりやすいです。

```text
SystemMinikubeHost
  minikube の起動 / 停止 / 削除
  driver の指定
  addon 有効化
  dashboard URL 表示
  コンテナイメージの build / load / ls / rm
  kubeconfig 出力

UserKubeClient
  kubectl で状態確認
  YAML apply / delete
  Pod logs / describe
  port-forward
  helm 操作
  k9s 起動
```

README の各ステップとの対応は次の通りです。

| 学習フロー | MinikubeSystemLauncher で使うもの |
| --- | --- |
| minikube を起動する | `SystemMinikubeHost` の `起動` |
| node / pod / service を確認する | `UserKubeClient` の `状態確認` / `Pod一覧` / `Service一覧` |
| C# コンソールアプリを Job で動かす | サンプルのイメージ作成後、`UserKubeClient` の `YAML適用` |
| Blazor Server / Web API を動かす | イメージ作成後、`YAML適用` と `port-forward` |
| ConfigMap / Secret を試す | YAML を作成して `UserKubeClient` の `YAML適用` |
| Ingress を試す | `SystemMinikubeHost` の `ingress addon有効化` と `YAML適用` |
| PostgreSQL + PVC を試す | YAML または Helm を使って `UserKubeClient` から適用 |
| Helm を使う | `UserKubeClient` の `helm 任意コマンド` |
| k9s で見る | `UserKubeClient` の `k9s 起動` |
| Dashboard で見る | `SystemMinikubeHost` の `dashboard URL表示` |

すでに用意しているサンプルがある場合は、次の範囲はそのまま進められます。

```text
できること
  minikube 起動 / 停止 / 削除
  addon 有効化
  Dashboard 表示
  k9s 起動
  kubectl / helm 操作
  コンソール Job の実行
  Blazor Server の Deployment / Service / port-forward
  A方式: Dockerfile + minikube image build
  B方式: dotnet publish /t:PublishContainer + minikube image load
  Temporal の入口
```

追加サンプルを作ると、さらに学習フローを埋めやすくなります。

```text
追加するとよいもの
  Web API サンプル
  ConfigMap / Secret サンプル
  Ingress サンプル
  PostgreSQL + PVC サンプル
  CronJob サンプル
```

進め方としては、まず `MinikubeSystemLauncher` で minikube と kubectl 操作に慣れ、次にサンプル YAML を自分で少しずつ変更していくのがおすすめです。
最初から Helm / Temporal / KEDA に寄せすぎるより、`Pod`、`Deployment`、`Service`、`Job`、`ConfigMap`、`Secret`、`Ingress` の動きを順番に見ていく方が理解しやすくなります。

---

## 23. まとめ

Kubernetes 学習で最初に大事なのは、便利ツールを全部入れることではありません。

まずは次を確実にできるようにします。

```text
minikube を起動する
kubectl で状態を見る
イメージを作る
YAML を apply / delete する
Pod の logs / describe を見る
Job と Deployment の違いを理解する
常駐 Worker は Deployment、単発処理は Job として考える
Service と port-forward でアクセスする
ConfigMap / Secret で設定を渡す
Ingress / PVC / Helm に進む
```

ここまでできれば、Temporal や KEDA のような応用も、単なる謎の黒魔術ではなく、Deployment、Service、ConfigMap、Secret、Job などの組み合わせとして読めるようになります。
