using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using SWF = System.Windows.Forms; //引用了Forms避免命名空间不明确
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EHentaiDownloader.Download;

namespace EHentaiDownloader.Controls
{
    /// <summary>
    /// DownloadPath.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPath : UserControl //引用了Forms避免命名空间不明确
    {
        public DownloadPath()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_choseFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Excel Files (*.sql)|*.sql"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                this.FilePath.Text = openFileDialog.FileName;
            }
        }
        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_chose_Click(object sender, RoutedEventArgs e)
        {
            SWF.FolderBrowserDialog m_Dialog = new SWF.FolderBrowserDialog();
            SWF.DialogResult result = m_Dialog.ShowDialog();

            if (result == SWF.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.FilePath.Text = m_Dir;
        }

        /// <summary>
        /// 开始解析并下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_download_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath.Text.Length == 0)
            {
                MessageBox.Show("请设置下载路径！");
            }
            else
            {
                if (BookId.Text.Length < 7)
                {
                    AsmDownload book1 = new AsmDownload(BookId.Text, FilePath.Text);
                    //string a = book1.askBookURL();
                    BookId.Text = book1.book.downloadPath;
                    FilePath.Text = "bookDownloadPath: " + book1.book.downloadPath;
                }
                else
                {
                    MessageBox.Show("请输入正确的编号！\n例：298224!");
                }
            }
        }
    }
}
