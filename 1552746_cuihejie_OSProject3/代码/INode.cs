using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;

using System.Collections;

using OsVirtualFileSystem.Core;

namespace OsVirtualFileSystem.Core
{
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    //对一组block进行管理，一般一个节点对应一个文件或文件夹
    public class INode
    {
        public Block[] Blocks { set; get; }

        public long CTime { set; get; }

        public long MTime { set; get; }

        public bool IsOpen { set; get; }

        private int groupIndex;

        public int inodeIndex; 

        private int blockNum = 15;

        public INode(int groupIndex, int inodeIndex)
        {
            Blocks = new Block[blockNum];

            IsOpen = false;

            CTime = Utils.GetUnixTimeStamp();
            MTime = CTime;

            this.groupIndex = groupIndex;
            this.inodeIndex = inodeIndex;

        }

        public long GetSize()
        {
            long size = 0;
            for(int i=0;i<blockNum;i++)
            {
                if (Blocks[i] == null) break;
                size += Blocks[i].GetSize();
            }
            return size;
        }

        public String GetContent()
        {
            String content = "";
            for (int i = 0; i < 15; i++)
            {
                if (Blocks[i] != null)
                    content += Blocks[i].GetContent();
                else
                    break;
            }
            return content;
        }

        public void Save(String content)
        {
            MTime = Utils.GetUnixTimeStamp();

            ClearBlock();

            char[] contents = content.ToCharArray();

            int length = contents.GetLength(0);

            if (length == 0)
                return;

            int num = length / (int)Configs.BLOCK_SIZE + 1;

            ArrayList freeBlocks = VFS.GetFreeBlocks(15);

            for(int i=0;i<Math.Min(12,num);i++)
            {
                Blocks[i] = freeBlocks[i] as Block;
                Blocks[i].SetType(0);
            }

            if(num>12)
            {
                Blocks[12] = freeBlocks[12] as Block;
                Blocks[12].SetType(1);
            }

            if (num > 12 + Configs.BLOCK_SIZE)
            {
                Blocks[13] = freeBlocks[13] as Block;
                Blocks[13].SetType(2);
            }

            if (num > 12 + Configs.BLOCK_SIZE + Configs.BLOCK_SIZE * Configs.BLOCK_SIZE)
            {
                Blocks[14] = freeBlocks[14] as Block;
                Blocks[14].SetType(3);
            }


            int currentIndex = 0;
            for(int i=0;i<num;i++)
            {
                long realBlockSize = Math.Min(Configs.BLOCK_SIZE, length - i * Configs.BLOCK_SIZE);
                char[] contentSeq = new char[realBlockSize];
                Array.Copy(contents, Configs.BLOCK_SIZE * i, contentSeq, 0, realBlockSize);

                if(currentIndex<12)
                {
                    Blocks[currentIndex].SaveData(contentSeq);
                    currentIndex++;
                }
                else
                {
                    Blocks[currentIndex].SaveData(contentSeq);
                    if (Blocks[currentIndex].IsFull())
                        currentIndex++;
                }
            }
        }

        public void ClearBlock()
        {
            foreach(var v in Blocks)
            {
                if (v == null)
                    break;
                v.Delete();
            }
        }

        public void Delete()
        {
            //刷新位图
            VFS.BLOCK_GROUPS[groupIndex].UpdateINodeIndex(inodeIndex, false);
        }
    } 

}
