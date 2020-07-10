using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace _2DMapEditor
{
    public class MapSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    /// <summary>
    /// StartDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class StartDialog : Window
    {
        private ObservableCollection<MapSet> _MapList = new ObservableCollection<MapSet>();

        public StartDialog(bool StartActive)
        {
            InitializeComponent();

            _MapList.Add(new MapSet { Id = 1, Name = "少", Row = 12, Column = 12 });
            _MapList.Add(new MapSet { Id = 2, Name = "中", Row = 24, Column = 24 });
            _MapList.Add(new MapSet { Id = 3, Name = "大", Row = 36, Column = 36 });

            MyComboBox.ItemsSource = _MapList;

            this.Loaded += StartDialog_Loaded;

            if (!StartActive)
            {
                CreateGrid.SetValue(Grid.ColumnSpanProperty, 2);
                OpenGrid.Visibility = Visibility.Collapsed;
                this.Title = "新規作成";
            }
        }

        private void StartDialog_Loaded(object sender, RoutedEventArgs e)
        {
            MyComboBox.SelectedIndex = 0;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var item = MyComboBox.SelectedItem as MapSet;
            if(item != null)
            {
                var owner = (MainWindow)Owner;
                owner.SetRow(item.Row);
                owner.SetColumn(item.Column);
                owner.ResetEdit();
                owner.InitEditGrid("Map1");
                
                this.DialogResult = true;
                this.Close();
            }
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "プロジェクトデータ|*.m2d";
            dialog.InitialDirectory = Directory.GetCurrentDirectory()+"\\Projects";
            if (dialog.ShowDialog() == true)
            {
                string FilePath = dialog.FileName;

                var obj = DataIO.BinaryRead(FilePath);

                var setting = obj as EditorSetting;
                if (setting != null)
                {
                    var main = this.Owner as MainWindow;
                    main.FileName = FilePath;
                    main.AttachToSetting(setting);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("非対応のファイルです。");
                }
            }
        }
    }
}
