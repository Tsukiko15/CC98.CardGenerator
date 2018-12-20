using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Media.Effects;
using Newtonsoft.Json;


//using System.Drawing;

namespace CC98.CardGenerator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string userinfoUrl = @"http://www.cc98.org/dispuser.asp?name=";
        private const string userinfoApiUrl = @"https://api.cc98.org/user/name/";
        private HttpClient httpClient;
        private UserInfoV2 userInfo;

        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            userInfo = new UserInfoV2();
        }


        private void OnPortraitDownloadCompleted(object sender, EventArgs e)
        {
            var portraitimg = (BitmapImage) sender;
            Debug.WriteLine("original image size: {0} * {1}", portraitimg.Width, portraitimg.Height);
            Debug.WriteLine("original image pixel size: {0} * {1}", portraitimg.PixelWidth, portraitimg.PixelHeight);
            var sc = new ScaleTransform();
            if (portraitimg.PixelWidth > portraitimg.PixelHeight)
            {
                sc.ScaleX = 200.0 / portraitimg.PixelWidth;
                sc.ScaleY = 200.0 / portraitimg.PixelWidth;
            }
            else
            {
                sc.ScaleY = 200.0 / portraitimg.PixelHeight;
                sc.ScaleX = 200.0 / portraitimg.PixelHeight;
            }

            var resizedimg = new TransformedBitmap();
            resizedimg.BeginInit();
            resizedimg.Source = portraitimg;
            resizedimg.Transform = sc;
            resizedimg.EndInit();

            ShowImage.Source = resizedimg;
            Debug.WriteLine("resized image size: {0} * {1}", resizedimg.Width, resizedimg.Height);
            Debug.WriteLine("resized image pixel size: {0} * {1}", resizedimg.PixelWidth, resizedimg.PixelHeight);

            var imgfilename = Directory.GetCurrentDirectory();
            Brush brush;

            if (NRadioButton.IsChecked == true)
            {
                imgfilename += @"\N.bmp";
                brush = new SolidColorBrush(Color.FromRgb(35, 98, 98));
            }
            else if (RRadioButton.IsChecked == true)
            {
                imgfilename += @"\R.bmp";
                brush = new SolidColorBrush(Color.FromRgb(15, 79, 121));
            }
            else if (SRRadioButton.IsChecked == true)
            {
                imgfilename += @"\SR.bmp";
                brush = new SolidColorBrush(Color.FromRgb(129, 108, 33));
            }
            else
            {
                imgfilename += @"\SSR.bmp";
                brush = new SolidColorBrush(Color.FromRgb(51, 118, 116));
            }

            var cardimg = new BitmapImage();
            //cardimg.DownloadCompleted += OnCardTemplateDownloadCompleted;
            cardimg.BeginInit();
            cardimg.UriSource = new Uri(imgfilename);
            cardimg.EndInit();

            var width = cardimg.PixelWidth;
            var height = cardimg.PixelHeight;
            var dg = new DrawingGroup();

            //ShowImage.Source = cardimg;
            using (var dc = dg.Open())
            {
                dc.DrawImage(cardimg, new Rect(0, 0, width, height));

                var portraitx = 31;
                var portraity = 107;
                if (resizedimg.PixelWidth < 200)
                    portraitx += (200 - resizedimg.PixelWidth) / 2;
                if (resizedimg.PixelHeight < 200)
                    portraity += (200 - resizedimg.PixelHeight) / 2;
                dc.DrawImage(resizedimg, new Rect(portraitx, portraity, resizedimg.PixelWidth, resizedimg.PixelHeight));

                var username = new FormattedText(userInfo.Name, CultureInfo.GetCultureInfo("zh-cn"),
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("黑体"), FontStyles.Normal, FontWeights.Bold, FontStretches.Medium), 26,
                    brush, null, TextFormattingMode.Ideal);

                var geo = username.BuildGeometry(new Point(32, 32));
                dc.DrawGeometry(brush, new Pen(Brushes.LightGoldenrodYellow, 0.8), geo);
                //dc.DrawText(username, new Point(32, 32));

                var date = "注册时间: " + userInfo.RegisterTime.ToLongDateString();
                var registerdate = new FormattedText(date, CultureInfo.GetCultureInfo("zh-cn"),
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("华文楷体"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium),
                    18, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(registerdate, new Point(33, 315));

                string genderstr;
                switch (userInfo.Gender)
                {
                    case UserGender.Male:
                        genderstr = "男";
                        break;
                    case UserGender.Female:
                        genderstr = "女";
                        break;
                    default:
                        genderstr = "未知";
                        break;
                }

                var ge = "性别: " + genderstr;
                var gender = new FormattedText(ge, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium),
                    14, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(gender, new Point(240, 140));

                var ti = "风评: " + userInfo.Popularity;
                var tittle = new FormattedText(ti, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium),
                    14, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(tittle, new Point(240, 180));

                var fa = "帖数: " + userInfo.PostCount;
                var faction = new FormattedText(fa, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium),
                    14, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(faction, new Point(240, 220));

                string rankstr=userInfo.LevelTitle;
                if (userInfo.Privilege != "注册用户")
                    rankstr = userInfo.Privilege;
                else if (userInfo.BoardMasterTitles.Any(m => m.BoardMasterLevel == 11))
                    rankstr = "版主";
                else if (userInfo.BoardMasterTitles.Any(m => m.BoardMasterLevel == 23))
                    rankstr = "实习版主";
                else if (userInfo.BoardMasterTitles.Any(m => m.BoardMasterLevel == 13))
                    rankstr = "VIP";
                else if (userInfo.BoardMasterTitles.Any(m => m.BoardMasterLevel == 4))
                    rankstr = "认证用户";
                var ra = "权限: " + rankstr;
                var rank = new FormattedText(ra, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium),
                    14, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(rank, new Point(240, 260));

                var userlife = GetUserLife(userInfo);
                var hp = new FormattedText(userlife.ToString(), CultureInfo.GetCultureInfo("zh-cn"),
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("华文行楷"), FontStyles.Normal, FontWeights.ExtraLight,
                        FontStretches.Medium), 36, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(hp, new Point(80, 400));

                var pr = new FormattedText(userInfo.Prestige.ToString(), CultureInfo.GetCultureInfo("zh-cn"),
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("华文行楷"), FontStyles.Normal, FontWeights.ExtraLight,
                        FontStretches.Medium), 36, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(pr, new Point(270, 400));

                var de = new FormattedText(DescriptionTextBox.Text, CultureInfo.GetCultureInfo("zh-cn"),
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium),
                    18, brush, null, TextFormattingMode.Ideal);
                dc.DrawText(de, new Point(40, 520));
            }

            ShowImage.Source = new DrawingImage(dg);
            var drawingimagesource = new DrawingImage(dg);
            var drawingimage = new Image {Source = drawingimagesource};
            drawingimage.Arrange(new Rect(0, 0, width, height));

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingimage);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            var resultfilename = Directory.GetCurrentDirectory() + @"\" + CardNumTextBox.Text + @".png";
            using (var stream = new FileStream(resultfilename, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(IdTextBox.Text))
            {
                return;
            }

            userInfo = await GetUserInfoByApiAsync();

            var portraitimg = new BitmapImage();
            portraitimg.DownloadCompleted += OnPortraitDownloadCompleted;
            portraitimg.BeginInit();
            portraitimg.UriSource = new Uri(userInfo.PortraitUrl);
            portraitimg.EndInit();
        }

        private async Task<UserInfoV2> GetUserInfoByApiAsync()
        {
            var url = userinfoApiUrl + WebUtility.UrlEncode(IdTextBox.Text);
            var str = await httpClient.GetStringAsync(url);
            if (string.IsNullOrEmpty(str))
                return null;
            return JsonConvert.DeserializeObject<UserInfoV2>(str);
        }

        private async Task<UserInfo> GetUserInfo()
        {
            var url = userinfoUrl + WebUtility.UrlEncode(IdTextBox.Text);
            var userinfo = new UserInfo() {UserName = IdTextBox.Text};
            var str = await httpClient.GetStringAsync(url);

            Match match;

            var tittlepattern = @"头衔： (.+)<br>用户等级";
            match = Regex.Match(str, tittlepattern);
            userinfo.Tittle = WebUtility.HtmlDecode(match.Groups[1].Value);

            var factionpattern = @"门派： (.+)<br>精华";
            match = Regex.Match(str, factionpattern);
            userinfo.Faction = WebUtility.HtmlDecode(match.Groups[1].Value);

            var genderpattern = @"性 别： (.+)<br>生";
            match = Regex.Match(str, genderpattern);
            var ge = WebUtility.HtmlDecode(match.Groups[1].Value);
            if (string.IsNullOrEmpty(ge))
                userinfo.Gender = "未知";
            else
                userinfo.Gender = ge;

            var registerdatepattern = @"注册时间： (.+)<br>登录次数";
            match = Regex.Match(str, registerdatepattern);
            DateTime date;
            if (DateTime.TryParse(WebUtility.HtmlDecode(match.Groups[1].Value), out date))
            {
                userinfo.RegisterDate = date;
            }

            var hppattern = @"/<font color=""#ff0000"">(.+)</font>";
            match = Regex.Match(str, hppattern);
            int hp;
            if (int.TryParse(WebUtility.HtmlDecode(match.Groups[1].Value), out hp))
            {
                userinfo.HP = hp;
            }

            var rankpattern = @"用户等级： (.+)<br>用户门派";
            match = Regex.Match(str, rankpattern);
            userinfo.Rank = WebUtility.HtmlDecode(match.Groups[1].Value);

            var prestigepattern = @"用户威望： (.+)<br>注册时间";
            match = Regex.Match(str, prestigepattern);
            int prestige;
            if (int.TryParse(WebUtility.HtmlDecode(match.Groups[1].Value), out prestige))
            {
                if (prestige < -20)
                    prestige = -prestige;
                userinfo.Prestige = prestige;
            }

            var portraitpattern = @"&nbsp;<img src='(http.+)' width='.+' height='.+' align=""absmiddle""><br>";
            match = Regex.Match(str, portraitpattern);
            userinfo.PortraitUrl = WebUtility.HtmlDecode(match.Groups[1].Value);
            if (string.IsNullOrEmpty(userinfo.PortraitUrl))
            {
                portraitpattern = @"&nbsp;<img src='(Preset.+)' width=";
                match = Regex.Match(str, portraitpattern);
                userinfo.PortraitUrl = WebUtility.HtmlDecode(match.Groups[1].Value);
                userinfo.PortraitUrl = @"http://www.cc98.org/" + userinfo.PortraitUrl;
            }

            Debug.WriteLine(userinfo.Faction);

            return userinfo;
        }

        private int GetUserLife(UserInfoV2 userInfo)
        {
            double lifeByArticle = 0, lifeByPower = 0, lifeByRegDate = 0, lifeByPopularity = 0;

            if (userInfo.PostCount > 40000)
                lifeByArticle = 204.5 + (userInfo.PostCount - 40000) * 0.00125;
            else if (userInfo.PostCount > 30000)
                lifeByArticle = 191.5 + (userInfo.PostCount - 30000) * 0.0013;
            else if (userInfo.PostCount > 20000)
                lifeByArticle = 177.5 + (userInfo.PostCount - 20000) * 0.0014;
            else if (userInfo.PostCount > 10000)
                lifeByArticle = 162.5 + (userInfo.PostCount - 10000) * 0.0015;
            else if (userInfo.PostCount > 7500)
                lifeByArticle = 157.5 + (userInfo.PostCount - 7500) * 0.002;
            else if (userInfo.PostCount > 5000)
                lifeByArticle = 150 + (userInfo.PostCount - 5000) * 0.003;
            else if (userInfo.PostCount > 2500)
                lifeByArticle = 137.5 + (userInfo.PostCount - 2500) * 0.005;
            else if (userInfo.PostCount > 1500)
                lifeByArticle = 122.5 + (userInfo.PostCount - 1500) * 0.015;
            else if (userInfo.PostCount > 1000)
                lifeByArticle = 110 + (userInfo.PostCount - 1000) * 0.025;
            else if (userInfo.PostCount > 750)
                lifeByArticle = 72.5 + (userInfo.PostCount - 750) * 0.15;
            else if (userInfo.PostCount > 500)
                lifeByArticle = 41.25 + (userInfo.PostCount - 500) * 0.125;
            else if (userInfo.PostCount > 250)
                lifeByArticle = 16.25 + (userInfo.PostCount - 250) * 0.1;
            else if (userInfo.PostCount > 100)
                lifeByArticle = 5 + (userInfo.PostCount - 100) * 0.075;
            else if (userInfo.PostCount > 0)
                lifeByArticle = userInfo.PostCount * 0.05;
            else lifeByArticle = 0;

            if (userInfo.Prestige > 200)
                lifeByPower = 200;
            else if (userInfo.Prestige > 150)
                lifeByPower = 100 + (userInfo.Prestige - 150) * 2;
            else if (userInfo.Prestige > 100)
                lifeByPower = 60 + (userInfo.Prestige - 100) * 0.8;
            else if (userInfo.Prestige > 60)
                lifeByPower = 30 + (userInfo.Prestige - 60) * 0.75;
            else if (userInfo.Prestige > 10)
                lifeByPower = 10 + (userInfo.Prestige - 10) * 0.4;
            else if (userInfo.Prestige > 0)
                lifeByPower = userInfo.Prestige;
            else if (userInfo.Prestige > -20)
                lifeByPower = userInfo.Prestige * 3;
            else lifeByPower = 200;

            var regDate = (DateTime.Today - userInfo.RegisterTime).Days;
            if (regDate > 1000)
                lifeByRegDate = 200;
            else lifeByRegDate = 0.2 * regDate;

            if (userInfo.Popularity > 100)
                lifeByPopularity = 300;
            else if (userInfo.Popularity > 0)
                lifeByPopularity = 3 * userInfo.Popularity;
            else lifeByPopularity = 0.5 * userInfo.Popularity;

            var userLife = (int)(lifeByArticle + lifeByPower + lifeByRegDate + lifeByPopularity + 98);
            if (userLife > 998)
                userLife = 998;
            else if (userLife < 98)
                userLife = 98;
            return userLife;


        }
    }

    public class UserInfoV2
    {
        /// <summary>
        /// 该用户的标识。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 该用户的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 该用户的自定义头衔。
        /// </summary>
        public string CustomTitle { get; set; }

        /// <summary>
        /// 该用户的文章数。
        /// </summary>
        public int PostCount { get; set; }

        /// <summary>
        /// 该用户的威望。
        /// </summary>
        public int Prestige { get; set; }

        /// <summary>
        /// 该用户全站权限等级名称。
        /// 4种：管理员，超级版主，注册用户，全站贵宾
        /// </summary>
        public string Privilege { get; set; }

        /// <summary>
        /// 该用户的门派。
        /// </summary>
        public string Faction { get; set; }

        /// <summary>
        /// 该用户的用户组名称。
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 该用户注册的时间。
        /// </summary>
        public DateTime RegisterTime { get; set; }

        /// <summary>
        /// 该用户最后登录的时间。
        /// </summary>
        public DateTime LastLogOnTime { get; set; }

        /// <summary>
        /// 该用户是否在线。
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 该用户头像的图片 URL 地址。
        /// </summary>
        public string PortraitUrl { get; set; }

        /// <summary>
        /// 该用户签名档的代码。
        /// </summary>
        public string SignatureCode { get; set; }

        /// <summary>
        /// 该用户的性别。
        /// </summary>
        public UserGender Gender { get; set; }

        /// <summary>
        /// 该用户是否认证。
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// 该用户显示的头衔。
        /// </summary>
        public string DisplayTitle { get; set; }

        /// <summary>
        /// 该用户显示的头衔id。
        /// </summary>
        public int? DisplayTitleId { get; set; }

        /// <summary>
        /// 该用户邮箱地址。
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// 该用户的生日。
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 该用户的 QQ 账号。
        /// </summary>
        public string QQ { get; set; }

        /// <summary>
        /// 该用户的财富值。
        /// </summary>
        public int Wealth { get; set; }

        /// <summary>
        /// 该用户最后登录的IP。
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// 该用户关注的版面标识列表。
        /// </summary>
        public int[] CustomBoards { get; set; }

        /// <summary>
        /// 该用户的粉丝数。
        /// </summary>
        public int FanCount { get; set; }

        /// <summary>
        /// 该用户的关注数。
        /// </summary>
        public int FollowCount { get; set; }

        /// <summary>
        /// 该用户是否被当前登录的用户关注。
        /// </summary>
        public bool IsFollowing { get; set; }

        /// <summary>
        /// 该用户发帖数等级。
        /// </summary>
        public string LevelTitle { get; set; }

        /// <summary>
        /// 该用户的个人介绍
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 该用户的风评值。
        /// </summary>
        public int Popularity { get; set; }

        /// <summary>
        /// 该用户的被删帖数。为负值。
        /// </summary>
        public int DeleteCount { get; set; }

        /// <summary>
        /// 该用户的当前锁定状态。
        /// </summary>
        public UserLockState LockState { get; set; }

        /// <summary>
        /// 该用户使用的论坛主题方案。
        /// </summary>
        public int Theme { get; set; }

        /// <summary>
        /// 该用户拥有的所有用户组id。
        /// </summary>
        public IEnumerable<int> UserTitleIds { get; set; }

        /// <summary>
        /// 该用户绑定的手机号。
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 该用户是否绑定手机。
        /// </summary>
        public bool HasPhoneNumber { get; set; }

        /// <summary>
        /// 指示当前小程序状态，测试用户返回1，普通用户正常返回0，普通用户维护返回。
        /// </summary>
        public int IsSUser { get; set; }

        /// <summary>
        /// 该用户拥有的版面头衔（版主、实习版主、版面贵宾、区务）。
        /// </summary>
        public BoardMasterInfo[] BoardMasterTitles { get; set; }
    }

    public enum UserGender
    {
        Male = 1,
        Female = 0
    }

    public enum UserLockState
    {
        /// <summary>
        /// 正常用户
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 锁定用户
        /// </summary>
        Locked = 1,
        /// <summary>
        /// 屏蔽用户
        /// </summary>
        Shielded = 2,
        /// <summary>
        /// 全站tp用户
        /// </summary>
        StopPost = 3
    }

    public class BoardMasterInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int BoardId { get; set; }
        public string BoardName { get; set; }
        public string Title { get; set; }
        public int BoardMasterLevel { get; set; }
    }

    public class UserInfo
	{
		//用户名
		public string UserName { set; get; }
		//头衔
		public string Tittle { set; get; }
		//门派
		public string Faction { set; get; }
		//性别
		public string Gender { set; get; }
		//注册日期
		public DateTime RegisterDate { set; get; }
		//血条
		public int HP { set; get; }
		//用户等级
		public string Rank { set; get; }
		//威望
		public int Prestige { set; get; }
		//头像地址
		public string PortraitUrl { set; get; }
	}
}
