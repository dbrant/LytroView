using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using LytroView.Properties;

namespace MpoViewer
{
    public partial class Form1 : Form
    {
        private List<Image> images = new List<Image>();
        private string FileName = "";

        public Form1(string[] args)
        {
            InitializeComponent();
            Functions.FixDialogFont(this);
            this.Text = Application.ProductName;
            
            if (args.Length > 0)
            {
                OpenLFP(args[0]);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog();
            openDlg.DefaultExt = ".lfp";
            openDlg.CheckFileExists = true;
            openDlg.Title = Resources.openDlgTitle;
            openDlg.Filter = Resources.openDlgFilter;
            openDlg.FilterIndex = 1;
            if (openDlg.ShowDialog() == DialogResult.Cancel) return;
            OpenLFP(openDlg.FileName);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.All;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;
            OpenLFP(files[0]);
        }
        
        private void OpenLFP(string fileName)
        {
            FileName = fileName;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                images = LytroImage.GetLfpImages(FileName);
                
                if (images.Count == 0)
                {
                    pictureBox.Image = null;
                    MessageBox.Show(this, Resources.errorFileInvalid, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    this.Text = fileName;
                    tbImage.Maximum = images.Count - 1;
                    tbImage_Scroll(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            if (images.Count <= tbImage.Value) return;
            try
            {
                var saveDlg = new SaveFileDialog();
                saveDlg.DefaultExt = ".jpg";
                saveDlg.OverwritePrompt = true;
                saveDlg.Title = Resources.saveDlgTitle;
                saveDlg.Filter = Resources.saveDlgFilter;
                saveDlg.FilterIndex = 1;
                saveDlg.InitialDirectory = Path.GetDirectoryName(FileName);
                saveDlg.FileName = Path.GetFileNameWithoutExtension(FileName) + "_" + (tbImage.Value + 1).ToString() + ".jpg";
                if (saveDlg.ShowDialog() == DialogResult.Cancel) return;
                images[tbImage.Value].Save(saveDlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, Resources.aboutText, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tbImage_Scroll(object sender, EventArgs e)
        {
            if (images.Count > tbImage.Value)
            {
                pictureBox.Image = images[tbImage.Value];
            } else
            {
                pictureBox.Image = null;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
