using System;
using System.Drawing;
using System.Windows.Forms;

namespace MpoViewer
{
    public class Functions
    {
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
