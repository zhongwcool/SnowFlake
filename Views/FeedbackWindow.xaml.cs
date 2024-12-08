using System.Text;
using System.Windows;
using System.Windows.Controls;
using Mar.Cheese;
using SnowFlake.Utils;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using MessageBox = System.Windows.MessageBox;

namespace SnowFlake.Views;

public partial class FeedbackWindow : Window
{
    public FeedbackWindow()
    {
        InitializeComponent();

        HttpUtil.GetFileContent("https://raw.githubusercontent.com/zhongwcool/SnowFlake/main/Assets/news.txt")
            .ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    Dispatcher.Invoke(() => TxtNews.Text = task.Result);
                }
            });
    }

    private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetData(DataFormats.Text, TxtFeedback.Text);
    }

    private void EmailSupport_Click(object sender, RoutedEventArgs e)
    {
        // 需要一个时间戳作为标题
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var builder = new StringBuilder();
        builder.Append(TxtFeedback.Text);
        builder.Append(Environment.NewLine);

        var task = SystemUtil.GetSystemInfo();
        task.ContinueWith(_ =>
        {
            builder.Append(task.Result);
            var body = builder.ToString();
            SendEmail("zhongwcool@163.com", $"用户反馈-{timestamp}", body);
            // 这里处理提交逻辑，例如保存反馈信息
            MessageBox.Show("反馈已提交！");
            Dispatcher.Invoke(Close);
        });
    }

    private static void SendEmail(string to, string subject, string body)
    {
        var task = EmailUtil.SendEmailAsync("2872700763@qq.com", to, subject, body,
            "smtp.qq.com", 587, "2872700763", "utyydjctjirrdfgc"
        );

        task.ContinueWith(_ =>
        {
            if (task.Result)
            {
                "邮件发送成功！".PrintGreen();
            }
            else
            {
                "邮件发送失败！".PrintErr();
            }
        });
    }

    private void TxtFeedback_TextChanged(object sender, TextChangedEventArgs e)
    {
        var wordCount = TxtFeedback.Text.Length;
        LblWordCount.Content = wordCount < 10 ? $"{wordCount}/不低于10个字符" : $"字数：{wordCount}";
        BtnSubmit.IsEnabled = TxtFeedback.Text.Length >= 10; // 当文本长度超过10时启用按钮
    }
}