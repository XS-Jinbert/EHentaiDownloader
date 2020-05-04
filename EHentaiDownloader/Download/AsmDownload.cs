using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace EHentaiDownloader.Download
{
    class AsmDownload
    {
        string bookURL = "https://asmhentai.com/g/";
        static string BookRootPath = "Data/AsmBook/";
        int downloadPage = 1;
        int pageCount;

        public AsmBook book = new AsmBook();

        /// <summary>
        /// 初始化book信息
        /// </summary>
        /// <param name="bookid"></param>
        /// <param name="downloadPath"></param>
        public AsmDownload(string bookid, string downloadPath)
        {
            book.bookID = bookid;
            // book.downloadPath = downloadPath;
            book.downloadPath = downloadPath + "\\" + bookid;
            Parsing(askBookURL());
            saveImage(askPageURL(book.downloadPage[0]), "cover");
            saveBook(book);
        }

        /// <summary>
        /// 访问本子网站
        /// </summary>
        /// <returns></returns>
        public string askBookURL()
        {
            try
            {
                string url = bookURL + book.bookID;
                //方法一 请求访问法 该方法稍微快一点
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";// POST OR GET

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse(); // 发送请求
                string html = new StreamReader(resp.GetResponseStream()).ReadToEnd(); // 流保存

                //方法二 直接下载法 该方法稍微慢了一点
                //System.Net.WebClient client = new WebClient();
                //byte[] page = client.DownloadData(url);
                //string html = System.Text.Encoding.UTF8.GetString(page);

                return html;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "Error";
            }
        }

        /// <summary>
        /// 解析本子网站获取信息并保存入book
        /// </summary>
        public void Parsing(string html)
        {
            if (html.Equals("Error")) return;

            Regex findTitle1 = new Regex(@"<h1>(?<Title1>[\s\S]*?)</h1>");
            Match match1 = findTitle1.Match(html);
            if (match1.Success)
            {
                book.title1 = match1.Groups["Title1"].Value;
                //book.downloadPath += ("/" + book.title1);
            }

            Regex findTitle2 = new Regex(@"<h2>(?<Title2>[\s\S]*?)</h2>");
            Match match2 = findTitle2.Match(html);
            if (match2.Success)
            {
                book.title2 = match2.Groups["Title2"].Value;
            }

            // new Regex里表示要匹配的字符串
            // (?<saveID>[\s\S]*?)表示要匹配的字符串中可变的格式
            Regex findSaveID = new Regex(@"images.asmhentai.com/(?<saveID>[\s\S]*?)/"+book.bookID+"/cover.jpg");
            Match match3 = findSaveID.Match(html);
            if (match3.Success)
            {
                book.saveID = match3.Groups["saveID"].Value;
            }

            Regex findPage = new Regex(@"<h3>Pages: (?<Page>[\s\S]*?)</h3>");
            Match match4 = findPage.Match(html);
            if (match4.Success)
            {
                book.page = match4.Groups["Page"].Value;
            }


            // 用@时""表示"     不用@时\"表示"
            Regex findArtist = new Regex(@"href=""/artists/name/(?<Artist>[\s\S]*?)/"">");
            MatchCollection match5 = findArtist.Matches(html);
            book.artists = new string[match5.Count];
            for(int i = 0; i<match5.Count; i++)
            {
                book.artists[i] = match5[i].Groups["Artist"].Value;
            }

            Regex findTags = new Regex(@"href=""/tags/name/(?<Tag>[\s\S]*?)/"">");
            MatchCollection match6 = findTags.Matches(html);
            book.tags = new string[match6.Count];
            for (int i = 0; i < match6.Count; i++)
            {
                book.tags[i] = match6[i].Groups["Tag"].Value;
            }

            pageCount = int.Parse(book.page)+1;
            book.downloadPage = new string[pageCount];
            book.downloadPage[0] = "https://images.asmhentai.com/" + book.saveID + "/" + book.bookID + "/cover.jpg";
            for (int i = 1; i<pageCount; i++)
            {
                book.downloadPage[i] = "https://images.asmhentai.com/" + book.saveID + "/" + book.bookID + "/" + i.ToString() + ".jpg";
            }
        }

        /// <summary>
        /// 访问本子页
        /// </summary>
        /// <returns></returns>
        public byte[] askPageURL(string pageURL)
        {
            try
            {
                System.Net.WebClient client = new WebClient();
                byte[] image = client.DownloadData(pageURL);
                return image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                byte[] error = null;
                return error;
            }
        }

        /// <summary>
        /// 保存图片到本地
        /// </summary>
        /// <param name="image"></param>
        public void saveImage(byte[] image,string number)
        {
            if (image == null) return;
            if (!Directory.Exists(book.downloadPath))
            {
                Directory.CreateDirectory(book.downloadPath);
            }
            File.WriteAllBytes(book.downloadPath+"\\" + number + ".jpg", image);
        }

        /// <summary>
        /// 保存本子信息
        /// </summary>
        public static void saveBook(AsmBook Book)
        {
            string savePath = BookRootPath + Book.bookID + ".bin";
            if (!Directory.Exists(BookRootPath))
            {
                Directory.CreateDirectory(BookRootPath);
            }
            if (!File.Exists(savePath))
            {
                using (FileStream fs = File.Create(savePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, Book);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(savePath, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, Book);
                }
            }
        }

        /// <summary>
        /// 打开本子信息
        /// </summary>
        /// <param name="AsmBookID"></param>
        public void openBook(string AsmBookID)
        {
            using (FileStream fs = new FileStream(BookRootPath + AsmBookID + ".bin", FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                book = bf.Deserialize(fs)as AsmBook;
            }
        }

        /// <summary>
        /// 保留下载信息
        /// </summary>
        public void saveDownload()
        {

        }

        public void deleteDownload()
        {

        }
    }

    [Serializable]
    class AsmBook
    {
        public string bookID{ get; set; }
        public string title1{ get; set; }
        public string title2{ get; set; }
        public string saveID{ get; set; }
        public string page{ get; set; }
        public string[] artists { get; set; }
        public string[] tags{ get; set; }
        public string downloadPath { get; set; }
        public string[] downloadPage{ get; set; }
    }
}
