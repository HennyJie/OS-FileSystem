using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.Collections;

namespace OsVirtualFileSystem.Core
{
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    
    //文本文件对象
    public class File: Entry
    {
        public INode inode { get; set; }

        private String name;

        private long CTime;

        private long MTime;

        //记录父目录
        [ProtoMember(101, AsReference = true)]

        public Directory parent;

        //复制构造函数
        public File(String name, File copiedFile, Directory parent)
        {
            this.name = name;
            this.parent = parent;

            //寻找空闲的inode
            for (int i = 0; i < Configs.GROUPS; i++)
            {
                if (VFS.BLOCK_GROUPS[i].HasFreeINode())
                {
                    inode = VFS.BLOCK_GROUPS[i].GetFreeInode();
                    break;
                }
            }

            //复制数据
            this.inode.Save(copiedFile.GetContent());

            UpdateCTime();
        }

        //创建文件
        public File(String name, Directory parent)
        {
            this.name = name;
            this.parent = parent;

            //寻找空闲的inode
            for (int i = 0; i < Configs.GROUPS; i++ )
            {
                if (VFS.BLOCK_GROUPS[i].HasFreeINode())
                {
                    this.inode = VFS.BLOCK_GROUPS[i].GetFreeInode();
                    break;
                }
            }

            UpdateCTime();
        }

        public override Directory GetParent()
        {
            return parent;
        }

        //获取文本文件所在的路径
        public override String GetPath()
        {
            if (parent.IsRootDir())
                return parent.GetPath() + name;
            else
                return parent.GetPath() + "/" + name;
        }

        public override string GetName()
        {
            return this.name;
        }

        public override void SetName(string name)
        {
            this.name = name;

            UpdateCTime();
        }

        //修改时间
        public override string GetModifiedTime()
        {
            DateTime dateTime = Utils.GetDateTime(this.MTime);
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }

        //创建时间
        public string GetCreatedTime()
        {
            DateTime dateTime = Utils.GetDateTime(this.CTime);
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }

        //文件类型为文本文件
        public override string GetType()
        {
            return "文本文件";
        }

        public override long GetSizeNum()
        {
            return inode.GetSize();
        }

        //获取文本文件的大小
        public override string GetSize()
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

        //接口，后面有实现
        public override Entry Add(Entry entry)
        {
            throw new NotImplementedException();
        }

        public override List<Entry> GetEntries()
        {
            throw new NotImplementedException();
        }

        //获取文件节点
        public override FileNode GetFileNode()
        {
            var fileNode = new FileNode();
            fileNode.Name = this.name;
            fileNode.Size = GetSize();
            fileNode.SubNodes = null;
            return fileNode;        
        }

        //写文件                 
        public void Save(String content)
        {
            inode.Save(content);

            parent.UpdateCTime();
        }

        //读文件
        public override String GetContent()
        {
            return inode.GetContent();
        }

        //删除硬盘数据
        public override void DeleteData()
        {
            //删除block
            inode.ClearBlock();
            //删除inode
            inode.Delete();

            UpdateCTime();
        }

        //文件查找
        public override ArrayList Search(string name)
        {
            ArrayList result = new ArrayList();
            if (this.name.IndexOf(name) >= 0)
                result.Add(this);

            return result;
        }

        //更新时间戳
        public override void UpdateCTime()
        {
            MTime = Utils.GetUnixTimeStamp();
            inode.MTime = Utils.GetUnixTimeStamp();
            parent.UpdateCTime();
        }
    }
}
