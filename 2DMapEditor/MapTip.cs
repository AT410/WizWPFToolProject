using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml.Serialization;

namespace _2DMapEditor
{
    [Serializable]
    public struct CellIndex
    {
        [XmlAttribute("X")]
        public int X;
        [XmlAttribute("Y")]
        public int Y;


        public CellIndex(int x,int y)
        {
            X = x;
            Y = y;
        }

        public void Swap()
        {
            int temp = X;
            X = Y;
            Y = temp;
        }
    }
    public class MapTip : Button
    {
        public uint index;
        public string TipFileName;
    }

    public class EditMT :Button
    {
        public string TipFileName;

        private uint Layer;

        public CellIndex Cell;

        public int index;

        public EditMT(uint layer)
        {
            
            Layer = layer;
            this.BorderBrush = Brushes.Black;
            this.BorderThickness = new System.Windows.Thickness(1);
            index = -1;
        }

        public uint GetLayer()
        {
            return Layer;
        }

        public void UpdeteStatus(MapTip mapTip)
        {
            this.Background = mapTip.Background;
            this.TipFileName = mapTip.TipFileName;
            this.index = (int)mapTip.index;
        }
    }

    public class OriTab:TabItem
    {
        private uint LayerCount;

        public OriTab()
        {
            LayerCount = 1;
        }

        public uint GetLayerCount() { return LayerCount; }

        public void AddLayer() { LayerCount++; }

        public void DellLayer() { LayerCount = LayerCount <= 1 ? 1 : LayerCount--; }
    }
}
