
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using DataTemplateTestModel.ImageProcessor;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AppTest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    
    public sealed partial class DetailPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<File> ImageInfos;
        ///绑定FileManager数据

        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapImage _showedImage;
        public BitmapImage ShowedImage
        {
            get => _showedImage;
            set
            {
                if (_showedImage != value)
                {
                    _showedImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowedImage)));
                }
            }
        }
        ///绑定当前图片数据
        
        public DetailPage()
        {
            ShowedImage = FileManager.ShowedImageFile.bitmapImage;
            ImageInfos = FileManager.ImageInfos;
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }
        ///DetailPage初始化

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameDetail.Navigate(typeof(MainPage));
        }
        ///返回按钮


        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FileManager.ShowedImageFile = (File)e.ClickedItem;
            ShowedImage = FileManager.ShowedImageFile.bitmapImage;
        }
        ///点击ListView切换图片

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double v = e.NewValue;
            ImageDisplay.Width = 300 * v/5;
        }
        ///缩放控制
    }
}
