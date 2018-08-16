using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GTA_Single_Session {

    public partial class MainWindow : Window {

        int status;
        uint KEY1;
        uint KEY2;

        public MainWindow() {
            InitializeComponent();
            bar.Visibility = Visibility.Hidden;
            HideText();


            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            label1.Content = "v." + version;

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string settings = Path.Combine(path, "settings");


            if (!File.Exists(settings)) {
                string key = "0x0002" + "\r\n" + "0x73";

                using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(path, "settings"))) {
                    outputFile.WriteLine(key);
                }

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            } else {
                string key1 = File.ReadLines(@settings).Skip(0).FirstOrDefault();
                string key2 = File.ReadLines(@settings).Skip(1).FirstOrDefault();

                KEY1 = Convert.ToUInt16(key1, 16);
                KEY2 = Convert.ToUInt16(key2, 16);
            }

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(250);
            dispatcherTimer.Start();

            check();

            void dispatcherTimer_Tick(object sender, EventArgs e) {
                check();

                CommandManager.InvalidateRequerySuggested();
            }

            void check() {
                Brush green = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                Brush red = new SolidColorBrush(Color.FromRgb(206, 58, 47));
                Brush black = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                Process[] p = Process.GetProcessesByName("GTA5");
                if (p.Length > 0) {
                    if (status == 1) {
                        bar.Visibility = Visibility.Visible;
                        bar.IsIndeterminate = true;
                        label.Foreground = black;
                        label.Content = "Wait a moment...";
                        button.IsEnabled = false;
                        button1.IsEnabled = false;
                    } else if (status == 2) {
                        bar.Visibility = Visibility.Hidden;
                        label.Foreground = green;
                        label.Content = "Done. You can play now";
                        button.IsEnabled = false;
                        button1.IsEnabled = true;
                    } else {
                        label.Foreground = green;
                        label.Content = "GTA is running";
                        button.IsEnabled = true;
                    }
                } else {
                    label.Foreground = red;
                    label.Content = "GTA isn't running";
                    button.IsEnabled = false;
                }
            }

        }

        [Flags]
        public enum ThreadAccess : int {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint keys);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int KILL = 1;

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e) {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey() {
            var helper = new WindowInteropHelper(this);
            if (!RegisterHotKey(helper.Handle, KILL, KEY1, KEY2)) {
                UnregisterHotKey();
            }
        }

        private void UnregisterHotKey() {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, KILL);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int WM_HOTKEY = 0x0312;
            switch (msg) {
                case WM_HOTKEY:
                    switch (wParam.ToInt32()) {
                        case KILL:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed() {
            Process[] p = Process.GetProcessesByName("GTA5");
            for (int i = 0; i < p.Length; i++) {
                Process.GetProcessById(p[i].Id).Kill();
            }
        }


        private static void SuspendProcess(int pid) {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads) {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero) {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid) {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads) {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero) {
                    continue;
                }

                var suspendCount = 0;
                do {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }


        private void button_Click(object sender, RoutedEventArgs e) {
            Process[] p = Process.GetProcessesByName("GTA5");
            Brush black = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            status = 1;
            List<int> processes = new List<int>();
            for (int i = 0; i < p.Length; i++) {
                processes.Add(p[i].Id);
                Brush green = new SolidColorBrush(Color.FromRgb(76, 175, 80));

                if (i == p.Length - 1) {
                    int[] array = processes.ToArray();
                    for (int j = 0; j < array.Length; j++) {
                        SuspendProcess(p[j].Id);
                    }
                    Task.Factory.StartNew(() => {
                        Task.Delay(10000).ContinueWith(_ => {
                            for (int j = 0; j < array.Length; j++) {
                                ResumeProcess(p[j].Id);
                                status = 2;
                            }
                        });
                    });
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void source_click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/PawelPleskaczynski/GTA_Single_Session");
        }

        private void help_click (object sender, RoutedEventArgs e) {
            Help help = new Help();
            help.Show();
        }

        public async void HideText() {
            await Task.Delay(10000);
            label2.Visibility = Visibility.Hidden;
        }

        private void settings_Click(object sender, RoutedEventArgs e) {
            Shortcut shortcut = new Shortcut();
            shortcut.Show();
            this.Close();
        }
    }
}
