using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Serilog;
using Serilog.Events;

namespace SystemMinikubeHost
{
    internal sealed class AppOptions
    {
        public string WorkDirectory { get; private set; }
        public string MinikubeHomeDirectory { get; private set; }
        public string SystemKubeConfigPath { get; private set; }
        public string LogsDirectory { get; private set; }
        public string ExportedKubeConfigPath { get; private set; }
        public string MinikubeExePath { get; private set; }
        public string KubectlExePath { get; private set; }
        public string MinikubeDriver { get; private set; }
        public int LogRetentionDays { get; private set; }

        public static AppOptions Load()
        {
            AppOptions options = new AppOptions();
            options.WorkDirectory = FullPath(Read("WorkDirectory", @"C:\Users\taro\.minikube-system-learning"));
            options.MinikubeHomeDirectory = FullPath(Read("MinikubeHomeDirectory", @"C:\Users\taro\.minikube-system-learning\home"));
            options.SystemKubeConfigPath = FullPath(Read("SystemKubeConfigPath", @"C:\Users\taro\.minikube-system-learning\kube\config"));
            options.LogsDirectory = FullPath(Read("LogsDirectory", @"C:\Users\taro\.minikube-system-learning\logs"));
            options.ExportedKubeConfigPath = FullPath(Read("ExportedKubeConfigPath", @"C:\Users\taro\.kube\config"));
            options.MinikubeExePath = Read("MinikubeExePath", @"C:\Program Files\Kubernetes\Minikube\minikube.exe");
            options.KubectlExePath = Read("KubectlExePath", @"C:\Program Files\Kubernetes\kubectl.exe");
            options.MinikubeDriver = Read("MinikubeDriver", "hyperv");
            options.LogRetentionDays = ReadInt("LogRetentionDays", 7);
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

        private static int ReadInt(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            int parsed;
            if (int.TryParse(value, out parsed) && parsed > 0) return parsed;
            return defaultValue;
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
        private static AppOptions Config;

        public static int Main(string[] args)
        {
            ConfigureConsoleEncoding();

            try
            {
                Config = AppOptions.Load();
                ConfigureLogging();
                PrintStartupNotice();

                if (args.Length == 0) return RunMenu();

                string command = args[0].ToLowerInvariant();
                string[] rest = args.Skip(1).ToArray();

                if (command == "start") return StartMinikube(false);
                if (command == "start-verbose" || command == "start-debug") return StartMinikube(true);
                if (command == "stop") return StopMinikube();
                if (command == "delete") return DeleteMinikube();
                if (command == "status") return Status();
                if (command == "export-kubeconfig") return ExportKubeconfig();
                if (command == "enable-hyperv") return EnableHyperV();
                if (command == "addons-list") return AddonsList();
                if (command == "dashboard-enable") return DashboardAddonEnable();
                if (command == "ingress-enable") return IngressAddonEnable();
                if (command == "metrics-server-enable") return MetricsServerAddonEnable();
                if (command == "registry-enable") return RegistryAddonEnable();
                if (command == "storage-enable") return StorageAddonsEnable();
                if (command == "addons-enable-recommended") return RecommendedAddonsEnable();
                if (command == "dashboard-url") return DashboardUrl();
                if (command == "image-ls") return ImageList();
                if (command == "image-load") return ImageLoadFromArgs(rest);
                if (command == "image-build") return ImageBuildFromArgs(rest);
                if (command == "image-rm") return ImageRemoveFromArgs(rest);
                if (command == "dev-help") return LocalDevelopmentHelp();
                if (command == "logs") return ShowLatestLogs();
                if (command == "paths") return ShowPaths();
                if (command == "help" || command == "--help" || command == "-h") return Usage();

                Warn("不明なコマンドです: " + command);
                return Usage();
            }
            catch (Exception ex)
            {
                try { Log.Fatal(ex, "SystemMinikubeHost failed."); } catch { }
                Console.Error.WriteLine(ex);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
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

        private static void ConfigureLogging()
        {
            try
            {
                Directory.CreateDirectory(Config.LogsDirectory);

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.WithProperty("App", "SystemMinikubeHost")
                    .Enrich.WithProperty("RunAs", WindowsIdentity.GetCurrent().Name)
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: Path.Combine(Config.LogsDirectory, "system-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileTimeLimit: TimeSpan.FromDays(Config.LogRetentionDays),
                        retainedFileCountLimit: null,
                        shared: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
            }
            catch
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();
            }
        }

        private static void PrintStartupNotice()
        {
            Ui.Banner("SystemMinikubeHost", "minikube host console");
            Ui.KeyValue("User", WindowsIdentity.GetCurrent().Name);
            Ui.KeyValue("Role", AuthorityText());
            Ui.KeyValue("Driver", Config.MinikubeDriver);
        }

        private static string AuthorityText()
        {
            if (IsSystem()) return "SYSTEM";
            if (IsAdministrator()) return "管理者";
            return "標準ユーザー";
        }

        private static bool IsSystem()
        {
            return WindowsIdentity.GetCurrent().Name.Equals(@"NT AUTHORITY\SYSTEM", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdministrator()
        {
            try
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static bool CanRunElevatedJob(string jobName)
        {
            if (IsSystem() || IsAdministrator()) return true;

            Warn(jobName + " は SYSTEM または管理者での実行を想定しています。");
            Warn("このコンソールは標準ユーザーで起動されています。ジョブは実行しません。");
            return false;
        }

        private static int RunMenu()
        {
            while (true)
            {
                Ui.Menu("SystemMinikubeHost", new MenuItem[]
                {
                    new MenuItem("-", "Cluster", null, ConsoleColor.DarkGray),
                    new MenuItem("1", "起動: minikube start + kubeconfig 出力", null, ConsoleColor.Green),
                    new MenuItem("22", "詳細起動: minikube start --alsologtostderr -v=2", null, ConsoleColor.DarkYellow),
                    new MenuItem("2", "状態確認: minikube status / kubectl get nodes", null, ConsoleColor.Cyan),
                    new MenuItem("3", "kubeconfig 出力のみ", null, ConsoleColor.Cyan),
                    new MenuItem("7", "停止: minikube stop", null, ConsoleColor.Yellow),
                    new MenuItem("8", "削除: minikube delete", null, ConsoleColor.Red),
                    new MenuItem("-", "Addons", null, ConsoleColor.DarkGray),
                    new MenuItem("9", "addon一覧: minikube addons list", null, ConsoleColor.Cyan),
                    new MenuItem("10", "dashboard addon有効化: minikube addons enable dashboard", null, ConsoleColor.Cyan),
                    new MenuItem("11", "dashboard URL表示: minikube dashboard --url", null, ConsoleColor.Cyan),
                    new MenuItem("12", "ingress addon有効化: minikube addons enable ingress", null, ConsoleColor.Cyan),
                    new MenuItem("13", "metrics-server addon有効化: minikube addons enable metrics-server", null, ConsoleColor.Cyan),
                    new MenuItem("14", "registry addon有効化: minikube addons enable registry", null, ConsoleColor.Cyan),
                    new MenuItem("15", "storage系 addon有効化: default-storageclass / storage-provisioner", null, ConsoleColor.Cyan),
                    new MenuItem("16", "おすすめaddonまとめて有効化", null, ConsoleColor.Green),
                    new MenuItem("-", "Images", null, ConsoleColor.DarkGray),
                    new MenuItem("17", "イメージ一覧: minikube image ls", null, ConsoleColor.Magenta),
                    new MenuItem("18", "イメージ読み込み: minikube image load <imageまたはtar>", null, ConsoleColor.Magenta),
                    new MenuItem("19", "イメージビルド: minikube image build -t <image> <path>", null, ConsoleColor.Magenta),
                    new MenuItem("20", "イメージ削除: minikube image rm <image>", null, ConsoleColor.Magenta),
                    new MenuItem("-", "Utility", null, ConsoleColor.DarkGray),
                    new MenuItem("4", "Hyper-V 有効化コマンドを実行", null, ConsoleColor.DarkYellow),
                    new MenuItem("5", "最新ログを表示", null, ConsoleColor.DarkGray),
                    new MenuItem("6", "現在のパス設定を表示", null, ConsoleColor.DarkGray),
                    new MenuItem("21", "ローカル開発コマンド例を表示", null, ConsoleColor.DarkGray),
                    new MenuItem("0", "終了", null, ConsoleColor.DarkGray)
                });
                Ui.Prompt("番号を入力してください");

                string input = Console.ReadLine();
                input = input == null ? string.Empty : input.Trim();
                Console.WriteLine();

                if (input == "1") RunMenuAction("起動", delegate { return StartMinikube(false); });
                else if (input == "2") RunMenuAction("状態確認", delegate { return Status(); });
                else if (input == "3") RunMenuAction("kubeconfig 出力", delegate { return ExportKubeconfig(); });
                else if (input == "4") RunMenuAction("Hyper-V 有効化", delegate { return EnableHyperV(); });
                else if (input == "5") RunMenuAction("最新ログ", delegate { return ShowLatestLogs(); });
                else if (input == "6") RunMenuAction("パス設定", delegate { return ShowPaths(); });
                else if (input == "7") RunMenuAction("停止", delegate { return StopMinikube(); });
                else if (input == "8") RunMenuAction("削除", delegate { return DeleteMinikube(); });
                else if (input == "9") RunMenuAction("addon 一覧", delegate { return AddonsList(); });
                else if (input == "10") RunMenuAction("dashboard 有効化", delegate { return DashboardAddonEnable(); });
                else if (input == "11") RunMenuAction("dashboard URL", delegate { return DashboardUrl(); });
                else if (input == "12") RunMenuAction("ingress 有効化", delegate { return IngressAddonEnable(); });
                else if (input == "13") RunMenuAction("metrics-server 有効化", delegate { return MetricsServerAddonEnable(); });
                else if (input == "14") RunMenuAction("registry 有効化", delegate { return RegistryAddonEnable(); });
                else if (input == "15") RunMenuAction("storage 有効化", delegate { return StorageAddonsEnable(); });
                else if (input == "16") RunMenuAction("おすすめ addon 一括", delegate { return RecommendedAddonsEnable(); });
                else if (input == "17") RunMenuAction("イメージ一覧", delegate { return ImageList(); });
                else if (input == "18") RunMenuAction("イメージ読み込み", delegate { return ImageLoadInteractive(); });
                else if (input == "19") RunMenuAction("イメージビルド", delegate { return ImageBuildInteractive(); });
                else if (input == "20") RunMenuAction("イメージ削除", delegate { return ImageRemoveInteractive(); });
                else if (input == "21") RunMenuAction("ローカル開発コマンド例", delegate { return LocalDevelopmentHelp(); });
                else if (input == "22") RunMenuAction("詳細起動", delegate { return StartMinikube(true); });
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
                try { Log.Error(ex, "Menu action failed: {Title}", title); } catch { }
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

        private static int StartMinikube(bool verbose)
        {
            if (!CanRunElevatedJob(verbose ? "詳細起動" : "起動")) return 100;
            EnsureDirectories();

            Dictionary<string, string> env = MinikubeEnvironment();
            Log.Information(verbose ? "Start minikube with verbose logging." : "Start minikube.");

            string startArgs = " start --driver=" + Config.MinikubeDriver + " --profile=minikube";
            if (verbose) startArgs += " --alsologtostderr -v=2";

            int rc = RunCmd(Quote(Config.MinikubeExePath) + startArgs, env);
            Log.Information("minikube start exit code: {ExitCode}", rc);

            if (rc != 0)
            {
                Warn("minikube start が失敗したため、後続の status / kubectl get nodes / kubeconfig 出力は実行しません。");
                Warn("まず上の minikube のエラー内容を解消してから、再度 起動 を実行してください。");
                return rc;
            }

            int statusRc = Status();
            if (statusRc != 0)
            {
                Warn("minikube start 後の状態確認に失敗したため、kubeconfig 出力は実行しません。");
                return statusRc;
            }

            return ExportKubeconfig();
        }


        private static int StopMinikube()
        {
            if (!CanRunElevatedJob("停止")) return 100;
            EnsureDirectories();

            Console.WriteLine("minikube を停止します。");
            Log.Information("Stop minikube.");

            return RunCmd(Quote(Config.MinikubeExePath) + " stop --profile=minikube", MinikubeEnvironment());
        }

        private static int DeleteMinikube()
        {
            if (!CanRunElevatedJob("削除")) return 100;
            EnsureDirectories();

            Console.WriteLine("minikube クラスターを削除します。");
            Console.WriteLine("この操作を行うと、minikube の VM / クラスター状態は削除されます。");
            Console.Write("本当に削除しますか？ yes と入力した場合だけ実行します: ");

            string answer = Console.ReadLine();
            if (!string.Equals(answer == null ? string.Empty : answer.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("削除をキャンセルしました。");
                Log.Information("minikube delete canceled by user.");
                return 0;
            }

            Log.Information("Delete minikube.");
            int rc = RunCmd(Quote(Config.MinikubeExePath) + " delete --profile=minikube", MinikubeEnvironment());

            if (rc == 0)
            {
                DeleteExportedKubeconfigIfRequested();
            }

            return rc;
        }

        private static void DeleteExportedKubeconfigIfRequested()
        {
            if (!File.Exists(Config.ExportedKubeConfigPath))
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("出力済み kubeconfig が残っています:");
            Console.WriteLine("  " + Config.ExportedKubeConfigPath);
            Console.Write("この kubeconfig も削除しますか？ yes と入力した場合だけ削除します: ");

            string answer = Console.ReadLine();
            if (!string.Equals(answer == null ? string.Empty : answer.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("kubeconfig は残しました。");
                Log.Information("Exported kubeconfig was kept: {Path}", Config.ExportedKubeConfigPath);
                return;
            }

            try
            {
                File.Delete(Config.ExportedKubeConfigPath);
                Console.WriteLine("kubeconfig を削除しました:");
                Console.WriteLine("  " + Config.ExportedKubeConfigPath);
                Log.Information("Deleted exported kubeconfig: {Path}", Config.ExportedKubeConfigPath);
            }
            catch (Exception ex)
            {
                Warn("kubeconfig の削除に失敗しました: " + ex.Message);
                Log.Warning(ex, "Failed to delete exported kubeconfig: {Path}", Config.ExportedKubeConfigPath);
            }
        }

        private static int Status()
        {
            if (!CanRunElevatedJob("状態確認")) return 100;
            EnsureDirectories();

            Dictionary<string, string> env = MinikubeEnvironment();

            int rc1 = RunCmd(Quote(Config.MinikubeExePath) + " status --profile=minikube", env);
            if (rc1 != 0)
            {
                Warn("minikube status が失敗したため、kubectl get nodes は実行しません。");
                return rc1;
            }

            return RunCmd(Quote(Config.KubectlExePath) + " --kubeconfig " + Quote(Config.SystemKubeConfigPath) + " get nodes -o wide", env);
        }

        private static int ExportKubeconfig()
        {
            if (!CanRunElevatedJob("kubeconfig 出力")) return 100;
            EnsureDirectories();

            if (!File.Exists(Config.SystemKubeConfigPath))
            {
                Warn("SystemMinikubeHost 側の kubeconfig がまだありません:");
                Warn("  " + Config.SystemKubeConfigPath);
                Warn("先に minikube start を成功させてください。");
                return 3;
            }

            string command = Quote(Config.KubectlExePath)
                + " --kubeconfig " + Quote(Config.SystemKubeConfigPath)
                + " config view --minify --flatten --raw > "
                + Quote(Config.ExportedKubeConfigPath);

            int rc = RunCmd(command, MinikubeEnvironment());
            if (rc == 0)
            {
                Console.WriteLine();
                Ui.Success("kubeconfig を出力しました:");
                Console.WriteLine("  " + Config.ExportedKubeConfigPath);
            }
            return rc;
        }

        private static int EnableHyperV()
        {
            if (!CanRunElevatedJob("Hyper-V 有効化")) return 100;

            Console.WriteLine("Hyper-V 有効化コマンドを実行します。再起動が必要になる場合があります。");
            int rc1 = RunCmd(@"DISM /Online /Enable-Feature /All /FeatureName:Microsoft-Hyper-V /NoRestart", null);
            int rc2 = RunCmd(@"bcdedit /set hypervisorlaunchtype auto", null);
            Console.WriteLine("完了。必要に応じて Windows を再起動してください。");
            return rc1 != 0 ? rc1 : rc2;
        }

        /*
        // VirtualBox などを試すために Hyper-V を一時的に止めたい場合の退避用です。
        // メニューには接続していません。必要な場合だけコメントアウトを外して使ってください。
        private static int DisableHyperV()
        {
            if (!CanRunElevatedJob("Hyper-V 無効化")) return 100;

            Console.WriteLine("Hyper-V 無効化コマンドを実行します。再起動が必要になる場合があります。");
            int rc1 = RunCmd(@"DISM /Online /Disable-Feature /FeatureName:Microsoft-Hyper-V-All /NoRestart", null);
            int rc2 = RunCmd(@"bcdedit /set hypervisorlaunchtype off", null);
            Console.WriteLine("完了。必要に応じて Windows を再起動してください。");
            return rc1 != 0 ? rc1 : rc2;
        }
        */

        private static int AddonsList()
        {
            if (!CanRunElevatedJob("addon一覧")) return 100;
            EnsureDirectories();

            Console.WriteLine("minikube addon の一覧を表示します。");
            Log.Information("List minikube addons.");

            return RunCmd(Quote(Config.MinikubeExePath) + " addons list --profile=minikube", MinikubeEnvironment());
        }

        private static int DashboardAddonEnable()
        {
            return EnableAddon("dashboard", "dashboard addon有効化");
        }

        private static int IngressAddonEnable()
        {
            return EnableAddon("ingress", "ingress addon有効化");
        }

        private static int MetricsServerAddonEnable()
        {
            return EnableAddon("metrics-server", "metrics-server addon有効化");
        }

        private static int RegistryAddonEnable()
        {
            return EnableAddon("registry", "registry addon有効化");
        }

        private static int StorageAddonsEnable()
        {
            string[] addons = new string[] { "default-storageclass", "storage-provisioner" };
            return EnableAddonGroup(addons, "storage系 addon有効化");
        }

        private static int RecommendedAddonsEnable()
        {
            string[] addons = new string[] { "dashboard", "ingress", "metrics-server", "default-storageclass", "storage-provisioner" };
            return EnableAddonGroup(addons, "おすすめaddonまとめて有効化");
        }

        private static int EnableAddon(string addonName, string jobName)
        {
            if (!CanRunElevatedJob(jobName)) return 100;
            EnsureDirectories();

            Console.WriteLine(addonName + " addon を有効化します。");
            Log.Information("Enable minikube addon: {AddonName}", addonName);

            return RunCmd(Quote(Config.MinikubeExePath) + " addons enable " + addonName + " --profile=minikube", MinikubeEnvironment());
        }

        private static int EnableAddonGroup(string[] addonNames, string jobName)
        {
            if (!CanRunElevatedJob(jobName)) return 100;
            EnsureDirectories();

            Console.WriteLine(jobName + " を実行します。");
            Console.WriteLine("対象 addon: " + string.Join(", ", addonNames));
            Log.Information("Enable minikube addon group: {JobName}", jobName);

            int result = 0;
            for (int i = 0; i < addonNames.Length; i++)
            {
                string addonName = addonNames[i];
                Console.WriteLine();
                Console.WriteLine("--- " + addonName + " ---");
                int rc = RunCmd(Quote(Config.MinikubeExePath) + " addons enable " + addonName + " --profile=minikube", MinikubeEnvironment());
                if (rc != 0 && result == 0) result = rc;
            }

            Console.WriteLine();
            Console.WriteLine("addon 有効化処理が完了しました。現在の addon 状態を確認します。");
            AddonsList();
            return result;
        }

        private static int DashboardUrl()
        {
            if (!CanRunElevatedJob("dashboard URL表示")) return 100;
            EnsureDirectories();

            Console.WriteLine("dashboard の URL を表示します。");
            Console.WriteLine("注: minikube dashboard --url はプロキシを維持するため、終了しない場合があります。");
            Console.WriteLine("    終了したい場合は Ctrl+C を押してください。");
            Log.Information("Show minikube dashboard URL.");

            return RunCmd(Quote(Config.MinikubeExePath) + " dashboard --url --profile=minikube", MinikubeEnvironment());
        }

        private static int ImageList()
        {
            if (!CanRunElevatedJob("イメージ一覧")) return 100;
            EnsureDirectories();

            Console.WriteLine("minikube 内のイメージ一覧を表示します。");
            Log.Information("List minikube images.");

            return RunCmd(Quote(Config.MinikubeExePath) + " image ls --profile=minikube", MinikubeEnvironment());
        }

        private static int ImageLoadInteractive()
        {
            Console.WriteLine("minikube に読み込むイメージ名、または tar ファイルのフルパスを入力してください。");
            Console.WriteLine("例: sample-app:dev");
            Console.WriteLine("例: C:\\Users\\taro\\images\\sample-app-dev.tar");
            Console.Write("image or tar path: ");
            string value = Console.ReadLine();
            return ImageLoad(value);
        }

        private static int ImageLoadFromArgs(string[] args)
        {
            if (args == null || args.Length == 0) return ImageLoadInteractive();
            return ImageLoad(JoinArguments(args));
        }

        private static int ImageLoad(string imageOrTarPath)
        {
            if (!CanRunElevatedJob("イメージ読み込み")) return 100;
            EnsureDirectories();

            imageOrTarPath = imageOrTarPath == null ? string.Empty : imageOrTarPath.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(imageOrTarPath))
            {
                Warn("イメージ名または tar ファイルパスが空です。");
                return 2;
            }

            Console.WriteLine("minikube にイメージを読み込みます:");
            Console.WriteLine("  " + imageOrTarPath);
            Log.Information("Load image into minikube: {ImageOrTarPath}", imageOrTarPath);

            return RunCmd(Quote(Config.MinikubeExePath) + " image load " + QuoteIfNeeded(imageOrTarPath) + " --overwrite=true --profile=minikube", MinikubeEnvironment());
        }

        private static int ImageBuildInteractive()
        {
            Console.WriteLine("minikube 内でコンテナイメージをビルドします。");
            Console.WriteLine("例: sample-app:dev");
            Console.Write("image tag: ");
            string tag = Console.ReadLine();

            Console.WriteLine("Dockerfile があるフォルダのフルパスを入力してください。");
            Console.WriteLine("例: C:\\Users\\taro\\src\\sample-app");
            Console.Write("build path: ");
            string path = Console.ReadLine();

            return ImageBuild(tag, path);
        }

        private static int ImageBuildFromArgs(string[] args)
        {
            if (args == null || args.Length < 2) return ImageBuildInteractive();
            return ImageBuild(args[0], JoinArguments(args.Skip(1).ToArray()));
        }

        private static int ImageBuild(string imageTag, string buildPath)
        {
            if (!CanRunElevatedJob("イメージビルド")) return 100;
            EnsureDirectories();

            imageTag = imageTag == null ? string.Empty : imageTag.Trim().Trim('"');
            buildPath = buildPath == null ? string.Empty : buildPath.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(imageTag))
            {
                Warn("イメージタグが空です。");
                return 2;
            }
            if (string.IsNullOrWhiteSpace(buildPath))
            {
                Warn("ビルドパスが空です。");
                return 2;
            }

            Console.WriteLine("minikube 内でイメージをビルドします:");
            Console.WriteLine("  tag : " + imageTag);
            Console.WriteLine("  path: " + buildPath);
            Log.Information("Build image in minikube: {ImageTag} {BuildPath}", imageTag, buildPath);

            return RunCmd(Quote(Config.MinikubeExePath) + " image build -t " + QuoteIfNeeded(imageTag) + " " + QuoteIfNeeded(buildPath) + " --profile=minikube", MinikubeEnvironment());
        }

        private static int ImageRemoveInteractive()
        {
            Console.WriteLine("minikube から削除するイメージ名を入力してください。");
            Console.WriteLine("例: sample-app:dev");
            Console.Write("image: ");
            string value = Console.ReadLine();
            return ImageRemove(value);
        }

        private static int ImageRemoveFromArgs(string[] args)
        {
            if (args == null || args.Length == 0) return ImageRemoveInteractive();
            return ImageRemove(JoinArguments(args));
        }

        private static int ImageRemove(string imageName)
        {
            if (!CanRunElevatedJob("イメージ削除")) return 100;
            EnsureDirectories();

            imageName = imageName == null ? string.Empty : imageName.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(imageName))
            {
                Warn("イメージ名が空です。");
                return 2;
            }

            Console.WriteLine("minikube からイメージを削除します:");
            Console.WriteLine("  " + imageName);
            Log.Information("Remove image from minikube: {ImageName}", imageName);

            return RunCmd(Quote(Config.MinikubeExePath) + " image rm " + QuoteIfNeeded(imageName) + " --profile=minikube", MinikubeEnvironment());
        }

        private static int LocalDevelopmentHelp()
        {
            Console.WriteLine("ローカル開発の最小フロー例");
            Console.WriteLine("------------------------");
            Console.WriteLine("1. SystemMinikubeHost 側でイメージを用意します。");
            Console.WriteLine("   minikube image build -t sample-app:dev C:\\Users\\taro\\src\\sample-app");
            Console.WriteLine("   または、既存イメージ/tarを読み込みます。");
            Console.WriteLine("   minikube image load sample-app:dev --overwrite=true");
            Console.WriteLine("   minikube image load C:\\Users\\taro\\images\\sample-app-dev.tar.gz --overwrite=true");
            Console.WriteLine();
            Console.WriteLine("2. UserKubeClient 側で YAML を apply します。");
            Console.WriteLine("   kubectl apply -f deployment.yaml");
            Console.WriteLine();
            Console.WriteLine("3. YAML の imagePullPolicy はローカルイメージ向けに設定します。");
            Console.WriteLine("   image: sample-app:dev");
            Console.WriteLine("   imagePullPolicy: Never");
            Console.WriteLine();
            Console.WriteLine("4. 確認します。");
            Console.WriteLine("   kubectl get pods -A");
            Console.WriteLine("   kubectl logs -n default <pod-name>");
            Console.WriteLine("   kubectl describe pod -n default <pod-name>");
            return 0;
        }

        private static int ShowPaths()
        {
            Console.WriteLine("現在の明示パス設定");
            Console.WriteLine("--------------------");
            Console.WriteLine("WorkDirectory        : " + Config.WorkDirectory);
            Console.WriteLine("MINIKUBE_HOME        : " + Config.MinikubeHomeDirectory);
            Console.WriteLine("System KUBECONFIG    : " + Config.SystemKubeConfigPath);
            Console.WriteLine("LogsDirectory        : " + Config.LogsDirectory);
            Console.WriteLine("Exported kubeconfig  : " + Config.ExportedKubeConfigPath);
            Console.WriteLine("minikube.exe         : " + Config.MinikubeExePath);
            Console.WriteLine("kubectl.exe          : " + Config.KubectlExePath);
            Console.WriteLine("MinikubeDriver       : " + Config.MinikubeDriver);
            Console.WriteLine("LogRetentionDays     : " + Config.LogRetentionDays);
            return 0;
        }

        private static int ShowLatestLogs()
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
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine("  " + files[i].FullName);
            }

            Console.WriteLine();
            Console.WriteLine("最新ログの末尾:");
            PrintTail(files[0].FullName, 80);
            return 0;
        }

        private static int Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  SystemMinikubeHost.exe");
            Console.WriteLine("  SystemMinikubeHost.exe start");
            Console.WriteLine("  SystemMinikubeHost.exe start-verbose");
            Console.WriteLine("  SystemMinikubeHost.exe stop");
            Console.WriteLine("  SystemMinikubeHost.exe delete");
            Console.WriteLine("  SystemMinikubeHost.exe status");
            Console.WriteLine("  SystemMinikubeHost.exe export-kubeconfig");
            Console.WriteLine("  SystemMinikubeHost.exe enable-hyperv");
            Console.WriteLine("  SystemMinikubeHost.exe addons-list");
            Console.WriteLine("  SystemMinikubeHost.exe dashboard-enable");
            Console.WriteLine("  SystemMinikubeHost.exe ingress-enable");
            Console.WriteLine("  SystemMinikubeHost.exe metrics-server-enable");
            Console.WriteLine("  SystemMinikubeHost.exe registry-enable");
            Console.WriteLine("  SystemMinikubeHost.exe storage-enable");
            Console.WriteLine("  SystemMinikubeHost.exe addons-enable-recommended");
            Console.WriteLine("  SystemMinikubeHost.exe dashboard-url");
            Console.WriteLine("  SystemMinikubeHost.exe image-ls");
            Console.WriteLine("  SystemMinikubeHost.exe image-load sample-app:dev");
            Console.WriteLine("  SystemMinikubeHost.exe image-load C:\\Users\\taro\\images\\sample-app-dev.tar");
            Console.WriteLine("  SystemMinikubeHost.exe image-build sample-app:dev C:\\Users\\taro\\src\\sample-app");
            Console.WriteLine("  SystemMinikubeHost.exe image-rm sample-app:dev");
            Console.WriteLine("  SystemMinikubeHost.exe dev-help");
            Console.WriteLine("  SystemMinikubeHost.exe logs");
            Console.WriteLine("  SystemMinikubeHost.exe paths");
            return 2;
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(Config.WorkDirectory);
            Directory.CreateDirectory(Config.MinikubeHomeDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(Config.SystemKubeConfigPath));
            Directory.CreateDirectory(Config.LogsDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(Config.ExportedKubeConfigPath));
        }

        private static Dictionary<string, string> MinikubeEnvironment()
        {
            Dictionary<string, string> env = new Dictionary<string, string>();
            env["MINIKUBE_HOME"] = Config.MinikubeHomeDirectory;
            env["KUBECONFIG"] = Config.SystemKubeConfigPath;
            return env;
        }

        private static int RunCmd(string command, Dictionary<string, string> env)
        {
            Ui.Command(command);
            Log.Information("cmd /d /s /c chcp 65001 > nul & {Command}", command);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = BuildCmdArguments(command);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.CreateNoWindow = true;

            if (env != null)
            {
                foreach (KeyValuePair<string, string> pair in env)
                {
                    psi.EnvironmentVariables[pair.Key] = pair.Value;
                }
            }

            using (Process p = new Process())
            {
                p.StartInfo = psi;
                p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data == null) return;
                    Console.WriteLine(e.Data);
                    try { Log.Information("stdout: {Line}", e.Data); } catch { }
                };
                p.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data == null) return;
                    Console.Error.WriteLine(e.Data);
                    try { Log.Warning("stderr: {Line}", e.Data); } catch { }
                };

                if (!p.Start()) throw new InvalidOperationException("cmd.exe を開始できません。");

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                p.WaitForExit();

                Ui.ExitCode(p.ExitCode);
                Log.Information("exit code: {ExitCode}", p.ExitCode);
                return p.ExitCode;
            }
        }

        private static string BuildCmdArguments(string command)
        {
            return "/d /s /c \"chcp 65001 > nul & " + command + "\"";
        }

        private static void PrintTail(string path, int maxLines)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            int start = Math.Max(0, lines.Length - maxLines);
            for (int i = start; i < lines.Length; i++) Console.WriteLine(lines[i]);
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

        private static void Warn(string message)
        {
            Ui.Warn(message);
            try { Log.Warning(message); } catch { }
        }
    }
}
