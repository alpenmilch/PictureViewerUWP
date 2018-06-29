using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using DataTemplateTestModel.ImageProcessor;
using Windows.Storage;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Graphics.Imaging;
using Windows.UI.Popups;


// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace AppTest
{

    public sealed partial class MainPage : Page , INotifyPropertyChanged
    {
        private ObservableCollection<File> FolderInfos;
        private ObservableCollection<File> ImageInfos;
        ///用于绑定数据集合
        public event PropertyChangedEventHandler PropertyChanged;

        private MainPage Current;

        private string _currentFolderName;
        public string CurrentFolderName {
            get => _currentFolderName;
            set {
                if (_currentFolderName != value) {
                    _currentFolderName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFolderName)));
                    
                }
            }
        }
        ///绑定当前文件夹名
        

        private string _clickable;
        public string Clickable
        {
        get => _clickable;
            set
            {
                if (_clickable != value)
                {
                    _clickable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Clickable)));
                }
            }
        }
        ///绑定按钮或文件夹是否可以点击

        private string _savePath;
        public string SavePath
        {
            get => _savePath;
            set
            {
                if (_savePath != value)
                {
                    _savePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SavePath)));
                }
            }
        }
        ///绑定格式转化框文件目录


        public MainPage()
        {
            Current = this;
            Initialize();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        }
        ///MainPage初始化
        
        private async void Initialize() {

            await BindData();
            
            Current.InitializeComponent();
        }
        ///初始化
        
        private async Task BindData() {
            if (!FileManager.PageChanged)
            {
                await FileManager.SetPrimaryFolder();
                await FileManager.LoadFolder();
            }
            ///判断是否切换过页面，避免重复初始化
            Clickable = "True";
            CurrentFolderName = FileManager.CurrentFolder.Name;
            FolderInfos = FileManager.FolderInfos;
            ImageInfos = FileManager.ImageInfos;
            ///绑定数据
        }


        private async void FolderSelector_ItemClick(object sender, ItemClickEventArgs e)
        {
            Clickable = "False";
            ///锁死
            var folder = (StorageFolder)(((File)e.ClickedItem).item);
            CurrentFolderName = folder.Name;
            await FileManager.RefreshFolder(folder);
            ///更改文件夹并重载
            Clickable = "True";
            ///解锁
        }
        ///点击文件夹

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Off");
            Clickable = "False";
            try
            {
                CurrentFolderName = FileManager.ParrentFolder.Name;
                await FileManager.RefreshFolder(FileManager.ParrentFolder);
                
            }
            catch (Exception ee)
            {
                MessageDialog messageDialog = new MessageDialog(ee.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();
            }
            Debug.WriteLine("On");
            Clickable = "True";
        }
        ///上级文件夹按钮

        private void ImageGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var file = ImageGridView.SelectedItem;
            if (file != null)
            {
                FileManager.ShowedImageFile = (File)file;
                FileManager.PageChanged = true;
                FrameMain.Navigate(typeof(DetailPage));
            }
        }
        ///双击转换至DetailPage

        private void Button_Click2(object sender, RoutedEventArgs e) {
            Clickable = "False";
            SelectedItems.Clear();
            if (ImageGridView.SelectedItems.Count() > 0)
            {
                foreach (var f in ImageGridView.SelectedItems)
                {
                    var file = (File)f;
                    SelectedItems.Add(file);
                }
                ///记录需转换图片
                SavePath = "";
                PopUp.IsOpen = true;
                ///打开弹窗
            }
            else { Clickable = "False"; }
        }
        ///格式转换按钮 记录SelectedItems并打开弹窗选择详细内容

        private async void Button_Click3(object sender, RoutedEventArgs e)
        {
            Clickable = "False";
            try
            {
                var folder = await FileManager.PickFolder();
                CurrentFolderName = folder.Name;
                await FileManager.RefreshFolder(folder);
                Debug.WriteLine(folder.Name);
            }
            catch (Exception ee)
            {
            }
            Clickable = "True";
        }
        ///当前文件夹选择按钮

        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            PopUp.IsOpen = false;
            Clickable = "True";
        }
        ///格式转换框返回按钮

        private async void Button_Click5(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = await FileManager.PickFolder();
                SavePath = folder.Path;
            }
            catch (Exception ee)
            {
            }
        }
        ///格式转换框选择文件夹按钮

        private List<File> SelectedItems = new List<File>();
        ///声明对象

        private async void Button_Click6(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePath = PathTextBox.Text;
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(SavePath);
                ///打开保存文件夹
                
                try
                {
                    if (SelectedItems.Count() > 0)
                    {

                        string extype = (string)TypeComboBox.SelectedItem;
                        Guid imageid;
                        switch (extype)
                        {
                            case ".jpg": imageid = BitmapEncoder.JpegEncoderId; break;
                            case ".bmp": imageid = BitmapEncoder.BmpEncoderId; break;
                            case ".png": imageid = BitmapEncoder.PngEncoderId; break;
                            case ".tif": imageid = BitmapEncoder.TiffEncoderId; break;
                            case ".gif": imageid = BitmapEncoder.GifEncoderId; break;
                        }
                        ///生成BitmapEncoder所需的对应格式标识
             
                        FileManager.ProgressCounter.ProgressStart(SelectedItems.Count());
                        ///进度记录开始
                        InProgress.IsOpen = true;
                        ///正在处理弹窗打开
                        
                        foreach (File f in SelectedItems)
                        {
                            SaveImage(f, folder, extype, imageid);
                        }
                        ///遍历转换图片
                        PopUp.IsOpen = false;
                        ///格式转换弹窗关闭
                    }
                }
                catch (Exception ee)
                {
                    MessageDialog messageDialog = new MessageDialog(ee.Message, "错误");
                    messageDialog.Commands.Add(new UICommand("确定"));
                    await messageDialog.ShowAsync();
                }
            }
            catch (Exception ee) {
                MessageDialog messageDialog = new MessageDialog(ee.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();

            }
        }
        ///格式转换

        public async void SaveImage(File f, StorageFolder folder, string extype, Guid imageid)
        {
            bool result = await FileManager.SaveImage(f,folder,extype,imageid);
            if (result) {
                Clickable = "True";
                InProgress.IsOpen = false;
                if (FileManager.ProgressCounter.failed > 0) {
                    string o = FileManager.ProgressCounter.failed.ToString()+"个文件发生错误";
                    MessageDialog messageDialog = new MessageDialog(o, "错误");
                    messageDialog.Commands.Add(new UICommand("确定"));
                    await messageDialog.ShowAsync();
                    ///报错
                }
            }
        }
        ///调用FileManager的SaveImage方法进行转换并判断进程，控制正在处理弹窗

        private async void Button_Click7(object sender, RoutedEventArgs e)
        {
            Clickable = "False";
            await FileManager.LoadFolder();
            Clickable = "True";
        }
        ///刷新按钮
    }
}
