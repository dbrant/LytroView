using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MpoViewer
{
    public static class LytroImage
    {
        public static List<Image> GetLfpImages(string fileName)
        {
            var images = new List<Image>();
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
                            f.Read(imageBytes, 0, (int)blockLength);
                            f.Seek(-blockLength, SeekOrigin.Current);

                            MemoryStream stream = new MemoryStream(imageBytes, 0, (int)blockLength);
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
            return images;
        }
    }
}
