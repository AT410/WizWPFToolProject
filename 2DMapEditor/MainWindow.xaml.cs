using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _2DMapEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Dirty = false;
        private string FileName = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "*.*";
            if (dialog.ShowDialog() == true)
            {

            }
        }

        private void SaveFile_Click(object sender,RoutedEventArgs e)
        {
            if(FileName == "")
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = "MapFile";
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*"; ;
                if(dialog.ShowDialog() == true)
                {
                    string FileName = dialog.FileName;
                    SaveFiles(FileName);
                }
            }
            else
            {
                SaveFiles(FileName);
            }
        }

        private void SaveFiles(string FullPath)
        {
            if (!Dirty)//変更がなければ戻る
                return;

            //ファイルを保存する
        }
    }
}
