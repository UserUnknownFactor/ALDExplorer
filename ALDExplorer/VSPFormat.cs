using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using FreeImageAPI;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using ZLibNet;
using System.Diagnostics;
using System.Globalization;

namespace ALDExplorer.Formats
{
    public class VspHeader : ICloneable
    {
        public int xLocation;
        public int yLocation;
        public int width;
        public int height;
        public int paletteBank;
        public int is8Bit;
        public int unknownA, unknownC, unknown10, unknown14, unknown18, unknown1C;


        public VspHeader()
        {

        }

        public string GetComment()
        {
            return ImageConverter.GetComment(this);
        }

        public bool ParseComment(string comment)
        {
            return ImageConverter.ParseComment(this, comment);
        }

        public PmsHeader ToPmsHeader()
        {
            var pmsHeader = new PmsHeader();
            pmsHeader.addressOfData = 0x320;
            pmsHeader.addressOfPalette = 0x20;
            pmsHeader.colorDepth = 8;
            pmsHeader.headerSize = 0x20;
            pmsHeader.paletteBank = this.paletteBank;
            pmsHeader.height = this.height;
            pmsHeader.width = ((this.width + 7) / 8) * 8;
            pmsHeader.xLocation = this.xLocation;
            pmsHeader.yLocation = this.yLocation;
            return pmsHeader;
        }

        public VspHeader Clone()
        {
            return (VspHeader)this.MemberwiseClone();
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }

    static class Vsp
    {
        public static FreeImageBitmap LoadImage(byte[] bytes)
        {
            var vspHeader = GetHeader(bytes);
            if (vspHeader.width > 0 && ((vspHeader.is8Bit == 0) ? vspHeader.width : ((vspHeader.width + 7) / 8)) <= 80 && vspHeader.height > 0 && vspHeader.height <= 480)
            {
            }
            else
            {
                return null;
            }
            if (vspHeader.is8Bit == 1)
            {
                var pmsHeader = vspHeader.ToPmsHeader();
                var imageData = Pms.GetImageData8Bit(pmsHeader, bytes);
                FreeImageBitmap bitmap = new FreeImageBitmap(pmsHeader.width, pmsHeader.height, pmsHeader.width, 8, FREE_IMAGE_TYPE.FIT_BITMAP, imageData);
                Pms.GetPalette(bitmap.Palette, pmsHeader, bytes);
                bitmap.Comment = vspHeader.GetComment();
                bitmap.Tag = vspHeader;
                return bitmap;
            }
            else
            {
                var imageData = GetImage(vspHeader, bytes);
                FreeImageBitmap bitmap = new FreeImageBitmap(vspHeader.width * 8, vspHeader.height, vspHeader.width * 8, 8, FREE_IMAGE_TYPE.FIT_BITMAP, imageData);
                GetPalette(bitmap.Palette, bytes, vspHeader);
                bitmap.Comment = vspHeader.GetComment();
                bitmap.Tag = vspHeader;
                return bitmap;
            }
        }

        public static void SaveImage(Stream stream, FreeImageBitmap bitmap)
        {
            string comment = bitmap.Comment;
            var vspHeader = new VspHeader();
            if (!String.IsNullOrEmpty(comment))
            {
                vspHeader.ParseComment(comment);
            }
            if (bitmap.ColorDepth != 4 && bitmap.ColorDepth != 8)
            {
                if (bitmap.ColorDepth > 8)
                {
                    if (bitmap.ColorDepth == 32)
                    {
                        bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_24_BPP);
                    }
                    if (vspHeader.is8Bit == 0)
                    {
                        bitmap.Quantize(FREE_IMAGE_QUANTIZE.FIQ_WUQUANT, 16);
                    }
                    else
                    {
                        bitmap.Quantize(FREE_IMAGE_QUANTIZE.FIQ_WUQUANT, 256);
                    }
                }
                //throw new ArgumentException("image must be 4-bit or 8-bit");
            }
            if ((bitmap.Width & 7) != 0)
            {
                int slackPixels = (bitmap.Width & 7);
                int pixelsToAdd = 8 - slackPixels;
                bitmap.EnlargeCanvas<RGBQUAD>(0, 0, pixelsToAdd, 0, new RGBQUAD(Color.Black));
            }

            if (vspHeader.is8Bit == 0)
            {
                vspHeader.height = bitmap.Height;
                vspHeader.width = bitmap.Width / 8;
                SaveHeader(vspHeader, stream);
                SavePalette(bitmap.Palette, stream);
                SaveImageData(stream, bitmap);
            }
            else
            {
                vspHeader.height = bitmap.Height;
                if (vspHeader.width != bitmap.Width - 1)
                {

                }
                vspHeader.width = bitmap.Width - 1;
                SaveHeader(vspHeader, stream);
                var pmsHeader = vspHeader.ToPmsHeader();
                Pms.SavePalette(stream, bitmap.Palette);
                Pms.SaveImageData8Bit(stream, bitmap);
            }
        }

        public static void SaveHeader(VspHeader vspHeader, Stream stream)
        {
            var bw = new BinaryWriter(stream);
            bw.Write((short)vspHeader.xLocation);
            bw.Write((short)vspHeader.yLocation);
            bw.Write((short)(vspHeader.width + vspHeader.xLocation));
            bw.Write((short)(vspHeader.height + vspHeader.yLocation));
            bw.Write((byte)vspHeader.is8Bit);
            bw.Write((byte)vspHeader.paletteBank);
            if (vspHeader.is8Bit == 1)
            {
                bw.Write((ushort)vspHeader.unknownA);
                bw.Write(vspHeader.unknownC);

                bw.Write(vspHeader.unknown10);
                bw.Write(vspHeader.unknown14);
                bw.Write(vspHeader.unknown18);
                bw.Write(vspHeader.unknown1C);
            }
        }

        public static void SavePalette(Palette palette, Stream stream)
        {
            var bw = new BinaryWriter(stream);
            for (int i = 0; i < 16; i++)
            {
                var color = palette[i];
                int r = (color.rgbRed + 7) / 17;
                int g = (color.rgbGreen + 7) / 17;
                int b = (color.rgbBlue + 7) / 17;
                if (r > 15) r = 15;
                if (g > 15) g = 15;
                if (b > 15) b = 15;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                bw.Write((byte)b);
                bw.Write((byte)r);
                bw.Write((byte)g);
            }
        }

        public static VspHeader GetHeader(byte[] bytes)
        {
            VspHeader vspHeader = new VspHeader();

            vspHeader.xLocation = BitConverter.ToInt16(bytes, 0);
            vspHeader.yLocation = BitConverter.ToInt16(bytes, 2);
            vspHeader.width = BitConverter.ToInt16(bytes, 4) - vspHeader.xLocation;
            vspHeader.height = BitConverter.ToInt16(bytes, 6) - vspHeader.yLocation;
            vspHeader.is8Bit = bytes[8];
            vspHeader.paletteBank = bytes[9];

            if (vspHeader.is8Bit != 0)
            {
                //vspHeader.width += 7;
                //vspHeader.width /= 8;

                vspHeader.unknownA = BitConverter.ToUInt16(bytes, 0x0A);
                vspHeader.unknownC = BitConverter.ToInt32(bytes, 0x0c);
                vspHeader.unknown10 = BitConverter.ToInt32(bytes, 0x10);
                vspHeader.unknown14 = BitConverter.ToInt32(bytes, 0x14);
                vspHeader.unknown18 = BitConverter.ToInt32(bytes, 0x18);
                vspHeader.unknown1C = BitConverter.ToInt32(bytes, 0x1c);

            }

            return vspHeader;
        }

        /*
         * Get palette from raw data
         *   pal: palette to be stored
         *   b  : raw data (pointer to palette)
        */
        public static void GetPalette(Palette palette, byte[] bytes, VspHeader vspHeader)
        {
            int red, green, blue, i;

            int address = 0x0A;
            for (i = 0; i < 16; i++)
            {
                blue = bytes[i * 3 + 0 + address];
                red = bytes[i * 3 + 1 + address];
                green = bytes[i * 3 + 2 + address];

                red = (red & 0x0F) * 17;
                green = (green & 0x0F) * 17;
                blue = (blue & 0x0F) * 17;
                palette[i] = Color.FromArgb(red, green, blue);
            }
        }

        /*
         * Do extract vsp image
         *   vsp: vsp header information
         *   pic: pixel to be stored
         *   b  : raw data (pointer to pixel)
        */

        static byte[][] _bp = new byte[][] { new byte[480], new byte[480], new byte[480], new byte[480] };
        static byte[][] _bc = new byte[][] { new byte[480], new byte[480], new byte[480], new byte[480] };

        static byte[][] bp = new byte[][] { _bp[0], _bp[1], _bp[2], _bp[3] };
        static byte[][] bc = new byte[][] { _bc[0], _bc[1], _bc[2], _bc[3] };

        public static byte[] GetImage(VspHeader vspHeader, byte[] bytes)
        {
            int address = 0x3A;

            byte[] pic = new byte[vspHeader.height * vspHeader.width * 8];
            int c0;
            byte b0, b1, b2, b3, mask = 0;
            byte[] bt;
            int i, l, x, y, pl, loc;

            bp[0] = _bp[0]; bc[0] = _bc[0];
            bp[1] = _bp[1]; bc[1] = _bc[1];
            bp[2] = _bp[2]; bc[2] = _bc[2];
            bp[3] = _bp[3]; bc[3] = _bc[3];
            for (x = 0; x < vspHeader.width; x++)
            {
                for (pl = 0; pl < 4; pl++)
                {
                    y = 0;
                    while (y < vspHeader.height)
                    {
                        c0 = bytes[address++];
                        if (c0 >= 0x08)
                        {
                            //literal
                            bc[pl][y] = (byte)c0; y++;
                        }
                        else if (c0 == 0x00)
                        {
                            //take L+1 bytes from previous column (same plane)
                            l = bytes[address] + 1; address++;
                            memcpy(bc[pl], bp[pl], y, l);
                            y += l;
                        }
                        else if (c0 == 0x01)
                        {
                            //RLE run: L+1  bytes long
                            l = bytes[address] + 1; address++;
                            b0 = bytes[address++];
                            memset(bc[pl], y, b0, l);
                            y += l;
                        }
                        else if (c0 == 0x02)
                        {
                            //Alternating RLE run: (L+1)*2 bytes long
                            l = bytes[address] + 1; address++;
                            b0 = bytes[address++]; b1 = bytes[address++];
                            for (i = 0; i < l; i++)
                            {
                                bc[pl][y] = b0; y++;
                                bc[pl][y] = b1; y++;
                            }
                        }
                        else if (c0 == 0x03)
                        {
                            //Copy from plane 0 xor Mask, set mask to 0
                            l = bytes[address] + 1; address++;
                            for (i = 0; i < l; i++)
                            {
                                bc[pl][y] = (byte)(bc[0][y] ^ mask); y++;
                            }
                            mask = 0;
                        }
                        else if (c0 == 0x04)
                        {
                            //Copy from plane 1 xor Mask, set mask to 0
                            l = bytes[address] + 1; address++;
                            for (i = 0; i < l; i++)
                            {
                                bc[pl][y] = (byte)(bc[1][y] ^ mask); y++;
                            }
                            mask = 0;
                        }
                        else if (c0 == 0x05)
                        {
                            //Copy from plane 2 xor Mask, set mask to 0
                            l = bytes[address] + 1; address++;
                            for (i = 0; i < l; i++)
                            {
                                bc[pl][y] = (byte)(bc[2][y] ^ mask); y++;
                            }
                            mask = 0;
                        }
                        else if (c0 == 0x06)
                        {
                            //set mask to FF
                            mask = 0xff;
                        }
                        else if (c0 == 0x07)
                        {
                            //Escaped literal
                            bc[pl][y] = bytes[address++]; y++;
                        }
                    }
                }
                /* conversion from plane -> packed bytes */
                for (y = 0; y < vspHeader.height; y++)
                {
                    loc = (y * vspHeader.width + x) * 8;
                    b0 = bc[0][y]; b1 = bc[1][y];
                    b2 = bc[2][y]; b3 = bc[3][y];
                    pic[loc + 0] = (byte)(((b0 >> 7) & 0x01) | ((b1 >> 6) & 0x02) | ((b2 >> 5) & 0x04) | ((b3 >> 4) & 0x08));
                    pic[loc + 1] = (byte)(((b0 >> 6) & 0x01) | ((b1 >> 5) & 0x02) | ((b2 >> 4) & 0x04) | ((b3 >> 3) & 0x08));
                    pic[loc + 2] = (byte)(((b0 >> 5) & 0x01) | ((b1 >> 4) & 0x02) | ((b2 >> 3) & 0x04) | ((b3 >> 2) & 0x08));
                    pic[loc + 3] = (byte)(((b0 >> 4) & 0x01) | ((b1 >> 3) & 0x02) | ((b2 >> 2) & 0x04) | ((b3 >> 1) & 0x08));
                    pic[loc + 4] = (byte)(((b0 >> 3) & 0x01) | ((b1 >> 2) & 0x02) | ((b2 >> 1) & 0x04) | ((b3) & 0x08));
                    pic[loc + 5] = (byte)(((b0 >> 2) & 0x01) | ((b1 >> 1) & 0x02) | ((b2) & 0x04) | ((b3 << 1) & 0x08));
                    pic[loc + 6] = (byte)(((b0 >> 1) & 0x01) | ((b1) & 0x02) | ((b2 << 1) & 0x04) | ((b3 << 2) & 0x08));
                    pic[loc + 7] = (byte)(((b0) & 0x01) | ((b1 << 1) & 0x02) | ((b2 << 2) & 0x04) | ((b3 << 3) & 0x08));
                }
                /* bc -> bp swap */
                bt = bp[0]; bp[0] = bc[0]; bc[0] = bt;
                bt = bp[1]; bp[1] = bc[1]; bc[1] = bt;
                bt = bp[2]; bp[2] = bc[2]; bc[2] = bt;
                bt = bp[3]; bp[3] = bc[3]; bc[3] = bt;
            }
            return pic;
        }

        private static void memset(byte[] bytes, int y, byte b0, int l)
        {
            for (int i = 0; i < l; i++)
            {
                bytes[y + i] = b0;
            }
        }

        private static void memcpy(byte[] dest, byte[] src, int index, int length)
        {
            Array.Copy(src, index, dest, index, length);
        }

        public static void SaveImageData(Stream stream, FreeImageBitmap image)
        {
            byte[] imageData = new byte[image.Width * image.Height / 2];
            int o = 0;
            for (int y = 0; y < image.Height; y++)
            {
                if (image.ColorDepth == 4)
                {
                    var scanline = image.GetScanlineFromTop4Bit(y);
                    for (int x = 0; x < image.Width; x += 2)
                    {
                        int p1 = scanline[x];
                        int p2 = scanline[x + 1];
                        imageData[o++] = (byte)((p1 << 4) + p2);
                    }
                }
                else if (image.ColorDepth == 8)
                {
                    var scanline = image.GetScanlineFromTop8Bit(y);
                    for (int x = 0; x < image.Width; x += 2)
                    {
                        int p1 = scanline[x] & 0x0F;
                        int p2 = scanline[x + 1] & 0x0F;
                        imageData[o++] = (byte)((p1 << 4) + p2);
                    }
                }
            }
            byte[] bytes = new byte[imageData.Length];

            int[] lengths = new int[9];

            TransformImage(bytes, imageData, image.Width, image.Height);
            int height = image.Height;
            //encode the bytes
            for (int i = 0; i < bytes.Length; i++)
            {
                int x = i / image.Height;
                int p = x & 3;

                Array.Clear(lengths, 0, lengths.Length);
                lengths[0] = MeasureRleRun(bytes, i, height);
                lengths[1] = MeasureRleRun2(bytes, i, height);
                lengths[2] = MeasurePreviousColumnRun(bytes, i, height, height * 4, 0);
                if (p >= 1)
                {
                    lengths[3] = MeasurePreviousColumnRun(bytes, i, height, height * p, 0);
                    lengths[6] = MeasurePreviousColumnRun(bytes, i, height, height * p, 255);
                }
                if (p >= 2)
                {
                    lengths[4] = MeasurePreviousColumnRun(bytes, i, height, height * (p - 1), 0);
                    lengths[7] = MeasurePreviousColumnRun(bytes, i, height, height * (p - 1), 255);
                }
                if (p >= 3)
                {
                    lengths[5] = MeasurePreviousColumnRun(bytes, i, height, height * (p - 2), 0);
                    lengths[8] = MeasurePreviousColumnRun(bytes, i, height, height * (p - 2), 255);
                }
                int maxIndex;
                int max = lengths.Max(out maxIndex);

                byte b = bytes[i];
                if (max < 2)
                {
                    maxIndex = -1;
                }
                else if (max == 2)
                {
                    if (b >= 0x08)
                    {
                        maxIndex = -1;
                    }
                    else
                    {
                        if (maxIndex < 2 || maxIndex >= 6)
                        {
                            maxIndex = -1;
                        }
                    }
                }
                else if (max == 3)
                {
                    //don't compress 3 bytes to 3 bytes unless first byte would be a literal
                    if (b >= 0x08 && maxIndex < 2 || maxIndex >= 6)
                    {
                        maxIndex = -1;
                    }
                }

                switch (maxIndex)
                {
                    default:
                        {
                            //No compression - raw byte or escaped literal
                            if (b >= 0x08)
                            {
                                //raw byte
                                stream.WriteByte((byte)b);
                            }
                            else
                            {
                                //encode a literal as 2 bytes
                                stream.WriteByte((byte)0x07);
                                stream.WriteByte((byte)b);
                            }
                            i++;
                        }
                        break;
                    case 0:
                        {
                            //RLE - 01
                            stream.WriteByte(0x01);
                            stream.WriteByte((byte)(max - 1));
                            stream.WriteByte(b);
                            i += max;
                        }
                        break;
                    case 1:
                        {
                            //Alternating RLE - 02
                            stream.WriteByte(0x02);
                            stream.WriteByte((byte)(max / 2 - 1));
                            stream.WriteByte(b);
                            stream.WriteByte(bytes[i + 1]);
                            i += max;
                        }
                        break;
                    case 2:
                        {
                            //from previous column (same plane) - 00
                            stream.WriteByte(0x00);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                    case 3:
                        {
                            //from plane 0 without mask - 03
                            stream.WriteByte(0x03);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                    case 6:
                        {
                            //from plane 0 with mask - 06 03
                            stream.WriteByte(0x06);
                            stream.WriteByte(0x03);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                    case 4:
                        {
                            //from plane 1 without mask - 04
                            stream.WriteByte(0x04);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                    case 7:
                        {
                            //from plane 1 with mask - 06 04
                            stream.WriteByte(0x06);
                            stream.WriteByte(0x04);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                    case 5:
                        {
                            //from plane 2 without mask - 05
                            stream.WriteByte(0x05);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                    case 8:
                        {
                            //from plane 2 with mask - 06 05
                            stream.WriteByte(0x06);
                            stream.WriteByte(0x05);
                            stream.WriteByte((byte)(max - 1));
                            i += max;
                        }
                        break;
                }
                i--;
            }
        }

        static int MeasureRleRun(byte[] pic, int i, int height)
        {
            int maxLength = 256;
            int y = i % height;
            if (maxLength > height - y)
            {
                maxLength = height - y;
            }

            byte b = pic[i];
            int i0 = i;
            i++;
            while (i < pic.Length && pic[i] == b)
            {
                if (i - i0 >= maxLength) break;
                i++;
            }
            return i - i0;
        }

        static int MeasureRleRun2(byte[] pic, int i, int height)
        {
            int maxLength = 512;
            int y = i % height;
            if (maxLength > height - y)
            {
                maxLength = height - y;
            }

            int i0 = i;
            if (i + 1 >= pic.Length) return 0;
            if (i + 2 == pic.Length) return 2;
            if (maxLength < 4) return 0;
            byte b1 = pic[i];
            byte b2 = pic[i + 1];
            i += 2;
            while (i + 1 < pic.Length && pic[i] == b1 && pic[i + 1] == b2)
            {
                if (i - i0 + 1 >= maxLength) break;
                i += 2;
            }
            return i - i0;
        }

        static int MeasurePreviousColumnRun(byte[] pic, int i, int height, int offset, byte mask)
        {
            int maxLength = 256;
            int y = i % height;
            if (maxLength > height - y)
            {
                maxLength = height - y;
            }

            int i0 = i;
            if (i - offset < 0)
            {
                return 0;
            }
            while (i < pic.Length && pic[i] == (pic[i - offset] ^ mask))
            {
                if (i - i0 >= maxLength) break;
                i++;
            }
            return i - i0;
        }

        static uint GetBigEndian(byte[] bytes, int index)
        {
            unchecked
            {
                return (uint)((bytes[index + 0] << 24) + (bytes[index + 1] << 16) + (bytes[index + 2] << 8) + bytes[index + 3]);
            }
        }

        static void TransformImage(byte[] dest, byte[] src, int w, int h)
        {
            unchecked
            {
                //transforms an image from standard linear 4 bit to column-major bitplanes
                int x0, y;

                byte plane0, plane1, plane2, plane3;

                for (x0 = 0; x0 < w; x0 += 8)
                {
                    int destIndex = (x0 / 8) * h * 4;
                    for (y = 0; y < h; y++)
                    {
                        uint sourcePixels = GetBigEndian(src, w * y / 2 + x0 / 2);
                        plane0 = 0;
                        plane1 = 0;
                        plane2 = 0;
                        plane3 = 0;
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            uint nibble = (sourcePixels >> 28);
                            int bitValue = 1 << (7 - x2);
                            if (0 != (nibble & 1))
                            {
                                plane0 |= (byte)bitValue;
                            }
                            if (0 != (nibble & 2))
                            {
                                plane1 |= (byte)bitValue;
                            }
                            if (0 != (nibble & 4))
                            {
                                plane2 |= (byte)bitValue;
                            }
                            if (0 != (nibble & 8))
                            {
                                plane3 |= (byte)bitValue;
                            }
                            sourcePixels <<= 4;
                        }
                        dest[destIndex] = plane0;
                        dest[destIndex + h * 1] = plane1;
                        dest[destIndex + h * 2] = plane2;
                        dest[destIndex + h * 3] = plane3;
                        destIndex++;
                    }
                }
            }
        }
    }

}