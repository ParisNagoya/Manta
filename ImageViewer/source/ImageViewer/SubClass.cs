using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace RenameAllocateViewer
{
    public partial class MainForm
    {
        private void UIDisplay()
        {
            if (Properties.Settings.Default.ClassificDisplay == true)
            {
                仕分けフォルダを非表示するToolStripMenuItem.Text = "仕分けフォルダを表示しない";
                仕分けフォルダToolStripMenuItem1.Text = "仕分けフォルダを表示しない";
            }
            else
            {
                仕分けフォルダを非表示するToolStripMenuItem.Text = "仕分けフォルダを表示する";
                仕分けフォルダToolStripMenuItem1.Text = "仕分けフォルダを表示する";
            }

            if (Properties.Settings.Default.ArrowDisplay == true)
            {
                矢印表示非表示ToolStripMenuItem.Text = "矢印を表示しない";
                矢印ToolStripMenuItem.Text = "矢印を表示しない";
            }
            else
            {
                矢印表示非表示ToolStripMenuItem.Text = "矢印を表示する";
                矢印ToolStripMenuItem.Text = "矢印を表示する";
            }

            if (Properties.Settings.Default.FileInfoDisplay == true)
            {
                ファイル情報を非表示するToolStripMenuItem.Text = "画像情報を表示しない";
                ファイル情報ToolStripMenuItem.Text = "画像情報を表示しない";
            }
            else
            {
                ファイル情報を非表示するToolStripMenuItem.Text = "画像情報を表示する";
                ファイル情報ToolStripMenuItem.Text = "画像情報を表示する";
            }

            if (Properties.Settings.Default.ClassificDisplay == true)
            {
                仕分け後ToolStripMenuItem.Text = "画像仕分け後に情報を表示しない";
                仕分けの後ToolStripMenuItem.Text = "画像仕分け後に情報を表示しない";
            }
            else
            {
                仕分け後ToolStripMenuItem.Text = "画像仕分け後に情報を表示する";
                仕分けの後ToolStripMenuItem.Text = "画像仕分け後に情報を表示する";
            }
        }

        private void TransparentLabel()
        {
            //Label1の親コントロールをpicImageとする
            picImage.Controls.Add(label1);

            //Label1の位置をpicImage内の位置に変更する
            label1.Top = label1.Top - picImage.Top;
            label1.Left = label1.Left - picImage.Left;

            picImage.Controls.Add(label2);
            label2.Top = label2.Top - picImage.Top;
            label2.Left = label2.Left - picImage.Left;

            picImage.Controls.Add(label5);
            label5.Top = label5.Top - picImage.Top;
            label5.Left = label5.Left - picImage.Left;

            picImage.Controls.Add(label6);
            label6.Top = label6.Top - picImage.Top;
            label6.Left = label6.Left - picImage.Left;

            picImage.Controls.Add(label11);
            label11.Top = label11.Top - picImage.Top;
            label11.Left = label11.Left - picImage.Left;
        }

        private void FileDelete()
        {
            
            FileSystem.DeleteFile(_openedFileName, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            label3.Visible = true;
            label3.Text = (Path.GetFileName(_openedFileName) + "を削除しました。");
            OpenNextFile(_openedFileName);
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

        private void OpenDirSelect(string filePath, Direction direction)
        {
            if (filePath == "") return;

            string dirPath = Path.GetDirectoryName(filePath);
            DirectoryInfo dirParentInfo = Directory.GetParent(dirPath);
            string[] dirNameList = Directory.GetDirectories(dirParentInfo.FullName);
            int index = Array.IndexOf(dirNameList, dirPath);

            if(direction == Direction.Previous)
            {
                if (index > 0)
                {
                    string nextDirPath = Path.Combine(dirParentInfo.FullName, dirNameList[index - 1]);
                    string[] nextFileList = Directory.GetFiles(nextDirPath);
                    string[] listTemp = _fileList;
                    LimitedList(nextDirPath);

                    if (_fileList.Length != 0)
                    {
                        OpenImageFile(_fileList[0]);
                    }
                    else
                    {
                        MessageBox.Show("前のフォルダには画像ファイルがありません。");
                        _fileList = listTemp;
                    }
                }
                else
                {
                    MessageBox.Show("最初のフォルダです。");
                }
            }else if(direction == Direction.Next)
            {
                if (index < dirNameList.Length - 1)
                {
                    string nextDirPath = Path.Combine(dirParentInfo.FullName, dirNameList[index + 1]);
                    string[] nextFileList = Directory.GetFiles(nextDirPath);
                    string[] listTemp = _fileList;
                    LimitedList(nextDirPath);

                    if (_fileList.Length != 0)
                    {
                        OpenImageFile(_fileList[0]);
                    }
                    else
                    {
                        MessageBox.Show("次のフォルダには画像ファイルがありません。");
                        _fileList = listTemp;
                    }
                }
                else
                {
                    MessageBox.Show("最後のフォルダです。");
                }
            }

        }

        /// <summary>
        /// 前のディレクトリを開く
        /// </summary>
        private void OpenPreviousDir(string filePath)
        {
            OpenDirSelect(filePath, Direction.Previous);
        }

        /// <summary>
        /// 次のディレクトリを開く
        /// </summary>
        private void OpenNextDir(string filePath)
        {
            OpenDirSelect(filePath, Direction.Next);
        }

        /// <summary>
        /// 画像ファイルを開く
        /// </summary>
        /// <param name="filename">画像ファイルのパス</param>
        private void OpenImageFile(string filename)
        {
            if (IsImageFile(filename) == false) return;

            LabelVisualizer();

            // 画像データの確保
            if (_img != null)
            {
                _img.Dispose();
            }
            if (_bmp != null)
            {
                _bmp.Dispose();
            }

            string dirPath = System.IO.Path.GetDirectoryName(filename);

            FileAttributes fa = File.GetAttributes(dirPath);

            fa = fa & ~FileAttributes.ReadOnly;

            File.SetAttributes(filename, fa);


            _img = new ImagingSolution.Imaging.ImageData(filename);//ここで表示できない
            // 表示用
            _bmp = _img.ToBitmap();


            // 画像サイズ
            /**lblImageInfo.Text =
                _img.Width.ToString() + " x " +
                _img.Height.ToString() + " x " +
                _img.ImageBit.ToString() + "bit";
            **/
            // 表示する画像の領域

            _srcRect = new RectangleF(-0.5f, -0.5f, _img.Width, _img.Height);
            // 描画元を指定する３点の座標（左上、右上、左下の順）
            _srcPoints[0] = new PointF(_srcRect.Left, _srcRect.Top);
            _srcPoints[1] = new PointF(_srcRect.Right, _srcRect.Top);
            _srcPoints[2] = new PointF(_srcRect.Left, _srcRect.Bottom);

            if (_fitSizeMode == FitMode.AutoReverse)
            {
                if (_img.Width > _img.Height)
                {
                    _imageOrientation = ImageOrientation.Horizontal;
                }
                else
                {
                    _imageOrientation = ImageOrientation.Vertical;
                }
            }
            else
            {
                /**/
            }

            // 画像全体を表示
            ZoomFit(ref _matAffine, _img, picImage);
            _openedFileName = filename;
            if (_fitSizeMode == FitMode.Horizontal)
            {
                // ImagePullTop();
            }
            // 画像の描画
            DrawImage();

            this.Text = System.IO.Path.GetFileName(filename) + " - RenameAllocateViewer";

            LabelVisualizer();
            // 指定したファイルのディレクトリ
            _fileDirectory = System.IO.Path.GetDirectoryName(filename);
            // ディレクトリ内のファイル一覧
        }


        /// <summary>
        /// リストの最初のファイルを開く
        /// </summary>
        private void OpenImageFIleFirst(string filename)
        {   
            // 指定したファイルのディレクトリ
            _fileDirectory = System.IO.Path.GetDirectoryName(filename);
            // ディレクトリ内のファイル一覧
            LimitedList(_fileDirectory);
            OpenImageFile(_openedFileName);
        }

        /// <summary>
        /// 画像の描画
        /// </summary>
        private void DrawImage()
        {
            if (_img == null) return;
            if (_bmp == null) return;
            // ピクチャボックスのクリア
            _gPicbox.Clear(picImage.BackColor);

            // 描画先の座標をアフィン変換で求める（左上、右上、左下の順）
            PointF[] destPoints = (PointF[])_srcPoints.Clone();
            // 描画先の座標をアフィン変換で求める（変換後の座標は上書きされる）
            _matAffine.TransformPoints(destPoints);
            // 描画
            _gPicbox.DrawImage(
                _bmp,
                destPoints,
                _srcRect,
                GraphicsUnit.Pixel
                );
            // 再描画
            picImage.Refresh();

        }

        /// <summary>
        /// 指定した点（point）周りの拡大縮小
        /// </summary>
        /// <param name="scale">倍率</param>
        /// <param name="point">基準点の座標</param>
        private void ScaleAt(ref System.Drawing.Drawing2D.Matrix mat,
            float scale, PointF point)
        {
            // 原点へ移動
            mat.Translate(-point.X, -point.Y,
                System.Drawing.Drawing2D.MatrixOrder.Append);
            // 拡大縮小
            mat.Scale(scale, scale,
                System.Drawing.Drawing2D.MatrixOrder.Append);
            // 元へ戻す
            mat.Translate(point.X, point.Y,
                System.Drawing.Drawing2D.MatrixOrder.Append);
        }

        /// <summary>
        /// 画像をピクチャボックスのサイズに合わせて全体に表示するアフィン変換行列を求める
        /// </summary>
        /// <param name="mat">アフィン変換行列</param>
        /// <param name="image">画像データ</param>
        /// <param name="dst">描画先のピクチャボックス</param>
        private void ZoomFit(ref System.Drawing.Drawing2D.Matrix mat,
            ImagingSolution.Imaging.ImageData image, PictureBox dst)
        {
            // アフィン変換行列の初期化（単位行列へ）
            mat.Reset();

            int srcWidth = image.Width;
            int srcHeight = image.Height;
            int dstWidth = dst.Width;
            int dstHeight = dst.Height;

            float scale;

            if (_fitSizeMode == FitMode.Auto)
            {

                // 縦に合わせるか？横に合わせるか？
                if (srcHeight * dstWidth > dstHeight * srcWidth)
                {
                    // ピクチャボックスの縦方法に画像表示を合わせる場合
                    scale = dstHeight / (float)srcHeight;
                    mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                    // 中央へ平行移動
                    //mat.Translate((dstWidth - srcWidth * scale) / 2f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
                    mat.Translate((dstWidth - srcWidth * scale) / 2f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
                else
                {
                    // ピクチャボックスの横方法に画像表示を合わせる場合
                    scale = dstWidth / (float)srcWidth;
                    mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                    // 中央へ平行移動
                    //mat.Translate(0f, (dstHeight - srcHeight * scale) / 2f, System.Drawing.Drawing2D.MatrixOrder.Append);
                    mat.Translate(0f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
            }
            else if (_fitSizeMode == FitMode.AutoReverse)
            {

                // 縦に合わせるか？横に合わせるか？
                if (srcHeight * dstWidth < dstHeight * srcWidth)
                {
                    // ピクチャボックスの縦方法に画像表示を合わせる場合
                    scale = dstHeight / (float)srcHeight;
                    mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                    // 中央へ平行移動
                    //mat.Translate((dstWidth - srcWidth * scale) / 2f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
                    mat.Translate((dstWidth - srcWidth * scale) / 2f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
                else
                {
                    // ピクチャボックスの横方法に画像表示を合わせる場合
                    scale = dstWidth / (float)srcWidth;
                    mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                    // 中央へ平行移動
                    //mat.Translate(0f, (dstHeight - srcHeight * scale) / 2f, System.Drawing.Drawing2D.MatrixOrder.Append);
                    mat.Translate(0f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
            }
            else if (_fitSizeMode == FitMode.Horizontal)
            {
                // ピクチャボックスの横方法に画像表示を合わせる場合
                scale = dstWidth / (float)srcWidth;
                mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 中央へ平行移動
                //mat.Translate(0f, (dstHeight - srcHeight * scale) / 2f, System.Drawing.Drawing2D.MatrixOrder.Append);
                mat.Translate(0f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
            }
            else if (_fitSizeMode == FitMode.Vertical)
            {
                // ピクチャボックスの縦方法に画像表示を合わせる場合
                scale = dstHeight / (float)srcHeight;
                mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 中央へ平行移動
                mat.Translate((dstWidth - srcWidth * scale) / 2f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
            }
        }

        /// <summary>
        /// ソートメソッド
        /// </summary>
        private void FileSortName()
        {
            if (_openedFileName != null)
            {
                _fileList = _fileList.OrderBy(x => x).ToArray();
            }
            else
            {
                /**/
            }
        }

        private void FileSortNameR()
        {
            if (_openedFileName != null)
            {
                _fileList = _fileList.OrderByDescending(x => x).ToArray();
            }
            else
            {
                /**/
            }
        }

        private void FileSortDay()
        {
            if (_openedFileName != null)
            {
                // 更新日時でソート。古いものが前に並ぶ。
                _fileList = _fileList.OrderBy(x => File.GetLastWriteTime(x)).ToArray();


            }
            else
            {
                /**/
            }
        }

        private void FileSortRand()
        {
            if (_openedFileName != null)
            {
                _fileList = _fileList.OrderBy(i => Guid.NewGuid()).ToArray();
                OpenFirstFile(_openedFileName);
            }
            else
            {
                /**/
            }
        }

        private void FileSortDayR()
        {
            if (_openedFileName != null)
            {
                // 更新日時でソート。新しいものが前に並ぶ。
                _fileList = _fileList.OrderByDescending(x => File.GetLastWriteTime(x)).ToArray();
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// マウスポインタの位置の画像の輝度値を表示
        /// </summary>
        /// <param name="mat">画像を表示しているアフィン変換行列</param>
        /// <param name="image">表示している画像</param>
        /// <param name="pointPictureBox">表示先のピクチャボックス</param>
        private void DispPixelInfo(System.Drawing.Drawing2D.Matrix mat,
            ImagingSolution.Imaging.ImageData image, PointF pointPictureBox)
        {
            if (image == null) return;

            // ピクチャボックス→画像上の座標のアフィン変換行列
            var matInvert = mat.Clone();
            matInvert.Invert();

            // 画像上の座標
            var pointImage = new PointF[1];
            pointImage[0] = pointPictureBox;
            matInvert.TransformPoints(pointImage);

            int picX = (int)Math.Floor(pointImage[0].X + 0.5);
            int picY = (int)Math.Floor(pointImage[0].Y + 0.5);

            string bright = " = ";

            if (
                (picX >= 0) &&              // ポインタ座標が画像の範囲内の場合
                (picY >= 0) &&
                (picX < image.Width) &&
                (picY < image.Height) &&
                (image.ImageBit >= 24)    // カラー画像の場合
                )
            {
                bright += "(" +
                    image[picY, picX, 2].ToString() + ", " +    // R
                    image[picY, picX, 1].ToString() + ", " +    // G
                    image[picY, picX, 0].ToString() + ")";      // B
            }
            else
            {
                bright += image[picY, picX].ToString();
            }

            // 輝度値の表示（モノクロを除く）
            /**
            lblPixelInfo.Text =
                "(" +
                picX.ToString() + ", " +
                picY.ToString() + ")" +
                bright;
            **/
        }

        /// <summary>
        /// 指定したファイルが画像ファイルかどうか？調べる
        /// </summary>
        /// <param name="filename">調べるファイル名</param>
        /// <returns></returns>
        public bool IsImageFile(string filename)
        {
            if (System.IO.File.Exists(filename) == false) return false;

            // ファイル形式の確認
            string ext = System.IO.Path.GetExtension(filename).ToLower();
            if (
                (ext != ".bmp") &&
                (ext != ".jpg") &&
                (ext != ".png") &&
                (ext != ".tif") &&
                (ext != ".ico")
                ) return false;

            return true;
        }

        /// <summary>
        /// ラベルを一時的に処理から省く
        /// </summary>
        private void LabelVisualizer()
        {
            if (label1.Visible == true)
            {
                label1.Visible = false;
                label2.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
            }
            else if (label1.Visible == false)
            {
                label1.Visible = true;
                label2.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
            }
        }

        /// <summary>
        /// 一つ後の画像ファイルを開く
        /// </summary>
        /// <param name="filename">基準となるファイル名</param>
        private void OpenNextFile(string filename)
        {
            if (filename == "") return;

            // 一覧からのIndex番号を取得
            int index = Array.IndexOf(_fileList, filename);

            if (index == _fileList.Length - 1)
            {
                MessageBox.Show("最後のファイルです。");
            }
            else
            {

                for (int i = index + 1; i < _fileList.Length; i++)
                {
                    if (IsImageFile(_fileList[i]))
                    {
                        OpenImageFile(_fileList[i]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 一つ前の画像ファイルを開く
        /// </summary>
        /// <param name="filename">基準となるファイル名</param>
        private void OpenPreviousFile(string filename)
        {
            if (filename == "") return;

            // 一覧からのIndex番号を取得
            int index = Array.IndexOf(_fileList, filename);

            if (index == 0)
            {
                MessageBox.Show("最初のファイルです。");
            }
            else
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (IsImageFile(_fileList[i]))
                    {
                        OpenImageFile(_fileList[i]);
                        break;
                    }
                }
            }
        }

        private void OpenNextFile_5(string filename)
        {
            if (_openedFileName == null) return;

            // 一覧からのIndex番号を取得
            int index = Array.IndexOf(_fileList, _openedFileName);

            if (index > _fileList.Length - 6)
            {
                /**/
            }
            else
            {

                OpenImageFile(_fileList[index + 5]);
            }
        }

        private void OpenPreviousFile_5(string filename)
        {
            if (_openedFileName == null) return;

            // 一覧からのIndex番号を取得
            int index = Array.IndexOf(_fileList, _openedFileName);

            if (index <= 4)
            {
                /**/
            }
            else
            {

                OpenImageFile(_fileList[index - 5]);
            }

        }

        /// <summary>
        /// フォルダ先頭のファイルを開く
        /// </summary>
        private void OpenFirstFile(string filename)
        {
            if (_openedFileName != null)
            {
                if (filename == "") return;

                // 一覧からのIndex番号を取得
                int index = Array.IndexOf(_fileList, filename);

                OpenImageFile(_fileList[0]);
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// フルスクリーン切り替え
        /// </summary>
        private void FullScreen()
        {
            //全画面表示
            if (this.FormBorderStyle != FormBorderStyle.None)
            {
                menuStrip1.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                menuStrip1.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
            }

            if (_openedFileName != null)
            {

                if (_fitSizeMode == FitMode.Horizontal)
                {
                    FitSizeHorizontal();
                }
                else if (_fitSizeMode == FitMode.Vertical)
                {
                    FitSizeVertical();
                }
                else if (_fitSizeMode == FitMode.Auto)
                {
                    FitSizeAuto();
                }
                else if (_fitSizeMode == FitMode.AutoReverse)
                {
                    FitSizeAutoReverse();
                }
                else
                {
                    /**/
                }
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// 指定した仕分けフォルダにコピーする
        /// </summary>
        private void Classification(string targetPath, string filePath)
        {
            if (targetPath == "なし")
            {
                MessageBox.Show("フォルダが指定されていません。");
            }
            else
            {
                if (targetPath != System.IO.Path.GetDirectoryName(_openedFileName))
                {
                    string fileName = Path.GetFileName(_openedFileName);
                    string destFile = System.IO.Path.Combine(targetPath, fileName);
                    string dirName = Path.GetFileName(targetPath);

                    if (dirName != "")
                    {
                        File.Copy(_openedFileName, destFile, true);
                        if (_cutOrCopy == CutOrCopy.CutMode)
                        {
                            if (_fileList.Length == 0)
                            {
                                MessageBox.Show("このフォルダの最後の画像です。今消すことはできません。");
                            }
                            else
                            {
                                FileDelete();
                            }
                        }
                        else
                        {
                            /**/
                        }

                        if (Properties.Settings.Default.CopyInfoDisplay == true)
                        {
                            label3.Visible = true;
                            label3.Text = fileName + "を" + dirName + " にコピー。";
                        }
                        else
                        {
                            /**/
                        }
                    }
                    else
                    {
                        MessageBox.Show("フォルダが指定されていません。");
                    }
                }
                else
                {
                    //MessageBox.Show("この画像と同じフォルダを選択しています。");
                }
            }


        }

        /// <summary>
        /// フィットモードを横に広げる
        /// </summary>
        private void FitSizeHorizontal()
        {
            if (_img != null)
            {
                _fitSizeMode = FitMode.Horizontal;
                ZoomFit(ref _matAffine, _img, picImage);
                _imageOrientation = ImageOrientation.Vertical;
                DrawImage();
                ZoomFit(ref _matAffine, _img, picImage);
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// フィットモードを縦に広げる
        /// </summary>
        private void FitSizeVertical()
        {
            if (_openedFileName != null)
            {
                _fitSizeMode = FitMode.Vertical;
                ZoomFit(ref _matAffine, _img, picImage);
                _imageOrientation = ImageOrientation.Horizontal;
                DrawImage();
                ZoomFit(ref _matAffine, _img, picImage);
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// 画面フィットモードを自動調整にする
        /// </summary>
        private void FitSizeAuto()
        {
            if (_openedFileName != null)
            {
                _fitSizeMode = FitMode.Auto;
                ZoomFit(ref _matAffine, _img, picImage);
                DrawImage();
                ZoomFit(ref _matAffine, _img, picImage);
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// 画面フィットモードを自動調整：拡大にする
        /// </summary>
        private void FitSizeAutoReverse()
        {
            if (_openedFileName != null)
            {
                _fitSizeMode = FitMode.AutoReverse;
                ZoomFit(ref _matAffine, _img, picImage);

                DrawImage();
                ZoomFit(ref _matAffine, _img, picImage);
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// 仕分けフォルダの一覧を開く際に名前を更新する
        /// </summary>
        private void StrageLabel()
        {
            if (Properties.Settings.Default.ClassificDisplay)
            {
                //listBox1の情報を更新
                listBox1.Items.Clear();

                listBox1.Items.Add("仕分けフォルダ設定");
                listBox1.Items.Add("1: " + Path.GetFileName(Properties.Settings.Default.StoreDir1));
                listBox1.Items.Add("Q: " + Path.GetFileName(Properties.Settings.Default.StoreDir2));
                listBox1.Items.Add("A: " + Path.GetFileName(Properties.Settings.Default.StoreDir3));
                listBox1.Items.Add("Z: " + Path.GetFileName(Properties.Settings.Default.StoreDir4));
                listBox1.Items.Add("2: " + Path.GetFileName(Properties.Settings.Default.StoreDir5));
                listBox1.Items.Add("W: " + Path.GetFileName(Properties.Settings.Default.StoreDir6));
                listBox1.Items.Add("S: " + Path.GetFileName(Properties.Settings.Default.StoreDir7));
                listBox1.Items.Add("X: " + Path.GetFileName(Properties.Settings.Default.StoreDir8));
                listBox1.Items.Add("3: " + Path.GetFileName(Properties.Settings.Default.StoreDir9));
                listBox1.Items.Add("E: " + Path.GetFileName(Properties.Settings.Default.StoreDir10));
                listBox1.Items.Add("D: " + Path.GetFileName(Properties.Settings.Default.StoreDir11));
                listBox1.Items.Add("C: " + Path.GetFileName(Properties.Settings.Default.StoreDir12));
                listBox1.Items.Add("4: " + Path.GetFileName(Properties.Settings.Default.StoreDir13));
                listBox1.Items.Add("R: " + Path.GetFileName(Properties.Settings.Default.StoreDir14));
                listBox1.Items.Add("F: " + Path.GetFileName(Properties.Settings.Default.StoreDir15));
                listBox1.Items.Add("V: " + Path.GetFileName(Properties.Settings.Default.StoreDir16));

                listBox1.Refresh();
                listBox1.Visible = true;
            }
            else
            {
                /**/
            }

            if (Properties.Settings.Default.FileInfoDisplay)
            {
                //label4の情報を更新
                if (_fileList != null)
                {
                    int index = Array.IndexOf(_fileList, _openedFileName);
                    index++;
                    label5.Text = ("        " + index + "/" + _fileList.Length + " :" + System.IO.Path.GetFileName(_openedFileName));
                }
                else
                {
                    /**/
                }
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// 現在のファイルに対して仕分けをする
        /// </summary>
        private void DoClassification()
        {
            FolderSelectDialog fsd = new FolderSelectDialog();

            if (fsd.ShowDialog() == DialogResult.OK)
            {
                if (System.IO.Path.GetDirectoryName(_openedFileName) != fsd.Path)
                {
                    Classification(fsd.Path, _openedFileName);
                }
                else
                {
                    MessageBox.Show("この画像と同じフォルダを選択しています。");
                }
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// ブックマークの中身を作成する
        /// </summary>
        private void BookMarker(int num)
        {
            if (_openedFileName != null)
            {
                string filename = System.IO.Path.GetFileName(_openedFileName);

                if (num == 0)
                {
                    Properties.Settings.Default.BookMark0 = _openedFileName;

                }
                else if (num == 1)
                {
                    Properties.Settings.Default.BookMark1 = _openedFileName;

                }
                else if (num == 2)
                {
                    Properties.Settings.Default.BookMark2 = _openedFileName;

                }
                else if (num == 3)
                {
                    Properties.Settings.Default.BookMark3 = _openedFileName;

                }
                else if (num == 4)
                {
                    Properties.Settings.Default.BookMark4 = _openedFileName;

                }
                else if (num == 5)
                {
                    Properties.Settings.Default.BookMark5 = _openedFileName;

                }
                else if (num == 6)
                {
                    Properties.Settings.Default.BookMark6 = _openedFileName;

                }
                else if (num == 7)
                {
                    Properties.Settings.Default.BookMark7 = _openedFileName;

                }
                else if (num == 8)
                {
                    Properties.Settings.Default.BookMark8 = _openedFileName;

                }
                else if (num == 9)
                {
                    Properties.Settings.Default.BookMark9 = _openedFileName;

                }
                else
                {
                    /**/
                }

                Form f = new Form();
                f.TopMost = true;

                DialogResult result = MessageBox.Show(f, "この画像をブックマークしますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);   // アイコンの設定

                if (result == DialogResult.Yes)
                {
                    label3.Text = "ブックマーク" + (num + 1) + " にマークしました。";
                    BookMarkNaming();

                    Properties.Settings.Default.Save();
                }
                else if (result == DialogResult.No)
                {
                    /**/
                }


            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// ブックマーク一覧を開く際に名前を更新する
        /// </summary>
        private void BookMarkNaming()
        {
            string fileName;

            if (File.Exists(Properties.Settings.Default.BookMark0))
            {
                fileName = "1: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark0);
                toolStripMenuItem32.Text = fileName;
                toolStripMenuItem42.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark1))
            {
                fileName = "2: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark1);
                toolStripMenuItem33.Text = fileName;
                toolStripMenuItem43.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark2))
            {
                fileName = "3: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark2);
                toolStripMenuItem34.Text = fileName;
                toolStripMenuItem44.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark3))
            {
                fileName = "4: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark3);
                toolStripMenuItem35.Text = fileName;
                toolStripMenuItem45.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark4))
            {
                fileName = "5: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark4);
                toolStripMenuItem36.Text = fileName;
                toolStripMenuItem46.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark5))
            {
                fileName = "6: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark5);
                toolStripMenuItem37.Text = fileName;
                toolStripMenuItem47.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark6))
            {
                fileName = "7: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark6);
                toolStripMenuItem38.Text = fileName;
                toolStripMenuItem48.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark7))
            {
                fileName = "8: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark7);
                toolStripMenuItem39.Text = fileName;
                toolStripMenuItem49.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark8))
            {
                fileName = "9: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark8);
                toolStripMenuItem40.Text = fileName;
                toolStripMenuItem50.Text = fileName;
            }
            else
            {
                /**/
            }
            if (File.Exists(Properties.Settings.Default.BookMark9))
            {
                fileName = "10: " + System.IO.Path.GetFileName(Properties.Settings.Default.BookMark9);
                toolStripMenuItem41.Text = fileName;
                toolStripMenuItem51.Text = fileName;
            }
            else
            {
                /**/
            }
        }

        /// <summary>
        /// ブックマークを開く
        /// </summary>
        private void BookMarkOpen(int num)
        {
            string filePath = null;

            if (num == 0)
            {
                filePath = Properties.Settings.Default.BookMark0;
            }
            else if (num == 1)
            {
                filePath = Properties.Settings.Default.BookMark1;
            }
            else if (num == 2)
            {
                filePath = Properties.Settings.Default.BookMark2;
            }
            else if (num == 3)
            {
                filePath = Properties.Settings.Default.BookMark3;
            }
            else if (num == 4)
            {
                filePath = Properties.Settings.Default.BookMark4;
            }
            else if (num == 5)
            {
                filePath = Properties.Settings.Default.BookMark5;
            }
            else if (num == 6)
            {
                filePath = Properties.Settings.Default.BookMark6;
            }
            else if (num == 7)
            {
                filePath = Properties.Settings.Default.BookMark7;
            }
            else if (num == 8)
            {
                filePath = Properties.Settings.Default.BookMark8;
            }
            else if (num == 9)
            {
                filePath = Properties.Settings.Default.BookMark9;
            }
            else
            {
                return;
            }

            if (filePath != null)
            {
                Form f = new Form();
                f.TopMost = true;
                
                //string filename = System.IO.Path.GetFileName(_openedFileName);
                DialogResult result = MessageBox.Show(f,"このブックマークを開きますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);   // アイコンの設定

                if (result == DialogResult.Yes)
                {
                    _openedFileName = filePath;
                    OpenImageFIleFirst(_openedFileName);
                }
                else if (result == DialogResult.No)
                {
                    /**/
                }

            }

            /**/
            
        }

        private void ArrowImageSwitch(int num, bool key)
        {
            if (Properties.Settings.Default.ArrowDisplay == false)
            {
                /**/
            }
            else
            {
                if (num == 1)
                {
                    if (key == true)
                    {
                        label1.Image = RenameAllocateViewer.Properties.Resources.arrow_right;
                    }
                    else
                    {
                        label1.Image = null;
                    }
                }
                else if (num == 2)
                {
                    if (key == true)
                    {
                        label2.Image = RenameAllocateViewer.Properties.Resources.arrow;
                    }
                    else
                    {
                        label2.Image = null;
                    }
                }
            }
        }

        private void HelpDisplay(HelpNum helpNum)
        {
            if (helpNum == HelpNum.Classification)
            {
                label8.Text = "ヘルプ：「画像仕分け」とは";
                label9.Text = "　ファイルをワンタッチで指定したフォルダにコピーする事ができます。\n(コピーではなくカットして元のフォルダには残さないモードもあります)" +
                              "\n\n1.「画像仕分け」から「仕分けフォルダ設定」をクリックしてみましょう。" +
                              "\n2.「保存フォルダ」の下に並ぶ空欄をクリックすると、ファイルをコピーする(または送る)フォルダを選択できます。" +
                              "\n3.その左に記載されたキーボードキーを押すと、開いている画像ファイルがそのフォルダにコピーされます。" +
                              "\n複数のフォルダを設定することで、画像ファイルを閲覧しながら様々なフォルダに振り分ける事が効率的に行えます。" +
                              "\nまた、「カット＆ペーストに切り替える」をクリックする事でコピーの代わりにそのように振舞います。";
                groupBox2.Visible = true;
            }
            else if (helpNum == HelpNum.Rename)
            {
                label8.Text = "ヘルプ：「リネーム」とは";
                label9.Text = "　フォルダ内の画像をXXX0000.拡張子～XXX9999.拡張子というように連番名で改名します。" +
                              "\n\n1.「リネーム」から「リネーム画面を開く」をクリックしてみましょう。" +
                              "\n2.「リネームするフォルダを選ぶ」をクリックし、リネームしたいファイルの入ったフォルダを選択してみましょう。" +
                              "\n(この選択はドラッグアンドドロップでも可能です)" +
                              "\n3.その下の空欄に「名前」を入力するとその際の例がその下に記されます。" +
                              "「年月日時分秒を元に名前を自動生成する」をクリックすると日時に即した名前を自動生成します。" +
                              "\n4.リネームするファイルの並びを選択してください。" +
                              "\n5.「この並び順でリネームする」をクリックするとファイルがリネームされ、結果が表示されます。";
                groupBox2.Visible = true;
            }
            else if (helpNum == HelpNum.Bookmark)
            {
                label8.Text = "ヘルプ：「ブックマーク」とは";
                label9.Text = "　現在オープンしている画像を記憶し、後に簡単にそのファイルを開きなおせます。" +
                              "\n\n1.「ブックマーク」から「ブックマークを開く」をクリックしてみましょう。" +
                              "\n2.現れたリストにマウスを合わせると右に「オープン中の画像をブックマーク」というテキストが現れます。" +
                              "\nこれをクリックするとオープン中の画像がそのリストにブックマークされます。" +
                              "\n3.開く際はファイル名をクリックしてください。";
                groupBox2.Visible = true;
            }
            else if (helpNum == HelpNum.Basic)
            {
                label8.Text = "ヘルプ：基本操作";
                label9.Text = "・上部メニューバーから全ての操作が行えます。" +
                              "\n・右クリックで現れるメニューではそれと同じことが全て行えます。" +
                              "\n・一部操作は画像画面のクリックなどでも可能で、それは「ヘルプ」内の「画面上クリック機能：表示」をクリックすると確認できます。";
                groupBox2.Visible = true;
            }
            else if (helpNum == HelpNum.Listbox)
            {
                label8.Text = "ヘルプ：画面右端のフォルダ名リストについて";
                label9.Text = "・仕分けフォルダに設定されたフォルダのリストと対応するキーが記載されています。" +
                              "\n・フォルダ名をクリックすると画像を仕分けます。" +
                              "\n・画面上部のエリアにマウスが入ると出現し、出ると消えます。消えない場合は出入りしてみてください。" +
                              "\n・「UI表示設定」の欄から「仕分けフォルダを表示しない」をクリックするとそのように振舞います。";
                groupBox2.Visible = true;
            }
            else if (helpNum == HelpNum.Zoom)
            {
                label8.Text = "ヘルプ：ズームをするには";
                label9.Text = "　右クリックして「スクロール/ズームの切り替え」を選択(又はCtrl+Zを押)します。" +
                              "\nこれによりマウスホイールを上下することでマウスポインタのある地点を基準に拡大・縮小します。" +
                              "\nもう一度「スクロール/ズームの切り替え」をクリックする事でマウスホイールでスクロールするようになります。";
                groupBox2.Visible = true;
            }
        }

    }
}
