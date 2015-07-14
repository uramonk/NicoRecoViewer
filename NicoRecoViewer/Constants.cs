using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoRecoViewer
{
    public static class Constants
    {
        public static readonly string LoginUrl = "https://secure.nicovideo.jp/secure/login?site=niconico";
        public static readonly string HistoryUrl = "http://www.nicovideo.jp/api/videoviewhistory/list";
        public static readonly string RelatedUrl = "http://www.nicovideo.jp/api/getrelation?page=1&sort=p&order=d&video=";

        public static readonly string LoginMessage = "メールアドレスとパスワードを入力してください。";
        public static readonly string LoginFailedMessage = "メールアドレスもしくはパスワードが間違っています。";
        public static readonly string GetHistoryFailedMessage = "視聴履歴を取得できませんでした。";

        public static readonly string CaptionError = "Error";

        public static class HistoryStatus
        {
            public static readonly string Fail = "fail";
        }

        public enum Result
        {
            Failed,
            Success
        }
    }
}
