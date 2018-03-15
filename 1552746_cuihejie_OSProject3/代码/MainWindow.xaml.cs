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
using System.Configuration;
using Infragistics.Controls.Interactions;
using System.Windows.Media.Effects;
using OsVirtualFileSystem.Core;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;
using ProtoBuf;

namespace OsVirtualFileSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VFS vfs;

        public Directory currentDir;

        private int index;

        public FileNode FileNodes;

        private List<Directory> recorder;

        public MainWindow()
        {
            InitializeComponent();
            InitiallizeVFS();
            this.Closing += closingEvent;

        }
        private void closingEvent(object o, System.ComponentModel.CancelEventArgs e)
        {
            Utils.SerializeNow();
        }


        public void InitiallizeVFS()
        {
            recorder = new List<Directory>();
            if(System.IO.File.Exists("./vfs.bin"))
            {
                Utils.DeSerializeNow();
            }
            else
            {
                for(int i=0;i<Configs.GROUPS;i++)
                {
                    VFS.BLOCK_GROUPS[i] = new BlockGroup(i);
                }

                VFS.ROOT_DIR = new Directory("/", null);
                

                var bootDir = new Directory("Boot", VFS.ROOT_DIR);

                var userDir = new Directory("User", VFS.ROOT_DIR);

                VFS.ROOT_DIR.Add(bootDir);

                VFS.ROOT_DIR.Add(userDir);

                var test = new File("test", userDir);
                userDir.Add(test);

            }
            currentDir = VFS.ROOT_DIR;
            recorder.Add(currentDir);
            listView.ItemsSource = VFS.ROOT_DIR.directoryData;
            url.Text = currentDir.GetPath();

        }

        private void ButtonToolNewFile(object sender, RoutedEventArgs e)
        {
            String file = Utils.GetLegalNewName("新建文本文件", currentDir);
            var newFile = new File(file, currentDir);
            currentDir.Add(newFile);

        }

        private void ButtonToolNewFolder(object sender, RoutedEventArgs e)
        {
            String name = Utils.GetLegalNewName("新建文件夹", currentDir);
            var newFile = new Directory(name, currentDir);
            currentDir.Add(newFile);
        }

        private void ButtonToolDelete(object sender, RoutedEventArgs e)
        {

            XamDialogWindow win = new XamDialogWindow()
            {
                Width = 350,
                Height = 150,
                StartupPosition = StartupPosition.Center,
                CloseButtonVisibility = Visibility.Collapsed
            };

            win.Header = "删除";
            StackPanel stack = new StackPanel();
            stack.VerticalAlignment = VerticalAlignment.Center;
            stack.Margin = new Thickness(10);

            Label label = new Label();
            Information inf = (Information)listView.SelectedItem;
            if(inf==null)
            {
                System.Windows.Forms.MessageBox.Show("请先选中要删除文件或者文件夹", "删除失败", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
        

            label.Content = "您确认要删除该" + (inf.extension=="文本文件" ? "文件" : "文件夹") + "吗?";
            stack.Children.Add(label);

            StackPanel stackButton = new StackPanel();
            stackButton.Orientation = Orientation.Horizontal;
            stackButton.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Children.Add(stackButton);

            Button btnOK = new Button();
            btnOK.Content = "删除";
            btnOK.Padding = new Thickness(10, 0, 10, 0);
            btnOK.Margin = new Thickness(10);
            stackButton.Children.Add(btnOK);
            Button btnCancel = new Button();
            btnCancel.Content = "取消";
            btnCancel.Padding = new Thickness(10, 0, 10, 0);
            btnCancel.Margin = new Thickness(10);
            stackButton.Children.Add(btnCancel);

            //FadeOutWindow();

            win.Content = stack;
            win.IsModal = true;
            grid.Children.Add(win);

            btnOK.Click += delegate {
                inf.entry.DeleteData();
                currentDir.directory.Remove(inf.entry);

                currentDir.directoryData.Remove(inf);
                win.Close();
                grid.Children.Remove(win);
            };

            btnCancel.Click += delegate
            {
                win.Close();
                grid.Children.Remove(win);
            };
        }

        public void listView_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Information inf = (Information)listView.SelectedItem;
            if (inf == null)
            {
                System.Windows.Forms.MessageBox.Show("请先选中文件或者文件夹", "选中失败", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            if (inf.extension=="文本文件")
            {
                File file = (File)inf.entry;
                var textEditor = new TextEditor(file,this);
                textEditor.ShowDialog();

            }
            else
            {
                currentDir = inf.entry as Directory;
                recorder.Add(currentDir);
                index++;
                listView.ItemsSource = currentDir.directoryData;
                url.Text = currentDir.GetPath();
            }
        }


        public void UpdateInfor(ObservableCollection<Information> data)
        {
            foreach (var v in data)
            {
                v.UpdateInfor();
            }
        }


        private void ButtonTool_Up(object sender, RoutedEventArgs e)
        {
            if (currentDir.GetParent() == null) return;
            currentDir = currentDir.GetParent();
            
            UpdateInfor(currentDir.directoryData);
            listView.ItemsSource = currentDir.directoryData;
            url.Text = currentDir.GetPath();
        }

        private void ButtonTool_Front(object sender, RoutedEventArgs e)
        {
            int index = recorder.IndexOf(currentDir);
            if (index == recorder.Count - 1)
                return;
            index++;
            currentDir = recorder[index];

            UpdateInfor(currentDir.directoryData);
            listView.ItemsSource = currentDir.directoryData;
            url.Text = currentDir.GetPath();
        }

        private void ButtonTool_Back(object sender, RoutedEventArgs e)
        {
            int index = recorder.IndexOf(currentDir);
            if (index == 0)
                return;
            index--;
            currentDir = recorder[index];

            UpdateInfor(currentDir.directoryData);
            listView.ItemsSource = currentDir.directoryData;
            url.Text = currentDir.GetPath();
          
        }

        private void ButtonTool_Rename(object sender, RoutedEventArgs e)
        {
            Information inf = (Information)listView.SelectedItem;
            if (inf == null)
            {
                System.Windows.Forms.MessageBox.Show("请先选中文件或者文件夹", "选中失败", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            XamDialogWindow win = new XamDialogWindow()
            {
                Width = 350,
                Height = 150,
                StartupPosition = StartupPosition.Center,
                CloseButtonVisibility = Visibility.Collapsed
            };

            win.Header = "重命名";
            StackPanel stack = new StackPanel();
            stack.VerticalAlignment = VerticalAlignment.Center;
            stack.Margin = new Thickness(10);

            Label label = new Label();
            label.Content = "请输入新的名称：";
            stack.Children.Add(label);

            TextBox txtName = new TextBox();
            txtName.Text = inf.name;
            stack.Children.Add(txtName);

            StackPanel stackButton = new StackPanel();
            stackButton.Orientation = Orientation.Horizontal;
            stackButton.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Children.Add(stackButton);

            Button btnOK = new Button();
            btnOK.Content = "确认";
            btnOK.Padding = new Thickness(10, 0, 10, 0);
            btnOK.Margin = new Thickness(10);
            stackButton.Children.Add(btnOK);
            Button btnCancel = new Button();
            btnCancel.Content = "取消";
            btnCancel.Padding = new Thickness(10, 0, 10, 0);
            btnCancel.Margin = new Thickness(10);
            stackButton.Children.Add(btnCancel);

            //FadeOutWindow();

            win.Content = stack;
            win.IsModal = true;
            grid.Children.Add(win);

            btnOK.Click += delegate {
                String name = Utils.GetLegalCopyName(txtName.Text, currentDir);
                inf.entry.SetName(name);
                foreach(var v in currentDir.directoryData)
                {
                    if(inf.name==v.name)
                    {
                        inf.name = name;
                        ICollectionView view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
                        view.Refresh();
                        break;
                    }
                }
                win.Close();
                grid.Children.Remove(win);
            };

            btnCancel.Click += delegate
            {
                win.Close();
                grid.Children.Remove(win);
            };
        }

        private void ButtonTool_Click(object sender, RoutedEventArgs e)
        {
            XamDialogWindow win = new XamDialogWindow()
            {
                Width = 350,
                Height = 150,
                StartupPosition = StartupPosition.Center,
                CloseButtonVisibility = Visibility.Collapsed
            };

            win.Header = "格式化";
            StackPanel stack = new StackPanel();
            stack.VerticalAlignment = VerticalAlignment.Center;
            stack.Margin = new Thickness(10);

            Label label = new Label();

            label.Content = "您确认要格式化文件系统吗?";
            stack.Children.Add(label);

            StackPanel stackButton = new StackPanel();
            stackButton.Orientation = Orientation.Horizontal;
            stackButton.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Children.Add(stackButton);

            Button btnOK = new Button();
            btnOK.Content = "格式化";
            btnOK.Padding = new Thickness(10, 0, 10, 0);
            btnOK.Margin = new Thickness(10);
            stackButton.Children.Add(btnOK);
            Button btnCancel = new Button();
            btnCancel.Content = "取消";
            btnCancel.Padding = new Thickness(10, 0, 10, 0);
            btnCancel.Margin = new Thickness(10);
            stackButton.Children.Add(btnCancel);

            //FadeOutWindow();

            win.Content = stack;
            win.IsModal = true;
            grid.Children.Add(win);

            btnOK.Click += delegate {
                VFS.Format();
                currentDir = VFS.ROOT_DIR;
                recorder.Add(currentDir);
                listView.ItemsSource = VFS.ROOT_DIR.directoryData;
                url.Text = currentDir.GetPath();
                win.Close();
                grid.Children.Remove(win);
            };

            btnCancel.Click += delegate
            {
                win.Close();
                grid.Children.Remove(win);
            };
          
        }
    }

    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class Information
    {
        public Uri image { get; set; }
        public String name { get; set; }
        public String extension { get; set; }
        public String size { get; set; }
        public String modifyTime { get; set; }
        public String createTime { get; set; }
        public String inode { get; set; }

        public Entry entry;

        public Information()
        {

        }

        public Information(Entry en, String CTime, String MTime)
        {
            entry = en;
            modifyTime = MTime;
            createTime = CTime;
            name = en.GetName();
            //image = new Image();
            String path = System.Environment.CurrentDirectory;
            if (en.GetType()=="文本文件")
            {
                extension = "文本文件";
                path += "\\Images\\filesmall.png";
                inode = (en as File).inode.inodeIndex.ToString();
            }
            else
            {
                extension = "文件夹";
                path += "\\Images\\foldersmall.png";
                inode = "-";
            }
            //image.Source = new BitmapImage(new Uri(path));
            image = new Uri(path);
            size = en.GetSize();
            
        }


        public void UpdateInfor()
        {
            name = entry.GetName();
            size = entry.GetSize();
        }

    }
}
