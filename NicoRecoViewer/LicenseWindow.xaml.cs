using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace NicoRecoViewer
{
    /// <summary>
    /// LicenseWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();

            Uri fileUri = new Uri("/Resources/license.txt", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(fileUri);
            using (StreamReader sr = new StreamReader(info.Stream))
            {
                textBlock.Text = sr.ReadToEnd();
            }
        }
    }
}
