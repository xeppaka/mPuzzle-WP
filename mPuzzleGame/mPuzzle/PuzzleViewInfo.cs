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
using System.ComponentModel;

namespace mPuzzle
{
    public class PuzzleViewInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PuzzleViewInfo(Puzzle puzzle)
        {
            update(puzzle);
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void update(Puzzle puzzle)
        {
            name = puzzle.name;
            type = puzzle.genTypeString;
            dimensions = puzzle.genDimensionsString;
            if (puzzle.opened)
            {
                closedVisibility = Visibility.Collapsed;
                otherVisibility = Visibility.Visible;
            }
            else
            {
                closedVisibility = Visibility.Visible;
                otherVisibility = Visibility.Collapsed;
            }
            puzzleThumbnail = puzzle.puzzleThumbnail;
            timeeasy = Utils.formatTime(puzzle.timeeasy);
            timemedium = Utils.formatTime(puzzle.timemedium);
            timehard = Utils.formatTime(puzzle.timehard);
            timeexpert = Utils.formatTime(puzzle.timeexpert);
            moveseasy = puzzle.moveseasy;
            movesmedium = puzzle.movesmedium;
            moveshard = puzzle.moveshard;
            movesexpert = puzzle.movesexpert;

            timeeasyb = Utils.formatTime(puzzle.timeeasyb);
            timemediumb = Utils.formatTime(puzzle.timemediumb);
            timehardb = Utils.formatTime(puzzle.timehardb);
            timeexpertb = Utils.formatTime(puzzle.timeexpertb);
            moveseasyb = puzzle.moveseasyb;
            movesmediumb = puzzle.movesmediumb;
            moveshardb = puzzle.moveshardb;
            movesexpertb = puzzle.movesexpertb;

            placeeasy = puzzle.easyp;
            placemedium = puzzle.mediump;
            placehard = puzzle.hardp;
            placeexpert = puzzle.expertp;

            id = puzzle.id;
            opened = puzzle.opened;

            NotifyPropertyChanged(null);
        }

        public string name { get;set; }
        public string type { get;set; }
        public string dimensions { get; set; }
        public Visibility closedVisibility { get;set; }
        public Visibility otherVisibility { get; set; }
        public string puzzleThumbnail { get; set; }
        public string timeeasy { get; set; }
        public string timemedium { get; set; }
        public string timehard { get; set; }
        public string timeexpert { get; set; }
        public int moveseasy { get; set; }
        public int movesmedium { get; set; }
        public int moveshard { get; set; }
        public int movesexpert { get; set; }

        public string timeeasyb { get; set; }
        public string timemediumb { get; set; }
        public string timehardb { get; set; }
        public string timeexpertb { get; set; }
        public int moveseasyb { get; set; }
        public int movesmediumb { get; set; }
        public int moveshardb { get; set; }
        public int movesexpertb { get; set; }

        public int placeeasy { get; set; }
        public int placemedium { get; set; }
        public int placehard { get; set; }
        public int placeexpert { get; set; }

        public int id { get; set; }
        public bool opened { get; set; }
    }
}
