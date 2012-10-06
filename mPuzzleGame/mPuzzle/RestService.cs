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
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Phone.Net.NetworkInformation;

namespace mPuzzle
{
    public interface IRestCallback
    {
        void createUserCallback(bool requestOk, int userid);
        void changeUsernameCallback(bool requestOk, int result);
        void syncOneCallback(bool requestOk, SyncOneResponse results);
        void syncAllCallback(bool requestOk, SyncAllResponse results);
    }

    public class UserCreateRequest
    {
        public string username;
        public UserCreateRequest(string username)
        {
            this.username = username;
        }
    }

    public class UserCreateResponse
    {
        public int userid;
    }

    public class UserChangeRequest
    {
        public string oldUsername;
        public string newUsername;
        public UserChangeRequest(string oldUsername, string newUsername)
        {
            this.oldUsername = oldUsername;
            this.newUsername = newUsername;
        }    
    }

    // result: 0 - Ok, 1 - oldusername doesn't exist, 2 - userid and oldusername doesn't correspond each other
    public class UserChangeResponse
    {
        public int result;
    }

    public class SyncOneRequest
    {
        public SyncOneRequest()
        {
        }

        public SyncOneRequest(long time, int moves)
        {
            this.time = time;
            this.moves = moves;
        }

        public long time;
        public int moves;
    }

    public class SyncOneResponse
    {
        public long besttime;
        public int bestmoves;
        public int place;
        [XmlArrayItem("user")]
        public User[] userlist;
    }

    public class PuzzleResult
    {
        public PuzzleResult(Puzzle puzzle)
        {
            timeeasy = puzzle.timeeasy;
            timemedium = puzzle.timemedium;
            timehard = puzzle.timehard;
            timeexpert = puzzle.timeexpert;
            moveseasy = puzzle.moveseasy;
            movesmedium = puzzle.movesmedium;
            moveshard = puzzle.moveshard;
            movesexpert = puzzle.movesexpert;
            puzzleid = puzzle.id;
        }

        public long timeeasy;
        public long timemedium;
        public long timehard;
        public long timeexpert;
        public int moveseasy;
        public int movesmedium;
        public int moveshard;
        public int movesexpert;
        [XmlAttribute("puzzleid")]
        public int puzzleid;
    }

    public class SyncAllRequest
    {
        public SyncAllRequest(Puzzle[] puzzles)
        {
            puzzleResults = new PuzzleResult[puzzles.Length];
            int i = 0;
            foreach (Puzzle puzzle in puzzles)
            {
                puzzleResults[i] = new PuzzleResult(puzzle);
                ++i;
            }
        }
        [XmlElement("PuzzleResult")]
        public PuzzleResult[] puzzleResults;
    }

    public class PuzzleBestResult
    {
        public long timeeasy;
        public long timemedium;
        public long timehard;
        public long timeexpert;
        public int moveseasy;
        public int movesmedium;
        public int moveshard;
        public int movesexpert;
        public int placeeasy;
        public int placemedium;
        public int placehard;
        public int placeexpert;
        [XmlArrayItem("user")]
        public User[] userlisteasy;
        [XmlArrayItem("user")]
        public User[] userlistmedium;
        [XmlArrayItem("user")]
        public User[] userlisthard;
        [XmlArrayItem("user")]
        public User[] userlistexpert;
        [XmlAttribute("puzzleid")]
        public int puzzleid;
    }

    public class SyncAllResponse
    {
        [XmlElement("PuzzleBestResult")]
        public PuzzleBestResult[] bestResults;
    }

    //public class PuzzleResultPlace : PuzzleResult
    //{
    //    public PuzzleResultPlace()
    //    {
    //        placeeasy = 0;
    //        placemedium = 0;
    //        placehard = 0;
    //        placeexpert = 0;
    //    }

    //    public PuzzleResultPlace(Puzzle puzzle)
    //        : base(puzzle)
    //    {
    //        placeeasy = 0;
    //        placemedium = 0;
    //        placehard = 0;
    //        placeexpert = 0;
    //    }

    //    public int placeeasy;
    //    public int placemedium;
    //    public int placehard;
    //    public int placeexpert;
    //}

    public class RestService
    {
        private static string USERNAME_SERVICE = "http://xapp.mobi/rest/user";
        private HttpWebRequest webRequest;
        private IRestCallback callback;
        private Timer timer;

        public RestService()
        {
        }

        public void createUser(string username, IRestCallback cb)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                cb.createUserCallback(false, 0);
                return;
            }

            lock (this)
            {
                if (webRequest != null)
                {
                    cb.createUserCallback(false, 0);
                    return;
                }
            }

            callback = cb;

            string request = String.Format(USERNAME_SERVICE);
            webRequest = (HttpWebRequest)WebRequest.Create(new Uri(request));
            webRequest.Method = "POST";

            IAsyncResult result = webRequest.BeginGetRequestStream(new AsyncCallback(requestCreateUserStream), new UserCreateRequest(username));

            timer = new Timer(new TimerCallback(requestCreateUserTimeout), callback, 30 * 1000, Timeout.Infinite);
        }

        public void cancelRequest()
        {
            lock (this)
            {
                if (webRequest != null)
                {
                    webRequest.Abort();
                    webRequest = null;
                }
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }
        }

        private void requestCreateUserStream(IAsyncResult result)
        {
            Stream postStream = webRequest.EndGetRequestStream(result);
            UserCreateRequest ucr = (UserCreateRequest)result.AsyncState;
            XmlSerializer xml = new XmlSerializer(typeof(UserCreateRequest));
            xml.Serialize(postStream, ucr);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(requestCreateUserCompleted), null);
        }

        private void requestCreateUserCompleted(IAsyncResult result)
        {
            HttpWebRequest wr = webRequest;
            lock (this)
            {
                webRequest = null;
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }

            if (wr == null || !wr.HaveResponse)
            {
                callback.createUserCallback(false, 0);
                return;
            }

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)wr.EndGetResponse(result);

                if (webResponse.StatusCode != HttpStatusCode.Created)
                {
                    callback.createUserCallback(false, 0);
                    return;
                }

                XmlSerializer xml = new XmlSerializer(typeof(UserCreateResponse));
                UserCreateResponse ucr = (UserCreateResponse)xml.Deserialize(webResponse.GetResponseStream());
                callback.createUserCallback(true, ucr.userid);
            }
            catch (WebException)
            {
                callback.createUserCallback(false, 0);
                return;
            }
        }

        private void requestCreateUserTimeout(object callback)
        {
            cancelRequest();
            IRestCallback cb = (IRestCallback)callback;
            //if (callback != null)
            //    cb.createUserCallback(false, 0);
        }

        public void sync(long time, int moves, Difficulty diff, int userid, int puzzleid, IRestCallback cb)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                cb.syncOneCallback(false, null);
                return;
            }

            lock (this)
            {
                if (webRequest != null)
                {
                    cb.syncOneCallback(false, null);
                    return;
                }
            }

            callback = cb;
            string sdiff = "easy";
            switch (diff)
            {
                case Difficulty.EASY:
                    sdiff = "easy";
                    break;
                case Difficulty.MEDIUM:
                    sdiff = "medium";
                    break;
                case Difficulty.HARD:
                    sdiff = "hard";
                    break;
                case Difficulty.EXPERT:
                    sdiff = "expert";
                    break;
            }

            string request = String.Format(USERNAME_SERVICE + "/{0}/puzzle/{1}/{2}", userid, puzzleid, sdiff);
            webRequest = (HttpWebRequest)WebRequest.Create(new Uri(request));
            webRequest.Method = "POST";

            IAsyncResult result = webRequest.BeginGetRequestStream(new AsyncCallback(requestSyncOneCreateStream), new SyncOneRequest(time, moves));

            timer = new Timer(new TimerCallback(requestSyncOneTimeout), callback, 15 * 1000, Timeout.Infinite);
        }

        private void requestSyncOneCreateStream(IAsyncResult result)
        {
            Stream postStream = webRequest.EndGetRequestStream(result);
            SyncOneRequest pr = (SyncOneRequest)result.AsyncState;
            XmlSerializer xml = new XmlSerializer(typeof(SyncOneRequest));
            xml.Serialize(postStream, pr);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(requestSyncOneCompleted), null);
        }

        private void requestSyncOneCompleted(IAsyncResult result)
        {
            HttpWebRequest wr = webRequest;
            lock (this)
            {
                webRequest = null;
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }

            if (wr == null || !wr.HaveResponse)
            {
                callback.syncOneCallback(false, null);
                return;
            }

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)wr.EndGetResponse(result);

                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    callback.syncOneCallback(false, null);
                    return;
                }

                XmlSerializer xml = new XmlSerializer(typeof(SyncOneResponse));
                try
                {
                    SyncOneResponse sor = (SyncOneResponse)xml.Deserialize(webResponse.GetResponseStream());
                    callback.syncOneCallback(true, sor);
                }
                catch (InvalidOperationException)
                {
                    callback.syncOneCallback(false, null);
                }
            }
            catch (WebException)
            {
                callback.syncOneCallback(false, null);
                return;
            }
        }

        private void requestSyncOneTimeout(object callback)
        {
            cancelRequest();
            IRestCallback cb = (IRestCallback)callback;
            //if (callback != null)
            //    cb.syncOneCallback(false, null);
        }

        public void sync(Puzzle[] puzzles, int userid, IRestCallback cb)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                cb.syncAllCallback(false, null);
                return;
            }

            lock (this)
            {
                if (webRequest != null)
                {
                    cb.syncAllCallback(false, null);
                    return;
                }
            }

            callback = cb;

            string request = String.Format(USERNAME_SERVICE + "/{0}/puzzle", userid);
            webRequest = (HttpWebRequest)WebRequest.Create(new Uri(request));
            webRequest.Method = "POST";

            IAsyncResult result = webRequest.BeginGetRequestStream(new AsyncCallback(requestSyncAllCreateStream), new SyncAllRequest(puzzles));

            timer = new Timer(new TimerCallback(requestSyncAllTimeout), callback, 15 * 1000, Timeout.Infinite);
        }

        private void requestSyncAllCreateStream(IAsyncResult result)
        {
            Stream postStream = webRequest.EndGetRequestStream(result);
            SyncAllRequest pr = (SyncAllRequest)result.AsyncState;
            XmlSerializer xml = new XmlSerializer(typeof(SyncAllRequest));
            xml.Serialize(postStream, pr);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(requestSyncAllCompleted), null);
            //XmlSerializer xml1 = new XmlSerializer(typeof(SyncAllResponse));
            //SyncAllResponse sar = new SyncAllResponse();
            //sar.bestResults = new PuzzleBestResult[2];
            //sar.bestResults[0] = new PuzzleBestResult();
            //sar.bestResults[1] = new PuzzleBestResult();
            //sar.bestResults[0].userlisteasy = new User[2];
            //sar.bestResults[0].userlisteasy[0] = new User();
            //sar.bestResults[0].userlisteasy[1] = new User();
            //sar.bestResults[1].userlisteasy = new User[2];
            //sar.bestResults[1].userlisteasy[0] = new User();
            //sar.bestResults[1].userlisteasy[1] = new User();
            //StringWriter stringWriter = new StringWriter();
            //xml1.Serialize(stringWriter, sar);
            //string ss = stringWriter.ToString();
            //return;
        }

        private void requestSyncAllCompleted(IAsyncResult result)
        {
            HttpWebRequest wr = webRequest;
            lock (this)
            {
                webRequest = null;
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }

            if (wr == null || !wr.HaveResponse)
            {
                callback.syncAllCallback(false, null);
                return;
            }

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)wr.EndGetResponse(result);

                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    callback.syncAllCallback(false, null);
                    return;
                }

                XmlSerializer xml = new XmlSerializer(typeof(SyncAllResponse));
                try
                {
                    SyncAllResponse sor = (SyncAllResponse)xml.Deserialize(webResponse.GetResponseStream());
                    callback.syncAllCallback(true, sor);
                }
                catch (InvalidOperationException)
                {
                    callback.syncAllCallback(false, null);
                }
            }
            catch (WebException)
            {
                callback.syncAllCallback(false, null);
                return;
            }
        }

        private void requestSyncAllTimeout(object callback)
        {
            cancelRequest();
            IRestCallback cb = (IRestCallback)callback;
            //if (callback != null)
            //    cb.syncAllCallback(false, null);
        }

        public void changeUsername(int id, string oldUsername, string newUsername, IRestCallback cb)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                cb.changeUsernameCallback(false, 0);
                return;
            }

            lock (this)
            {
                if (webRequest != null)
                {
                    cb.changeUsernameCallback(false, 0);
                    return;
                }
            }

            callback = cb;

            string request = String.Format(USERNAME_SERVICE + "/{0}", id);
            webRequest = (HttpWebRequest)WebRequest.Create(new Uri(request));
            webRequest.Method = "POST";

            IAsyncResult result = webRequest.BeginGetRequestStream(new AsyncCallback(requestChangeUserStream), new UserChangeRequest(oldUsername, newUsername));

            timer = new Timer(new TimerCallback(requestChangeUserTimeout), callback, 15 * 1000, Timeout.Infinite);
        }

        private void requestChangeUserStream(IAsyncResult result)
        {
            Stream postStream = webRequest.EndGetRequestStream(result);
            UserChangeRequest ucr = (UserChangeRequest)result.AsyncState;
            XmlSerializer xml = new XmlSerializer(typeof(UserChangeRequest));
            xml.Serialize(postStream, ucr);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(requestChangeUserCompleted), null);
        }

        private void requestChangeUserCompleted(IAsyncResult result)
        {
            HttpWebRequest wr = webRequest;
            lock (this)
            {
                webRequest = null;
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }

            if (wr == null || !wr.HaveResponse)
            {
                callback.changeUsernameCallback(false, 0);
                return;
            }

            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)wr.EndGetResponse(result);

                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    callback.changeUsernameCallback(false, 0);
                    return;
                }

                XmlSerializer xml = new XmlSerializer(typeof(UserChangeResponse));
                UserChangeResponse ucr = (UserChangeResponse)xml.Deserialize(webResponse.GetResponseStream());
                callback.changeUsernameCallback(true, ucr.result);
            }
            catch (WebException)
            {
                callback.changeUsernameCallback(false, 0);
                return;
            }
        }

        private void requestChangeUserTimeout(object callback)
        {
            cancelRequest();
            IRestCallback cb = (IRestCallback)callback;
            //if (callback != null)
            //    cb.changeUsernameCallback(false, 0);
        }
    }
}
