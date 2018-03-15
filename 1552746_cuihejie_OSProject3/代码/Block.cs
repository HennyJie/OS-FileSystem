using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ProtoBuf;
using OsVirtualFileSystem;

namespace OsVirtualFileSystem.Core
{
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    
    //最基本的存储块，定义块的属性和操作
    public class Block
    {
        public char[] Data { set; get; }//直接索引时，块中的数据

        public List<Block> Blocks { set; get; }//多级索引时，块中存储的块链表
    
        //type为多级索引的标记 其中： 0-数据 1-一级索引 2-二级索引
        private int type;

        private bool IsEffective;

        //块组索引
        private int groupIndex;

        //块索引
        private int blockIndex;

        public Block(int groupIndex, int blockIndex)
        {
            IsEffective = false;
            this.groupIndex = groupIndex;
            this.blockIndex = blockIndex; 
        }

        //判断存储块是否已满
        public bool IsFull()
        {
            if(type==0||Blocks.Count>=Configs.BLOCK_SIZE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //设置存储块的类型
        public void SetType(int type)
        {
            IsEffective = true;
            this.type = type;
            Blocks = null;
            Data = null;
            if(type!=0)
            {
                Blocks = new List<Block>();
            }
            //刷新位图
            VFS.BLOCK_GROUPS[groupIndex].UpdateBlockIndex(blockIndex, true);
        }

        //将新数据存入存储块
        public void SaveData(char[] newData)
        {
            if(type==0)
            {
                Data = newData;
            }
            else
            {
                if(Blocks.Count==0||Blocks[Blocks.Count-1].IsFull())
                {
                    var freeBlocks = VFS.GetFreeBlocks(1);
                    var freeBlock = (Block)freeBlocks[0];
                    freeBlock.SetType(type - 1);
                    freeBlock.SaveData(newData);
                    Blocks.Add(freeBlock);
                }
                else
                {
                    Blocks[Blocks.Count - 1].SaveData(newData);
                }
            }
        }

        //获得块的大小
        public long GetSize()
        {
            if (!IsEffective)
                return 0;
            if(type==0)
            {
                return 1024;
            }
            else
            {
                long size = 0;
                foreach(var v in Blocks)
                {
                    size += v.GetSize();
                }
                return size;
            }
        } 

        //获取块中内容
        public String GetContent()
        {
            if (!IsEffective)
                return "";
            if(type==0)
            {
                return new String(Data);
            }
            else
            {
                String s = "";
                foreach(var v in Blocks)
                {
                    s += v.GetContent();
                }
                return s;
            }
        }

        //删除块中内容，刷新位图
        public void Delete()
        {
            if (!IsEffective)
                return;
            IsEffective = false;

            //刷新位图
            VFS.BLOCK_GROUPS[groupIndex].UpdateBlockIndex(blockIndex, false);

            if (type!=0)
            {
                foreach(var v in Blocks)
                {
                    v.Delete();
                }
            }
        }
    }
}
