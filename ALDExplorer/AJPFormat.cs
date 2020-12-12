using System;
using System.Collections.Generic;
using System.IO;
using FreeImageAPI;
using ZLibNet;
using System.Globalization;

namespace ALDExplorer.Formats
{
  public class AjpHeader : ICloneable
    {
        public int signature;
        public int version;
        public int headerSize1;  //0x38
        public int width;
        public int height;
        public int headerSize2;  //0x38
        public int jpegDataLength;
        public int alphaLocation;
        public int sizeOfDataAfterJpeg;
        public byte[] unknown1;
        public int unknown2;
        public byte[] unknown3;
        public byte[] jpegFooter;

        public bool HasAlpha
        {
            get
            {
                return alphaLocation != 0 && sizeOfDataAfterJpeg != 0;
            }
        }

        public string GetComment()
        {
            return ImageConverter.GetComment(this);
        }

        public bool ParseComment(string comment)
        {
            return ImageConverter.ParseComment(this, comment);
        }

        public bool Validate()
        {
            if (signature != 0x00504a41)
            {
                return false;
            }
            if (width < 0 || height < 0 || width > 65535 || height > 65535 || width * height > 64 * 1024 * 1024)
            {
                return false;
            }
            if (headerSize1 < 0 || headerSize1 > 1024 * 1024)
            {
                return false;
            }
            if (headerSize2 < 0 || headerSize2 > 1024 * 1024)
            {
                return false;
            }
            if (sizeOfDataAfterJpeg < 0 || sizeOfDataAfterJpeg > 1024 * 1024 * 64)
            {
                return false;
            }
            return true;
        }

        //header before jpeg:         1F DB C2 B6 03 7B C6 01 E1 FF 1A A7 FB
        //common header before jpeg?  7A C6 01 00 00 00 00 A2 49 51 67 4A 46 0B 8B CA AA 4C 93 B7 CA 16 7C
        //footer after jpeg:          0D DC AC 87 0A 56 49 CD 83 EC 4C 92 B5 CB 16 34

        public AjpHeader Clone()
        {
            var clone = (AjpHeader)this.MemberwiseClone();
            if (clone.unknown1 != null) clone.unknown1 = (byte[])(clone.unknown1.Clone());
            if (clone.unknown3 != null) clone.unknown3 = (byte[])(clone.unknown3.Clone());
            if (clone.jpegFooter != null) clone.jpegFooter = (byte[])(clone.jpegFooter.Clone());
            return clone;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
    
    static class Ajp
    {
        public static byte[] GetBytes(string token)
        {
            if ((token.Length & 1) == 1)
            {
                return null;
            }
            List<byte> bytes = new List<byte>(token.Length / 2);
            for (int i = 0; i < token.Length; i += 2)
            {
                string hexPair = token.Substring(i, 2);
                byte b;
                if (byte.TryParse(hexPair, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out b))
                {
                    bytes.Add(b);
                }
                else
                {
                    return null;
                }
            }
            return bytes.ToArray();
        }

        public static FreeImageBitmap LoadImage(byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            return LoadImage(ms);
        }

        public static void LoadImage(byte[] bytes, out byte[] jpegBytes, out FreeImageBitmap alpha, out AjpHeader ajpHeader)
        {
            var ms = new MemoryStream(bytes);
            LoadImage(ms, out jpegBytes, out alpha, out ajpHeader);
        }

        public static void LoadImage(Stream ajpStream, out byte[] jpegBytes, out FreeImageBitmap alpha, out AjpHeader ajpHeader)
        {
            MemoryStream jpegFile;
            MemoryStream pmsFile;

            LoadImage(ajpStream, out jpegFile, out pmsFile, out ajpHeader);
            jpegBytes = jpegFile.ToArray();
            jpegFile.Dispose();

            alpha = null;
            if (pmsFile != null)
            {
                FreeImageBitmap pmsImage = Pms.LoadImage(pmsFile.ToArray());
                pmsFile.Dispose();
                alpha = pmsImage;
            }
        }

        public static FreeImageBitmap LoadImage(Stream ajpStream)
        {

            //var br = new BinaryReader(ms);
            //var ajpHeader = ReadAjpHeader(br);
            //if (ajpHeader == null)
            //{
            //    return null;
            //}
            //var jpegData = br.ReadBytes(ajpHeader.jpegDataLength - 16);
            //var jpegFooter = br.ReadBytes(16);
            //ajpHeader.jpegFooter = jpegFooter;
            //MemoryStream newJpegFileStream = new MemoryStream();
            //var bw = new BinaryWriter(newJpegFileStream);
            //UInt16 resolution = BitConverter.ToUInt16(jpegData, 0);

            //byte[] newJpegHeader = new byte[]
            //{
            //    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, (byte)'J',(byte)'F',(byte)'I',(byte)'F', 0x00, 0x01, 0x02, 0x00, (byte)(resolution>> 8), (byte)(resolution & 0xFF)
            //};

            //bw.Write(newJpegHeader);
            //bw.Write(jpegData);

            MemoryStream jpegFile;
            MemoryStream pmsFile;
            AjpHeader ajpHeader;

            LoadImage(ajpStream, out jpegFile, out pmsFile, out ajpHeader);

            if (jpegFile == null)
            {
                return null;
            }

            jpegFile.Position = 0;
            FreeImageBitmap jpegImage = new FreeImageBitmap(jpegFile, FREE_IMAGE_FORMAT.FIF_JPEG);
            jpegImage.Tag = ajpHeader;
            jpegImage.Comment = ajpHeader.GetComment();

            if (pmsFile != null && pmsFile.Length > 0)
            {
                pmsFile.Position = 0;
                using (var pmsImage = Pms.LoadImage(pmsFile.ToArray()))
                {
                    if (pmsImage == null)
                        return jpegImage;
                    jpegImage.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                    jpegImage.SetChannel(pmsImage, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                }

            }

            return jpegImage;
            /*
            int pmsSize = ajpHeader.sizeOfDataAfterJpeg - 16;
            if (pmsSize < 0)
            {
               return jpegImage;
            }
            var pmsData = br.ReadBytes(pmsSize);

            var newPmsFileStream = new MemoryStream();
            var newPmsHeader = new byte[] { 0x50, 0x4D, 0x02, 0x00, 0x40, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var bw2 = new BinaryWriter(newPmsFileStream);
            bw2.Write(newPmsHeader);
            bw2.Write(pmsData);
            FreeImageBitmap pmsImage = Pms.LoadImage(newPmsFileStream.ToArray());
            jpegImage.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
            jpegImage.SetChannel(pmsImage, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
            pmsImage.Dispose();
            return jpegImage;
            */
        }

        private static void LoadImage(Stream ajpStream, out MemoryStream jpegFile, out MemoryStream pmsFile, out AjpHeader ajpHeader)
        {
            jpegFile = null;
            pmsFile = null;

            var brAjp = new BinaryReader(ajpStream);
            ajpHeader = ReadAjpHeader(brAjp);
            if (ajpHeader == null)
                return;
            MemoryStream newJpegFileStream = new MemoryStream();

            jpegFile = new MemoryStream(ajpHeader.jpegDataLength);
            jpegFile.WriteZeroes(16);
            jpegFile.WriteFromStream(ajpStream, ajpHeader.jpegDataLength - 16);
            jpegFile.Position = 16;
            var brJpegFile = new BinaryReader(jpegFile);
            ushort resolution = brJpegFile.ReadUInt16();
            jpegFile.Position = 0;

            byte[] newJpegHeader = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, (byte)'J',(byte)'F',(byte)'I',(byte)'F', 0x00, 0x01, 0x02, 0x00, (byte)(resolution>> 8), (byte)(resolution & 0xFF)
            };

            jpegFile.Write(newJpegHeader, 0, newJpegHeader.Length);

            jpegFile.Position = 0;

            ajpHeader.jpegFooter = brAjp.ReadBytes(16);

            int pmsSize = ajpHeader.sizeOfDataAfterJpeg - 16;
            if (pmsSize < 0)
            {
                return;
            }

            pmsFile = new MemoryStream(pmsSize + 16);
            var newPmsHeader = new byte[] { 0x50, 0x4D, 0x02, 0x00, 0x40, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            pmsFile.Write(newPmsHeader, 0, newPmsHeader.Length);
            pmsFile.WriteFromStream(ajpStream);

            pmsFile.Position = 0;
            return;
        }

        static bool GetJpegSize(byte[] data, out int width, out int height)
        {
            width = -1;
            height = -1;
            int dataSize = data.Length;
            //Check for valid JPEG image
            int i = 0;   // Keeps track of the position within the file
            if (data[i] == 0xFF && data[i + 1] == 0xD8 && data[i + 2] == 0xFF && data[i + 3] == 0xE0)
            {
                i += 4;
                // Check for valid JPEG header (null terminated JFIF)
                if (data[i + 2] == 'J' && data[i + 3] == 'F' && data[i + 4] == 'I' && data[i + 5] == 'F' && data[i + 6] == 0x00)
                {
                    //Retrieve the block length of the first block since the first block will not contain the size of file
                    int blockLength = data[i] * 256 + data[i + 1];
                    while (i < dataSize)
                    {
                        i += blockLength;               //Increase the file index to get to the next block
                        if (i >= dataSize) return false;   //Check to protect against segmentation faults
                        if (data[i] != 0xFF) return false;   //Check that we are truly at the start of another block
                        if (data[i + 1] == 0xC0)
                        {            //0xFFC0 is the "Start of frame" marker which contains the file size
                            //The structure of the 0xFFC0 block is quite simple [0xFFC0][ushort length][uchar precision][ushort x][ushort y]
                            height = data[i + 5] * 256 + data[i + 6];
                            width = data[i + 7] * 256 + data[i + 8];
                            return true;
                        }
                        else
                        {
                            i += 2;                              //Skip the block marker
                            blockLength = data[i] * 256 + data[i + 1];   //Go to the next block
                        }
                    }
                    return false;                     //If this point is reached then no size was found
                }
                else
                {
                    //Not a valid JFIF string
                    return false;
                }
            }
            else
            {
                //Not a valid SOI header
                return false;
            }
        }

        public static void SaveImage(Stream stream, byte[] jpegFile, FreeImageBitmap alpha, AjpHeader ajpHeader)
        {
            byte[] pmsFile = null;
            if (alpha != null)
            {
                var ms = new MemoryStream();
                alpha.Comment = "signature = 19792, version = 2, headerSize = 64";
                Pms.SaveImage(ms, alpha);
                pmsFile = ms.ToArray();
            }

            SaveImage(stream, jpegFile, pmsFile, ajpHeader);
        }

        private static void SaveImage(Stream stream, byte[] jpegFile, byte[] pmsFile, AjpHeader ajpHeader)
        {
            if (ajpHeader == null)
            {
                ajpHeader = new AjpHeader();
            }
            if (ajpHeader.signature == 0)
            {
                ajpHeader.signature = 0x00504a41;
            }
            if (ajpHeader.headerSize1 == 0)
            {
                ajpHeader.headerSize1 = 0x38;
            }
            if (ajpHeader.headerSize2 == 0)
            {
                ajpHeader.headerSize2 = 0x38;
            }
            if (ajpHeader.unknown1 == null || ajpHeader.unknown1.Length == 0)
            {
                ajpHeader.unknown1 = GetBytes("1FDBC2B6037BC601E1FF1AA7FB7AC601");
            }
            if (ajpHeader.unknown3 == null || ajpHeader.unknown1.Length == 0)
            {
                ajpHeader.unknown3 = GetBytes("A24951674A460B8BCAAA4C93B7CA167C");
            }
            if (ajpHeader.jpegFooter == null || ajpHeader.jpegFooter.Length == 0)
            {
                ajpHeader.jpegFooter = GetBytes("0DDCAC870A5649CD83EC4C92B5CB1634");
            }

            if (ajpHeader.width == 0 && ajpHeader.height == 0)
            {
                int width, height;
                if (GetJpegSize(jpegFile, out width, out height))
                {
                    ajpHeader.width = width;
                    ajpHeader.height = height;
                }
                else
                {
                    var ms = new MemoryStream(jpegFile);
                    using (FreeImageBitmap jpegImage = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_JPEG))
                    {
                        ajpHeader.width = jpegImage.Width;
                        ajpHeader.height = jpegImage.Height;
                    }
                }
            }

            ajpHeader.jpegDataLength = jpegFile.Length;

            bool hasAlpha = pmsFile != null && pmsFile.Length > 0;

            if (!hasAlpha)
            {
                ajpHeader.alphaLocation = 0;
                ajpHeader.sizeOfDataAfterJpeg = 0;
            }
            else
            {
                ajpHeader.sizeOfDataAfterJpeg = pmsFile.Length - 16 + 16;
                ajpHeader.alphaLocation = ajpHeader.jpegDataLength + ajpHeader.headerSize1;
            }

            WriteAjpHeader(stream, ajpHeader);
            stream.Write(jpegFile, 16, jpegFile.Length - 16);
            stream.Write(ajpHeader.jpegFooter, 0, ajpHeader.jpegFooter.Length);
            if (pmsFile != null && pmsFile.Length > 16)
            {
                stream.Write(pmsFile, 16, pmsFile.Length - 16);
            }
        }

        public static void SaveImage(Stream stream, FreeImageBitmap bitmap)
        {
            AjpHeader ajpHeaderFromBitmap = bitmap.Tag as AjpHeader;
            AjpHeader ajpHeaderFromComment = null;
            if (!string.IsNullOrEmpty(bitmap.Comment))
            {
                ajpHeaderFromComment = new AjpHeader();
                if (!ajpHeaderFromComment.ParseComment(bitmap.Comment))
                {
                    ajpHeaderFromComment = null;
                }
            }

            var ms = new MemoryStream();
            var ms2 = new MemoryStream();
            bitmap.Save(ms, FREE_IMAGE_FORMAT.FIF_JPEG, FREE_IMAGE_SAVE_FLAGS.JPEG_PROGRESSIVE | FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYGOOD);
            using (var alpha = bitmap.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA))
            {
                if (alpha != null)
                {
                    alpha.Comment = "signature = 19792, version = 2, headerSize = 64, colorDepth = 8";
                    Pms.SaveImage(ms2, alpha);
                }
            }

            AjpHeader ajpHeader = ajpHeaderFromBitmap;
            if (ajpHeader == null) ajpHeader = ajpHeaderFromComment;
            SaveImage(stream, ms.ToArray(), ms2.ToArray(), ajpHeader);
            /*
            if (ajpHeader == null)
            {
               ajpHeader = new AjpHeader();
            }



            if (ajpHeader.signature == 0)
            {
               ajpHeader.signature = 0x00504a41;
            }
            if (ajpHeader.headerSize1 == 0)
            {
               ajpHeader.headerSize1 = 0x38;
            }
            if (ajpHeader.headerSize2 == 0)
            {
               ajpHeader.headerSize2 = 0x38;
            }
            if (ajpHeader.unknown1 == null || ajpHeader.unknown1.Length == 0)
            {
               ajpHeader.unknown1 = GetBytes("1FDBC2B6037BC601E1FF1AA7FB7AC601");
            }
            if (ajpHeader.unknown3 == null || ajpHeader.unknown1.Length == 0)
            {
               ajpHeader.unknown3 = GetBytes("A24951674A460B8BCAAA4C93B7CA167C");
            }
            if (ajpHeader.jpegFooter == null || ajpHeader.jpegFooter.Length == 0)
            {
               ajpHeader.jpegFooter = GetBytes("0DDCAC870A5649CD83EC4C92B5CB1634");
            }

            ajpHeader.width = bitmap.Width;
            ajpHeader.height = bitmap.Height;
            ajpHeader.jpegDataLength = (int)ms.Length + 0;

            bool hasAlpha = ms2.Length != 0;

            if (!hasAlpha)
            {
               ajpHeader.alphaLocation = 0;
               ajpHeader.sizeOfDataAfterJpeg = 0;
            }
            else
            {
               // TOFIX
               ajpHeader.sizeOfDataAfterJpeg = (int)ms2.Length - 16 + 16;
               ajpHeader.alphaLocation = ajpHeader.jpegDataLength + ajpHeader.headerSize1;
            }

            WriteAjpHeader(stream, ajpHeader);

            ms.Position = 16;
            stream.WriteFromStream(ms);
            var bw = new BinaryWriter(stream);
            bw.Write(ajpHeader.jpegFooter);

            ms2.Position = 16;
            stream.WriteFromStream(ms2);

            throw new NotImplementedException();
            */
        }

        private static void WriteAjpHeader(Stream stream, AjpHeader ajp)
        {
            var bw = new BinaryWriter(stream);
            bw.Write(ajp.signature);
            bw.Write(ajp.version);
            bw.Write(ajp.headerSize1);
            bw.Write(ajp.width);
            bw.Write(ajp.height);
            bw.Write(ajp.headerSize2);
            bw.Write(ajp.jpegDataLength);
            bw.Write(ajp.alphaLocation);
            bw.Write(ajp.sizeOfDataAfterJpeg);
            bw.Write(ajp.unknown1);
            bw.Write(ajp.unknown2);
            bw.Write(ajp.unknown3);
        }

        public static AjpHeader ReadAjpHeader(BinaryReader br)
        {
            var ajp = new AjpHeader();
            try
            {
                ajp.signature = br.ReadInt32();
                ajp.version = br.ReadInt32();
                ajp.headerSize1 = br.ReadInt32();
                ajp.width = br.ReadInt32();
                ajp.height = br.ReadInt32();
                ajp.headerSize2 = br.ReadInt32();
                if (ajp.width > 100000 || ajp.width > 100000 || ajp.headerSize2 > 10000)
                    throw new System.IO.IOException("AJP header error");
                ajp.jpegDataLength = br.ReadInt32();
                ajp.alphaLocation = br.ReadInt32();
                ajp.sizeOfDataAfterJpeg = br.ReadInt32();

                ajp.unknown1 = br.ReadBytes(16);
                ajp.unknown2 = br.ReadInt32();
                ajp.unknown3 = br.ReadBytes(16);
            }
            catch (IOException)
            {
                return null;
            }

            return ajp;
        }
    }

}