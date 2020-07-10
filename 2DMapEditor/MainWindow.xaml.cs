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
        public string FileName = "";

        private int RowCount;
        private int ColumnCount;

        private MapTip tip,EraseTip;

        private OriTab ActiveTab;

        private List<DependencyObject> hitTestResults;

        // -- マウスフラグ --
        enum DrawMode
        {
            None,
            Draw,
            Erase
        };

        DrawMode mode;

        private bool SizeActive;

        private float buttonsize;

        public void SetRow(int Row) { RowCount = Row > 0 ? Row : 1; }
        public void SetColumn(int Col) { ColumnCount = Col > 0 ? Col : 1; }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += StartEvent;
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;

            SizeActive = false;
            buttonsize = 35;

            MapTipGrid.Children.Clear();
            MapTipGrid.Rows = 2;
            MapTipGrid.Columns = 6;
            MapTipGrid.HorizontalAlignment = HorizontalAlignment.Center;
            tip = new MapTip();
            EraseTip = new MapTip();
            LayerTab.Items.Clear();
            LayerTab.SelectionChanged += LayerTab_SelectionChanged;

            ActiveTab = new OriTab();

            hitTestResults = new List<DependencyObject>();

            mode = DrawMode.None;

            MapView.PreviewMouseLeftButtonDown += (s,e) => mode = DrawMode.Draw;
            MapView.PreviewMouseLeftButtonUp += (s,e) => mode = DrawMode.None;
            MapView.PreviewMouseRightButtonDown += (s,e) => mode = DrawMode.Erase;
            MapView.PreviewMouseRightButtonUp += (s,e) => mode = DrawMode.None;

            // -- ウィンドウクロージング --
            this.Closing += MainWindow_Closing;

            // -- ディレクトリ作成（初回のみ） --
            string MapFolder = Directory.GetCurrentDirectory() + "/Maps/";
            string ProjectFolder = Directory.GetCurrentDirectory() + "/Projects/";
            string MediaFolder = Directory.GetCurrentDirectory() + "/Media/";
            if(!Directory.Exists(MapFolder))
            {
                Directory.CreateDirectory(MapFolder);
            }

            if(!Directory.Exists(ProjectFolder))
            {
                Directory.CreateDirectory(ProjectFolder);
            }

            if(!Directory.Exists(MediaFolder))
            {
                Directory.CreateDirectory(MediaFolder);
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                SizeActive = false;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.LeftCtrl)
            {
                SizeActive = true;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Dirty)
            {
                MessageBoxResult result = MessageBox.Show("作業ファイルが未保存です。保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (MessageBoxResult.Yes == result)
                {
                    if (SaveAction() == false)
                    {
                        e.Cancel = true;

                    }
                }
                else if(MessageBoxResult.Cancel ==result)
                {
                    e.Cancel = true;
                }
            }
        }

        //----------------------------------------------------------------------------
        //XAMLクリックイベント
        //----------------------------------------------------------------------------
        private void NewCreate_Click(object sender, RoutedEventArgs e)
        {
            StartDialog sd = new StartDialog(false);
            sd.Owner = this;

            if (sd.ShowDialog() == true)
            {

            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (Dirty)
            {
                MessageBoxResult result = MessageBox.Show("作業ファイルが未保存です。保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (MessageBoxResult.Yes == result)
                {
                    if (SaveAction() == false)
                    {
                        return;
                    }
                }
                else if (MessageBoxResult.Cancel == result)
                {
                    return;
                }
            }


            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = "プロジェクトデータ (*.m2d)|*.m2d";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (dialog.ShowDialog() == true)
            {
                LayerTab.Items.Clear();
                string FilePath = dialog.FileName;

                var obj = DataIO.BinaryRead(FilePath);

                var setting = obj as EditorSetting;
                if(setting !=null)
                {
                    AttachToSetting(setting);
                    FileName = FilePath;
                    UpdataTitle();
                }
                else
                {
                    MessageBox.Show("非対応のファイルです。");
                }
            }

            Dirty = false;
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveAction();
        }
        private void SaveFileRename_Click(object sender, RoutedEventArgs e)
        {
            NameSave();
        }

        private void ExportXML_Click(object sender, RoutedEventArgs e)
        {
            EditorSetting setting = new EditorSetting();

            ReadToSetting(ref setting);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Xml Files|*.xml";
            dialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Maps";
            dialog.FileName = "EXMap";
            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;
                DataIO.XmlSerialize(filename, setting);

                MessageBox.Show("XMLマップの書き出しに成功しました。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
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

            Dirty = true;
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
            StartDialog sd = new StartDialog(true);
            sd.Owner = this;

            if (sd.ShowDialog() == false)
            {
                this.Close();
            }

            UpdataTitle();
            Dirty = false;
        }

        private void LayerTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //有効になったタブを参照する
            var test1 = (TabControl)sender;
            if(test1.Items.Count==0)
            {
                return;
            }

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

        private void DrawTip(object sender, MouseButtonEventArgs e)
        {
            UpdateMap(sender, tip);
        }

        private void DrawErase(object sender, MouseButtonEventArgs e)
        {
            UpdateMap(sender, EraseTip);
        }

        private void Mt_Click(object sender, RoutedEventArgs e)
        {
            tip = (MapTip)sender;
        }

        private void Mt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MapTipEditor mapTipEditor = new MapTipEditor((MapTip)sender);

            mapTipEditor.Owner = this;
            mapTipEditor.ShowDialog();
        }

        private void Bt_MouseEnter(object sender, MouseEventArgs e)
        {
            switch(mode)
            {
                case DrawMode.None:
                    var obj = (EditMT)sender;
                    obj.SetForcus(true);
                    break;
                case DrawMode.Draw:
                    UpdateMap(sender,tip);
                    break;
                case DrawMode.Erase:
                    UpdateMap(sender, EraseTip);
                    break;
            }
        }

        private void Bt_MouseLeave(object sender, MouseEventArgs e)
        {
            var obj = (EditMT)sender;
            obj.SetForcus(false);
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

        public void ResetEdit()
        {
            LayerTab.Items.Clear();
            MapTipGrid.Children.Clear();
            FileName = "";
            UpdataTitle();
        }

        private bool NameSave()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "MapFile";
            dialog.Filter = "プロジェクトデータ (*.m2d)|*.m2d|All files (*.*)|*.*";
            dialog.InitialDirectory = Directory.GetCurrentDirectory()+ "\\Projects";
            if (dialog.ShowDialog() == true)
            {
                Dirty = true;
                string FilePath = dialog.FileName;
                SaveFiles(FilePath);
                FileName = FilePath;
                UpdataTitle();
                return true;
            }
            return false;
        }

        private bool SaveAction()
        {
            if (FileName == "")
            {
                return NameSave();            }
            else
            {
                SaveFiles(FileName);
                return true;
            }
        }

        private void SaveFiles(string FullPath)
        {
            if (!Dirty)//変更がなければ戻る
                return;

            //ファイルを保存する
            EditorSetting setting = new EditorSetting();

            ReadToSetting(ref setting);

            DataIO.SaveTOBinary(setting, FullPath);

            MessageBox.Show(this, "保存完了しました。");
            Dirty = false;
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
                    bt.Width = buttonsize;
                    bt.Height = buttonsize;
                    bt.Cell = new CellIndex(row, Col);
                    bt.Background = Brushes.Transparent;
                    bt.BorderBrush = Brushes.Black;
                    bt.MouseLeftButtonDown += DrawTip;
                    bt.MouseRightButtonDown += DrawErase;
                    bt.MouseEnter += Bt_MouseEnter;
                    bt.MouseLeave += Bt_MouseLeave;
                    bt.MouseWheel += bt_MouseWheel;
                    grid.Children.Add(bt);
                }
            }

            Dirty = true;
        }

        private void UpdateMap(object sender,MapTip select)
        {
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
                                Bt.UpdeteStatus(select);
                                Dirty = true;
                                return;
                            }
                        }
                        continue;
                    }
                    break;
                }
            }
        }

        private void ReadToSetting(ref EditorSetting setting)
        {
            foreach (var tab in LayerTab.Items.OfType<OriTab>())
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

                    LayerSetting layerSetting = new LayerSetting();

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

            foreach (var MapTip in MapTipGrid.Children.OfType<MapTip>())
            {
                MapTipSetting Eset = new MapTipSetting();
                Eset.index = MapTip.index;
                Eset.TipFileName = MapTip.TipFileName;
                setting.tips.Add(Eset);
            }
        }

        public void AttachToSetting(EditorSetting setting)
        {
            // -- マップチップ読込 --
            foreach(var maptip in setting.tips)
            {
                MapTip mt = new MapTip();
                mt.Width = 35;
                mt.Height = 35;
                mt.MouseDoubleClick += Mt_MouseDoubleClick; //<-マップチップエディタを開く
                mt.Click += Mt_Click;
                mt.index = maptip.index;
                mt.TipFileName = maptip.TipFileName;
                mt.Background = new ImageBrush(
                    new BitmapImage(new Uri(System.IO.Path.GetFullPath(maptip.TipFileName))));
                MapTipGrid.Children.Add(mt);

            }

            bool selected = false;

            foreach (var tabset in setting.tabSettings)
            {
                OriTab tab = new OriTab();
                tab.Header = tabset.Header;
                tab.SetLayerCount(tabset.LayerCount);

                if(!selected)
                {
                    tab.IsSelected = true;
                    selected = true;
                }

                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollViewer.Background = Brushes.WhiteSmoke;

                Grid grid = new Grid();
                foreach(var layer in tabset.Layers)
                {
                    UniformGrid uniform = new UniformGrid();
                    uniform.Rows = tabset.GridRows;
                    uniform.Columns = tabset.GridColumns;
                    uniform.HorizontalAlignment = HorizontalAlignment.Left;
                    uniform.VerticalAlignment = VerticalAlignment.Top;
                    uniform.Background = Brushes.Transparent;

                    uniform.Children.Clear();

                    foreach(var cell in layer.cellmap)
                    {
                        EditMT edit = new EditMT(layer.Layer);
                        edit.Cell = new CellIndex(cell.X, cell.Y);
                        edit.index = cell.MapTipIndex;
                        // -- マップチップを反映させる --
                        if (edit.index != -1)
                        {
                            foreach(var tip in MapTipGrid.Children.OfType<MapTip>())
                            {
                                if(tip.index == edit.index)
                                {
                                    edit.Background = tip.Background;
                                    edit.MipSet = tip.Background;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            edit.Background = Brushes.Transparent;
                        }

                        edit.Width = buttonsize;
                        edit.Height = buttonsize;
                        edit.BorderBrush = Brushes.Black;
                        edit.MouseLeftButtonDown += DrawTip;
                        edit.MouseRightButtonDown += DrawErase;
                        edit.MouseEnter += Bt_MouseEnter;
                        edit.MouseLeave += Bt_MouseLeave;
                        edit.MouseWheel += bt_MouseWheel;

                        uniform.Children.Add(edit);
                    }

                    grid.Children.Add(uniform);
                }

                scrollViewer.Content = grid;

                tab.Content = scrollViewer;

                LayerTab.Items.Add(tab);
            }
        }

        private void bt_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!SizeActive)
                return;

            if(e.Delta>0)
            {
                //拡大
                UpdateEditTipSize(+0.5f);
            }
            else if(e.Delta<0)
            {
                UpdateEditTipSize(-0.5f);
            }
        }

        private void UpdateEditTipSize(float size)
        {
            var scroll = (ScrollViewer)ActiveTab.Content;
            var grid = (Grid)scroll.Content;

            foreach (var uniform in grid.Children.OfType<UniformGrid>())
            {
                foreach (var Bt in uniform.Children.OfType<EditMT>())
                {
                    Bt.Width += size;
                    Bt.Height += size;

                    if (Bt.Width <10)
                    {
                        Bt.Width = 10;
                        Bt.Height = 10;
                    }
                    else if(Bt.Width>50)
                    {
                        Bt.Width = 50;
                        Bt.Height = 50;
                    }

                    buttonsize = (float)Bt.Width;
                }
            }
        }

        private void UpdataTitle()
        {
            this.Title = string.Format("MainWindow:{0}" ,FileName);
        }
    }
}
