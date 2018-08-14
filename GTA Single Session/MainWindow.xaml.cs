using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GTA_Single_Session {

    public partial class MainWindow : Window {
        
        // 1 = wait a moment
        // 2 = done

        int status = 0;

        public MainWindow() {
            InitializeComponent();
            bar.Visibility = Visibility.Hidden;

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
    }
}
