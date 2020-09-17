using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RenameAllocateViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.label4.Text = Properties.Settings.Default.StoreDir1;
            this.label5.Text = Properties.Settings.Default.StoreDir2;
            this.label7.Text = Properties.Settings.Default.StoreDir3;
            this.label6.Text = Properties.Settings.Default.StoreDir4;
            this.label8.Text = Properties.Settings.Default.StoreDir5;
            this.label9.Text = Properties.Settings.Default.StoreDir6;
            this.label10.Text = Properties.Settings.Default.StoreDir7;
            this.label11.Text = Properties.Settings.Default.StoreDir8;
            this.label12.Text = Properties.Settings.Default.StoreDir9;
            this.label13.Text = Properties.Settings.Default.StoreDir10;
            this.label23.Text = Properties.Settings.Default.StoreDir11;
            this.label24.Text = Properties.Settings.Default.StoreDir12;
            this.label25.Text = Properties.Settings.Default.StoreDir13;
            this.label26.Text = Properties.Settings.Default.StoreDir14;
            this.label27.Text = Properties.Settings.Default.StoreDir15;
            this.label28.Text = Properties.Settings.Default.StoreDir16;
        }

        /// <summary>
        /// 選択したラベルにフォルダを設定
        /// </summary>
        private void label_Click(object sender, EventArgs e)
        {
            FolderSelectDialog fsd = new FolderSelectDialog();

            if ((sender as Label).Name == "label4")
            {
                fsd.Path = label4.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label4.Text = fsd.Path;
                Properties.Settings.Default.StoreDir1 = this.label4.Text;
            }else if((sender as Label).Name == "label5")
            {
                fsd.Path = label5.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label5.Text = fsd.Path;
                Properties.Settings.Default.StoreDir2 = this.label5.Text;
            }
            else if ((sender as Label).Name == "label7")
            {
                fsd.Path = label7.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label7.Text = fsd.Path;
                Properties.Settings.Default.StoreDir3 = this.label7.Text;
            }
            else if ((sender as Label).Name == "label6")
            {
                fsd.Path = label6.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label6.Text = fsd.Path;
                Properties.Settings.Default.StoreDir4 = this.label6.Text;
            }
            else if ((sender as Label).Name == "label8")
            {
                fsd.Path = label8.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label8.Text = fsd.Path;
                Properties.Settings.Default.StoreDir5 = this.label8.Text;
            }
            else if ((sender as Label).Name == "label9")
            {
                fsd.Path = label9.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label9.Text = fsd.Path;
                Properties.Settings.Default.StoreDir6 = this.label9.Text;
            }
            else if ((sender as Label).Name == "label10")
            {
                fsd.Path = label10.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label10.Text = fsd.Path;
                Properties.Settings.Default.StoreDir7 = this.label10.Text;
            }
            else if ((sender as Label).Name == "label11")
            {
                fsd.Path = label11.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label11.Text = fsd.Path;
                Properties.Settings.Default.StoreDir8 = this.label11.Text;
            }
            else if ((sender as Label).Name == "label12")
            {
                fsd.Path = label12.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label12.Text = fsd.Path;
                Properties.Settings.Default.StoreDir9 = this.label12.Text;
            }
            else if ((sender as Label).Name == "label13")
            {
                fsd.Path = label13.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label13.Text = fsd.Path;
                Properties.Settings.Default.StoreDir10 = this.label13.Text;
            }
            else if ((sender as Label).Name == "label23")
            {
                fsd.Path = label23.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label23.Text = fsd.Path;
                Properties.Settings.Default.StoreDir11 = this.label23.Text;
            }
            else if ((sender as Label).Name == "label24")
            {
                fsd.Path = label24.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label24.Text = fsd.Path;
                Properties.Settings.Default.StoreDir12 = this.label24.Text;
            }
            else if ((sender as Label).Name == "label25")
            {
                fsd.Path = label25.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label25.Text = fsd.Path;
                Properties.Settings.Default.StoreDir13 = this.label25.Text;
            }
            else if ((sender as Label).Name == "label26")
            {
                fsd.Path = label26.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label26.Text = fsd.Path;
                Properties.Settings.Default.StoreDir14 = this.label26.Text;
            }
            else if ((sender as Label).Name == "label27")
            {
                fsd.Path = label27.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label27.Text = fsd.Path;
                Properties.Settings.Default.StoreDir15 = this.label27.Text;
            }
            else if ((sender as Label).Name == "label28")
            {
                fsd.Path = label28.Text;
                if (fsd.ShowDialog() == DialogResult.OK) label28.Text = fsd.Path;
                Properties.Settings.Default.StoreDir16 = this.label28.Text;
            }

            Properties.Settings.Default.Save();
        }
    }
}
