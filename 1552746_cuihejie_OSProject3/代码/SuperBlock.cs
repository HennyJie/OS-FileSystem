using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using OsVirtualFileSystem;

namespace OsVirtualFileSystem.Core
{

    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]

    //超级块，用来存储描述文件系统整体信息的数据结构
    public class SuperBlock
    {
        public long SpInodeCount { set; get; }//节点数目
        public long SpBlocksCount { set; get; }//超级块数目
        public long SpFreeInodeCount { set; get; }//空闲节点数目
        public long SpFreeBlocksCount { set; get; }//自由节点数目
        public long SpFirstDataBlock { set; get; }//第一个数据块
        public long SpLogBlockSize { set; get; }
        public long SpBlocksPerGroup { set; get; }//每个块组中的块数
        public long SpInodePerGroup { set; get; }//每个块组中的节点数
        public long SpMTime { set; get; }
        public long SpWTime { set; get; }
        public long SpMntCount { set; get; }
        public long SpMaxMntCount { set; get; }
        public String SpMagic { set; get; }

        public SuperBlock()
        {
            SpInodeCount = Configs.GROUPS * Configs.INODE_PER_GROUP; //inode总数
            SpBlocksCount = Configs.GROUPS * Configs.BLOCK_PER_GROUP; //block总数
            SpFreeInodeCount = SpInodeCount; //空闲inodes数
            SpFreeBlocksCount = SpFreeBlocksCount;//空闲block数
            SpFirstDataBlock = 0;
            SpLogBlockSize = Configs.BLOCK_SIZE; //块大小
            SpBlocksPerGroup = Configs.BLOCK_PER_GROUP; //每个块组包含的block数
            SpInodePerGroup = Configs.INODE_PER_GROUP; //每个块组包含的inode数

            SpMTime = Utils.GetUnixTimeStamp();
            SpWTime = SpMTime;
            SpMntCount = 1;
            SpMaxMntCount = 1024;
            SpMagic = "EXT2";
        }

    }
}
