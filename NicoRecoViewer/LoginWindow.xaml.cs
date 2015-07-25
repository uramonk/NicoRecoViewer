using System;
using System.Collections.Generic;
using System.Linq;
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

namespace NicoRecoViewer
{
    /// <summary>
    /// LoginWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public string Id { get; private set; }
        public string Password { get; private set; }
        public bool IsExited { get; private set; }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null)
            {
                return;
            }

            if (tb.Foreground == Brushes.Gray)
            {
                tb.Foreground = Brushes.Black;
                tb.Text = "";
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }

            if (pb.Foreground == Brushes.Gray)
            {
                pb.Foreground = Brushes.Black;
                pb.Password = "";
            }
        }
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Id = userText.Text;
            Password = passText.Password;
            this.Close();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            IsExited = true;
            this.Close();
        }
    }
}
