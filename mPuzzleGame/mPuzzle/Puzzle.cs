using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

//it is good to have some more comments in source code

namespace mPuzzle
{
    public enum Difficulty
    {
        EASY = 0,
        MEDIUM = 1,
        HARD = 2,
        EXPERT = 3
    }

    public enum FileType 
    {
        JPG,
        PNG
    }

    public enum Orientation
    {
        PORTRAIT,
        LANDSCAPE
    }

    public class Puzzle
    {
        public static readonly string thumbnail_suffix = "_thumbnail";
        public static readonly string JPG = "JPG";
        public static readonly string PNG = "PNG";
        public static readonly string JPG_FILE = ".jpg";
        public static readonly string PNG_FILE = ".png";

        public Puzzle()
        {
            easyp = mediump = hardp = expertp = 0;
        }

        public int id;
        [XmlIgnoreAttribute]
        public string name;
        [XmlIgnoreAttribute]
        public FileType filetype;
        [XmlIgnoreAttribute]
        public string filepath;
        [XmlIgnoreAttribute]
        public Orientation orientation;
        [XmlIgnoreAttribute]
        public int xdimension;
        [XmlIgnoreAttribute]
        public int ydimension;
        [XmlIgnoreAttribute]
        public string puzzleImage;
        [XmlIgnoreAttribute]
        public bool opened;
        [XmlIgnoreAttribute]
        public string puzzleThumbnail 
        {
            get
            {
                string thumb;
                if (opened)
                    thumb = "_thumbnail";
                else
                    thumb = "_thumbnailbw";

                if (filetype == FileType.JPG)
                    return filepath.Substring(0, filepath.Length - 4) + thumb + JPG_FILE;
                else
                    return filepath.Substring(0, filepath.Length - 4) + thumb + PNG_FILE;
            }
        }
        [XmlIgnoreAttribute]
        public string puzzleThumbnailBase;

        // local scores
        public int moveseasy { get; set; }
        public long timeeasy { get; set; }
        public int movesmedium { get; set; }
        public long timemedium { get; set; }
        public int moveshard { get; set; }
        public long timehard { get; set; }
        public int movesexpert { get; set; }
        public long timeexpert { get; set; }

        // best scores from internet
        public int moveseasyb { get; set; }
        public long timeeasyb { get; set; }
        public int movesmediumb { get; set; }
        public long timemediumb { get; set; }
        public int moveshardb { get; set; }
        public long timehardb { get; set; }
        public int movesexpertb { get; set; }
        public long timeexpertb { get; set; }
        [XmlArrayItem("user")]
        public User[] userlisteasy;
        [XmlArrayItem("user")]
        public User[] userlistmedium;
        [XmlArrayItem("user")]
        public User[] userlisthard;
        [XmlArrayItem("user")]
        public User[] userlistexpert;

        // place
        public int easyp;
        public int mediump;
        public int hardp;
        public int expertp;

        // best players


        [XmlIgnoreAttribute]
        public string genTypeString
        {
            get 
            { 
                string result = "type: ";
                if (filetype == FileType.JPG)
                    result += JPG;
                else
                    result += PNG;
                if (orientation == Orientation.LANDSCAPE)
                    result += ", landscape";
                else
                    result += ", portrait";
                return result;
            }
        }

        [XmlIgnoreAttribute]
        public string genDimensionsString
        {
            get
            {
                string result = "dimensions: " + xdimension + "x" + ydimension;
                return result;
            }
        }
    }
}
