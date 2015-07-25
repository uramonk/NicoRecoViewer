using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NicoRecoViewer
{
    public static class NicoVideoApiAccessor
    {
        public class ResponseResult
        {
            public ResponseResult(Constants.Result result, CookieContainer cc)
            {
                Result = result;
                CookieContainer = cc;
            }

            public Constants.Result Result { get; private set; }
            public CookieContainer CookieContainer { get; private set; }
        }

        /// <summary>
        /// ニコニコ動画にログインする。
        /// </summary>
        /// <param name="userId">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <returns></returns>
        public static ResponseResult Login(string userId, string password)
        {
            ResponseResult result = null;

            // 参考サイト：http://qiita.com/katabamisan/items/8028584b2b6224ce0c92
            string content = "mail=" + userId + "&password=" + password;
            byte[] contentBytes = Encoding.ASCII.GetBytes(content);

            HttpWebRequest request = HttpWebRequest.CreateHttp(Constants.LoginUrl);
            request.CookieContainer = new CookieContainer();
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = contentBytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(contentBytes, 0, contentBytes.Length);
            }

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                if(ex.Status == WebExceptionStatus.ProtocolError)
                {
                    return new ResponseResult(Constants.Result.ProtocolError, null);
                }

                return null;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            finally
            {
                
                if(response != null)
                {
                    Constants.Result r;
                    if(response.ResponseUri.AbsoluteUri.Contains("cant_login"))
                    {
                        r = Constants.Result.Failed;
                    }
                    else
                    {
                        r = Constants.Result.Success;
                    }

                    result = new ResponseResult(r, request.CookieContainer);
                    response.Close();
                }
            }

            return result;
        }

        public static void Logout()
        {

        }

        /// <summary>
        /// 視聴履歴を取得する。
        /// </summary>
        /// <param name="cc">ログイン時のクッキーコンテナー</param>
        /// <returns>履歴情報クラス</returns>
        public static HistoryData GetHistory(CookieContainer cc)
        {
            string json = Request(cc, Constants.HistoryUrl);
            HistoryData historyData = GetMovieDataFromJson(json);

            return historyData;
        }

        public static void GetMovie(string movieId)
        {

        }

        public static related_video GetRelatedMovie(CookieContainer cc, string videoId)
        {
            string xml = Request(cc, Constants.RelatedUrl + videoId);
            related_video list = GetRelatedVideoFromXml(xml);
            return list;
        }

        /// <summary>
        /// XMLデータから関連動画データに変換する。
        /// </summary>
        /// <param name="xml">XML</param>
        /// <returns>関連情報データ</returns>
        public static related_video GetRelatedVideoFromXml(string xml)
        {
            related_video list = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(related_video));
                    list = (related_video)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return list;
        }

        /// <summary>
        /// JSONデータから視聴履歴データに変換する。
        /// </summary>
        /// <param name="json">JSON</param>
        /// <returns>視聴履歴データ</returns>
        private static HistoryData GetMovieDataFromJson(string json)
        {
            HistoryData historyData = null;

            // 参考サイト：http://kyufast.hatenablog.jp/entry/2014/03/04/000308
            List<MovieData> list = new List<MovieData>();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HistoryData));

            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                historyData = (HistoryData)serializer.ReadObject(ms);
            }
           
            return historyData;
        }

        private static string Request(CookieContainer cc, string url)
        {
            string result = null;

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);
            request.CookieContainer = cc;
            request.ContentType = null;
            request.ContentLength =0;

            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }

                if(reader != null)
                {
                    reader.Close();
                }
            }

            return result;
        }
    }
}

