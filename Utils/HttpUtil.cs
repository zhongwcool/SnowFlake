using System.IO;
using System.Net.Http;

namespace SnowFlake.Utils;

public static class HttpUtil
{
    public static async Task<string> GetFileContent(string url)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        var stream = response.Content.ReadAsStreamAsync().Result;
        //实例化文件内容
        var streamReader = new StreamReader(stream);
        //读取文件内容
        var content = await streamReader.ReadToEndAsync();
        return content;
    }

    public static async Task<long> GetFileSize(string url)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        var fileSize = response.Content.ReadAsStreamAsync().Result.Length;
        return fileSize;
    }
}