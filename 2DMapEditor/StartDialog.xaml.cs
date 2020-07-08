using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public StartDialog()
        {
            InitializeComponent();

            _MapList.Add(new MapSet { Id = 1, Name = "少", Row = 12, Column = 12 });
            _MapList.Add(new MapSet { Id = 2, Name = "中", Row = 24, Column = 24 });
            _MapList.Add(new MapSet { Id = 3, Name = "大", Row = 36, Column = 36 });

            MyComboBox.ItemsSource = _MapList;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var item = MyComboBox.SelectedItem as MapSet;
            if(item != null)
            {
                var owner = (MainWindow)Owner;
                owner.SetRow(item.Row);
                owner.SetColumn(item.Column);
                owner.InitEditGrid();

                this.Close();
            }
        }
    }
}
