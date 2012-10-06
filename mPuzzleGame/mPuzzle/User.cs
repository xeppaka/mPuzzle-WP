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
using System.Xml.Serialization;

namespace mPuzzle
{
    public class User
    {
        [XmlAttribute("place")]
        public int place;
        [XmlIgnore]
        private string nickname;
        [XmlIgnore]
        public string fnickname {get; set;}
        public long time;
        public int moves;
        [XmlIgnore]
        private bool you;

        [XmlIgnore]
        public string ftime 
        { 
            get 
            {
                if (time > 0)
                    return "Time: " + Utils.formatTime(time);
                else
                    return "";
            } 
        }

        [XmlIgnore]
        public string fmoves 
        { 
            get 
            {
                if (moves > 0)
                    return "Moves: " + moves;
                else
                    return "";
            } 
        }

        [XmlIgnore]
        public string fplace
        {
            get
            {
                if (place > 0)
                    return String.Format("{0}", place);
                else
                    return "";
            }
        }

        [XmlIgnore]
        public string fcolor
        {
            get
            {
                if (you)
                    return "#ffff0000";
                else
                    return "#ffffffff";
            }
        }

        [XmlAttribute("username")]
        public string xnickname
        {
            get { return nickname; }
            set
            {
                if (value.EndsWith("[[[[[[[[[[you]]]]]]]]]]"))
                {
                    fnickname = value.Substring(0, value.Length - 23) + " (you)";
                    you = true;
                }
                else
                {
                    fnickname = value;
                    you = false;
                }
                nickname = value;
            }
        }
    }
}
