using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OsVirtualFileSystem.Core;
using System.Windows.Data;

namespace OsVirtualFileSystem
{
    //文本编辑界面背后逻辑的实现
    public partial class TextEditor : Form
    {
        public TextEditor()
        {
            InitializeComponent();
        }
        private File file;//文件对象
        private bool isSave;//保存
        private MainWindow main;


        public TextEditor(File file, MainWindow m)
        {
            InitializeComponent();
            this.isSave = true;//保存
            this.file = file;//文件对象
            main = m;
        }

        //文本内容加载
        private void TextEditor_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = file.GetContent();
        }

        //文本内容发生了修改
        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            isSave = false;
        }

        //保存文本
        public void OnSave()
        {
            isSave = true;
            String content = richTextBox1.Text;
            file.Save(content);
            main.UpdateInfor(main.currentDir.directoryData);
            ICollectionView view = CollectionViewSource.GetDefaultView(main.listView.ItemsSource);
            view.Refresh();
        }

        //左上角文本保存键
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OnSave();
        }

        //点击右上角关闭按钮控制的
        private void TextEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isSave)
            {
                var result = MessageBox.Show("是否保存到" + file.GetName()+"?","记事本", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                //选择保存键
                if (result == DialogResult.Yes)
                {
                    OnSave();
                }
                //不保存键
                else if (result == DialogResult.No)
                {
                }
                //取消键
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
