using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EHentaiDownloader.Download
{
    class AsmDownloadControl
    {
        const string AsmRoot = "Data/AsmData/";
        const string AsmHistoryIDPath = "Data/AsmData/historyID.txt";

        const string BookRootPath = "Data/AsmBook/";
        const string bookURL = "https://asmhentai.com/g/";

        // BlockingCollection快捷实现生产者消费者问题  // 分析对列
        static readonly BlockingCollection<string> parsingQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
        static Queue<string> downloadWaitQ = new Queue<string>();        // 等待下载队列

        public AsmDownloadControl()
        {
            // 添加BookID线程
            Task.Run(() =>
            {
                while (!parsingQueue.IsCompleted)
                {
                    string bookID = "";
                    bookID = parsingQueue.Take();
                    string html = askBookURL(bookID);
                    AsmBook book = Parsing(html, bookID);
                    if (book == null) return;
                    saveBook(book);
                    downloadWaitQ.Enqueue(bookID);
                }
            }
            );
        }

        public bool addId(string bookID)
        {
            if (CheckDownloadID(bookID))
            {
                MessageBox.Show("已下载或正在下载该本子，请到下载队列或历史记录查看！");
                return false;
            }
            else
            {
                parsingQueue.Add(bookID);
                return true;
            }
        }

        // 判断是否重复
        private bool CheckDownloadID(string ID)
        {
            if (!Directory.Exists(AsmRoot))
            {
                Directory.CreateDirectory(AsmRoot);
            }
            using (FileStream fs = new FileStream(AsmHistoryIDPath, FileMode.OpenOrCreate))
            {
                StreamReader sr = new StreamReader(fs);
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Equals(ID)) return true;
                }
                StreamWriter sw = new StreamWriter(fs);
                fs.Position = fs.Length;//设置尾部添加
                sw.WriteLine(ID);
                sw.Close();
            }
            return false;
        }

        /// <summary>
        /// 访问本子网站
        /// </summary>
        /// <param name="bookID"></param>
        /// <returns></returns>
        private static string askBookURL(string bookID)
        {
            try
            {
                string url = bookURL + bookID;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "Error";
            }
        }

        /// <summary>
        /// 解析本子网站获取信息并保存入book
        /// </summary>
        /// <param name="html"></param>
        /// <param name="bookID"></param>
        /// <returns></returns>
        private static AsmBook Parsing(string html, string bookID)
        {
            if (html.Equals("Error")) return null;  // 如果html为空则直接退出
            AsmBook book = new AsmBook();

            book.bookID = bookID;

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
            Regex findSaveID = new Regex(@"images.asmhentai.com/(?<saveID>[\s\S]*?)/" + book.bookID + "/cover.jpg");
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
            for (int i = 0; i < match5.Count; i++)
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

            int pageCount = int.Parse(book.page) + 1;
            book.downloadPageURL = new string[pageCount];
            book.downloadPageURL[0] = "https://images.asmhentai.com/" + book.saveID + "/" + book.bookID + "/cover.jpg";
            for (int i = 1; i < pageCount; i++)
            {
                book.downloadPageURL[i] = "https://images.asmhentai.com/" + book.saveID + "/" + book.bookID + "/" + i.ToString() + ".jpg";
            }

            return book;
        }

        /// <summary>
        /// 访问本子图片页
        /// </summary>
        /// <param name="pageURL"></param>
        /// <returns></returns>
        private byte[] askPageURL(string pageURL)
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
        /// <param name="downloadPath"></param>
        /// <param name="number"></param>
        private void saveImage(byte[] image, string downloadPath, string number)
        {
            if (image == null) return;
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            File.WriteAllBytes(downloadPath + "\\" + number + ".jpg", image);
        }

        /// <summary>
        /// 保存本子信息
        /// </summary>
        /// <param name="Book"></param>
        private static void saveBook(AsmBook Book)
        {
            string savePath = BookRootPath + Book.bookID + ".txt";
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
        /// 读取本子信息
        /// </summary>
        /// <param name="AsmBookID"></param>
        /// <returns></returns>
        private AsmBook openBook(string AsmBookID)
        {
            using (FileStream fs = new FileStream(BookRootPath + AsmBookID + ".txt", FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(fs) as AsmBook;
            }
        }

        private void saveDownload()
        {

        }

        private void deleteDownload()
        {

        }
    }

    [Serializable]
    class AsmBook
    {
        public string bookID { get; set; }
        public string title1 { get; set; }
        public string title2 { get; set; }
        public string saveID { get; set; }
        public string page { get; set; }
        public string[] artists { get; set; }
        public string[] tags { get; set; }
        public string downloadPath { get; set; }
        public string[] downloadPageURL { get; set; }
    }

}
