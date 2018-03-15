using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Collections;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace OsVirtualFileSystem.Core
{
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    
    //文件夹对象,继承自基类 Entry
    public class Directory: Entry
    {
        //文件夹对象的一系列属性
        public String name;//文件名

        public long modifiedTime;//修改时间

        public long createTime;//创建时间

        public FileNode fileNode;//所在文件节点

        //记录父目录
        public Directory parent;

        public List<Entry> directory = new List<Entry>();

        public ObservableCollection<Information> directoryData = new ObservableCollection<Information>();


        //默认构造函数
        public Directory() { }


        //复制构造函数，深度复制
        public Directory (String name, Directory copiedDir, Directory parent)
        {
            this.name = name;
            this.parent = parent;
            this.createTime = Utils.GetUnixTimeStamp();

            foreach (Entry entry in copiedDir.directory)
                if (entry.GetType() == "文本文件")
                {
                    File newFile = new File(entry.GetName(), entry as File, this);

                    directory.Add(newFile);

                    var inf = new Information(newFile, newFile.GetCreatedTime(), newFile.GetModifiedTime());

                    directoryData.Add(inf);

                }
                else
                {
                    Directory newDir = new Directory(entry.GetName(), entry as Directory, this);
                    directory.Add(newDir);
                    var inf = new Information(newDir, newDir.GetModifiedTime(), newDir.GetModifiedTime());

                    directoryData.Add(inf);

                }
            UpdateCTime();//更新时间
        }

        //文件的构造函数
        public Directory(String name, Directory parent)
        {
            this.name = name;

            this.parent = parent;

            UpdateCTime();
        }

        //获取父级
        public override Directory GetParent()
        {
            return parent;
        }

        //获取当前文件所在路径
        public override String GetPath()
        {
            //自己是根节点
            if (IsRootDir())
                return "/";
            else
            {
                if (parent.IsRootDir())
                    return parent.GetPath() + name;
                else
                    return parent.GetPath() + "/" + name;
            }
        }

        //判断自己是根结点
        public bool IsRootDir()
        {
            if (parent == null)
                return true;
            else
                return false;
        }

        public override String GetName()
        {
            return name;
        }

        //设置文件名并更新
        public override void SetName(string name)
        {
            this.name = name;

            UpdateCTime();
        }
        
        //修改时间
        public override String GetModifiedTime()
        {
            DateTime dateTime = Utils.GetDateTime(this.modifiedTime);
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }

        //创建实践
        public String GetCreateTime()
        {
            DateTime dateTime = Utils.GetDateTime(this.createTime);
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public override String GetType()
        {
            return "文件夹";
        }

        //获取文件夹大小
        public override long GetSizeNum()
        {
            long size = 0;

            foreach (Entry entry in directory)
                size += entry.GetSizeNum();

            return size;
        }

        //获取显示的文件大小信息
        public override String GetSize()
        {
            long size = GetSizeNum();
            if (size >= 1024 * 1024)
            {
                size /= 1024 * 1024;
                return size.ToString() + "MB";
            }
            else
            {
                size /= 1024;
                return size.ToString() + "KB";
            }
        }

        public override String GetContent()
        {
            throw new NotImplementedException();
        }

        //增加文件节点属性信息
        public override Entry Add(Entry entry)
        {
            this.modifiedTime = Utils.GetUnixTimeStamp();//获取时间戳，记录修改时间

            directory.Add(entry);

            var inf = new Information(entry, entry.GetModifiedTime(), entry.GetModifiedTime());

            directoryData.Add(inf);

            UpdateCTime();

            return this;
        }

        //获取文件节点
        public override FileNode GetFileNode()
        {
            var fileNode = new FileNode();
            fileNode.Name = this.name;
            fileNode.Size = this.GetSize();
            foreach(var file in directory)
            {
                fileNode.SubNodes.Add(file.GetFileNode());
            }
            return fileNode;
        }

        //获取文件
        public override List<Entry> GetEntries()
        {
            return directory;
        }

        //按照名字查找
        public Entry FindByName(String name)
        {
            foreach (Entry entry in directory)
                if (entry.GetName() == name)
                    return entry;

            return null;
        }

        //文件搜索
        public bool IsExist(String name)
        {
            if (FindByName(name) != null)
                return true;
            else
                return false;
        }

        //删除数据
        public override void DeleteData() 
        {
            foreach (Entry entry in directory)
                entry.DeleteData();
        }

        //删除文件
        public void DeleteEntry(Entry entry)
        {
            directory.Remove(entry);

            UpdateCTime();
        }

        public override ArrayList Search(String name)
        {
            ArrayList result = new ArrayList();

            //检查儿子
            foreach (Entry entry in directory)
                result.AddRange(entry.Search(name));

            //检查自己
            if (this.name.IndexOf(name) >= 0)
                result.Add(this);

            return result;
        }

        //修改后更新时间
        public override void UpdateCTime()
        {
            this.modifiedTime = Utils.GetUnixTimeStamp();
            if (!IsRootDir())
                parent.UpdateCTime();
        }
    }

}
