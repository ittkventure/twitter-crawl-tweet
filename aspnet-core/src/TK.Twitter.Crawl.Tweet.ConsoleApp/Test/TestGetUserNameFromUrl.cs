using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using System.Text.RegularExpressions;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestGetUserNameFromUrl : ITransientDependency
    {
        public async Task RunAsync()
        {
            // Khởi tạo một danh sách các URL
            List<string> urls = new List<string>()
            {
                "http://twitter.com/TGCasino_",
                "https://twitter.com/TGCasino_",
                "twitter.com/TGCasino_",
                "www.twitter.com/TGCasino_",
                "https://twitter.com/TGCasino_?s=21",
                "www.twitter.com/TGCasino_?s=21",
                "twitter.com/TGCasino_?s=21"
            };

            // Duyệt qua danh sách các URL
            foreach (string url in urls)
            {
                string queryParam;
                try
                {
                    queryParam = GetQueryParamAfterSlash(url);
                }
                catch
                {
                    queryParam = GetQueryParam(url);
                }

                // In ra kết quả
                Console.WriteLine("Query param: " + queryParam);
            }

            Console.ReadLine();
        }

        static string GetQueryParamAfterSlash(string url)
        {
            Uri uri = new Uri(url);
            string path = uri.AbsolutePath;
            int slashIndex = path.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < path.Length - 1)
            {
                string param = path.Substring(slashIndex + 1);
                return param;
            }
            else
            {
                return "";
            }
        }

        static string GetQueryParam(string url)
        {
            // Sử dụng Regex để lấy giá trị của query parameter
            // Bỏ qua giá trị đằng sau dấu ?
            Regex regex = new Regex(@"/([^/?]+)");
            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Trường hợp không tìm thấy
            return "Không tìm thấy";
        }
    }
}
