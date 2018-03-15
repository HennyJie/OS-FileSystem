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

    //块组
    public class BlockGroup
    {
        public SuperBlock BGSuperBlock { get; set; } //超级块

        private int blockGroupIndex; //块组序号

        private long gFreeBlocksCount;//当前块组中空闲的块数量
        private long gFreeInodesCount;//当前块组中空闲的节点数量

        private bool[] blockIndex; //块索引
        private bool[] inodeIndex; //节点索引

        private INode[] inodes;
        private Block[] blocks;

        public BlockGroup(int index)
        {
            BGSuperBlock = new SuperBlock();

            gFreeBlocksCount = Configs.BLOCK_PER_GROUP;
            gFreeInodesCount = Configs.INODE_PER_GROUP;

            blockIndex = new bool[Configs.BLOCK_PER_GROUP];
            inodeIndex = new bool[Configs.INODE_PER_GROUP];

            blocks = new Block[Configs.BLOCK_PER_GROUP];
            inodes = new INode[Configs.INODE_PER_GROUP];

            for (int i = 0; i < Configs.BLOCK_PER_GROUP; i++)
                blocks[i] = new Block(blockGroupIndex, i);
            for (int i = 0; i < Configs.INODE_PER_GROUP; i++)
                inodes[i] = new INode(blockGroupIndex, i);
        }

        //该块组中有空节点
        public bool HasFreeINode()
        {
            if (gFreeInodesCount > 0)
                return true;
            else
                return false;
        }

        //在块组中有空块
        public bool HasFreeBlock()
        {
            if (gFreeBlocksCount > 0)
                return true;
            else
                return false;
        }

        //获得空节点
        public INode GetFreeInode()
        {
            for (int i = 0; i < Configs.INODE_PER_GROUP; i++)
            {
                if (!inodeIndex[i])
                {
                    //刷新Inode位图
                    UpdateINodeIndex(i, true);
                    return inodes[i];
                }
            }
            return null;
        }

        //刷新block
        public void UpdateBlockIndex(int index, bool flag)
        {
            if (flag)
            {
                gFreeBlocksCount--;
                VFS.BLOCK_GROUPS[0].BGSuperBlock.SpFreeBlocksCount--;
            }
            else
            {
                gFreeBlocksCount++;
                VFS.BLOCK_GROUPS[0].BGSuperBlock.SpFreeBlocksCount++;
            }
            this.blockIndex[index] = flag;
        }

        //刷新Inode
        public void UpdateINodeIndex(int index, bool flag)
        {
            if (flag)
            {
                gFreeInodesCount--;
                VFS.BLOCK_GROUPS[0].BGSuperBlock.SpFreeInodeCount--;
            }
            else
            {
                gFreeInodesCount++;
                VFS.BLOCK_GROUPS[0].BGSuperBlock.SpFreeInodeCount++;
            }
            this.inodeIndex[index] = flag;
        }

        //获得空块
        public ArrayList GetFreeBlocks()
        {
            ArrayList free_blocks = new ArrayList();
            for (int i = 0; i < Configs.BLOCK_PER_GROUP; i++)
            {
                if (!blockIndex[i])
                    free_blocks.Add(blocks[i]);
            }

            return free_blocks;
        }

    }
}
