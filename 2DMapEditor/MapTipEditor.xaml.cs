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
            if(dialog.ShowDialog()==true)
            {
                ImageBox.Text = dialog.FileName;
                TipView.Background = new ImageBrush(
                        new BitmapImage(new Uri(ImageBox.Text)));
                test.TipFileName = ImageBox.Text;
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
