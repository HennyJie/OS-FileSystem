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

    //虚拟文件系统的管理类
    public class VFS
    {
        public static Directory ROOT_DIR { set; get; }
        public static BlockGroup[] BLOCK_GROUPS = new BlockGroup[Configs.GROUPS];

        public Directory RootDirSerialize;
        public BlockGroup[] BlockGroupsSerialize;

        public VFS()
        {
            RootDirSerialize = ROOT_DIR;
            BlockGroupsSerialize = BLOCK_GROUPS;
        }

        public void Update()
        {
            ROOT_DIR = RootDirSerialize;
            BLOCK_GROUPS = BlockGroupsSerialize;
        }

        //获取空闲空间
        public static ArrayList GetFreeBlocks(int num)
        {
            ArrayList result = new ArrayList();

            foreach (var blockGroup in BLOCK_GROUPS)
            {
                if (blockGroup.HasFreeBlock())
                {
                    ArrayList tempArr = blockGroup.GetFreeBlocks();
                    result.AddRange(tempArr);
                    if (result.Count >= num)
                        return result;
                }
            }
            return null;
        }

        //格式化
        public static void Format()
        {
            //初始化全局块组
            for (int i = 0; i < Configs.GROUPS; i++)
                VFS.BLOCK_GROUPS[i] = new BlockGroup(i);

            //初始化目录树
            VFS.ROOT_DIR = new Directory("/", null);
        }
    }
}
