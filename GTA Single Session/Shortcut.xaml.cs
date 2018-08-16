using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GTA_Single_Session {

    public partial class Shortcut : Window {

        string first_key_uint;
        string second_key_uint;

        public Shortcut() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.KeyDown += new KeyEventHandler(firstBox_KeyDown);
            this.KeyDown += new KeyEventHandler(secondBox_KeyDown);
        }

        void firstBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                firstBox.Text = e.Key.ToString().Replace("Left","").Replace("Right","");
                first_key_uint = "0x0002";
                secondBox.Focus();
            } else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt) {
                firstBox.Text = e.Key.ToString().Replace("Left", "").Replace("Right", "");
                first_key_uint = "0x0001";
                secondBox.Focus();
            } else if (e.Key == Key.LeftShift || e.Key == Key.RightShift) {
                firstBox.Text = e.Key.ToString().Replace("Left", "").Replace("Right", "");
                first_key_uint = "0x0004";
                secondBox.Focus();
            }
        }

        void secondBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl && e.Key != Key.LeftAlt && e.Key != Key.RightAlt && e.Key != Key.LeftShift && e.Key != Key.RightShift && e.Key != Key.System) {
                if (KeyInterop.VirtualKeyFromKey(e.Key).ToString().Length > 1) {
                    second_key_uint = "0x" + KeyInterop.VirtualKeyFromKey(e.Key).ToString("X");
                    secondBox.Text = e.Key.ToString();
                } else {
                    second_key_uint = "0x0" + KeyInterop.VirtualKeyFromKey(e.Key).ToString("X");
                    secondBox.Text = e.Key.ToString();
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string key = first_key_uint + "\r\n" + second_key_uint;
            
            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(path, "settings"))) {
                outputFile.WriteLine(key);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
