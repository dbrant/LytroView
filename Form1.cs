using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace MpoViewer
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();
            Functions.FixDialogFont(this);
            this.Text = Application.ProductName;

            images = new List<Image>();
            
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
            openDlg.Title = "Open LFP file...";
            openDlg.Filter = "LFP Files (*.lfp)|*.lfp|All Files (*.*)|*.*";
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


        private List<Image> images;
        private Image stereoImage = null;
        private string FileName = "";


        private void OpenLFP(string fileName)
        {
            FileName = fileName;
            images.Clear();

            try
            {
                this.Cursor = Cursors.WaitCursor;
                byte[] tempBytes = new byte[0x10];
                using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    while (f.Position < f.Length)
                    {
                        f.Read(tempBytes, 0, tempBytes.Length);
                        string blockName = Encoding.ASCII.GetString(tempBytes, 1, 3);
                        UInt32 blockLength = Functions.BigEndian(BitConverter.ToUInt32(tempBytes, 12));

                        if (!blockName.StartsWith("LF"))
                        {
                            continue;
                        }
                        if (blockLength == 0)
                        {
                            continue;
                        }

                        f.Seek(0x50, SeekOrigin.Current);

                        if (blockLength > 0x100)
                        {
                            f.Read(tempBytes, 0, 4);
                            f.Seek(-4, SeekOrigin.Current);

                            if (tempBytes[0] == 0xff && tempBytes[1] == 0xD8 && tempBytes[2] == 0xFF)
                            {
                                byte[] imageBytes = new byte[blockLength];
                                f.Read(imageBytes, 0, (int) blockLength);
                                f.Seek(-blockLength, SeekOrigin.Current);
                                
                                MemoryStream stream = new MemoryStream(imageBytes, 0, (int) blockLength);
                                images.Add(new Bitmap(stream));
                            }
                        }

                        f.Seek(blockLength, SeekOrigin.Current);

                        if (blockLength % 0x10 > 0)
                        {
                            f.Seek(0x10 - (blockLength % 0x10), SeekOrigin.Current);
                        }
                    }
                }
                
                if (images.Count == 0)
                {
                    pictureBox.Image = null;
                    MessageBox.Show(this, "This does not appear to be a valid LFP stacked image.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
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
            if (images.Count == 0) return;
            try
            {
                var saveDlg = new SaveFileDialog();
                saveDlg.DefaultExt = ".jpg";
                saveDlg.OverwritePrompt = true;
                saveDlg.Title = "Save file...";
                saveDlg.Filter = "JPG Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
                saveDlg.FilterIndex = 1;
                saveDlg.InitialDirectory = Path.GetDirectoryName(FileName);
                saveDlg.FileName = Path.GetFileNameWithoutExtension(FileName) + "_" + (tbImage.Value + 1).ToString() + ".jpg";
                if (saveDlg.ShowDialog() == DialogResult.Cancel) return;

                //images[cbImage.SelectedIndex].Save(saveDlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, Application.ProductName + "\nCopyright © 2015 by Dmitry Brant\n\nhttp://dmitrybrant.com", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    public class Functions
    {

        /// <summary>
        /// Sets the font of a given control, and all child controls, to
        /// the current system font, while preserving font styles.
        /// </summary>
        /// <param name="c0">Control whose font will be set.</param>
        public static void FixDialogFont(Control c0)
        {
            Font old = c0.Font;
            c0.Font = new Font(SystemFonts.MessageBoxFont.FontFamily.Name, old.Size, old.Style);
            if (c0.Controls.Count > 0)
                foreach (Control c in c0.Controls)
                    FixDialogFont(c);
        }

        private static UInt32 conv_endian(UInt32 val)
        {
            UInt32 temp = (val & 0x000000FF) << 24;
            temp |= (val & 0x0000FF00) << 8;
            temp |= (val & 0x00FF0000) >> 8;
            temp |= (val & 0xFF000000) >> 24;
            return (temp);
        }

        public static UInt32 BigEndian(UInt32 val)
        {
            if (!BitConverter.IsLittleEndian) return val;
            return conv_endian(val);
        }
    }

}
