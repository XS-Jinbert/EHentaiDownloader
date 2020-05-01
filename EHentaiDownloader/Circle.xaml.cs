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
    /// Circle.xaml 的交互逻辑
    /// </summary>
    public partial class Circle : UserControl
    {
        private Brush _previousFill = null; //声明私有Brush，初始值为null
        public Circle()
        {
            InitializeComponent();
        }
        public Circle(Circle c)  //重写构造
        {
            InitializeComponent();
            this.circleUI.Height = c.circleUI.Height;
            this.circleUI.Width = c.circleUI.Height;
            this.circleUI.Fill = c.circleUI.Fill;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            // 判断左键是否按下
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //声明DataObject,并打包圆控件的图像绘制方式(包含颜色)、高度及其副本。
                DataObject data = new DataObject();
                data.SetData(DataFormats.StringFormat, circleUI.Fill.ToString());
                data.SetData("Double", circleUI.Height);
                data.SetData("Object", this);

                //使用DragDrop的DoDragDrop方法开启拖动功能。拖动方式为拖动复制或移动
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);

            //Effects获取拖动类型
            if (e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.Cross); //拖动复制显示红十字
            }
            else if (e.Effects.HasFlag(DragDropEffects.Move))
            {
                Mouse.SetCursor(Cursors.Pen); //移动显示笔形
            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            //检查数据是否包含制定字符串类型数据
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                //获取字符串
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                //使用BrushConverter将字符串转换成Brush对象并应用于提供圆形控件 UI 的 Ellipse 的 Fill
                BrushConverter converter = new BrushConverter();
                if (converter.IsValid(dataString))
                {
                    Brush newFill = (Brush)converter.ConvertFromString(dataString);
                    circleUI.Fill = newFill;

                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
            }
            e.Handled = true;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            //设置Effects 为不接受数据
            e.Effects = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
                BrushConverter converter = new BrushConverter();

                //判断拖动值是否有效
                if (converter.IsValid(dataString))
                {
                    //判断Ctrl键是否按下
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
            }
            e.Handled = true;
        }
        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            //控件原始属性存放在全局私有Brush对象中
            _previousFill = circleUI.Fill;

            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
                BrushConverter converter = new BrushConverter();
                if (converter.IsValid(dataString))
                {
                    Brush newFill = (Brush)converter.ConvertFromString(dataString.ToString());
                    circleUI.Fill = newFill;
                }
            }
        }
        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            //若直接离开(未释放鼠标)，把存在全局私有变量_previousFill属性重新赋值
            circleUI.Fill = _previousFill;
        }
    }
}
