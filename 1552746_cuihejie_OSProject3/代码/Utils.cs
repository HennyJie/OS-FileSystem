using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Collections;

namespace OsVirtualFileSystem.Core
{
    //Install-Package protobuf-net -Version 2.0.0.602

    //公用的操作函数
    static class Utils
    {
        public static long GetUnixTimeStamp()
        {
            DateTime timeStamp = new DateTime(1970, 1, 1);
            return (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000;
        }

        public static DateTime GetDateTime(long timeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(timeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static String GetLegalNewName(String name, Directory dir)
        {
            int index = 0;
            String legalNewName = "";
            
            while (true)
            {
                index++;
                if (index == 1)
                {
                    legalNewName = name;
                }
                else
                {
                    legalNewName = name + "(" + index.ToString() + ")";
                }
                if (!dir.IsExist(legalNewName))
                    return legalNewName;
            }
        }

        //复制文件操作的副本
        public static String GetLegalCopyName(String name, Directory dir)
        {
            String legalCopyName = name;
            while (true)
            {
                if (!dir.IsExist(legalCopyName))
                    return legalCopyName;
                legalCopyName += "_副本";
            }
        }

        //退出文件系统时，文件被自动保存到同路径下vfs.bin文件中。若第一次打开文件系统则会自动创建vfs.bin文件。
        public static void SerializeNow()
        {
            FileStream fileStream = new FileStream("./vfs.bin", FileMode.Create);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(fileStream, new VFS());
            fileStream.Close();
        }

        public static void DeSerializeNow()
        {
            FileStream fileStream = new FileStream("./vfs.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter b = new BinaryFormatter();
            (b.Deserialize(fileStream) as VFS).Update();
            fileStream.Close();
        }
    }

}
