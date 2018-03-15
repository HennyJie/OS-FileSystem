using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace OsVirtualFileSystem.Core
{

    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    
    //文件节点信息储存节点
    public class FileNode
    {
        public FileNode()
        {
            this.SubNodes = new List<FileNode>();
        }
        public string Name { get; set; }
        public String Size { get; set; }
        public List<FileNode> SubNodes { get; set; }
    }
}
