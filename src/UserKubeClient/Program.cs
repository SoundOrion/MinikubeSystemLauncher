using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace UserKubeClient
{
    internal sealed class AppOptions
    {
        public string KubeConfigPath { get; private set; }
        public string LogsDirectory { get; private set; }
        public string KubectlExePath { get; private set; }
        public string HelmExePath { get; private set; }
        public string K9sExePath { get; private set; }

        public static AppOptions Load()
        {
            AppOptions options = new AppOptions();
            options.KubeConfigPath = FullPath(Read("KubeConfigPath", @"C:\Users\taro\.kube\config"));
            options.LogsDirectory = FullPath(Read("LogsDirectory", @"C:\Users\taro\.minikube-system-learning\logs"));
            options.KubectlExePath = Read("KubectlExePath", @"C:\Program Files\Kubernetes\kubectl.exe");
            options.HelmExePath = Read("HelmExePath", @"C:\Program Files\Helm\helm.exe");
            options.K9sExePath = Read("K9sExePath", @"C:\Program Files\k9s\k9s.exe");
            return options;
        }

        private static string Read(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().Trim('"');
        }

        private static string FullPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return Path.GetFullPath(value.Trim().Trim('"'));
        }
    }


    internal sealed class MenuItem
    {
        public string Key { get; private set; }
        public string Label { get; private set; }
        public string Hint { get; private set; }
        public ConsoleColor Accent { get; private set; }

        public MenuItem(string key, string label, string hint, ConsoleColor accent)
        {
            Key = key;
            Label = label;
            Hint = hint;
            Accent = accent;
        }
    }

    internal static class Ui
    {
        private const int MenuColumns = 2;
        private const int MenuCellWidth = 58;

        public static void Banner(string title, string subtitle)
        {
            Console.WriteLine();
            WriteColor("========================================", ConsoleColor.Gray);
            WriteColor("  " + title, ConsoleColor.White);
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                WriteColor("  " + subtitle, ConsoleColor.DarkGray);
            }
            WriteColor("========================================", ConsoleColor.Gray);
        }

        public static void Menu(string title, MenuItem[] items)
        {
            Console.WriteLine();
            WriteColor("========================================", ConsoleColor.Gray);
            WriteColor(title + " メニュー", ConsoleColor.White);
            WriteColor("========================================", ConsoleColor.Gray);

            string currentSection = string.Empty;
            System.Collections.Generic.List<MenuItem> sectionItems = new System.Collections.Generic.List<MenuItem>();

            for (int i = 0; i < items.Length; i++)
            {
                MenuItem item = items[i];
                if (item.Key == "-")
                {
                    WriteMenuSection(currentSection, sectionItems);
                    currentSection = item.Label ?? string.Empty;
                    sectionItems.Clear();
                    continue;
                }

                if (item.Key == "0")
                {
                    continue;
                }

                sectionItems.Add(item);
            }

            WriteMenuSection(currentSection, sectionItems);
            Console.WriteLine();
            WriteColor("  [0] 終了", ConsoleColor.Gray);
            Console.WriteLine();
        }

        private static void WriteMenuSection(string section, System.Collections.Generic.List<MenuItem> items)
        {
            if (items == null || items.Count == 0) return;

            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(section))
            {
                WriteColor("  " + section, ConsoleColor.DarkGray);
            }

            int index = 0;
            while (index < items.Count)
            {
                string firstCell = FormatMenuCell(items[index]);
                Console.Write("  ");

                if (DisplayWidth(firstCell) >= MenuCellWidth || index + 1 >= items.Count)
                {
                    WriteInline(firstCell, ConsoleColor.Gray);
                    Console.WriteLine();
                    index++;
                    continue;
                }

                string secondCell = FormatMenuCell(items[index + 1]);
                if (DisplayWidth(secondCell) >= MenuCellWidth)
                {
                    WriteInline(firstCell, ConsoleColor.Gray);
                    Console.WriteLine();
                    index++;
                    continue;
                }

                WriteInline(PadRightSafe(firstCell, MenuCellWidth), ConsoleColor.Gray);
                WriteInline(secondCell, ConsoleColor.Gray);
                Console.WriteLine();
                index += MenuColumns;
            }
        }

        private static string FormatMenuCell(MenuItem item)
        {
            return "[" + PadLeftSafe(item.Key, 2) + "] " + item.Label;
        }

        public static void KeyValue(string key, string value)
        {
            WriteInline("  " + PadRightSafe(key, 12), ConsoleColor.DarkGray);
            Console.WriteLine(value);
        }

        public static void Note(string message)
        {
            WriteInline("  info ", ConsoleColor.DarkGray);
            Console.WriteLine(message);
        }

        public static void Prompt(string text)
        {
            WriteInline(text + ": ", ConsoleColor.White);
        }

        public static void Section(string title)
        {
            Console.WriteLine();
            WriteColor("-- " + title + " --", ConsoleColor.Gray);
        }

        public static void Command(string command)
        {
            Console.WriteLine();
            WriteInline("$ ", ConsoleColor.Gray);
            WriteColor("cmd /d /s /c chcp 65001 > nul & " + command, ConsoleColor.Gray);
        }

        public static void Success(string message)
        {
            WriteInline("OK ", ConsoleColor.Gray);
            Console.WriteLine(message);
        }

        public static void Warn(string message)
        {
            WriteInline("警告: ", ConsoleColor.Yellow);
            WriteColor(message, ConsoleColor.Yellow);
        }

        public static void ExitCode(int exitCode)
        {
            WriteInline("exit code: ", ConsoleColor.Gray);
            Console.WriteLine(exitCode);
        }

        private static void WriteColor(string text, ConsoleColor color)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = old;
        }

        private static void WriteInline(string text, ConsoleColor color)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = old;
        }

        private static string PadRightSafe(string value, int width)
        {
            value = value ?? string.Empty;
            int textWidth = DisplayWidth(value);
            if (textWidth >= width) return value;
            return value + new string(' ', width - textWidth);
        }

        private static int DisplayWidth(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            int width = 0;
            for (int i = 0; i < value.Length; i++)
            {
                width += IsWide(value[i]) ? 2 : 1;
            }
            return width;
        }

        private static bool IsWide(char ch)
        {
            return (ch >= '\u1100' && ch <= '\u115F')
                || (ch >= '\u2E80' && ch <= '\uA4CF')
                || (ch >= '\uAC00' && ch <= '\uD7A3')
                || (ch >= '\uF900' && ch <= '\uFAFF')
                || (ch >= '\uFE10' && ch <= '\uFE6F')
                || (ch >= '\uFF00' && ch <= '\uFFE6');
        }

        private static string PadLeftSafe(string value, int width)
        {
            value = value ?? string.Empty;
            if (value.Length >= width) return value;
            return new string(' ', width - value.Length) + value;
        }
    }


    internal static class Program
    {
        private static readonly AppOptions Config = AppOptions.Load();

        public static int Main(string[] args)
        {
            ConfigureConsoleEncoding();
            PrintStartupNotice();

            if (args.Length == 0) return RunMenu();

            string command = args[0].ToLowerInvariant();
            string[] rest = args.Skip(1).ToArray();

            if (command == "status") return Status();
            if (command == "pods") return PodsAllNamespaces();
            if (command == "deployments") return DeploymentsAllNamespaces();
            if (command == "services") return ServicesAllNamespaces();
            if (command == "ingress") return IngressAllNamespaces();
            if (command == "events") return EventsAllNamespaces();
            if (command == "apply") return ApplyManifestFromArgs(rest);
            if (command == "delete-file") return DeleteManifestFromArgs(rest);
            if (command == "logs-pod") return PodLogsFromArgs(rest);
            if (command == "describe-pod") return DescribePodFromArgs(rest);
            if (command == "port-forward") return PortForwardFromArgs(rest);
            if (command == "kubectl") return RunKubectl(JoinArguments(rest));
            if (command == "helm") return RunHelm(JoinArguments(rest));
            if (command == "k9s") return OpenK9s(JoinArguments(rest));
            if (command == "shell") return OpenShell();
            if (command == "logs") return ShowLogsWithContent();
            if (command == "paths") return ShowPaths();
            if (command == "help" || command == "--help" || command == "-h") return Usage();

            return Usage();
        }

        private static void ConfigureConsoleEncoding()
        {
            try
            {
                Console.OutputEncoding = new UTF8Encoding(false);
                Console.InputEncoding = new UTF8Encoding(false);
            }
            catch
            {
                // コンソール環境によっては encoding 変更に失敗するため、学習・ローカル開発用アプリでは続行します。
            }
        }

        private static void PrintStartupNotice()
        {
            Ui.Banner("UserKubeClient", "kubectl client console");
            Ui.KeyValue("User", WindowsIdentity.GetCurrent().Name);
            Ui.KeyValue("KUBECONFIG", Config.KubeConfigPath);

            if (!File.Exists(Config.KubeConfigPath))
            {
                Warn("kubeconfig が見つかりません: " + Config.KubeConfigPath);
                Console.WriteLine();
            }
        }

        private static int RunMenu()
        {
            while (true)
            {
                Ui.Menu("UserKubeClient", new MenuItem[]
                {
                    new MenuItem("-", "Observe", null, ConsoleColor.DarkGray),
                    new MenuItem("1", "状態確認: kubectl get nodes -o wide", null, ConsoleColor.Green),
                    new MenuItem("2", "Pod一覧: kubectl get pods -A", null, ConsoleColor.Cyan),
                    new MenuItem("9", "Deployment一覧: kubectl get deploy -A", null, ConsoleColor.Cyan),
                    new MenuItem("10", "Service一覧: kubectl get svc -A", null, ConsoleColor.Cyan),
                    new MenuItem("11", "Ingress一覧: kubectl get ingress -A", null, ConsoleColor.Cyan),
                    new MenuItem("12", "Event一覧: kubectl get events -A", null, ConsoleColor.Cyan),
                    new MenuItem("-", "Develop", null, ConsoleColor.DarkGray),
                    new MenuItem("13", "YAML適用: kubectl apply -f <file>", null, ConsoleColor.Magenta),
                    new MenuItem("14", "YAML削除: kubectl delete -f <file>", null, ConsoleColor.Magenta),
                    new MenuItem("15", "Podログ表示: kubectl logs", null, ConsoleColor.Magenta),
                    new MenuItem("16", "Pod詳細表示: kubectl describe pod", null, ConsoleColor.Magenta),
                    new MenuItem("17", "port-forward: kubectl port-forward", null, ConsoleColor.Magenta),
                    new MenuItem("-", "Tools", null, ConsoleColor.DarkGray),
                    new MenuItem("3", "kubectl 任意コマンドを実行", null, ConsoleColor.Yellow),
                    new MenuItem("4", "Helm一覧: helm list -A", null, ConsoleColor.Yellow),
                    new MenuItem("5", "helm 任意コマンドを実行", null, ConsoleColor.Yellow),
                    new MenuItem("6", "KUBECONFIG設定済みの cmd.exe を開く", null, ConsoleColor.Yellow),
                    new MenuItem("18", "k9s 起動", null, ConsoleColor.Yellow),
                    new MenuItem("7", "最新ログを表示", null, ConsoleColor.DarkGray),
                    new MenuItem("8", "現在のパス設定を表示", null, ConsoleColor.DarkGray),
                    new MenuItem("0", "終了", null, ConsoleColor.DarkGray)
                });
                Ui.Prompt("番号を入力してください");

                string input = Console.ReadLine();
                input = input == null ? string.Empty : input.Trim();
                Console.WriteLine();

                if (input == "1") RunMenuAction("状態確認", delegate { return Status(); });
                else if (input == "2") RunMenuAction("Pod 一覧", delegate { return PodsAllNamespaces(); });
                else if (input == "3") RunMenuAction("kubectl 任意", delegate { return RunKubectlInteractive(); });
                else if (input == "4") RunMenuAction("Helm 一覧", delegate { return HelmListAllNamespaces(); });
                else if (input == "5") RunMenuAction("helm 任意", delegate { return RunHelmInteractive(); });
                else if (input == "6") RunMenuAction("KUBECONFIG shell", delegate { return OpenShell(); });
                else if (input == "7") RunMenuAction("最新ログ", delegate { return ShowLogsWithContent(); });
                else if (input == "8") RunMenuAction("パス設定", delegate { return ShowPaths(); });
                else if (input == "9") RunMenuAction("Deployment 一覧", delegate { return DeploymentsAllNamespaces(); });
                else if (input == "10") RunMenuAction("Service 一覧", delegate { return ServicesAllNamespaces(); });
                else if (input == "11") RunMenuAction("Ingress 一覧", delegate { return IngressAllNamespaces(); });
                else if (input == "12") RunMenuAction("Event 一覧", delegate { return EventsAllNamespaces(); });
                else if (input == "13") RunMenuAction("YAML 適用", delegate { return ApplyManifestInteractive(); });
                else if (input == "14") RunMenuAction("YAML 削除", delegate { return DeleteManifestInteractive(); });
                else if (input == "15") RunMenuAction("Pod ログ", delegate { return PodLogsInteractive(); });
                else if (input == "16") RunMenuAction("Pod 詳細", delegate { return DescribePodInteractive(); });
                else if (input == "17") RunMenuAction("port-forward", delegate { return PortForwardInteractive(); });
                else if (input == "18") RunMenuAction("k9s", delegate { return OpenK9s(string.Empty); });
                else if (input == "0") return 0;
                else
                {
                    Warn("番号が不正です。");
                    PauseForMenu();
                }
            }
        }


        private static void RunMenuAction(string title, Func<int> action)
        {
            Ui.Section(title);
            int rc = 0;
            try
            {
                rc = action();
            }
            catch (Exception ex)
            {
                rc = 1;
                Warn("処理中に例外が発生しました。アプリは終了せずメニューへ戻ります。");
                Console.Error.WriteLine(ex.Message);
            }

            Console.WriteLine();
            if (rc == 0) Ui.Success(title + " が完了しました。");
            else Warn(title + " が終了コード " + rc + " で終了しました。");
            PauseForMenu();
        }

        private static void PauseForMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Enter キーでメニューへ戻ります。");
            Console.ReadLine();
        }

        private static int Status()
        {
            return RunKubectl("get nodes -o wide");
        }

        private static int PodsAllNamespaces()
        {
            return RunKubectl("get pods -A");
        }

        private static int HelmListAllNamespaces()
        {
            return RunHelm("list -A");
        }

        private static int DeploymentsAllNamespaces()
        {
            return RunKubectl("get deploy -A");
        }

        private static int ServicesAllNamespaces()
        {
            return RunKubectl("get svc -A");
        }

        private static int IngressAllNamespaces()
        {
            return RunKubectl("get ingress -A");
        }

        private static int EventsAllNamespaces()
        {
            return RunKubectl("get events -A --sort-by=.metadata.creationTimestamp");
        }

        private static int ApplyManifestInteractive()
        {
            Console.WriteLine("適用する YAML ファイルのフルパスを入力してください。");
            Console.WriteLine("例: C:\\Users\\taro\\src\\sample-app\\k8s\\deployment.yaml");
            Console.Write("file: ");
            string path = Console.ReadLine();
            return ApplyManifest(path);
        }

        private static int ApplyManifestFromArgs(string[] args)
        {
            if (args == null || args.Length == 0) return ApplyManifestInteractive();
            return ApplyManifest(JoinArguments(args));
        }

        private static int ApplyManifest(string path)
        {
            path = path == null ? string.Empty : path.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(path))
            {
                Warn("YAML ファイルパスが空です。");
                return 2;
            }
            return RunKubectl("apply -f " + QuoteIfNeeded(path));
        }

        private static int DeleteManifestInteractive()
        {
            Console.WriteLine("削除に使う YAML ファイルのフルパスを入力してください。");
            Console.WriteLine("例: C:\\Users\\taro\\src\\sample-app\\k8s\\deployment.yaml");
            Console.Write("file: ");
            string path = Console.ReadLine();
            return DeleteManifest(path);
        }

        private static int DeleteManifestFromArgs(string[] args)
        {
            if (args == null || args.Length == 0) return DeleteManifestInteractive();
            return DeleteManifest(JoinArguments(args));
        }

        private static int DeleteManifest(string path)
        {
            path = path == null ? string.Empty : path.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(path))
            {
                Warn("YAML ファイルパスが空です。");
                return 2;
            }
            return RunKubectl("delete -f " + QuoteIfNeeded(path));
        }

        private static int PodLogsInteractive()
        {
            Console.Write("namespace [default]: ");
            string ns = Console.ReadLine();
            Console.Write("pod name: ");
            string pod = Console.ReadLine();
            Console.Write("container name [空なら省略]: ");
            string container = Console.ReadLine();
            return PodLogs(pod, ns, container);
        }

        private static int PodLogsFromArgs(string[] args)
        {
            if (args == null || args.Length == 0) return PodLogsInteractive();
            string pod = args[0];
            string ns = args.Length >= 2 ? args[1] : "default";
            string container = args.Length >= 3 ? args[2] : string.Empty;
            return PodLogs(pod, ns, container);
        }

        private static int PodLogs(string podName, string namespaceName, string containerName)
        {
            podName = podName == null ? string.Empty : podName.Trim().Trim('"');
            namespaceName = string.IsNullOrWhiteSpace(namespaceName) ? "default" : namespaceName.Trim().Trim('"');
            containerName = containerName == null ? string.Empty : containerName.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(podName))
            {
                Warn("Pod名が空です。");
                return 2;
            }

            string command = "logs -n " + QuoteIfNeeded(namespaceName) + " " + QuoteIfNeeded(podName) + " --tail=200";
            if (!string.IsNullOrWhiteSpace(containerName)) command += " -c " + QuoteIfNeeded(containerName);
            return RunKubectl(command);
        }

        private static int DescribePodInteractive()
        {
            Console.Write("namespace [default]: ");
            string ns = Console.ReadLine();
            Console.Write("pod name: ");
            string pod = Console.ReadLine();
            return DescribePod(pod, ns);
        }

        private static int DescribePodFromArgs(string[] args)
        {
            if (args == null || args.Length == 0) return DescribePodInteractive();
            string pod = args[0];
            string ns = args.Length >= 2 ? args[1] : "default";
            return DescribePod(pod, ns);
        }

        private static int DescribePod(string podName, string namespaceName)
        {
            podName = podName == null ? string.Empty : podName.Trim().Trim('"');
            namespaceName = string.IsNullOrWhiteSpace(namespaceName) ? "default" : namespaceName.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(podName))
            {
                Warn("Pod名が空です。");
                return 2;
            }

            return RunKubectl("describe pod -n " + QuoteIfNeeded(namespaceName) + " " + QuoteIfNeeded(podName));
        }

        private static int PortForwardInteractive()
        {
            Console.WriteLine("port-forward を開始します。終了する場合は Ctrl+C を押してください。");
            Console.Write("namespace [default]: ");
            string ns = Console.ReadLine();
            Console.WriteLine("対象リソースを入力してください。例: pod/sample-app または svc/sample-app");
            Console.Write("resource: ");
            string resource = Console.ReadLine();
            Console.WriteLine("ポート対応を入力してください。例: 8080:80");
            Console.Write("ports: ");
            string ports = Console.ReadLine();
            return PortForward(resource, ports, ns);
        }

        private static int PortForwardFromArgs(string[] args)
        {
            if (args == null || args.Length < 2) return PortForwardInteractive();
            string resource = args[0];
            string ports = args[1];
            string ns = args.Length >= 3 ? args[2] : "default";
            return PortForward(resource, ports, ns);
        }

        private static int PortForward(string resource, string ports, string namespaceName)
        {
            resource = resource == null ? string.Empty : resource.Trim().Trim('"');
            ports = ports == null ? string.Empty : ports.Trim().Trim('"');
            namespaceName = string.IsNullOrWhiteSpace(namespaceName) ? "default" : namespaceName.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(ports))
            {
                Warn("resource または ports が空です。");
                return 2;
            }

            return RunKubectl("port-forward -n " + QuoteIfNeeded(namespaceName) + " " + QuoteIfNeeded(resource) + " " + QuoteIfNeeded(ports));
        }

        private static int RunKubectlInteractive()
        {
            Console.WriteLine("kubectl の後ろに続く引数だけ入力してください。");
            Console.WriteLine("例: get svc -A");
            Console.Write("kubectl ");
            string args = Console.ReadLine();
            return RunKubectl(args ?? string.Empty);
        }

        private static int RunHelmInteractive()
        {
            Console.WriteLine("helm の後ろに続く引数だけ入力してください。");
            Console.WriteLine("例: list -A");
            Console.Write("helm ");
            string args = Console.ReadLine();
            return RunHelm(args ?? string.Empty);
        }

        private static int RunKubectl(string arguments)
        {
            if (!CheckKubeConfig()) return 2;
            return RunCmd(Quote(Config.KubectlExePath) + " " + arguments, true);
        }

        private static int RunHelm(string arguments)
        {
            if (!CheckKubeConfig()) return 2;
            return RunCmd(Quote(Config.HelmExePath) + " " + arguments, true);
        }

        private static bool CheckKubeConfig()
        {
            if (File.Exists(Config.KubeConfigPath)) return true;
            Warn("kubeconfig が見つかりません: " + Config.KubeConfigPath);
            return false;
        }

        private static int OpenShell()
        {
            if (!CheckKubeConfig()) return 2;

            Console.WriteLine("KUBECONFIG を設定した cmd.exe を開きます。");
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/k chcp 65001";
            psi.UseShellExecute = false;
            psi.EnvironmentVariables["KUBECONFIG"] = Config.KubeConfigPath;

            using (Process p = Process.Start(psi))
            {
                if (p == null) throw new InvalidOperationException("cmd.exe を開始できません。");
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        private static int OpenK9s(string extraArguments)
        {
            if (!CheckKubeConfig()) return 2;

            if (string.IsNullOrWhiteSpace(Config.K9sExePath) || !File.Exists(Config.K9sExePath))
            {
                Warn("k9s.exe が見つかりません: " + Config.K9sExePath);
                Warn("UserKubeClient.exe.config の K9sExePath を実際の配置に合わせて変更してください。");
                return 2;
            }

            string arguments = "--kubeconfig " + Quote(Config.KubeConfigPath);
            if (!string.IsNullOrWhiteSpace(extraArguments))
            {
                arguments += " " + extraArguments;
            }

            Console.WriteLine("k9s を起動します。終了する場合は k9s 内で :q または Ctrl+C を使ってください。");
            return RunInteractiveProcess(Config.K9sExePath, arguments, true);
        }

        private static int RunInteractiveProcess(string exePath, string arguments, bool setKubeConfig)
        {
            Console.WriteLine();
            Console.WriteLine(Quote(exePath) + " " + arguments);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exePath;
            psi.Arguments = arguments ?? string.Empty;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false;

            if (setKubeConfig) psi.EnvironmentVariables["KUBECONFIG"] = Config.KubeConfigPath;

            using (Process p = Process.Start(psi))
            {
                if (p == null) throw new InvalidOperationException(exePath + " を開始できません。");
                p.WaitForExit();
                Ui.ExitCode(p.ExitCode);
                return p.ExitCode;
            }
        }

        private static int ShowPaths()
        {
            Console.WriteLine("現在の明示パス設定");
            Console.WriteLine("--------------------");
            Console.WriteLine("KubeConfigPath : " + Config.KubeConfigPath);
            Console.WriteLine("LogsDirectory  : " + Config.LogsDirectory);
            Console.WriteLine("kubectl.exe    : " + Config.KubectlExePath);
            Console.WriteLine("helm.exe       : " + Config.HelmExePath);
            Console.WriteLine("k9s.exe        : " + Config.K9sExePath);
            return 0;
        }

        private static int ShowLogsWithContent()
        {
            if (!Directory.Exists(Config.LogsDirectory))
            {
                Warn("ログフォルダがありません: " + Config.LogsDirectory);
                return 1;
            }

            FileInfo[] files = new DirectoryInfo(Config.LogsDirectory)
                .GetFiles("*.log")
                .OrderByDescending(f => f.LastWriteTime)
                .Take(5)
                .ToArray();

            if (files.Length == 0)
            {
                Console.WriteLine("ログファイルはありません。");
                return 0;
            }

            Console.WriteLine("最新ログ:");
            for (int i = 0; i < files.Length; i++) Console.WriteLine("  " + files[i].FullName);
            Console.WriteLine();
            Console.WriteLine("最新ログの末尾:");
            PrintTail(files[0].FullName, 80);
            return 0;
        }

        private static int Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  UserKubeClient.exe");
            Console.WriteLine("  UserKubeClient.exe status");
            Console.WriteLine("  UserKubeClient.exe pods");
            Console.WriteLine("  UserKubeClient.exe deployments");
            Console.WriteLine("  UserKubeClient.exe services");
            Console.WriteLine("  UserKubeClient.exe ingress");
            Console.WriteLine("  UserKubeClient.exe events");
            Console.WriteLine("  UserKubeClient.exe apply C:\\Users\\taro\\src\\sample-app\\k8s\\deployment.yaml");
            Console.WriteLine("  UserKubeClient.exe delete-file C:\\Users\\taro\\src\\sample-app\\k8s\\deployment.yaml");
            Console.WriteLine("  UserKubeClient.exe logs-pod sample-app-xxxxx default");
            Console.WriteLine("  UserKubeClient.exe describe-pod sample-app-xxxxx default");
            Console.WriteLine("  UserKubeClient.exe port-forward svc/sample-app 8080:80 default");
            Console.WriteLine("  UserKubeClient.exe kubectl get pods -A");
            Console.WriteLine("  UserKubeClient.exe helm list -A");
            Console.WriteLine("  UserKubeClient.exe k9s");
            Console.WriteLine("  UserKubeClient.exe shell");
            Console.WriteLine("  UserKubeClient.exe logs");
            Console.WriteLine("  UserKubeClient.exe paths");
            return 2;
        }

        private static int RunCmd(string command, bool setKubeConfig)
        {
            Ui.Command(command);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = BuildCmdArguments(command);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.CreateNoWindow = false;

            if (setKubeConfig) psi.EnvironmentVariables["KUBECONFIG"] = Config.KubeConfigPath;

            using (Process p = new Process())
            {
                p.StartInfo = psi;
                p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data == null) return;
                    Console.WriteLine(e.Data);
                };
                p.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data == null) return;
                    Console.Error.WriteLine(e.Data);
                };

                if (!p.Start()) throw new InvalidOperationException("cmd.exe を開始できません。");

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                p.WaitForExit();

                Ui.ExitCode(p.ExitCode);
                return p.ExitCode;
            }
        }

        private static string BuildCmdArguments(string command)
        {
            return "/d /s /c \"chcp 65001 > nul & " + command + "\"";
        }

        private static string JoinArguments(string[] args)
        {
            if (args == null || args.Length == 0) return string.Empty;
            return string.Join(" ", args.Select(QuoteIfNeeded).ToArray());
        }

        private static string QuoteIfNeeded(string value)
        {
            if (value == null) return string.Empty;
            value = value.Trim();
            if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) return value;
            if (value.IndexOf(' ') >= 0 || value.IndexOf('\t') >= 0) return Quote(value);
            return value;
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static void PrintTail(string path, int maxLines)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            int start = Math.Max(0, lines.Length - maxLines);
            for (int i = start; i < lines.Length; i++) Console.WriteLine(lines[i]);
        }

        private static void Warn(string message)
        {
            Ui.Warn(message);
        }
    }
}
