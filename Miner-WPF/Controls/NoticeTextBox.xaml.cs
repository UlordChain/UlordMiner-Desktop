using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Miner_WPF.Controls
{
    /// <summary>
    /// NoticeTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class NoticeTextBox : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NoticeTextBox), new PropertyMetadata(default(string)));

        public Brush TBorderBrush
        {
            get { return (Brush)GetValue(TBorderBrushProperty); }
            set { SetValue(TBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty TBorderBrushProperty = DependencyProperty.Register("TBorderBrush", typeof(Brush), typeof(NoticeTextBox), new PropertyMetadata(Brushes.Black));

        public string BText
        {
            get { return (string)GetValue(BTextProperty); }
            set { SetValue(BTextProperty, value); }
        }
        public static readonly DependencyProperty BTextProperty = DependencyProperty.Register("BText", typeof(string), typeof(NoticeTextBox), new PropertyMetadata(default(string)));

        public string UText
        {
            get { return (string)GetValue(UTextProperty); }
            set { SetValue(UTextProperty, value); }
        }
        public static readonly DependencyProperty UTextProperty = DependencyProperty.Register("UText", typeof(string), typeof(NoticeTextBox), new PropertyMetadata(default(string)));
        public Visibility UVisibility
        {
            get { return (Visibility)GetValue(UVisibilityProperty); }
            set { SetValue(UVisibilityProperty, value); }
        }
        public static readonly DependencyProperty UVisibilityProperty = DependencyProperty.Register("UVisibility", typeof(Visibility), typeof(NoticeTextBox), new PropertyMetadata(default(Visibility)));
        public NoticeTextBox()
        {
            InitializeComponent();
        }
    }
}
