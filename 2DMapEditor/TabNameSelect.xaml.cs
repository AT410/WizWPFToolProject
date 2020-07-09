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
    /// TabNameSelect.xaml の相互作用ロジック
    /// </summary>
    public partial class TabNameSelect : Window
    {
        public TabNameSelect()
        {
            InitializeComponent();

            this.Loaded += TabNameSelect_Loaded;
        }

        private void TabNameSelect_Loaded(object sender, RoutedEventArgs e)
        {
            this.TabNameBox.Focus();
            this.TabNameBox.SelectAll();

            TabNameBox.KeyDown += TabCreate_KeyDown;
            TabNameBox.MouseDoubleClick += TabNameBox_MouseDoubleClick;
        }

        private void TabNameBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.TabNameBox.SelectAll();
        }

        private void TabCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateTabItem();
        }

        private void TabCreate_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                CreateTabItem();
            }
        }

        private void CreateTabItem()
        {
            var owner = this.Owner as MainWindow;
            if (owner != null)
            {
                var Tabcont = owner.LayerTab;
                foreach (var tab in Tabcont.Items.OfType<OriTab>())
                {
                    if (tab.Header.ToString() == TabNameBox.Text.ToString())
                    {
                        MessageBox.Show("同名Mapが存在しています。");
                        return;
                    }
                }

                owner.InitEditGrid(TabNameBox.Text.ToString());
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
