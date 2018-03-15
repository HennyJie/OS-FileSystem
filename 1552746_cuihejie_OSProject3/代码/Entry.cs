using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.Collections;

namespace OsVirtualFileSystem.Core
{
    //基础对象，定义文件夹和文件的一些共有操作
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]

    //以下定义了抽象类中的一些接口，在文件类和文件夹类中继承实现
    public abstract class Entry
    {

        public abstract String GetName();

        public abstract void SetName(String name);

        public abstract String GetModifiedTime();

        public abstract String GetType();

        public abstract String GetSize();

        public abstract long GetSizeNum();

        public abstract String GetContent();

        public abstract Entry Add(Entry entry);

        public abstract FileNode GetFileNode();

        public abstract List<Entry> GetEntries();

        public abstract void DeleteData();

        public abstract Directory GetParent();

        public abstract String GetPath();

        public abstract ArrayList Search(String name);

        public abstract void UpdateCTime();

    }
}
