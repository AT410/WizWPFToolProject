using Microsoft.Win32;
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
using System.IO;

namespace _2DMapEditor
{
    /// <summary>
    /// MapTipEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class MapTipEditor : Window
    {
        MapTip test;
        public MapTipEditor(MapTip mapTip)
        {
            InitializeComponent();
            this.Closed += MapTipEditor_Closed;
            test = mapTip;
            TipView.Background = test.Background;
        }

        private void MapTipEditor_Closed(object sender, EventArgs e)
        {
            test.Background = TipView.Background;
        }

        private void RelativePathBT_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "png files (*.xml)|*.png|All files (*.*)|*.*";
            dialog.Title = "画像ファイルを開く";
            dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory() + "\\media";
            if (dialog.ShowDialog()==true)
            {
                string picFullPath = dialog.FileName;
                // -- ファイルがあるか --
                string picFileName = System.IO.Path.GetFileName(picFullPath);
                string mediaPath = System.IO.Directory.GetCurrentDirectory() + "\\media\\";
                string newPath = mediaPath + picFileName;
                if(!File.Exists(newPath))
                {
                    // -- メディアファイルにコピー --
                    File.Copy(picFullPath, newPath);
                }

                // -- 絶対パスを相対パスへ --
                var CurrntUri = new Uri(Directory.GetCurrentDirectory()+"\\");
                var picUri = new Uri(newPath);
                var reUri = CurrntUri.MakeRelativeUri(picUri);

                ImageBox.Text = reUri.ToString();
                TipView.Background = new ImageBrush(
                        new BitmapImage(new Uri(newPath)));
                test.TipFileName = ImageBox.Text;
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
