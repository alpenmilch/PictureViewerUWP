using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace DataTemplateTestModel.ImageProcessor
{

    public class File {
        public string Name;
        public string Path;
        public string DateCreated;
        public string Type;
        public BitmapImage bitmapImage;
        public IStorageItem item;
    }
    ///文件数据模板

    public static class FileManager
    {
        public static bool PageChanged = false;///是否切换过页面
        public static string FolderPath = "C:\\Users\\LENOVO\\Pictures";///默认目录
        public static StorageFolder CurrentFolder;///当前文件夹
        public static StorageFolder ParrentFolder;///上级文件夹
        public static ObservableCollection<File> FolderInfos;///文件夹集合
        public static ObservableCollection<File> ImageInfos;///图片集合
        public static File ShowedImageFile;///当前展示图片

        public static async Task SetPrimaryFolder()
        {
            FolderInfos = new ObservableCollection<File>();
            ImageInfos = new ObservableCollection<File>();

            try
            {
                CurrentFolder = await StorageFolder.GetFolderFromPathAsync(FolderPath);
            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog(e.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();
            }
        }
        ///初始化

        public static async Task LoadFolder()
        {
            Debug.WriteLine("aaa");
            try
            {
                Debug.WriteLine(CurrentFolder.DisplayName);
                await GetParent();///获取上级目录
                await GetFolders();///获取所有文件夹
                await GetImages();///获取所有图片
            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog(e.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();
            }
            Debug.WriteLine("bbb");
        }
        ///载入文件夹内容

        private static async Task GetParent()
        {
            try
            {
                ParrentFolder = await CurrentFolder.GetParentAsync();

            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog(e.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();
            }
        }
        ///获取上级文件夹

        private static async Task GetFolders()
        {
            FolderInfos.Clear();
            try
            {

                var allFolders = await CurrentFolder.GetFoldersAsync();
                foreach (StorageFolder folder in allFolders)
                {
                    var file = new File { Name = folder.Name, Path = folder.Path, DateCreated = folder.DateCreated.ToString(), Type = "Folder", bitmapImage = null, item = folder };
                    FolderInfos.Add(file);
                }

            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog(e.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();
            }
        }
        ///获取目录下所有文件夹

        private static async Task GetImages()
        {
            ImageInfos.Clear();
            ImageInfos.Clear();
            try
            {

                var allFiles = await CurrentFolder.GetFilesAsync();
                List<string> extlist = new List<string> { ".jpg", ".png", ".gif", ".tif", ".bmp" };
                foreach (StorageFile file in allFiles)
                {
                    var date = file.DateCreated.ToString().Split(" ");
                    var time = date[0] + " " + date[1];
                    var bitmapImage = new BitmapImage();
                    var stream = await file.OpenReadAsync();
                    bitmapImage.SetSource(stream);
                    var image = new File { Name = file.DisplayName, Path = file.Path, DateCreated = time, Type = file.FileType, bitmapImage = bitmapImage, item = file };
                    if (extlist.Contains(file.FileType))
                    {
                        ImageInfos.Add(image);
                    }
                }
            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog(e.Message, "错误");
                messageDialog.Commands.Add(new UICommand("确定"));
                await messageDialog.ShowAsync();
            }
        }
        ///获取所有图片

        public async static Task RefreshFolder(StorageFolder folder)
        {
            CurrentFolder = folder;
            await LoadFolder();
        }
        ///更改当前文件夹并重载文件夹

        public async static Task<bool> SaveImage(File f, StorageFolder folder, string extype, Guid imageid)
        {
            try
            {
                var newFile = await folder.CreateFileAsync(f.Name + extype);
                var preFile = (StorageFile)(f.item);
                ///新建写入文件并获取读入图片文件

                var WriteStream = await newFile.OpenAsync(FileAccessMode.ReadWrite);
                var ReadStream = await preFile.OpenReadAsync();
                ///打开读写流

                var bitmapDecoder = await BitmapDecoder.CreateAsync(ReadStream);
                var bitmapEncoder = await BitmapEncoder.CreateAsync(imageid, WriteStream);
                ///建立解码器和编码器

                var bitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
                bitmapEncoder.SetSoftwareBitmap(bitmap);
                ///读取图片并存入至编码器

                await bitmapEncoder.FlushAsync();
                ///提交图片数据至流中
            }

            catch (Exception ee)
            {
                ProgressCounter.failed++;
                ProgressCounter.failedlist.Add(f.Name);

            }
            var opened = ProgressCounter.add();
            return opened;
        }
        ///保存单个图片


        public static class ProgressCounter
        {
            private static int end;///总数
            private static int state;///当前完成数
            public static int failed;///失败数
            public static List<string> failedlist;///失败的文件

            public static void ProgressStart(int count)
            {
                end = count;
                state = 0;
                failed = 0;
                failedlist = new List<string>();
            }
            ///任务初始化
            
            public static Boolean add()
            {
                state++;
                if (state == end)
                {
                    return true;
                }
                return false;
            }
            ///完成任务数加1并判断总体是否完成
        }
        ///记录转换进程

        public static async Task<StorageFolder> PickFolder() {
            try{
                FolderPicker picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".bmp");
                picker.FileTypeFilter.Add(".tif");
                picker.FileTypeFilter.Add(".gif");
                StorageFolder folder = await picker.PickSingleFolderAsync();
                return folder;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        ///文件夹选择器
    }
}
