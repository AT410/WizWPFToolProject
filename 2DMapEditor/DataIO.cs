using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;


namespace _2DMapEditor
{
    //----------------------------------------------------------------------------
    //設定クラス
    //----------------------------------------------------------------------------
    // -- マップチップ設定クラス --
    [Serializable]
    public class MapTipSetting
    {
        [XmlAttribute("MapTipindex")]
        public uint index;

        [XmlAttribute("FileName")]
        public string TipFileName;

    }

    // -- CellSetting --
    [Serializable]
    public class CellSetting
    {
        [XmlAttribute("X")]
        public int X;

        [XmlAttribute("Y")]
        public int Y;

        [XmlAttribute("Index")]
        public int MapTipIndex;
    }

    // -- LayerSetting --
    [Serializable]
    public class LayerSetting
    {
        [XmlAttribute("Layer")]
        public uint Layer;

        [XmlElement("CellMap")]
        public List<CellSetting> cellmap;

        public LayerSetting()
        {
            cellmap = new List<CellSetting>();
        }
    }

    [Serializable]
    public class TabSetting
    {
        [XmlAttribute("Header")]
        public string Header;

        // -- Mapのレイヤー数 --
        [XmlAttribute("LayerCount")]
        public uint LayerCount;

        // -- グリッドの幅と高さ --
        [XmlAttribute("Rows")]
        public int GridRows;

        [XmlAttribute("Columns")]
        public int GridColumns;

        [XmlElement("Layers")]
        public List<LayerSetting> Layers;

        public TabSetting()
        {
            Layers = new List<LayerSetting>();
        }
    }

    // -- 大元 --
    [Serializable]
    public class EditorSetting
    {
        // -- エディタ情報 --
        [XmlElement("MapData")]
        public List<TabSetting> tabSettings;
        // -- マップチップ情報 --
        [XmlElement("MapTip")]
        public List<MapTipSetting> tips;

        public EditorSetting()
        {
            tips = new List<MapTipSetting>();
            tabSettings = new List<TabSetting>();
        }
    }


    [Serializable]
    public class TestS
    {
        // -- エディタ情報 --
        [XmlElement("TEST")]
        public string Sf;
        // -- マップチップ情報 --
        [XmlElement("TEST2")]
        public List<MapTipSetting> vs;

        public TestS()
        {
            Sf = "NULL";
            vs = new List<MapTipSetting>();

            for (int i = 0; i < 5; i++)
            {
                MapTipSetting Eset = new MapTipSetting();
                Eset.index = 5;
                Eset.TipFileName = "Test";
                vs.Add(Eset);
            }
        }
    }

    //----------------------------------------------------------------------------
    //書き出し用クラス
    //----------------------------------------------------------------------------

    //----------------------------------------------------------------------------
    //書き出しクラス
    //----------------------------------------------------------------------------

    [Serializable]
    public class DataIO
    {
        // -- バイナリ書き出し --
        public static void SaveTOBinary(object obj,string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    binary.Serialize(stream, obj);
                }
            }
            catch
            {

            }
        }
        // --  バイナリ読込 --
        public static object BinaryRead(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter binary = new BinaryFormatter();
                    object obj = binary.Deserialize(stream);
                    return obj;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void XmlSerialize<T>(string filename, T data, FileMode mode = FileMode.Create)
        {
                using (var stream = new FileStream(filename, mode))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, data);
                }
        }
    }
}
