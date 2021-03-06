﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using static NicoRecoViewer.NicoVideoApiAccessor;
using System.Runtime.InteropServices;
using CefSharp;
using System.Windows.Markup;
using System.Globalization;

namespace NicoRecoViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // 参考サイト（Cookie）：http://dobon.net/vb/bbs/log3-51/30166.html#ID30166
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        // 履歴情報の数
        private int historyCount = 0;
        // 表示中の履歴情報のインデックス
        private int currentHistoryidx = 0;

        private CookieContainer cc = null;

        HistoryData historyData = null;

        List<MovieData> movieViewList = null;

        public MainWindow()
        {
            InitializeComponent();

            CefSettings settings = new CefSettings();
            //settings.PackLoadingDisabled = false;
            //settings.Locale = "ja-JP";
            //settings.LocalesDirPath = System.AppDomain.CurrentDomain.BaseDirectory + "locales";
            settings.LogSeverity = LogSeverity.Verbose;
            settings.CefCommandLineArgs.Add("enable-npapi", "1");
            Cef.Initialize(settings);
            //cefbrowser.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);
            //cefbrowser.RequestHandler = this;

            cefbrowser.PreviewTextInput += (sender, e) =>
            {
                foreach (var character in e.Text)
                {
                    cefbrowser.SendKeyEvent((int)0x0102, character, 0);
                }

                e.Handled = true;
            };

            cefbrowser.FrameLoadEnd += Cefbrowser_FrameLoadEnd;
        }

        private void Cefbrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //cefbrowser.EvaluateScriptAsync("document.getElementById (\"external_nicoplayer\").ext_setVideoSize(\"fit\")");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResponseResult result = Login();
            if(result == null)
            {
                return;
            }

            CookieCollection cl = result.CookieContainer.GetCookies(new Uri("http://www.nicovideo.jp/"));
            if (cl != null && cl.Count > 1)
            {
                Cookie cookie = cl[1];
                Cef.SetCookie("http://www.nicovideo.jp/", cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Secure, cookie.HttpOnly, cookie.Expired, cookie.Expires);

                cefbrowser.Address = Constants.NicoNicoUrl;

                // 履歴情報を取得する。
                historyData = NicoVideoApiAccessor.GetHistory(result.CookieContainer);
                if (Constants.HistoryStatus.Fail.Equals(historyData.status))
                {
                    MessageBox.Show(Constants.GetHistoryFailedMessage, Constants.CaptionError, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                movieViewList = new List<MovieData>();

                historyCount = historyData.history.Count();
                currentHistoryidx = 0;

                HistoryData.History history = historyData.history[currentHistoryidx++];

                LoadMovieList(movieViewList, cc, history);

                // 動画一覧を表示する。
                movieList.ItemsSource = movieViewList;
            }
        }

        private ResponseResult Login()
        {
            // アカウント情報入力ダイアログを表示する。
            LoginWindow loginWin = new LoginWindow();
            loginWin.ShowDialog();
            string id = loginWin.Id;
            string pass = loginWin.Password;
            bool isExited = loginWin.IsExited;
            loginWin = null;

            if(isExited)
            {
                Application.Current.Shutdown();
                return null;
            }

            // ログインする。
            ResponseResult result = NicoVideoApiAccessor.Login(id, pass);
            if(result == null)
            {
                MessageBox.Show(Constants.LoginFailedMessage, Constants.CaptionError, MessageBoxButton.OK, MessageBoxImage.Error);
                return Login();
            }
            else if(result.Result == Constants.Result.ProtocolError)
            {
                MessageBox.Show(Constants.LoginProtocolFailedMessage, Constants.CaptionError, MessageBoxButton.OK, MessageBoxImage.Error);
                return Login();
            }
            else if(result.Result == Constants.Result.Failed)
            {
                MessageBox.Show(Constants.LoginIdOrPassFailedMessage, Constants.CaptionError, MessageBoxButton.OK, MessageBoxImage.Error);
                return Login();
            }

            return result;
        }

        private void LoadMovieList(List<MovieData> list, CookieContainer cc, HistoryData.History history)
        {
            if(list.Count() != 0 && list[list.Count() - 1].Title == "更に読み込む")
            {
                list.RemoveAt(list.Count() - 1);
            }

            related_video videos = NicoVideoApiAccessor.GetRelatedMovie(cc, history.video_id);

            for (int i = 0; i < videos.data_count; i++)
            {
                MovieData data = new MovieData();
                Video video = videos.video[i];
                data.Thumbnail = video.thumbnail;
                data.Title = video.title;
                data.View = "再生：" + video.view.ToString();
                data.Comment = "コメ：" + video.comment.ToString();
                data.Mylist = "マイ：" + video.mylist.ToString();

                TimeSpan ts = new TimeSpan(0, 0, video.length);
                data.Length ="再生時間：" + ts.ToString();
                data.Url = video.url;
                data.Type = "Movie";
                list.Add(data);
            }

            if(currentHistoryidx < historyData.history.Count())
            {
                MovieData button = new MovieData();
                button.Title = "更に読み込む";
                button.Type = null;

                list.Add(button);
            }

            movieList.Items.Refresh();
        }

        private void movieList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if(lv != null)
            {
                MovieData mv = lv.SelectedItem as MovieData;
                if(mv != null)
                {
                    if(mv.Type != null)
                    {
                        //System.Diagnostics.Process.Start(mv.Url);

                        //InternetSetCookie(mv.Url, "JSESSIONID", Globals.ThisDocument.sessionID);
                        //browser.Navigate(mv.Url);
                        cefbrowser.Address = mv.Url;
                    }
                    else if(historyData.history != null && currentHistoryidx < historyData.history.Count())
                    {
                        // 更に読み込むボタンの場合
                        HistoryData.History history = historyData.history[currentHistoryidx++];
                        LoadMovieList(movieViewList, cc, history);
                    }
                }
            }
           
        }
        /*
        private void browser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            // http://d.hatena.ne.jp/kiyo_hoge/20100819/1282240857
            System.Windows.Forms.HtmlDocument document = browser.Document;
            if(document != null)
            {
                System.Windows.Forms.HtmlElement player = document.GetElementById("external_nicoplayer");
                if(player != null)
                {
                    player.InvokeMember("ext_setVideoSize", new object[] { "fit" });
                }
            }
        }*/

        private void lisenceMenu_Click(object sender, RoutedEventArgs e)
        {
            LicenseWindow lw = new LicenseWindow();
            lw.ShowDialog();
        }
    }

    public class MovieData
    {
        public MovieData()
        {

        }

        public string Url
        {
            get;
            set;
        }

        public string Thumbnail
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string View
        {
            get;
            set;
        }

        public string Comment
        {
            get;
            set;
        }

        public string Mylist
        {
            get;
            set;
        }

        public string Length
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }
    }

    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class related_video
    {
        [System.Xml.Serialization.XmlElementAttribute("total_count")]
        public int total_count { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("page_count")]
        public int page_count { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("data_count")]
        public int data_count { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("type")]
        public string type { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("video")]
        public Video[] video { get; set; }
    }

    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Video
    {
        [System.Xml.Serialization.XmlElementAttribute("url")]
        public string url { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("thumbnail")]
        public string thumbnail { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("title")]
        public string title { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("view")]
        public int view { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("comment")]
        public int comment { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("mylist")]
        public int mylist { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("length")]
        public int length { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("time")]
        public int time { get; set; }
    }

    [DataContract]
    public class HistoryData
    {
        [DataMember]
        public List<History> history { get; set; }

        [DataContract]
        public class History
        {
            [DataMember]
            public int deleted { get; set; }

            [DataMember]
            public int device { get; set; }

            [DataMember]
            public string item_id { get; set; }

            [DataMember]
            public string length { get; set; }

            [DataMember]
            public string thumbnail_url { get; set; }

            [DataMember]
            public string title { get; set; }

            [DataMember]
            public string video_id { get; set; }

            [DataMember]
            public int watch_count { get; set; }

            [DataMember]
            public int watch_date { get; set; }
        }

        [DataMember]
        public string status { get; set; }

        [DataMember]
        public string token { get; set; }
    }
}
