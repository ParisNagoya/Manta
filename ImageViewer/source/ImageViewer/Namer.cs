using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RenameAllocateViewer
{
    public partial class Namer : Form
    {
        const int BASE = 36;

        /// <summary>
        /// 選択中のフォルダパス
        /// </summary>
        string _targetPath;

        /// <summary>
        /// 暦から自動生成した名前
        /// </summary>
        string _dayName;

        /// <summary>
        /// キーとなる名前
        /// </summary>
        string _fileNameKey;

        /// <summary>
        /// 選択したフォルダのファイルパスのリスト
        /// </summary>
        private string[] _fileList;

        public Namer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 日付の並びを36進数に変換
        /// </summary>
        private string DayTo36()
        {
            DateTime dt = DateTime.Now;
            long timeAdress = Convert.ToInt64(dt.ToString("yyMMddHHmmss"));
            string result = "INI";
            long quotient = timeAdress;
            List<long> remainderList = new List<long>();

            while (BASE < quotient)
            {
                remainderList.Add(quotient % BASE);
                quotient /= BASE;
            }

            remainderList.Add(quotient);

            remainderList.Reverse();

            foreach (int num in remainderList)
            {
                result += RadixConvert.ToString(num, 36, true);
            }

            result = result.Remove(0, 3);

            return result;
        }

        private void OpenDir()
        {
            _fileNameKey = System.IO.Path.GetFileName(_targetPath);
            textBox1.Text = _fileNameKey;
            RenameText();

            LimitedList(_targetPath);

            listBox1.Items.Clear();
            foreach (string filePath in _fileList)
            {
                listBox1.Items.Add(System.IO.Path.GetFileName(filePath));
            }
        }

        /// <summary>
        /// ディレクトリを選んでから実行
        /// </summary>
        private void DirSelect()
        {
            _fileNameKey = System.IO.Path.GetFileName(_targetPath);
            textBox1.Text = _fileNameKey;
            RenameText();

            LimitedList(_targetPath);

            listBox1.Items.Clear();
            foreach (string filePath in _fileList)
            {
                listBox1.Items.Add(System.IO.Path.GetFileName(filePath));
            }
        }

        /// <summary>
        /// リネーム情報を表示
        /// </summary>
        private void RenameText()
        {
            label3.Text = _fileNameKey + "0001.拡張子" +
                "\r\n" + "       ・" +
                "\r\n" + "       ・" +
                "\r\n" + "       ・" +
                "\r\n" + _fileNameKey + "9999.拡張子" +
                "\r\n" +
                        "という名前でリネームします。";
        }

        /// <summary>
        /// _fileNameKeyを元にリネーム
        /// </summary>
        private void RenameDir()
        {
            if (_targetPath != null && _fileList != null)
            {
                int num = 1;


                foreach (string filePath in _fileList)
                {
                    string filename = System.IO.Path.GetFileName(filePath);
                    string ext = Path.GetExtension(filename);
                    string filenameNew = filename;
                    filenameNew = _fileNameKey + num.ToString("0000") + ext;
                    string distPath = System.IO.Path.Combine(_targetPath, filenameNew);

                    File.Move(filePath, distPath);

                    num++;
                }

                LimitedList(_targetPath);

                listBox1.Items.Clear();

                foreach (string filePath in _fileList)
                {
                    listBox1.Items.Add(System.IO.Path.GetFileName(filePath));
                }
            }
            else
            {
                /**/
            }
        }

        private void LimitedList(string dirName)
        {
            _fileList = System.IO.Directory.GetFiles(
                     dirName, "*.jpg", System.IO.SearchOption.TopDirectoryOnly);

            string[] fileListPng = System.IO.Directory.GetFiles(
                    dirName, "*.png", System.IO.SearchOption.TopDirectoryOnly);

            _fileList = _fileList.Concat(fileListPng).ToArray();

            string[] fileListBmp = System.IO.Directory.GetFiles(
                    dirName, "*.bmp", System.IO.SearchOption.TopDirectoryOnly);

            _fileList = _fileList.Concat(fileListBmp).ToArray();

            string[] fileListTif = System.IO.Directory.GetFiles(
                    dirName, "*.tif", System.IO.SearchOption.TopDirectoryOnly);

            _fileList = _fileList.Concat(fileListTif).ToArray();

            string[] fileListIco = System.IO.Directory.GetFiles(
                    dirName, "*.ico", System.IO.SearchOption.TopDirectoryOnly);

            _fileList = _fileList.Concat(fileListIco).ToArray();
        }


        /********************/
        /*イベントハンドラ群*/
        /********************/
        private void button3_Click(object sender, EventArgs e)
        {
            RenameDir();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            if (_targetPath != null && _fileList != null) 
            {
                if (listBox2.SelectedIndex == 0)
                {
                    _fileList = _fileList.OrderBy(x => x).ToArray();
                }
                else if (listBox2.SelectedIndex == 1)
                {
                    _fileList = _fileList.OrderByDescending(x => x).ToArray();
                }
                else if (listBox2.SelectedIndex == 2)
                {
                    _fileList = _fileList.OrderBy(x => File.GetLastWriteTime(x)).ToArray();
                }
                else if (listBox2.SelectedIndex == 3)
                {
                    _fileList = _fileList.OrderByDescending(x => File.GetLastWriteTime(x)).ToArray();
                }

                foreach (string filePath in _fileList)
                {
                    listBox1.Items.Add(System.IO.Path.GetFileName(filePath));
                }
            }
            else
            {
                /**/
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _fileNameKey = textBox1.Text;
            RenameText();
        }

        /// <summary>
        /// リネームするフォルダを選択
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FolderSelectDialog fsd = new FolderSelectDialog();

            if (fsd.ShowDialog() == DialogResult.OK)
            {
                _targetPath = fsd.Path;

                OpenDir();

            }
            else
            {
                /**/
            }
        }

        private void button1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (System.IO.File.Exists(filePaths[0]))
            {
                //そのファイルのあるディレクトリを取得
                _targetPath = System.IO.Path.GetDirectoryName(filePaths[0]);
                DirSelect();
            }
            else
            {
                _targetPath = filePaths[0]; //そのディレクトリをそのまま選ぶ
                DirSelect();
            }
        }

        private void button1_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// _dayNameに暦生成ネームを設定
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            _dayName = DayTo36();
            _fileNameKey = _dayName;
            textBox1.Text = _dayName;
            RenameText();
        }
    }
}