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

//using System.Drawing;

namespace CC98.CardGenerator
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string userinfoUrl = @"http://www.cc98.org/dispuser.asp?name=";
		private HttpClient httpClient;
		private UserInfo userInfo;

		public MainWindow()
		{
			InitializeComponent();
			httpClient = new HttpClient();
			userInfo=new UserInfo();
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

			var cardimg= new BitmapImage();
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
					portraitx += (200 - resizedimg.PixelWidth)/2;
				if (resizedimg.PixelHeight < 200)
					portraity += (200 - resizedimg.PixelHeight)/2;
				dc.DrawImage(resizedimg, new Rect(portraitx, portraity, resizedimg.PixelWidth, resizedimg.PixelHeight));

				var username = new FormattedText(userInfo.UserName, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("黑体"), FontStyles.Normal, FontWeights.Bold, FontStretches.Medium), 26, brush,null, TextFormattingMode.Ideal);
				
				var geo = username.BuildGeometry(new Point(32, 32));
				dc.DrawGeometry(brush, new Pen(Brushes.LightGoldenrodYellow, 0.8), geo);
				//dc.DrawText(username, new Point(32, 32));

				var date = "注册时间: " + userInfo.RegisterDate.Date.ToLongDateString();
				var registerdate = new FormattedText(date, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("华文楷体"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium), 18, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(registerdate, new Point(33, 315));

				var ge = "性别: " + userInfo.Gender;
				var gender = new FormattedText(ge, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium), 14, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(gender, new Point(240, 140));

				var ti = "头衔: " + userInfo.Tittle;
				var tittle = new FormattedText(ti, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium), 14, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(tittle, new Point(240, 180));

				var fa = "门派: " + userInfo.Faction;
				var faction = new FormattedText(fa, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium), 14, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(faction, new Point(240, 220));

				var ra = "等级: " + userInfo.Rank;
				var rank = new FormattedText(ra, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium), 14, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(rank, new Point(240, 260));

				var hp = new FormattedText(userInfo.HP.ToString(), CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight,
					new Typeface(new FontFamily("华文行楷"), FontStyles.Normal, FontWeights.ExtraLight, FontStretches.Medium), 36, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(hp, new Point(80, 400));

				var pr = new FormattedText(userInfo.Prestige.ToString(), CultureInfo.GetCultureInfo("zh-cn"),
					FlowDirection.LeftToRight,
					new Typeface(new FontFamily("华文行楷"), FontStyles.Normal, FontWeights.ExtraLight, FontStretches.Medium), 36, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(pr, new Point(270, 400));

				var de = new FormattedText(DescriptionTextBox.Text, CultureInfo.GetCultureInfo("zh-cn"),
					FlowDirection.LeftToRight,
					new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Light, FontStretches.Medium), 18, brush, null, TextFormattingMode.Ideal);
				dc.DrawText(de, new Point(40, 520));
			}

			ShowImage.Source = new DrawingImage(dg);
			var drawingimagesource = new DrawingImage(dg);
			var drawingimage = new Image { Source = drawingimagesource };
			drawingimage.Arrange(new Rect(0, 0, width, height));
			
			var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
			bitmap.Render(drawingimage);
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			
			var resultfilename = Directory.GetCurrentDirectory() + @"\"+CardNumTextBox.Text+@".png";
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

			userInfo = await GetUserInfo();

			var portraitimg = new BitmapImage();
			portraitimg.DownloadCompleted += OnPortraitDownloadCompleted;
			portraitimg.BeginInit();
			portraitimg.UriSource = new Uri(userInfo.PortraitUrl);
			portraitimg.EndInit();
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
			if(DateTime.TryParse(WebUtility.HtmlDecode(match.Groups[1].Value),out date))
			{
				userinfo.RegisterDate = date;
			}

			var hppattern = @"/<font color=""#ff0000"">(.+)</font>";
			match = Regex.Match(str, hppattern);
			int hp;
			if(int.TryParse(WebUtility.HtmlDecode(match.Groups[1].Value),out hp))
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
