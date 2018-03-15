using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsVirtualFileSystem.Core
{
    //定义程序中用到的一些常量和参数
    static class Configs
    {
        public const int GROUPS = 256;//该项目中定义块数为256

        public const int BLOCK_SIZE = 256;//为提高访问速度，该项目中块的大小为256

        public const int BLOCK_PER_GROUP = 15;//每个块组中的块数为15

        public const int INODE_PER_GROUP = 50;//每个块组中的节点数为50

    }
}
