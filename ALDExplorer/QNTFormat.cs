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
    public class QntHeader : ICloneable
    {
        public int signature;
        public int fileVersion;   //always 2, but could be 0
        public int headerSize;     /* header size */
        public int xLocation;           /* display location x */
        public int yLocation;           /* display location y */
        public int width;        /* image width        */
        public int height;       /* image height       */
        public int bpp;          /* image data depth   */
        public int reserved;          /* reserved data      */
        public int pixelSize;   /* compressed pixel size       */
        public int alphaSize;   /* compressed alpha pixel size */
        public byte[] extraData;

        public string GetComment()
        {
            return ImageConverter.GetComment(this);
        }

        public bool ParseComment(string comment)
        {
            return ImageConverter.ParseComment(this, comment);
        }

        public QntHeader Clone()
        {
            var clone = (QntHeader)this.MemberwiseClone();
            if (clone.extraData != null) clone.extraData = (byte[])clone.extraData.Clone();
            return clone;
        }

        public bool Validate()
        {
            var qnt = this;
            if (qnt.signature != 0x00544e51)
            {
                return false;
            }
            if (qnt.width < 0 || qnt.height < 0 || qnt.width > 65535 || qnt.height > 65535 || qnt.width * qnt.height >= 64 * 1024 * 1024)
            {
                return false;
            }
            if (qnt.bpp == 1 || qnt.bpp == 4 || qnt.bpp == 8 || qnt.bpp == 16 || qnt.bpp == 24 || qnt.bpp == 32)
            {

            }
            else
            {
                return false;
            }
            return true;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }

    static class Qnt
    {
        public static FreeImageBitmap LoadImage(byte[] bytes)
        {
            var qntHeader = GetQntHeader(bytes);
            if (qntHeader == null || !qntHeader.Validate())
            {
                return null;
            }
            var pixels = GetQntPixels(bytes, qntHeader);
            byte[] alphaPixels = null;
            if (qntHeader.alphaSize != 0)
            {
                alphaPixels = GetQntAlpha(bytes, qntHeader);
            }

            FreeImageBitmap image = new FreeImageBitmap(qntHeader.width, qntHeader.height, qntHeader.width * 3, 24, FREE_IMAGE_TYPE.FIT_BITMAP, pixels);
            using (var blue = image.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_RED))
            using (var red = image.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE)) {
                if (alphaPixels != null)
                {
                    image.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                }
                image.SetChannel(blue, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                image.SetChannel(red, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
            }


            FreeImageBitmap alpha = null;
            try
            {
                if (alphaPixels != null)
                {
                    alpha = new FreeImageBitmap(qntHeader.width, qntHeader.height, qntHeader.width, 8, FREE_IMAGE_TYPE.FIT_BITMAP, alphaPixels);
                }

                if (alpha != null)
                {
                    image.SetChannel(alpha, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                }
            }
            finally
            {
                if (alpha != null) alpha.Dispose();
            }
            image.Comment = qntHeader.GetComment();
            image.Tag = qntHeader;
            return image;
        }

        public static QntHeader GetQntHeader(byte[] bytes)
        {
            return GetQntHeader(new MemoryStream(bytes));
        }

        public static QntHeader GetQntHeader(Stream ms)
        {
            long startPosition = ms.Position;
            var br = new BinaryReader(ms);
            QntHeader qnt = new QntHeader();
            qnt.signature = br.ReadInt32();
            qnt.fileVersion = br.ReadInt32();
            if (qnt.fileVersion == 0)
            {
                qnt.headerSize = 48;
            }
            else
            {
                qnt.headerSize = br.ReadInt32();
            }

            if (qnt.headerSize > 1024 * 1024 || qnt.headerSize < 0)
            {
                return null;
            }
            qnt.xLocation = br.ReadInt32();
            qnt.yLocation = br.ReadInt32();
            qnt.width = br.ReadInt32();
            qnt.height = br.ReadInt32();
            qnt.bpp = br.ReadInt32();

            qnt.reserved = br.ReadInt32();
            qnt.pixelSize = br.ReadInt32();
            qnt.alphaSize = br.ReadInt32();
            long endPosition = ms.Position - startPosition;
            int extraDataLength = qnt.headerSize - (int)endPosition;
            if (extraDataLength < 0 || extraDataLength > 1024 * 1024)
            {
                return null;
            }
            if (extraDataLength > 0)
            {
                qnt.extraData = br.ReadBytes(extraDataLength);
            }
            return qnt;
        }

        private static void SaveQntHeader(Stream stream, QntHeader qnt)
        {
            long startPosition = stream.Position;
            var bw = new BinaryWriter(stream);
            bw.WriteStringNullTerminated("QNT");
            bw.Write(qnt.fileVersion);
            if (qnt.fileVersion == 0)
            {
                qnt.headerSize = 48;
                bw.Write(qnt.xLocation);
                bw.Write(qnt.yLocation);
                bw.Write(qnt.width);
                bw.Write(qnt.height);
                bw.Write(qnt.bpp);
                bw.Write(qnt.reserved);
                bw.Write(qnt.pixelSize);
                bw.Write(qnt.alphaSize);
            }
            else
            {
                bw.Write(qnt.headerSize);
                bw.Write(qnt.xLocation);
                bw.Write(qnt.yLocation);
                bw.Write(qnt.width);
                bw.Write(qnt.height);
                bw.Write(qnt.bpp);
                bw.Write(qnt.reserved);
                bw.Write(qnt.pixelSize);
                bw.Write(qnt.alphaSize);
            }
            int position = (int)(stream.Position - startPosition);
            if (position < qnt.headerSize)
            {
                int remainingBytes = qnt.headerSize - position;
                if (qnt.extraData != null && qnt.extraData.Length >= remainingBytes)
                {
                    stream.Write(qnt.extraData, 0, remainingBytes);
                }
                else if (qnt.extraData != null && qnt.extraData.Length < remainingBytes)
                {
                    int sizeFromArray = qnt.extraData.Length;
                    stream.Write(qnt.extraData, 0, sizeFromArray);
                    stream.WriteZeroes(remainingBytes - sizeFromArray);
                }
                else
                {
                    //pad with zeroes
                    stream.WriteZeroes(remainingBytes);
                }
            }
        }

        public static byte[] GetQntPixels(byte[] inputBytes, QntHeader qntHeader)
        {
            byte[] pic;
            if (qntHeader.pixelSize > 0)
            {
                byte[] raw = null;
                using (var ms = new MemoryStream(inputBytes, qntHeader.headerSize, qntHeader.pixelSize))
                {
                    using (var rawMs = ZLibCompressor.DeCompress(ms)) { 
                        raw = rawMs.ToArray();
                    }

                    int endPosition = (int)ms.Position;
                    //int length = (int)ms.Position - qntHeader.headerSize;
                    //if (length != qntHeader.pixelSize + qntHeader.alphaSize)
                    //{
                    //}
                }

                int w = qntHeader.width;
                int h = qntHeader.height;
                if (raw != null && raw.Length < w * h * 3)
                {
                    throw new InvalidDataException("Size of decompressed data is wrong");
                }

                pic = DecodeQntPixels(raw, w, h);
                //VerifyQntPixels(raw, w, h, pic);
            }
            else
            {
                int w = qntHeader.width;
                int h = qntHeader.height;
                pic = new byte[w * h * 3];
            }

            return pic;
        }

        private static void VerifyQntPixels(byte[] raw, int w, int h, byte[] pic)
        {
            if (Debugger.IsAttached && false)
            {
                var raw2 = EncodeQntPixels(pic, w, h);
                if (raw.SequenceEqual(raw2))
                {

                }
                else
                {
                    for (int i = 0; i < raw.Length; i++)
                    {
                        if (raw[i] != raw2[i])
                        {
                            int minIndex = i - 4;
                            if (minIndex < 0) minIndex = 0;
                            var dummy1 = raw.Skip(minIndex).Take(8).ToArray();
                            var dummy2 = raw2.Skip(minIndex).Take(8).ToArray();
                        }
                    }
                }
            }
        }

        private static byte[] DecodeQntPixels(byte[] raw, int w, int h)
        {
            if (raw == null) return null;

            byte[] pic = new byte[h * w * 3];
            if (raw.Length < pic.Length)
            {

            }
            DeinterleaveQntPixels(raw, w, h, pic);
            VerifyDeinterleaveQntPixels(raw, w, h, pic);
            var picNotFiltered = (byte[])pic.Clone();
            DecodeQntPixelsFilter(w, h, pic);
            VerifyDecodeQntPixelsFilter(w, h, pic, picNotFiltered);

            return pic;
        }

        private static void VerifyDecodeQntPixelsFilter(int w, int h, byte[] pic, byte[] picNotFiltered)
        {
            if (Debugger.IsAttached && false)
            {
                var pic2 = EncodeQntPixelsFilter(pic, w, h);
                if (pic2.SequenceEqual(picNotFiltered))
                {

                }
                else
                {
                    for (int i = 0; i < picNotFiltered.Length; i++)
                    {
                        if (picNotFiltered[i] != pic2[i])
                        {
                            int minIndex = i - 4;
                            if (minIndex < 0) minIndex = 0;
                            var dummy1 = picNotFiltered.Skip(minIndex).Take(8).ToArray();
                            var dummy2 = pic2.Skip(minIndex).Take(8).ToArray();
                        }
                    }
                }
            }
        }

        private static void DecodeQntPixelsFilter(int w, int h, byte[] pic)
        {
            int x, y;

            if (w > 1)
            {
                for (x = 1; x < w; x++)
                {
                    pic[x * 3] = (byte)(pic[(x - 1) * 3] - pic[x * 3]);
                    pic[x * 3 + 1] = (byte)(pic[(x - 1) * 3 + 1] - pic[x * 3 + 1]);
                    pic[x * 3 + 2] = (byte)(pic[(x - 1) * 3 + 2] - pic[x * 3 + 2]);
                }
            }

            if (h > 1)
            {
                for (y = 1; y < h; y++)
                {
                    pic[(y * w) * 3] = (byte)(pic[((y - 1) * w) * 3] - pic[(y * w) * 3]);
                    pic[(y * w) * 3 + 1] = (byte)(pic[((y - 1) * w) * 3 + 1] - pic[(y * w) * 3 + 1]);
                    pic[(y * w) * 3 + 2] = (byte)(pic[((y - 1) * w) * 3 + 2] - pic[(y * w) * 3 + 2]);

                    for (x = 1; x < w; x++)
                    {
                        int px, py;
                        py = pic[((y - 1) * w + x) * 3];
                        px = pic[(y * w + x - 1) * 3];
                        pic[(y * w + x) * 3] = (byte)(((py + px) >> 1) - pic[(y * w + x) * 3]);
                        py = pic[((y - 1) * w + x) * 3 + 1];
                        px = pic[(y * w + x - 1) * 3 + 1];
                        pic[(y * w + x) * 3 + 1] = (byte)(((py + px) >> 1) - pic[(y * w + x) * 3 + 1]);
                        py = pic[((y - 1) * w + x) * 3 + 2];
                        px = pic[(y * w + x - 1) * 3 + 2];
                        pic[(y * w + x) * 3 + 2] = (byte)(((py + px) >> 1) - pic[(y * w + x) * 3 + 2]);
                    }
                }
            }
        }

        private static void VerifyDeinterleaveQntPixels(byte[] raw, int w, int h, byte[] pic)
        {
            if (Debugger.IsAttached && false)
            {
                var raw2 = InterleaveQntPixels(w, h, pic);
                if (raw.SequenceEqual(raw2))
                {

                }
                else
                {
                    for (int i = 0; i < raw.Length; i++)
                    {
                        if (raw[i] != raw2[i])
                        {
                            int minIndex = i - 4;
                            if (minIndex < 0) minIndex = 0;
                            var dummy1 = raw.Skip(minIndex).Take(8).ToArray();
                            var dummy2 = raw2.Skip(minIndex).Take(8).ToArray();
                        }
                    }
                }
            }
        }

        private static void DeinterleaveQntPixels(byte[] raw, int w, int h, byte[] pic)
        {
            int i, j, x, y;
            j = 0;
            /*
            if (pic.Length > raw.Length)
            {
               int numberOfChannels = raw.Length / (pic.Length / 3);
               if (numberOfChannels == 1)
               {
                   //this only happens on a corrupt file - non interleaved copy of the alpha channel
                   for (y = 0; y < h; y++)
                   {
                       for (x = 0; x < w; x++)
                       {
                           i = x + y * w;
                           pic[i * 3] = raw[i];
                           pic[i * 3 + 1] = raw[i];
                           pic[i * 3 + 2] = raw[i];
                       }
                   }
                   return;
               }
               else
               {}
            }
            */

            for (i = 3 - 1; i >= 0; i--)
            {
                for (y = 0; y < (h - 1); y += 2)
                {
                    for (x = 0; x < (w - 1); x += 2)
                    {
                        pic[(y * w + x) * 3 + i] = raw[j];
                        pic[((y + 1) * w + x) * 3 + i] = raw[j + 1];
                        pic[(y * w + x + 1) * 3 + i] = raw[j + 2];
                        pic[((y + 1) * w + x + 1) * 3 + i] = raw[j + 3];
                        j += 4;
                    }
                    if (x != w)
                    {
                        pic[(y * w + x) * 3 + i] = raw[j];
                        pic[((y + 1) * w + x) * 3 + i] = raw[j + 1];
                        j += 4;
                    }
                }
                if (y != h)
                {
                    for (x = 0; x < (w - 1); x += 2)
                    {
                        pic[(y * w + x) * 3 + i] = raw[j];
                        pic[(y * w + x + 1) * 3 + i] = raw[j + 2];
                        j += 4;
                    }
                    if (x != w)
                    {
                        pic[(y * w + x) * 3 + i] = raw[j];
                        j += 4;
                    }
                }
            }
        }

        static byte[] GetQntAlpha(byte[] inputBytes, QntHeader qntHeader)
        {
            byte[] raw = null;
            using (var ms = new MemoryStream(inputBytes)) { 
                ms.Position = qntHeader.headerSize + qntHeader.pixelSize;
                using (var rawMs = ZLibCompressor.DeCompress(ms))
                { 
                    raw = rawMs.ToArray();
                }
            }

            int w = qntHeader.width;
            int h = qntHeader.height;
            byte[] pic = DecodeQntAlpha(raw, w, h);
            VerifyQntAlpha(raw, w, h, pic);

            return pic;
        }

        private static void VerifyQntAlpha(byte[] raw, int w, int h, byte[] pic)
        {
            if (Debugger.IsAttached && false)
            {
                var raw2 = EncodeQntAlpha(w, h, pic);
                if (raw.SequenceEqual(raw2))
                {

                }
                else
                {
                    int length = Math.Min(raw.Length, raw2.Length);
                    if (raw.Length != raw2.Length)
                    {

                    }
                    for (int i = 0; i < length; i++)
                    {
                        if (raw[i] != raw2[i])
                        {
                            int minIndex = i - 4;
                            if (minIndex < 0) minIndex = 0;
                            var dummy1 = raw.Skip(minIndex).Take(8).ToArray();
                            var dummy2 = raw2.Skip(minIndex).Take(8).ToArray();
                        }
                    }
                }
            }
        }

        private static byte[] DecodeQntAlpha(byte[] raw, int w, int h)
        {
            if (raw == null) return null;
            byte[] pic = new byte[w * h];
            int i = 1;
            int x, y;
            if (w > 1)
            {
                pic[0] = raw[0];
                for (x = 1; x < w; x++)
                {
                    pic[x] = (byte)(pic[x - 1] - raw[i]);
                    i++;
                }
                if (0 != (w % 2)) i++;
            }

            if (h > 1)
            {
                for (y = 1; y < h; y++)
                {
                    pic[y * w] = (byte)(pic[(y - 1) * w] - raw[i]); i++;
                    for (x = 1; x < w; x++)
                    {
                        int pax, pay;
                        pax = pic[y * w + x - 1];
                        pay = pic[(y - 1) * w + x];
                        pic[y * w + x] = (byte)(((pax + pay) >> 1) - raw[i]);
                        i++;
                    }
                    if (0 != (w % 2)) i++;
                }
            }
            return pic;
        }

        static void SaveQntPixels(Stream stream, FreeImageBitmap bitmap)
        {
            int w = bitmap.Width;
            int h = bitmap.Height;

            byte[] pic = GetBGRFromBitmap(bitmap, w, h);
            byte[] raw = EncodeQntPixels(pic, w, h);

            using (var zlibStream = new ZLibStream(stream, CompressionMode.Compress, CompressionLevel.Level9, true))
            {
                zlibStream.Write(raw, 0, raw.Length);
                zlibStream.Flush();
            }
        }

        private static byte[] EncodeQntPixels(byte[] pic, int w, int h)
        {
            byte[] pic2 = EncodeQntPixelsFilter(pic, w, h);
            byte[] raw = InterleaveQntPixels(w, h, pic2);
            return raw;
        }

        private static byte[] InterleaveQntPixels(int w, int h, byte[] pic2)
        {
            byte[] raw = new byte[(w + 10) * (h + 10) * 3];
            int i, x, y;
            int j = 0;
            for (i = 2; i >= 0; i--)
            {
                for (y = 0; y < (h - 1); y += 2)
                {
                    for (x = 0; x < (w - 1); x += 2)
                    {
                        raw[j + 0] = pic2[(y * w + x) * 3 + i];
                        raw[j + 1] = pic2[((y + 1) * w + x) * 3 + i];
                        raw[j + 2] = pic2[(y * w + x + 1) * 3 + i];
                        raw[j + 3] = pic2[((y + 1) * w + x + 1) * 3 + i];
                        j += 4;
                    }
                    if (x != w)
                    {
                        raw[j] = pic2[(y * w + x) * 3 + i];
                        raw[j + 1] = pic2[((y + 1) * w + x) * 3 + i];
                        /*
                        raw[j + 2] = pic2[(y * w + x + 1) * 3 + i];
                        if (((y + 1) * w + x + 1) * 3 + i < pic2.Length)
                        {
                           raw[j + 3] = pic2[((y + 1) * w + x + 1) * 3 + i];
                        }
                        else
                        {}
                        */
                        j += 4;
                    }
                }
                if (y != h)
                {
                    for (x = 0; x < (w - 1); x += 2)
                    {
                        raw[j + 0] = pic2[(y * w + x) * 3 + i];
                        raw[j + 2] = pic2[(y * w + x + 1) * 3 + i];
                        j += 4;
                    }
                    if (x != w)
                    {
                        raw[j] = pic2[(y * w + x) * 3 + i];
                        j += 4;
                    }
                }
            }

            using (var ms = new MemoryStream(raw))
            using (var br = new BinaryReader(ms))
            {
                return br.ReadBytes(j);
            }

            //return raw;
        }

        private static byte[] EncodeQntPixelsFilter(byte[] pic, int w, int h)
        {
            byte[] pic2 = new byte[w * h * 3];

            int x, y;

            pic2[0] = pic[0];
            pic2[1] = pic[1];
            pic2[2] = pic[2];

            if (w > 1)
            {
                for (x = 1; x < w; x++)
                {
                    pic2[x * 3 + 0] = (byte)(pic[(x - 1) * 3 + 0] - pic[x * 3 + 0]);
                    pic2[x * 3 + 1] = (byte)(pic[(x - 1) * 3 + 1] - pic[x * 3 + 1]);
                    pic2[x * 3 + 2] = (byte)(pic[(x - 1) * 3 + 2] - pic[x * 3 + 2]);
                }
            }

            if (h > 1)
            {
                for (y = 1; y < h; y++)
                {
                    pic2[y * w * 3 + 0] = (byte)(pic[(y - 1) * w * 3 + 0] - pic[y * w * 3 + 0]);
                    pic2[y * w * 3 + 1] = (byte)(pic[(y - 1) * w * 3 + 1] - pic[y * w * 3 + 1]);
                    pic2[y * w * 3 + 2] = (byte)(pic[(y - 1) * w * 3 + 2] - pic[y * w * 3 + 2]);

                    for (x = 1; x < w; x++)
                    {
                        int px, py;
                        py = pic[((y - 1) * w + x) * 3];
                        px = pic[(y * w + x - 1) * 3];
                        pic2[(y * w + x) * 3] = (byte)-(pic[(y * w + x) * 3] - ((py + px) >> 1));
                        py = pic[((y - 1) * w + x) * 3 + 1];
                        px = pic[(y * w + x - 1) * 3 + 1];
                        pic2[(y * w + x) * 3 + 1] = (byte)-(pic[(y * w + x) * 3 + 1] - ((py + px) >> 1));
                        py = pic[((y - 1) * w + x) * 3 + 2];
                        px = pic[(y * w + x - 1) * 3 + 2];
                        pic2[(y * w + x) * 3 + 2] = (byte)-(pic[(y * w + x) * 3 + 2] - ((py + px) >> 1));
                    }
                }
            }

            return pic2;
        }

        private static byte[] GetBGRFromBitmap(FreeImageBitmap bitmap, int w, int h)
        {
            byte[] pic = new byte[(w + 10) * (h + 10) * 3];

            int o = 0;
            //get pic from bitmap
            for (int y = 0; y < h; y++)
            {
                var scanline = bitmap.GetScanlineFromTop8Bit(y);
                if (bitmap.ColorDepth == 24)
                {
                    for (int x = 0; x < w; x++)
                    {
                        //get image data from RGB to BGR
                        pic[o++] = scanline[x * 3 + 2];
                        pic[o++] = scanline[x * 3 + 1];
                        pic[o++] = scanline[x * 3 + 0];
                    }
                }
                else if (bitmap.ColorDepth == 32)
                {
                    for (int x = 0; x < w; x++)
                    {
                        //get image data from RGB to BGR
                        pic[o++] = scanline[x * 4 + 2];
                        pic[o++] = scanline[x * 4 + 1];
                        pic[o++] = scanline[x * 4 + 0];
                    }
                }
            }
            return pic;
        }

        static void SaveQntAlpha(Stream stream, FreeImageBitmap bitmap)
        {
            int w, h;
            w = bitmap.Width;
            h = bitmap.Height;

            byte[] pic = GetAlphaFromBitmap(bitmap, w, h);
            byte[] raw = EncodeQntAlpha(w, h, pic);

            using (var zlibStream = new ZLibStream(stream, CompressionMode.Compress, CompressionLevel.Level9, true))
            {
                zlibStream.Write(raw, 0, raw.Length);
                zlibStream.Flush();
            }
        }

        private static byte[] GetAlphaFromBitmap(FreeImageBitmap bitmap, int w, int h)
        {
            byte[] pic = new byte[(w + 10) * (h + 10)];
            int o = 0;
            //get pic from bitmap
            for (int y = 0; y < h; y++)
            {
                var scanline = bitmap.GetScanlineFromTop8Bit(y);
                for (int x = 0; x < w; x++)
                {
                    //get alpha channel
                    pic[o++] = scanline[x * 4 + 3];
                }
            }
            return pic;
        }

        private static byte[] EncodeQntAlpha(int w, int h, byte[] pic)
        {
            int outW = w;
            if (0 != (outW & 1))
            {
                outW++;
            }


            byte[] raw = new byte[outW * h];
            int i, x, y;
            i = 1;
            if (w > 1)
            {
                raw[0] = pic[0];
                for (x = 1; x < w; x++)
                {
                    raw[i] = (byte)(-pic[x] + pic[x - 1]);
                    i++;
                }
                if (0 != (w % 2)) i++;
            }

            if (h > 1)
            {
                for (y = 1; y < h; y++)
                {
                    //solve for raw[i]
                    raw[i] = (byte)(-pic[y * w] + pic[(y - 1) * w]);
                    i++;
                    for (x = 1; x < w; x++)
                    {
                        int pax, pay;
                        pax = pic[y * w + x - 1];
                        pay = pic[(y - 1) * w + x];
                        raw[i] = (byte)(-pic[y * w + x] + ((pax + pay) >> 1));
                        i++;
                    }
                    if (0 != (w % 2)) i++;
                }
            }

            return raw;
        }

        public static void SaveImage(Stream stream, FreeImageBitmap bitmap)
        {
            string comment = bitmap.Comment;
            var qntHeader = new QntHeader();
            if (string.IsNullOrEmpty(comment) || !qntHeader.ParseComment(comment))
            {
                qntHeader.headerSize = 0x44;
                qntHeader.fileVersion = 2;
            }
            if (qntHeader.bpp == 0 || qntHeader.bpp == 32)
            {
                qntHeader.bpp = 24;
            }
            qntHeader.height = bitmap.Height;
            qntHeader.width = bitmap.Width;
            qntHeader.pixelSize = 0;
            qntHeader.alphaSize = 0;

            long headerPosition = stream.Position;
            SaveQntHeader(stream, qntHeader);

            long imageStartPosition = stream.Position;
            long imageEndPosition;
            long alphaStartPosition = headerPosition;
            long alphaEndPosition = headerPosition;
            if (bitmap.ColorDepth == 32)
            {
                SaveQntPixels(stream, bitmap);
                imageEndPosition = stream.Position;
                alphaStartPosition = stream.Position;
                SaveQntAlpha(stream, bitmap);
                alphaEndPosition = stream.Position;
            }
            else
            {
                SaveQntPixels(stream, bitmap);
                imageEndPosition = stream.Position;
            }
            qntHeader.pixelSize = (int)(imageEndPosition - imageStartPosition);
            qntHeader.alphaSize = (int)(alphaEndPosition - alphaStartPosition);

            long endPosition = stream.Position;
            stream.Position = headerPosition;
            //update QNT header
            SaveQntHeader(stream, qntHeader);
            stream.Position = endPosition;
        }
    }
}