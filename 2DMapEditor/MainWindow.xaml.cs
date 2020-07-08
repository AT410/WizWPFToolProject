using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace _2DMapEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Dirty = false;
        private string FileName = "";

        private int RowCount;
        private int ColumnCount;

        private MapTip tip;

        private Dictionary<string, UniformGrid> GridDic;

        private OriTab ActiveTab;

        private List<DependencyObject> hitTestResults;

        public void SetRow(int Row) { RowCount = Row > 0 ? Row : 1; }
        public void SetColumn(int Col) { ColumnCount = Col > 0 ? Col : 1; }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += StartEvent;

            MapTipGrid.Children.Clear();
            MapTipGrid.Rows = 2;
            MapTipGrid.Columns = 6;
            MapTipGrid.HorizontalAlignment = HorizontalAlignment.Center;
            tip = new MapTip();
            LayerTab.Items.Clear();
            LayerTab.SelectionChanged += LayerTab_SelectionChanged;
            //GridTest();
            GridDic = new Dictionary<string, UniformGrid>();
            GridDic.Clear();
            ActiveTab = new OriTab();

            hitTestResults = new List<DependencyObject>();
        }

        private void LayerTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //有効になったタブを参照する
            var test1 = (TabControl)sender;
            var test = (OriTab)test1.Items[test1.SelectedIndex];
            if(test !=null)
            {
                ActiveTab = test;
            }
        }

        private void GridTest()
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Background = Brushes.WhiteSmoke;

            UniformGrid uniform = new UniformGrid();
            uniform.Name = "EditFormGrid";
            uniform.HorizontalAlignment = HorizontalAlignment.Left;
            uniform.VerticalAlignment = VerticalAlignment.Top;

            scrollViewer.Content = uniform;

            MapTipGrid.Children.Add(scrollViewer);
        }

        private void CreateGrid(string Header)
        {
            UniformGrid uniform = new UniformGrid();
            uniform.HorizontalAlignment = HorizontalAlignment.Left;
            uniform.VerticalAlignment = VerticalAlignment.Top;
            uniform.Background = Brushes.Transparent;

            Grid grid = new Grid();
            grid.Children.Add(uniform);
            grid.MouseDown += TestGrid_MouseLeftButtonDown;

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Background = Brushes.WhiteSmoke;


            scrollViewer.Content = grid;

            //grid.AddHandler(Label.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(RoutedTest));

            OriTab tab = new OriTab();
            tab.Header = Header;
            tab.Content = scrollViewer;

            LayerTab.Items.Add(tab);

            GridDic.Add(Header, uniform);
        }

        private void RoutedTest(object sender,RoutedEventArgs e)
        {
            var sc = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)sc.Content;

            var mip1 = (EditMT)e.Source;
            CellIndex pos = mip1.Cell;
            MessageBox.Show(string.Format("MouseDown: {0}, {1} ({2})\n Layer:{3}",
                              pos.X, pos.Y, e.Source.GetType().Name,mip1.GetLayer()));
        }

        private void TestGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var scroll = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)scroll.Content;


            hitTestResults.Clear();
            Point pt = e.GetPosition((UIElement)sender);
            VisualTreeHelper.HitTest(grid, null,new HitTestResultCallback(MyHitTestResult),new PointHitTestParameters(pt));

            UniformGrid uniform = new UniformGrid();

            if(hitTestResults.Count>0)
            {
                string types = "";
                foreach (var obj in hitTestResults)
                {
                    var name = obj.GetType();

                    types += name + "\n";

                    try
                    {
                        var bt = (UniformGrid)obj;

                        if (bt != null)
                        {
                            //types += string.Format("X:{0},Y{1}", bt.Cell.X, bt.Cell.Y);
                            types += "SET"+bt.Children.Count;
                            uniform = bt;
                            var ss = uniform.InputHitTest(pt);
                            MessageBox.Show(ss.GetType().Name);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                MessageBox.Show(string.Format("Count:{0}\n{1}", hitTestResults.Count,types));
            }

            hitTestResults.Clear();
            VisualTreeHelper.HitTest(uniform, null, new HitTestResultCallback(MyHitTestResult), new PointHitTestParameters(pt));

            if (hitTestResults.Count > 0)
            {
                string types = "";
                foreach (var obj in hitTestResults)
                {
                    var name = obj.GetType();

                    types += name + "\n";
                    
                    try
                    {
                        var bt = (Button)obj;

                        if (bt != null)
                        {
                            //types += string.Format("X:{0},Y{1}", bt.Cell.X, bt.Cell.Y);
                            types += "SET";
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                MessageBox.Show(string.Format("Count:{0}\n{1}", hitTestResults.Count, types));
            }

        }

        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            var Box = result.VisualHit as UIElement;
            if(Box !=null)
            {
                MessageBox.Show("AS"+ result.VisualHit.GetType());
                var Bt = Box as EditMT;
                if(Bt != null)
                {
                    MessageBox.Show("LOK");
                }
            }

            // Add the hit test result to the list that will be processed after the enumeration.
            hitTestResults.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        private void StartEvent(object sender, EventArgs e)
        {
            StartDialog sd = new StartDialog();
            sd.Owner = this;
            sd.Show();
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
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
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

        public void InitEditGrid()
        {
            CreateGrid("Map1");

            var grid = GridDic.Where(x => x.Key == "Map1").Select(x=>x.Value).First();
            grid.Children.Clear();
            grid.Rows = RowCount;
            grid.Columns = ColumnCount;

            for (int row = 0; row < RowCount;row++)
            {
                for(int Col = 0;Col <ColumnCount;Col++)
                {
                    EditMT bt = new EditMT(0);
                    bt.Width = 35;
                    bt.Height = 35;
                    bt.Cell = new CellIndex(row, Col);
                    bt.Background = Brushes.White;
                    bt.Click += DrawTip;
                    grid.Children.Add(bt);
                }
            }
            ActiveLayer.Content = "0";
        }

        private void TestEvet(object sender, RoutedEventArgs e)
        {
            var bt = (EditMT)sender;
            MessageBox.Show("X:" + bt.Cell.X.ToString() + "Y:" + bt.Cell.Y.ToString());
        }
        private void DrawTip(object sender, RoutedEventArgs e)
        {
            var layer = int.Parse(ActiveLayer.Content.ToString());
            var mt = (EditMT)sender;

            CellIndex cell = mt.Cell;

            uint ActiveLay = uint.Parse(ActiveLayer.Content.ToString());

            var scroll = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)scroll.Content;

            foreach (var uniform in grid.Children.OfType<UniformGrid>())
            {
                foreach (var Bt in uniform.Children.OfType<EditMT>())
                {
                    uint bl = Bt.GetLayer();
                    if(ActiveLay==bl)
                    {
                       if(Bt.Cell.Y == cell.Y)
                       {
                            if(Bt.Cell.X ==cell.X)
                            {
                                Bt.Background = tip.Background;
                                Bt.TipFileName = tip.TipFileName;
                                return;
                            }
                        }
                        continue;
                    }
                    break;
                }
            }
        }
        private void CreateMapTip_Click(object sender, RoutedEventArgs e)
        {
            MapTip mt = new MapTip();
            mt.Width = 35;
            mt.Height = 35;
            mt.MouseDoubleClick += Mt_MouseDoubleClick;
            mt.Click += Mt_Click;
            MapTipGrid.Children.Add(mt);
        }

        private void Mt_Click(object sender, RoutedEventArgs e)
        {
            tip = (MapTip)sender;
        }

        private void Mt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MapTipEditor mapTipEditor = new MapTipEditor((MapTip)sender);

            mapTipEditor.Owner = this;
            mapTipEditor.Show();
        }

        private void SaveFiles(string FullPath)
        {
            if (!Dirty)//変更がなければ戻る
                return;

            //ファイルを保存する
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CreateGrid("2");

            var grid = GridDic.Where(x => x.Key == "2").Select(x => x.Value).First();
            grid.Children.Clear();
            grid.Rows = RowCount;
            grid.Columns = ColumnCount;

            Color color = new Color();
            color = Color.FromArgb(1,1,1,0);

            var sb = new SolidColorBrush(color);
            sb.Opacity = color.A / 255.0f;

            for (int row = 0; row < RowCount; row++)
            {
                for (int Col = 0; Col < ColumnCount; Col++)
                {
                    EditMT bt = new EditMT(0);
                    bt.Width = 35;
                    bt.Height = 35;
                    bt.Cell = new CellIndex(row, Col);
                    bt.Background = sb;
                    bt.BorderBrush = Brushes.Black;
                    grid.Children.Add(bt);
                }
            }

        }

        private void CleateLayer_Click(object sender, RoutedEventArgs e)
        {
            var Sc = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)Sc.Content;
            if(grid != null)
            {
                UniformGrid uniform = new UniformGrid();
                uniform.HorizontalAlignment = HorizontalAlignment.Left;
                uniform.VerticalAlignment = VerticalAlignment.Top;
                uniform.Background = Brushes.Transparent;

                uniform.Children.Clear();
                uniform.Rows = RowCount;
                uniform.Columns = ColumnCount;

                Color color = new Color();
                color = Color.FromArgb(1, 1, 1, 0);

                var sb = new SolidColorBrush(color);
                sb.Opacity = 0.0f;

                for (int row = 0; row < RowCount; row++)
                {
                    for (int Col = 0; Col < ColumnCount; Col++)
                    {
                        EditMT bt = new EditMT(ActiveTab.GetLayerCount());
                        bt.Width = 35;
                        bt.Height = 35;
                        bt.Cell = new CellIndex(row, Col);
                        bt.Background = Brushes.Transparent;
                        bt.BorderBrush = Brushes.Black;
                        bt.Click += DrawTip;
                        uniform.Children.Add(bt);
                    }
                }

                grid.Children.Add(uniform);

                ActiveTab.AddLayer();
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            var str = ActiveLayer.Content;
            uint layer = uint.Parse(str.ToString());

            if(layer>0)
            {
                layer--;
                ActiveLayer.Content = layer.ToString();
            }
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            var str = ActiveLayer.Content;
            uint layer = uint.Parse(str.ToString());

            if (layer < ActiveTab.GetLayerCount()-1)
            {
                layer++;
                ActiveLayer.Content = layer.ToString();
            }

        }
    }
}
