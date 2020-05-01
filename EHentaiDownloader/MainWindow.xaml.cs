using System;
using System.Collections.Generic;
using System.Linq;
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

namespace EHentaiDownloader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void panel_DragOver(object sender, DragEventArgs e)
        {
            //检查拖动的数据是否包含由圆形用户控件打包在 DataObject 中并且在 DoDragDrop 调用中传递的“对象”数据
            if (e.Data.GetDataPresent("Object"))
            {
                if (e.KeyStates == DragDropKeyStates.ControlKey)
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
        }
        private void panel_Drop(object sender, DragEventArgs e)
        {
            if (e.Handled == false)
            {
                Panel _panel = (Panel)sender; //取得当前panel对象
                UIElement _element = (UIElement)e.Data.GetData("Object");//移动对象转换成WPF核心基类对象 

                if (_panel != null && _element != null)
                {
                    //获取移动对象的父对象
                    Panel _parent = (Panel)VisualTreeHelper.GetParent(_element);

                    if (_parent != null)
                    {
                        if (e.KeyStates == DragDropKeyStates.ControlKey &&
                            e.AllowedEffects.HasFlag(DragDropEffects.Copy))
                        {
                            Circle _circle = new Circle((Circle)_element);
                            _panel.Children.Add(_circle); //在其子元素中添加对象

                            e.Effects = DragDropEffects.Copy;
                        }
                        else if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
                        {
                            _parent.Children.Remove(_element); //移动时，从父对象中移除源文件
                            _panel.Children.Add(_element);

                            e.Effects = DragDropEffects.Move;
                        }
                    }
                }
            }
        }
    }
}
