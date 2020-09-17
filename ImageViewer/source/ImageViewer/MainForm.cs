using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RenameAllocateViewer
{
    public enum FitMode
    {
        Auto,
        AutoReverse,
        Horizontal,
        Vertical
    }

    public enum ScrollMode
    {
        Zoom,
        Scroll
    }

    public enum ImageOrientation
    {
        Horizontal,
        Vertical
    }

    public enum HelpNum
    {
        Classification,
        Rename,
        Bookmark,
        Basic,
        Listbox,
        Zoom
    }

    public enum Direction
    {
        Previous,
        Next
    }

    public enum CutOrCopy
    {
        CopyMode,
        CutMode
    }

    public partial class MainForm : Form
    {
        /// <summary>
        /// 開いた画像ファイル名
        /// </summary>
        private string _openedFileName;
        /// <summary>
        /// 画像データ
        /// </summary>
        private ImagingSolution.Imaging.ImageData _img;
        /// <summary>
        /// Bitmapクラス（表示用）
        /// </summary>
        private Bitmap _bmp;
        /// <summary>
        /// 表示する画像の領域
        /// </summary>
        private RectangleF _srcRect;
        /// <summary>
        /// 描画元を指定する３点の座標（左上、右上、左下の順）
        /// </summary>
        private PointF[] _srcPoints = new PointF[3];
        /// <summary>
        /// 描画用Graphicsオブジェクト
        /// </summary>
        private Graphics _gPicbox = null;
        /// <summary>
        /// マウスダウンフラグ
        /// </summary>
        private bool _mouseDownFlg = false;
        /// <summary>
        /// マウスをクリックした位置の保持用
        /// </summary>
        private PointF _oldPoint;
        /// <summary>
        /// アフィン変換行列
        /// </summary>
        private System.Drawing.Drawing2D.Matrix _matAffine;
        /// <summary>
        /// 開いているファイルのあるディレクトリ
        /// </summary>
        private string _fileDirectory;
        // ディレクトリ内のファイル一覧
        private string[] _fileList;

        private CutOrCopy _cutOrCopy;

        /// <summary>
        /// FitMode{Auto,Horizontal,Vertically}
        /// </summary>
        private FitMode _fitSizeMode = FitMode.Auto;
        /// <summary>
        /// ScrollMode{Zoom,Scroll}
        /// </summary>
        private ScrollMode _scrollMode = ScrollMode.Scroll;
        /// <summary>
        /// スクロール量
        /// </summary>
        private int _scrollAmount = 200;
        /// <summary>
        /// 画像が縦長か横長か
        /// </summary>
        private ImageOrientation _imageOrientation = ImageOrientation.Horizontal;

        public MainForm()
        {
            InitializeComponent();

            TransparentLabel();

            CutAnPasToolStripMenuItem.Text = "カット＆ペーストに切り替える";
            仕分け後ToolStripMenuItem.Text = "カット＆ペーストに切り替える";
            cutPasteToolStripMenuItem.Text = "カット＆ペーストに切り替える";

            _cutOrCopy = CutOrCopy.CopyMode;


            UIDisplay();

            BookMarkNaming();


        }

        /********************/
        /*イベントハンドラ群*/
        /********************/

        private void MainForm_Load(object sender, EventArgs e)
        {
            // ホイールイベントの追加
            this.picImage.MouseWheel
                += new System.Windows.Forms.MouseEventHandler(this.picImage_MouseWheel);

            picImage.AllowDrop = true;

            // リサイズイベントを強制的に実行（Graphicsオブジェクトの作成のため）
            MainForm_Resize(null, null);

            // Matrixクラスの確保（単位行列が代入される）
            _matAffine = new System.Drawing.Drawing2D.Matrix();

            // コマンドラインの確認
            var cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                OpenImageFile(cmds[1]);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 解放
            if (_img != null)
            {
                _img.Dispose();
            }
            if (_bmp != null)
            {
                _bmp.Dispose();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if ((picImage.Width == 0) || (picImage.Height == 0)) return;

            // PictureBoxと同じ大きさのBitmapクラスを作成する。
            Bitmap bmpPicBox = new Bitmap(picImage.Width, picImage.Height);
            // 空のBitmapをPictureBoxのImageに指定する。
            picImage.Image = bmpPicBox;
            // Graphicsオブジェクトの作成(FromImageを使う)
            _gPicbox = Graphics.FromImage(picImage.Image);

            // 補間モードの設定（このサンプルではNearestNeighborに設定）
            _gPicbox.InterpolationMode
                = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
            // 画像の描画
            DrawImage();
        }

        private void picImage_DragDrop(object sender, DragEventArgs e)
        {
            var fileName =
                    (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (System.IO.Directory.Exists(fileName[0]))
            {
                var underFileName = (string[])System.IO.Directory.GetFiles(fileName[0], "*", System.IO.SearchOption.TopDirectoryOnly);
                _openedFileName = underFileName[0];
                OpenImageFIleFirst(_openedFileName);
            }
            else
            {
                //コントロール内にドロップされたとき実行される
                //ドロップされたすべてのファイル名を取得する
                _openedFileName = fileName[0];
                OpenImageFIleFirst(_openedFileName);
            }
        }
        private void mnuFileOpen_Click(object sender, EventArgs e)
        {

        }
        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            // 終了
            this.Close();
        }
        private void picImage_MouseDown(object sender, MouseEventArgs e)
        {
            // フォーカスの設定
            //（クリックしただけではMouseWheelイベントが有効にならない）
            picImage.Focus();
            // マウスをクリックした位置の記録
            _oldPoint.X = e.X;
            _oldPoint.Y = e.Y;
            // マウスダウンフラグ
            _mouseDownFlg = true;
        }
        private void picImage_MouseMove(object sender, MouseEventArgs e)
        {
            // マウスをクリックしながら移動中のとき
            if (_mouseDownFlg == true)
            {
                LabelVisualizer();
                // 画像の移動
                _matAffine.Translate(e.X - _oldPoint.X, e.Y - _oldPoint.Y,
                    System.Drawing.Drawing2D.MatrixOrder.Append);
                // 画像の描画
                DrawImage();

                // ポインタ位置の保持
                _oldPoint.X = e.X;
                _oldPoint.Y = e.Y;

                LabelVisualizer();
            }

            // マウスポインタの位置の輝度値表示
            DispPixelInfo(_matAffine, _img, e.Location);
        }
        private void picImage_MouseUp(object sender, MouseEventArgs e)
        {
            // マウスダウンフラグ
            _mouseDownFlg = false;
        }
        private void picImage_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        ///  マウスホイールイベント
        /// </summary>
        private void picImage_MouseWheel(object sender, MouseEventArgs e)
        {
            LabelVisualizer();

            if (_scrollMode == ScrollMode.Zoom)
            {
                if (e.Delta > 0)
                {
                    // 拡大
                    if (_matAffine.Elements[0] < 100)  // X方向の倍率を代表してチェック
                    {
                        // ポインタの位置周りに拡大
                        ScaleAt(ref _matAffine, 1.5f, e.Location);
                    }
                }
                else
                {
                    // 縮小
                    if (_matAffine.Elements[0] > 0.01)  // X方向の倍率を代表してチェック
                    {
                        // ポインタの位置周りに縮小
                        ScaleAt(ref _matAffine, 1.0f / 1.5f, e.Location);
                    }
                }
                // 画像の描画
                DrawImage();
            }
            else if (_scrollMode == ScrollMode.Scroll)
            {
                int y = _scrollAmount;
                if (e.Delta < 0)
                {
                    y *= -1;
                }
                else
                {
                    y *= 1;
                }

                if (_imageOrientation == ImageOrientation.Vertical)
                {
                    // ピクチャボックスの縦方法に画像表示を合わせる場合
                    _matAffine.Translate(0, y, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
                else
                {
                    _matAffine.Translate(y, 0, System.Drawing.Drawing2D.MatrixOrder.Append);
                }

                DrawImage();
            }
            else
            {
                /*NOP*/
            }

            LabelVisualizer();
        }
        private void picImage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            if (Path.GetFileName(_openedFileName) != null)
            {
                if ((e.Modifiers & Keys.Shift) == Keys.Shift || (Control.ModifierKeys & Keys.Control) == Keys.Control)
                {

                }
                else
                {
                    if ((e.KeyCode == Keys.Right))
                    {
                        // 次の画像ファイルを開く
                        OpenNextFile(_openedFileName);
                    }
                    else if ((e.KeyCode == Keys.Left))
                    {
                        // 前の画像ファイルを開く
                        OpenPreviousFile(_openedFileName);
                    }
                    else if ((e.KeyCode == Keys.F2))
                    {
                        string name = Path.GetFileName(_openedFileName);
                        //クリップボードにファイル名をコピーする
                        Clipboard.SetText(name);
                        label3.Text = (name + "をクリップボードにコピーしました。");

                    }
                    else if (e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1)
                    {
                        // 仕分けフォルダ１を開く
                        Classification(Properties.Settings.Default.StoreDir1, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.Q)
                    {
                        // 仕分けフォルダを開く2
                        Classification(Properties.Settings.Default.StoreDir2, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.A)
                    {
                        // 仕分けフォルダを開く3
                        Classification(Properties.Settings.Default.StoreDir3, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.Z)
                    {
                        // 仕分けフォルダを開く4
                        Classification(Properties.Settings.Default.StoreDir4, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.D2)
                    {
                        // 仕分けフォルダを開く5
                        Classification(Properties.Settings.Default.StoreDir5, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.W)
                    {
                        // 仕分けフォルダを開く6
                        Classification(Properties.Settings.Default.StoreDir6, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.S)
                    {
                        // 仕分けフォルダを開く7
                        Classification(Properties.Settings.Default.StoreDir7, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.X)
                    {
                        // 仕分けフォルダを開く8
                        Classification(Properties.Settings.Default.StoreDir8, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.D3)
                    {
                        // 仕分けフォルダを開く9
                        Classification(Properties.Settings.Default.StoreDir9, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.E)
                    {
                        // 仕分けフォルダを開く10
                        Classification(Properties.Settings.Default.StoreDir10, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.D)
                    {
                        Classification(Properties.Settings.Default.StoreDir11, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.C)
                    {
                        Classification(Properties.Settings.Default.StoreDir12, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.NumPad4 || e.KeyCode == Keys.D4)
                    {
                        Classification(Properties.Settings.Default.StoreDir13, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.R)
                    {
                        Classification(Properties.Settings.Default.StoreDir14, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.F)
                    {
                        Classification(Properties.Settings.Default.StoreDir15, _openedFileName);
                    }
                    else if (e.KeyCode == Keys.V)
                    {
                        Classification(Properties.Settings.Default.StoreDir16, _openedFileName);
                    }

                }
            }
            else
            {
                /**/
            }

        }

        private void label1_Click_1(object sender, EventArgs e)
        {
            if (Path.GetFileName(_openedFileName) != null)
            {// 次の画像ファイルを開く
                OpenNextFile(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            if (Path.GetFileName(_openedFileName) != null)
            {
                // 前の画像ファイルを開く
                OpenPreviousFile(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void 次のフォルダの先頭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Path.GetFileName(_openedFileName) != null)
            {// 次の画像ファイルを開く
                OpenNextDir(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();

        }

        private void ファイル名で昇順ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSortName();
        }

        private void ファイル名で降順ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSortNameR();
        }

        private void ランダムに並べるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSortRand();
        }

        private void 更新日時で昇順ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSortDay();
        }

        private void 更新日時で降順ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSortDayR();
        }

        private void 先頭のファイルToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFirstFile(_openedFileName);
        }

        private void 全画面切り替えToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FullScreen();
        }

        private void 横に広げて合わせるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitSizeHorizontal();
        }

        private void 縦に広げて合わせるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitSizeVertical();
        }

        private void 画面内に収まるように自動調整ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitSizeAuto();
        }

        private void 常に拡大するように自動調整ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitSizeAutoReverse();
        }

        private void ファイル名で昇順ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileSortName();
        }

        private void ファイル名で降順ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileSortNameR();
        }

        private void ランダムに並べるToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileSortRand();
        }

        private void 更新日時で昇順ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileSortDay();
        }

        private void 更新日時で降順ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileSortDayR();
        }

        private void 横に広げて合わせるToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FitSizeHorizontal();
        }

        private void 縦に広げて合わせるToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FitSizeVertical();
        }

        private void 自動調整ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitSizeAuto();
        }

        private void 自動調整拡大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitSizeAutoReverse();
        }

        private void 全画面切り替えToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FullScreen();
        }

        private void picImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*全画面切り替え*/
            FullScreen();
        }

        private void スクロールToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_scrollMode == ScrollMode.Scroll)
            {
                _scrollMode = ScrollMode.Zoom;
            }
            else
            {
                _scrollMode = ScrollMode.Scroll;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string targetPath = Properties.Settings.Default.StoreDir1;

            if (listBox1.SelectedIndex == 0)
            {
                targetPath = Properties.Settings.Default.StoreDir1;
            }
            else if (listBox1.SelectedIndex == 1)
            {
                targetPath = Properties.Settings.Default.StoreDir2;
            }
            else if (listBox1.SelectedIndex == 2)
            {
                targetPath = Properties.Settings.Default.StoreDir3;
            }
            else if (listBox1.SelectedIndex == 3)
            {
                targetPath = Properties.Settings.Default.StoreDir4;
            }
            else if (listBox1.SelectedIndex == 4)
            {
                targetPath = Properties.Settings.Default.StoreDir5;
            }
            else if (listBox1.SelectedIndex == 5)
            {
                targetPath = Properties.Settings.Default.StoreDir6;
            }
            else if (listBox1.SelectedIndex == 6)
            {
                targetPath = Properties.Settings.Default.StoreDir7;
            }
            else if (listBox1.SelectedIndex == 7)
            {
                targetPath = Properties.Settings.Default.StoreDir8;
            }
            else if (listBox1.SelectedIndex == 8)
            {
                targetPath = Properties.Settings.Default.StoreDir9;
            }
            else if (listBox1.SelectedIndex == 9)
            {
                targetPath = Properties.Settings.Default.StoreDir10;
            }
            else if (listBox1.SelectedIndex == 10)
            {
                targetPath = Properties.Settings.Default.StoreDir11;
            }
            else if (listBox1.SelectedIndex == 11)
            {
                targetPath = Properties.Settings.Default.StoreDir12;
            }
            else if (listBox1.SelectedIndex == 12)
            {
                targetPath = Properties.Settings.Default.StoreDir13;
            }
            else if (listBox1.SelectedIndex == 13)
            {
                targetPath = Properties.Settings.Default.StoreDir14;
            }
            else if (listBox1.SelectedIndex == 14)
            {
                targetPath = Properties.Settings.Default.StoreDir15;
            }
            else if (listBox1.SelectedIndex == 15)
            {
                targetPath = Properties.Settings.Default.StoreDir16;
            }
            else
            {

            }

            if (targetPath != "" && _openedFileName != null)
            {
                try
                {
                    Classification(targetPath, _openedFileName);
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show("指定されたフォルダが見つかりませんでした。");
                }
            }
            else
            {
                /**/
            }
        }

        private void label5_MouseEnter(object sender, EventArgs e)
        {
            StrageLabel();
            UIDisplay();
        }
        private void listBox1_MouseLeave(object sender, EventArgs e)
        {
            listBox1.Visible = false;
            label3.Visible = false;
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            listBox1.Visible = false;
            label3.Visible = false;
            label5.ResetText();
        }

        private void label6_MouseEnter(object sender, EventArgs e)
        {

            StrageLabel();
            UIDisplay();

        }

        private void リストの先頭へ移動ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFirstFile(_openedFileName);
        }

        private void リネームToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Namer namer = new Namer();
            namer.Show();
        }

        private void 仕分けフォルダToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            FullScreen();
        }

        private void リネームToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Namer namer = new Namer();
            namer.Show();
        }

        private void 仕分けるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                DoClassification();
            }
            else
            {
                MessageBox.Show("画像ファイルを開いていません。");
            }
        }

        private void ファイル名をコピーF2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                string name = Path.GetFileName(_openedFileName);
                //クリップボードにファイル名をコピーする
                Clipboard.SetText(name);
                label3.Text = (name + "をクリップボードにコピーしました。");
            }
            else
            {
                /**/
            }
        }

        private void ファイルを開くCtrlOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ファイルを選択した場合
            // ファイルを開くダイアログの作成 
            OpenFileDialog dlg = new OpenFileDialog();
            // ファイルフィルタ 
            dlg.Filter = "画像ﾌｧｲﾙ(*.bmp,*.jpg,*.png,*.tif,*.ico|*.bmp;*.jpg;*.png;*.tif;*.ico";
            // ダイアログの表示 （Cancelボタンがクリックされた場合は何もしない）
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            _openedFileName = dlg.FileName;

            // 画像ファイルを開く
            OpenImageFIleFirst(_openedFileName);
        }

        private void 全画面切り替えToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FullScreen();
        }

        private void 前のフォルダToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Path.GetFileName(_openedFileName) != null)
            {
                OpenPreviousDir(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void 次のフォルダToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Path.GetFileName(_openedFileName) != null)
            {
                OpenNextDir(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void 前のフォルダToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Path.GetFileName(_openedFileName) != null)
            {
                OpenPreviousDir(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void ブックマーク１ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BookMarker(0);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            BookMarker(1);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            BookMarker(2);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            BookMarker(3);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            BookMarker(4);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            BookMarker(5);
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            BookMarker(6);
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            BookMarker(7);
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            BookMarker(8);
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            BookMarker(9);
        }

        private void 大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scrollAmount = 400;
        }

        private void 中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scrollAmount = 200;
        }

        private void 小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scrollAmount = 100;
        }

        private void ファイルを削除するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                FileDelete();
            }
            else
            {
                /**/
            }
        }

        private void ファイルを開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ファイルを開くダイアログの作成 
            OpenFileDialog dlg = new OpenFileDialog();
            // ファイルフィルタ 
            dlg.Filter = "画像ﾌｧｲﾙ(*.bmp,*.jpg,*.png,*.tif,*.ico)|*.bmp;*.jpg;*.png;*.tif;*.ico)";
            // ダイアログの表示 （Cancelボタンがクリックされた場合は何もしない）
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            _openedFileName = dlg.FileName;

            // 画像ファイルを開く
            OpenImageFile(_openedFileName);
        }

        private void フォルダ設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void 現在のファイルを仕分けるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                DoClassification();
            }
            else
            {
                MessageBox.Show("画像ファイルを開いていません。");
            }
        }

        private void 大ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _scrollAmount = 400;
        }

        private void 中ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _scrollAmount = 200;
        }

        private void 小ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _scrollAmount = 100;
        }

        private void ファイルを削除するToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                FileDelete();
            }
            else
            {
                /**/
            }
        }

        private void ファイル名をコピーするToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                string name = Path.GetFileName(_openedFileName);
                //クリップボードにファイル名をコピーする
                Clipboard.SetText(name);
                label3.Text = (name + "をクリップボードにコピーしました。");
            }
            else
            {
                /**/
            }
        }

        private void ブックマークToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            BookMarkNaming();
        }

        private void ブックマークするToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            BookMarkNaming();
        }

        private void ファイル名をパスごとコピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                //クリップボードにファイル名をコピーする
                Clipboard.SetText(_openedFileName);
                label3.Text = (_openedFileName + "をクリップボードにコピーしました。");
            }
            else
            {
                /**/
            }
        }

        private void ファイル名をパスごとコピーToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                //クリップボードにファイル名をコピーする
                Clipboard.SetText(_openedFileName);
                label3.Text = (_openedFileName + "をクリップボードにコピーしました。");
            }
            else
            {
                /**/
            }
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            ArrowImageSwitch(1, true);
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            ArrowImageSwitch(1, false);
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            ArrowImageSwitch(2, true);
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            ArrowImageSwitch(2, false);
        }

        private void 矢印表示非表示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ArrowDisplay == false)
            {
                Properties.Settings.Default.ArrowDisplay = true;
                Properties.Settings.Default.Save();
                矢印表示非表示ToolStripMenuItem.Text = "矢印を表示しない";
            }
            else
            {
                Properties.Settings.Default.ArrowDisplay = false;
                Properties.Settings.Default.Save();
                矢印表示非表示ToolStripMenuItem.Text = "矢印を表示する";
            }
        }

        private void 仕分けフォルダを非表示するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ClassificDisplay == true)
            {
                Properties.Settings.Default.ClassificDisplay = false;
                Properties.Settings.Default.Save();
                仕分けフォルダを非表示するToolStripMenuItem.Text = "仕分けフォルダを表示する";
            }
            else
            {
                Properties.Settings.Default.ClassificDisplay = true;
                Properties.Settings.Default.Save();
                仕分けフォルダを非表示するToolStripMenuItem.Text = "仕分けフォルダを表示しない";
            }
        }
        
        private void ファイル情報を非表示するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FileInfoDisplay == true)
            {
                Properties.Settings.Default.FileInfoDisplay = false;
                Properties.Settings.Default.Save();
                ファイル情報を非表示するToolStripMenuItem.Text = "画像情報を表示する";
            }
            else
            {
                Properties.Settings.Default.FileInfoDisplay = true;
                Properties.Settings.Default.Save();
                ファイル情報を非表示するToolStripMenuItem.Text = "画像情報を表示しない";
            }
        }

        private void cutPasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_cutOrCopy == CutOrCopy.CutMode)
            {
                _cutOrCopy = CutOrCopy.CopyMode;
                cutPasteToolStripMenuItem.Text = "カット＆ペーストに切り替える";
                仕分け後ToolStripMenuItem.Text = "カット＆ペーストに切り替える";

            }
            else
            {
                _cutOrCopy = CutOrCopy.CutMode;
                cutPasteToolStripMenuItem.Text = "コピー＆ペーストに切り替える";
                仕分け後ToolStripMenuItem.Text = "コピー＆ペーストに切り替える";
            }
        }

        private void 仕分けフォルダ設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void 現在のファイルを仕分けるToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                DoClassification();
            }
            else
            {
                MessageBox.Show("画像ファイルを開いていません。");
            }
        }

        private void 任意のフォルダに送るToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                DoClassification();
                var list = new List<string>();
                list.AddRange(_fileList);
                list.Remove(_openedFileName);
                _fileList = list.ToArray();
            }
            else
            {
                MessageBox.Show("画像ファイルを開いていません。");
            }
        }

        private void CutAnPasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_cutOrCopy == CutOrCopy.CutMode)
            {
                _cutOrCopy = CutOrCopy.CopyMode;
                cutPasteToolStripMenuItem.Text = "カット＆ペーストに切り替える";
                仕分け後ToolStripMenuItem.Text = "カット＆ペーストに切り替える";

            }
            else
            {
                _cutOrCopy = CutOrCopy.CutMode;
                cutPasteToolStripMenuItem.Text = "コピー＆ペーストに切り替える";
                仕分け後ToolStripMenuItem.Text = "コピー＆ペーストに切り替える";
            }
        }

        private void リネーム画面を開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Namer namer = new Namer();
            namer.Show();
        }

        private void 画面上クリック詳細ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (label2.BackColor == Color.Transparent)
            {
                label2.BackColor = Color.IndianRed;
                label1.BackColor = Color.MediumTurquoise;
                label5.BackColor = Color.OliveDrab;
                label6.BackColor = Color.OliveDrab;

                label1.Text = "「クリック」\n次のファイルへ進む";
                label2.Text = "「クリック」\n前のファイルへ進む";
                label10.Visible = true;
                label11.Visible = true;
                画面上クリック詳細ToolStripMenuItem.Text = "画面上クリック機能を表示しない";
                画面上クリック機能表示ToolStripMenuItem.Text = "画面上クリック機能を表示しない";
            }
            else
            {
                label2.BackColor = Color.Transparent;
                label1.BackColor = Color.Transparent;
                label5.BackColor = Color.Transparent;
                label6.BackColor = Color.Transparent;

                label1.Text = "";
                label2.Text = "";
                label10.Visible = false;
                label11.Visible = false;
                画面上クリック詳細ToolStripMenuItem.Text = "画面上クリック機能を表示する";
                画面上クリック機能表示ToolStripMenuItem.Text = "画面上クリック機能を表示する";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = false;
        }

        private void ヘルプフォルダ仕分けとはToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Classification);
        }

        private void ヘルプリネームとはToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Rename);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            BookMarker(0);
        }

        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            BookMarker(1);
        }

        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            BookMarker(2);
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            BookMarker(3);
        }

        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            BookMarker(4);
        }

        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            BookMarker(5);
        }

        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            BookMarker(6);
        }

        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            BookMarker(7);
        }

        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            BookMarker(8);
        }

        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            BookMarker(9);
        }

        private void ヘルプブックマークとはToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Bookmark);
        }

        private void ヘルプリネームとはToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Rename);
        }

        private void ヘルプブックマークとはToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Bookmark);
        }

        private void 矢印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ArrowDisplay == false)
            {
                Properties.Settings.Default.ArrowDisplay = true;
                Properties.Settings.Default.Save();
                矢印ToolStripMenuItem.Text = "矢印を表示しない";
            }
            else
            {
                Properties.Settings.Default.ArrowDisplay = false;
                Properties.Settings.Default.Save();
                矢印ToolStripMenuItem.Text = "矢印を表示する";
            }
        }

        private void 仕分けフォルダToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ClassificDisplay == true)
            {
                Properties.Settings.Default.ClassificDisplay = false;
                Properties.Settings.Default.Save();
                仕分けフォルダToolStripMenuItem1.Text = "仕分けフォルダを表示する";
            }
            else
            {
                Properties.Settings.Default.ClassificDisplay = true;
                Properties.Settings.Default.Save();
                仕分けフォルダToolStripMenuItem1.Text = "仕分けフォルダを表示しない";
            }
        }

        private void ファイル情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FileInfoDisplay == true)
            {
                Properties.Settings.Default.FileInfoDisplay = false;
                Properties.Settings.Default.Save();
                ファイル情報ToolStripMenuItem.Text = "画像情報を表示する";
            }
            else
            {
                Properties.Settings.Default.FileInfoDisplay = true;
                Properties.Settings.Default.Save();
                ファイル情報ToolStripMenuItem.Text = "画像情報を表示しない";
            }
        }

        private void 基本操作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Basic);
        }

        private void ヘルプフォルダ仕分けとはToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Classification);
        }

        private void 仕分け後ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.CopyInfoDisplay == true)
            {
                Properties.Settings.Default.CopyInfoDisplay = false;
                仕分け後ToolStripMenuItem.Text = "画像仕分け後に情報を表示する";
                仕分けの後ToolStripMenuItem.Text = "画像仕分け後に情報を表示する";
            }
            else
            {
                Properties.Settings.Default.CopyInfoDisplay = true;
                仕分け後ToolStripMenuItem.Text = "画像仕分け後に情報を表示しない";
                仕分けの後ToolStripMenuItem.Text = "画像仕分け後に情報を表示しない";
            }
        }

        private void 任意のフォルダに送るToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_openedFileName != null)
            {
                DoClassification();
                var list = new List<string>();
                list.AddRange(_fileList);
                list.Remove(_openedFileName);
                _fileList = list.ToArray();
            }
            else
            {
                MessageBox.Show("画像ファイルを開いていません。");
            }
        }

        private void 画面上クリック機能表示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (label2.BackColor == Color.Transparent)
            {
                label2.BackColor = Color.IndianRed;
                label1.BackColor = Color.MediumTurquoise;
                label5.BackColor = Color.OliveDrab;
                label6.BackColor = Color.OliveDrab;

                label1.Text = "「クリック」\n次のファイルへ進む";
                label2.Text = "「クリック」\n前のファイルへ進む";
                label10.Visible = true;
                label11.Visible = true;
                画面上クリック詳細ToolStripMenuItem.Text = "画面上クリック機能を表示しない";
                画面上クリック機能表示ToolStripMenuItem.Text = "画面上クリック機能を表示しない";
            }
            else
            {
                label2.BackColor = Color.Transparent;
                label1.BackColor = Color.Transparent;
                label5.BackColor = Color.Transparent;
                label6.BackColor = Color.Transparent;

                label1.Text = "";
                label2.Text = "";
                label10.Visible = false;
                label11.Visible = false;
                画面上クリック詳細ToolStripMenuItem.Text = "画面上クリック機能を表示";
                画面上クリック機能表示ToolStripMenuItem.Text = "画面上クリック機能を表示";
            }
        }

        private void ヘルプ右端のフォルダ名リストについてToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Listbox);
        }

        private void 画面右端のフォルダ名リストについてToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Listbox);
        }

        private void 基本操作ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Basic);
        }

        private void ヘルプ画像仕分けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Classification);
        }

        private void ヘルプリネームToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Rename);
        }

        private void ヘルプブックマークToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Bookmark);
        }

        private void 仕分けの後ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.CopyInfoDisplay == true)
            {
                Properties.Settings.Default.CopyInfoDisplay = false;
                仕分け後ToolStripMenuItem.Text = "画像仕分け後に情報を表示する";
                仕分けの後ToolStripMenuItem.Text = "画像仕分け後に情報を表示する";
            }
            else
            {
                Properties.Settings.Default.CopyInfoDisplay = true;
                仕分け後ToolStripMenuItem.Text = "画像仕分け後に情報を表示しない";
                仕分けの後ToolStripMenuItem.Text = "画像仕分け後に情報を表示しない";
            }
        }

        private void toolStripMenuItem32_Click(object sender, EventArgs e)
        {
            BookMarkOpen(0);
        }

        private void toolStripMenuItem33_Click(object sender, EventArgs e)
        {
            BookMarkOpen(1);
        }

        private void toolStripMenuItem34_Click(object sender, EventArgs e)
        {
            BookMarkOpen(2);
        }

        private void toolStripMenuItem35_Click(object sender, EventArgs e)
        {
            BookMarkOpen(3);
        }

        private void toolStripMenuItem36_Click(object sender, EventArgs e)
        {
            BookMarkOpen(4);
        }

        private void toolStripMenuItem37_Click(object sender, EventArgs e)
        {
            BookMarkOpen(5);
        }

        private void toolStripMenuItem38_Click(object sender, EventArgs e)
        {
            BookMarkOpen(6);
        }

        private void toolStripMenuItem39_Click(object sender, EventArgs e)
        {
            BookMarkOpen(7);
        }

        private void toolStripMenuItem40_Click(object sender, EventArgs e)
        {
            BookMarkOpen(8);
        }

        private void toolStripMenuItem41_Click(object sender, EventArgs e)
        {
            BookMarkOpen(9);
        }

        private void toolStripMenuItem42_Click(object sender, EventArgs e)
        {
            BookMarkOpen(0);
        }

        private void toolStripMenuItem43_Click(object sender, EventArgs e)
        {
            BookMarkOpen(1);
        }

        private void toolStripMenuItem44_Click(object sender, EventArgs e)
        {
            BookMarkOpen(2);
        }

        private void toolStripMenuItem45_Click(object sender, EventArgs e)
        {
            BookMarkOpen(3);
        }

        private void toolStripMenuItem46_Click(object sender, EventArgs e)
        {
            BookMarkOpen(4);
        }

        private void toolStripMenuItem47_Click(object sender, EventArgs e)
        {
            BookMarkOpen(5);
        }

        private void toolStripMenuItem48_Click(object sender, EventArgs e)
        {
            BookMarkOpen(6);
        }

        private void toolStripMenuItem49_Click(object sender, EventArgs e)
        {
            BookMarkOpen(7);
        }

        private void toolStripMenuItem50_Click(object sender, EventArgs e)
        {
            BookMarkOpen(8);
        }

        private void toolStripMenuItem51_Click(object sender, EventArgs e)
        {
            BookMarkOpen(9);
        }

        private void もう一つ開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm main2 = new MainForm();
            main2.Show();
        }

        private void アプリをもう一つ開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm main2 = new MainForm();
            main2.Show();
        }

        private void 画面をズームするにはToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Zoom);
        }

        private void ズームするにはToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDisplay(HelpNum.Zoom);
        }

        private void toolStripMenuItem32_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(0);
        }

        private void toolStripMenuItem33_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(1);
        }

        private void toolStripMenuItem34_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(2);
        }

        private void toolStripMenuItem35_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(3);
        }

        private void toolStripMenuItem36_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(4);
        }

        private void toolStripMenuItem37_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(5);
        }

        private void toolStripMenuItem38_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(6);
        }

        private void toolStripMenuItem39_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(7);
        }

        private void toolStripMenuItem40_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(8);
        }

        private void toolStripMenuItem41_Click_1(object sender, EventArgs e)
        {
            BookMarkOpen(9);
        }
        private void toolStripMenuItem3_Click_1(object sender, EventArgs e)
        {
            BookMarker(0);
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            BookMarker(1);
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            BookMarker(2);
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            BookMarker(3);
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            BookMarker(4);
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            BookMarker(5);
        }

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            BookMarker(6);
        }

        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            BookMarker(7);
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            BookMarker(8);
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            BookMarker(9);
        }

        private void オープン中の画像をブックマークToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BookMarker(0);
        }

        private void オープン中の画像をブックマークToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            BookMarker(1);
        }

        private void オープン中の画像をブックマークToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            BookMarker(2);
        }

        private void オープン中の画像をブックマークToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            BookMarker(3);
        }

        private void オープン中の画像をブックマークToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            BookMarker(4);
        }

        private void オープン中の画像をブックマークToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            BookMarker(5);
        }

        private void オープン中の画像をブックマークToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            BookMarker(6);
        }

        private void オープン中の画像をブックマークToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            BookMarker(7);
        }

        private void オープン中の画像をブックマークToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            BookMarker(8);
        }

        private void オープン中の画像をブックマークToolStripMenuItem9_Click(object sender, EventArgs e)
        {
            BookMarker(9);
        }

        private void ヘルプボタンToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 abForm = new AboutBox1();
            abForm.ShowDialog();
        }

        private void ページ進むToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenNextFile_5(_openedFileName);
        }

        private void ページ戻るToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenPreviousFile_5(_openedFileName);
        }

        private void ページ進むToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenNextFile_5(_openedFileName);
        }

        private void ページ戻るToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenPreviousFile_5(_openedFileName);
        }
    }
}
