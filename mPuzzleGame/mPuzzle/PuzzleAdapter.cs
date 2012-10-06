using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace mPuzzle
{
    public class PuzzleAdapter
    {
        public static readonly string PUZZLES_LIST = @"puzzles/list.xml";
        public static readonly string SCORES_FILENAME = @"scores.xml";

        // puzzles. <puzzle id, Puzzle>
        public Dictionary<int, Puzzle> puzzleDic;
        public Puzzle[] puzzle;
        public Thread loadThread;
        public IPuzzleLoader puzzleLoader;
        public bool stop_flag = false;

        //settings
        public string username;
        public int userid;
        public bool autoReadWriteInternet;
        public int openedPuzzles;

        // transient info
        public int selectedPuzzle;
        public Difficulty selectedDifficulty;

        public PuzzleAdapter()
        {
            puzzleDic = new Dictionary<int, Puzzle>();
            userid = -1;
        }

        public void load(IPuzzleLoader iloader)
        {
            if (loadThread != null && loadThread.IsAlive)
                return;
            puzzleLoader = iloader;
            //loadThread = new Thread(new ThreadStart(this.iloadAll));
            //loadThread.Start();
            iloadAll();
        }

        public bool isLoading()
        {
            return loadThread.IsAlive;
        }

        public void stop()
        {
            stop_flag = true;
        }

        private void iloadAll()
        {
            //if (puzzleLoader != null)
            //    puzzleLoader.loadStarted();

            _loadSettings();
            _iloadPuzzles();
            _loadScores();

            //if (puzzleLoader != null)
            //    puzzleLoader.loadFinished();
        }

        private void _iloadPuzzles()
        {
            Uri uri = new Uri(PUZZLES_LIST, UriKind.Relative);
            StreamResourceInfo sri = App.GetResourceStream(uri);
            XElement xe = XElement.Load(sri.Stream);
            sri.Stream.Close();

            int puzzleCount = xe.Elements("Puzzle").Count();

            if (puzzleCount > 0)
            {
                IEnumerable<XElement> ien = xe.Elements("Puzzle");

                puzzle = new Puzzle[puzzleCount];
                Puzzle tempPuzzle;
                int i = 0;
                foreach (XElement curPuzzle in ien)
                {
                    tempPuzzle = new Puzzle();

                    //read puzzle id
                    tempPuzzle.id = (int)curPuzzle.Element("id");

                    //read puzzle name
                    tempPuzzle.name = (string)curPuzzle.Element("name");

                    //is puzzle opened?
                    if (i < openedPuzzles)
                        tempPuzzle.opened = true;
                    else
                        tempPuzzle.opened = false;

                    //read puzzle picture filename
                    string path = tempPuzzle.filepath = (string)curPuzzle.Element("path");

                    //read filetype
                    if (path.EndsWith(Puzzle.JPG_FILE))
                        tempPuzzle.filetype = FileType.JPG;
                    else if (path.EndsWith(Puzzle.PNG_FILE))
                        tempPuzzle.filetype = FileType.PNG;

                    tempPuzzle.puzzleThumbnailBase = path.Substring(0, path.Length - 4) + "_thumbnail";
                    tempPuzzle.puzzleImage = path;

                    //read x dimension
                    tempPuzzle.xdimension = (int)curPuzzle.Element("xdimension");

                    //read y dimension
                    tempPuzzle.ydimension = (int)curPuzzle.Element("ydimension");

                    if (tempPuzzle.ydimension >= tempPuzzle.xdimension)
                        tempPuzzle.orientation = Orientation.PORTRAIT;
                    else
                        tempPuzzle.orientation = Orientation.LANDSCAPE;

                    puzzle[i] = tempPuzzle;
                    puzzleDic.Add(tempPuzzle.id, tempPuzzle);

                    if (stop_flag)
                        return;
                    ++i;

                    //if (puzzleLoader != null)
                    //    puzzleLoader.reportProgress(i / puzzleCount * 100);
                }
            }
        }

        private void _loadScores()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(SCORES_FILENAME))
            {
                saveScores();
            }
            else
            {
                IsolatedStorageFileStream stream = store.OpenFile(SCORES_FILENAME, FileMode.Open);
                XmlSerializer xml = new XmlSerializer(typeof(Puzzle[]));
                Puzzle[] puz = (Puzzle[])xml.Deserialize(stream);
                int count = puz.Length;
                for (int i = 0; i < count; ++i)
                {
                    Puzzle p = puzzleDic[puz[i].id];
                    p.easyp = puz[i].easyp;
                    p.mediump = puz[i].mediump;
                    p.hardp = puz[i].hardp;
                    p.expertp = puz[i].expertp;

                    p.timeeasy = puz[i].timeeasy;
                    p.timemedium = puz[i].timemedium;
                    p.timehard = puz[i].timehard;
                    p.timeexpert = puz[i].timeexpert;
                    p.timeeasyb = puz[i].timeeasyb;
                    p.timemediumb = puz[i].timemediumb;
                    p.timehardb = puz[i].timehardb;
                    p.timeexpertb = puz[i].timeexpertb;

                    p.moveseasy = puz[i].moveseasy;
                    p.movesmedium = puz[i].movesmedium;
                    p.moveshard = puz[i].moveshard;
                    p.movesexpert = puz[i].movesexpert;
                    p.moveseasyb = puz[i].moveseasyb;
                    p.movesmediumb = puz[i].movesmediumb;
                    p.moveshardb = puz[i].moveshardb;
                    p.movesexpertb = puz[i].movesexpertb;

                    p.userlisteasy = puz[i].userlisteasy;
                    p.userlistmedium = puz[i].userlistmedium;
                    p.userlisthard = puz[i].userlisthard;
                    p.userlistexpert = puz[i].userlistexpert;
                }
                //XElement root = XElement.Load(stream);
                //stream.Close();
                //store.Dispose();
                //int count = root.Elements("puzzle").Count();
                //if (count > 0)
                //{
                //    IEnumerable<XElement> ien = root.Elements("puzzle");
                //    foreach (XElement curPuzzle in ien)
                //    {
                //        int id = (int)curPuzzle.Attribute("id");
                //        Puzzle pz = puzzleDic[id];
                //        pz.moveseasy = (int)curPuzzle.Element("moveseasy");
                //        pz.movesmedium = (int)curPuzzle.Element("movesmedium");
                //        pz.moveshard = (int)curPuzzle.Element("moveshard");
                //        pz.movesexpert = (int)curPuzzle.Element("movesexpert");
                //        pz.timeeasy = (long)curPuzzle.Element("timeeasy");
                //        pz.timemedium = (long)curPuzzle.Element("timemedium");
                //        pz.timehard = (long)curPuzzle.Element("timehard");
                //        pz.timeexpert = (long)curPuzzle.Element("timeexpert");

                //        pz.moveseasyb = (int)curPuzzle.Element("moveseasyb");
                //        pz.movesmediumb = (int)curPuzzle.Element("movesmediumb");
                //        pz.moveshardb = (int)curPuzzle.Element("moveshardb");
                //        pz.movesexpertb = (int)curPuzzle.Element("movesexpertb");
                //        pz.timeeasyb = (long)curPuzzle.Element("timeeasyb");
                //        pz.timemediumb = (long)curPuzzle.Element("timemediumb");
                //        pz.timehardb = (long)curPuzzle.Element("timehardb");
                //        pz.timeexpertb = (long)curPuzzle.Element("timeexpertb");

                //        pz.easyp = (int)curPuzzle.Element("placeeasy");
                //        pz.mediump = (int)curPuzzle.Element("placemedium");
                //        pz.hardp = (int)curPuzzle.Element("placehard");
                //        pz.expertp = (int)curPuzzle.Element("placeexpert");
                //    }
                //}
            }
        }

        public void saveScores()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists(SCORES_FILENAME))
                store.DeleteFile(SCORES_FILENAME);
            IsolatedStorageFileStream stream = store.OpenFile(SCORES_FILENAME, FileMode.CreateNew);
            XmlSerializer xml = new XmlSerializer(typeof(Puzzle[]));
            xml.Serialize(stream, puzzle);
            stream.Flush();
            stream.Close();
        }

        private void _loadSettings()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("username"))
                username = (string)settings["username"];
            if (settings.Contains("autointernet"))
                autoReadWriteInternet = (bool)settings["autointernet"];
            if (settings.Contains("openedpuzzles"))
                openedPuzzles = (int)settings["openedpuzzles"];
            else
                openedPuzzles = 1;
            if (settings.Contains("userid"))
                userid = (int)settings["userid"];
            else
                userid = -1;
        }

        public void saveSettings()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (username != null)
                settings["username"] = username;
            if (userid > 0)
                settings["userid"] = userid;
            settings["autointernet"] = autoReadWriteInternet;
            settings["openedpuzzles"] = openedPuzzles;
            settings.Save();
        }

        public bool openNewPuzzle()
        {
            if (openedPuzzles < puzzle.Length)
            {
                ++openedPuzzles;
                puzzle[openedPuzzles - 1].opened = true;
                return true;
            }

            return false;
        }

        public bool validateUsername(string username)
        {
            if (username.Length < 3 || username.Length > 25)
                return false;
            if (username.Contains('<') || username.Contains('>') || username.Contains('#') || username.Contains('\'') || username.Contains('\"')
                || username.Contains('\\') || username.Contains('@') || username.Contains('$') || username.Contains('/') || username.Contains('%')
                || username.Contains('^') || username.Contains('&'))
                return false;
            return true;
        }
    }
}
