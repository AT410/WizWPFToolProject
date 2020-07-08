using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace _2DMapEditor
{
    public struct CellIndex
    {
        public int X;
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
        public string TipFileName;
    }

    public class EditMT :Button
    {
        public string TipFileName;

        private uint Layer;

        public CellIndex Cell;

        public EditMT(uint layer)
        {
            
            Layer = layer;
            this.BorderBrush = Brushes.Black;
            this.BorderThickness = new System.Windows.Thickness(1);
        }

        public uint GetLayer()
        {
            return Layer;
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
