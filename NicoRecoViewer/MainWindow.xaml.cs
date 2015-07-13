using System;
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

namespace NicoRecoViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<MovieData> list = new List<MovieData>();
            MovieData data = new MovieData();
            //data.Image = "";
            data.Title = "Title";
            list.Add(data);
            movieList.ItemsSource = list;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // アカウント情報入力ダイアログを表示する。

            // ログインする。
            CookieContainer cc = NicoVideoApiAccessor.Login("", "");

            // 履歴情報を取得する。
            HistoryData historyData = NicoVideoApiAccessor.GetHistory(cc);
            List<MovieData> list = new List<MovieData>();
            foreach (var history in historyData.history)
            {
                // 関連動画情報を取得する。
                related_video videos = NicoVideoApiAccessor.GetRelatedMovie(cc, history.video_id);
                for(int i = 0; i < videos.data_count; i++)
                {
                    MovieData data = new MovieData();
                    Video video = videos.video[i];
                    data.Thumbnail = video.thumbnail;
                    data.Title = video.title;
                    data.Url = video.url;
                    list.Add(data);
                }
            }

            // 動画一覧を表示する。
            movieList.ItemsSource = list;
        }

        private void movieList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if(lv != null)
            {
                MovieData mv = lv.SelectedItem as MovieData;
                if(mv != null)
                {
                    System.Diagnostics.Process.Start(mv.Url);
                }
            }
           
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

        public string Tag
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
