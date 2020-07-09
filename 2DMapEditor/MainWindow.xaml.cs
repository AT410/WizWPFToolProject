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

            ActiveTab = new OriTab();

            hitTestResults = new List<DependencyObject>();
        }

        //----------------------------------------------------------------------------
        //XAMLクリックイベント
        //----------------------------------------------------------------------------
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "*.*";
            if (dialog.ShowDialog() == true)
            {

            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            EditorSetting setting = new EditorSetting();

            foreach(var tab in LayerTab.Items.OfType<OriTab>())
            {
                TabSetting tabSetting = new TabSetting();
                tabSetting.Header = tab.Header.ToString();
                tabSetting.LayerCount = tab.GetLayerCount();

                var scroll = tab.Content as ScrollViewer;
                if (scroll == null)
                    return;

                var grid = scroll.Content as Grid;
                if (grid == null)
                    return;

                bool setSize = false;

                foreach (var uniform in grid.Children.OfType<UniformGrid>())
                {
                    if (!setSize)
                    {
                        tabSetting.GridRows = uniform.Rows;
                        tabSetting.GridColumns = uniform.Columns;
                        setSize = true;
                    }

                    LayerSetting layerSetting= new LayerSetting();

                    bool setlayer = false;

                    foreach (var editcell in uniform.Children.OfType<EditMT>())
                    {
                        if (!setlayer)
                        {
                            layerSetting.Layer = editcell.GetLayer();
                            setlayer = true;
                        }

                        CellSetting cell = new CellSetting();
                        cell.X = editcell.Cell.X;
                        cell.Y = editcell.Cell.Y;
                        cell.MapTipIndex = editcell.index;

                        layerSetting.cellmap.Add(cell);
                    }

                    tabSetting.Layers.Add(layerSetting);
                }

                setting.tabSettings.Add(tabSetting);
            }

            foreach(var MapTip in MapTipGrid.Children.OfType<MapTip>())
            {
                MapTipSetting Eset = new MapTipSetting();
                Eset.index = MapTip.index;
                Eset.TipFileName = MapTip.TipFileName;
                setting.tips.Add(Eset);
            }

            TestS test = new TestS();

            DataIO.XmlSerialize("test.xml", setting);
            DataIO.SaveTOBinary(setting, "test.bmf");


            var obj = DataIO.BinaryRead("test.bmf");

            var set = obj as EditorSetting;
            if (set != null)
            {

            }

            if (FileName == "")
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = "MapFile";
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                if (dialog.ShowDialog() == true)
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

        private void CreateMapTip_Click(object sender, RoutedEventArgs e)
        {
            MapTip mt = new MapTip();
            mt.Width = 35;
            mt.Height = 35;
            mt.MouseDoubleClick += Mt_MouseDoubleClick; //<-マップチップエディタを開く
            mt.Click += Mt_Click;
            mt.index = (uint)MapTipGrid.Children.Count;
            MapTipGrid.Children.Add(mt);
        }

        private void CreateMap_Click(object sender, RoutedEventArgs e)
        {
            // -- ファイル名を指定する --
            var TabWindow = new TabNameSelect();
            TabWindow.Owner = this;

            if (TabWindow.ShowDialog() == false)
            {
                this.Close();
            }
        }

        private void CleateLayer_Click(object sender, RoutedEventArgs e)
        {
            var Sc = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)Sc.Content;
            if (grid != null)
            {
                UniformGrid uniform = new UniformGrid();
                uniform.HorizontalAlignment = HorizontalAlignment.Left;
                uniform.VerticalAlignment = VerticalAlignment.Top;
                uniform.Background = Brushes.Transparent;

                grid.Children.Add(uniform);

                CreateEditTip(uniform, ActiveTab.GetLayerCount());

                ActiveTab.AddLayer();
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            var str = ActiveLayer.Content;
            uint layer = uint.Parse(str.ToString());

            if (layer > 0)
            {
                layer--;
                ActiveLayer.Content = layer.ToString();
            }
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            var str = ActiveLayer.Content;
            uint layer = uint.Parse(str.ToString());

            if (layer < ActiveTab.GetLayerCount() - 1)
            {
                layer++;
                ActiveLayer.Content = layer.ToString();
            }

        }

        //----------------------------------------------------------------------------
        //動的追加イベント
        //----------------------------------------------------------------------------
        private void StartEvent(object sender, EventArgs e)
        {
            StartDialog sd = new StartDialog();
            sd.Owner = this;

            if (sd.ShowDialog() == false)
            {
                this.Close();
            }
        }

        private void LayerTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //有効になったタブを参照する
            var test1 = (TabControl)sender;
            var test = (OriTab)test1.Items[test1.SelectedIndex];
            if(test !=null)
            {
                ActiveTab = test;
                ActiveLayer.Content = string.Format("{0}", test.GetLayerCount() - 1);
            }
        }

        private void RoutedTest(object sender, RoutedEventArgs e)
        {
            var sc = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)sc.Content;

            var mip1 = (EditMT)e.Source;
            CellIndex pos = mip1.Cell;
            MessageBox.Show(string.Format("MouseDown: {0}, {1} ({2})\n Layer:{3}",
                              pos.X, pos.Y, e.Source.GetType().Name, mip1.GetLayer()));
        }

        private void TestGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var scroll = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)scroll.Content;


            hitTestResults.Clear();
            Point pt = e.GetPosition((UIElement)sender);
            VisualTreeHelper.HitTest(grid, null, new HitTestResultCallback(MyHitTestResult), new PointHitTestParameters(pt));

            UniformGrid uniform = new UniformGrid();

            if (hitTestResults.Count > 0)
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
                            types += "SET" + bt.Children.Count;
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

                MessageBox.Show(string.Format("Count:{0}\n{1}", hitTestResults.Count, types));
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
            if (Box != null)
            {
                MessageBox.Show("AS" + result.VisualHit.GetType());
                var Bt = Box as EditMT;
                if (Bt != null)
                {
                    MessageBox.Show("LOK");
                }
            }

            // Add the hit test result to the list that will be processed after the enumeration.
            hitTestResults.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
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
                    if (ActiveLay == bl)
                    {
                        if (Bt.Cell.Y == cell.Y)
                        {
                            if (Bt.Cell.X == cell.X)
                            {
                                Bt.UpdeteStatus(tip);
                                return;
                            }
                        }
                        continue;
                    }
                    break;
                }
            }
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

        //----------------------------------------------------------------------------
        //内部関数
        //----------------------------------------------------------------------------
        private UniformGrid CreateMapGrid(string Header)
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

            tab.IsSelected = true;
            LayerTab.Items.Add(tab);

            return uniform;
        }

        public void InitEditGrid(string header)
        {
            var grid = CreateMapGrid(header);

            CreateEditTip(grid,0);
        }

        private void SaveFiles(string FullPath)
        {
            if (!Dirty)//変更がなければ戻る
                return;

            //ファイルを保存する
        }

        private void CreateEditTip(UniformGrid grid,uint Layer)
        {
            grid.Children.Clear();
            grid.Rows = RowCount;
            grid.Columns = ColumnCount;

            for (int row = 0; row < RowCount; row++)
            {
                for (int Col = 0; Col < ColumnCount; Col++)
                {
                    EditMT bt = new EditMT(Layer);
                    bt.Width = 35;
                    bt.Height = 35;
                    bt.Cell = new CellIndex(row, Col);
                    bt.Background = Brushes.Transparent;
                    bt.BorderBrush = Brushes.Black;
                    bt.Click += DrawTip;
                    grid.Children.Add(bt);
                }
            }
        }
    }
}
