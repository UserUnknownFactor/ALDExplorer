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

using System.Windows.Forms;

namespace ALDExplorer
{
    public class PmsHeader : ICloneable
    {
        public int signature;
        public int version;
        public int headerSize;
        public int colorDepth;
        public int shadowDepth;
        public int isSprite;
        public int paletteBank;
        public int word0C;
        public int xLocation;
        public int yLocation;
        public int width;
        public int height;
        public int addressOfData;
        public int addressOfPalette;
        public int addressOfComment;
        public byte[] extraData;
        //public string comment;

        public string GetComment()
        {
            return ImageConverter.GetComment(this);
        }

        public bool ParseComment(string comment)
        {
            return ImageConverter.ParseComment(this, comment);
        }

        public PmsHeader Clone()
        {
            var clone = (PmsHeader)MemberwiseClone();
            if (clone.extraData != null) clone.extraData = (byte[])clone.extraData.Clone();
            return clone;
        }

        public bool Validate()
        {
            if (this.signature != 0x4D50)
            {
                return false;
            }
            if (this.xLocation < 0 || this.yLocation < 0 || this.width < 0 || this.height < 0 || this.xLocation > 65535 || this.xLocation > 65535 || this.width > 65535 || this.height > 65535 || this.width * this.height >= 256 * 1024 * 1024)
            {
                return false;
            }
            int firstData = Math.Min(this.addressOfPalette, this.addressOfData);
            if (firstData == 0) firstData = Math.Max(this.addressOfPalette, this.addressOfData);
            int lastHeaderAddress = Math.Max(firstData, this.headerSize);
            if (lastHeaderAddress < 44 ||
                (this.addressOfData < lastHeaderAddress && this.shadowDepth == 0))
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

    public class ImageConverter
    {
        public static string GetComment(object obj)
        {
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            fields = GetIntStringAndByteArrayFields(fields);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldValue = field.GetValue(obj);
                if (fieldValue != null)
                {
                    sb.Append(field.Name);
                    sb.Append(" = ");
                    string stringValue = fieldValue.ToString();
                    if (field.FieldType == typeof(string))
                    {
                        stringValue = stringValue.Replace("\\", "\\\\");
                        stringValue = stringValue.Replace("\"", "\\\"");
                        stringValue = stringValue.Replace("\n", "\\n");
                        stringValue = stringValue.Replace("\r", "\\r");
                        stringValue = stringValue.Replace("\t", "\\t");
                        stringValue = "\"" + stringValue + "\"";
                    }
                    if (field.FieldType == typeof(byte[]))
                    {
                        var byteArray = fieldValue as byte[];
                        stringValue = "\"" + byteArray.ToHexString() + "\"";
                    }
                    sb.Append(stringValue);
                }
                if (i < fields.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }

        private static FieldInfo[] GetIntStringAndByteArrayFields(FieldInfo[] fields)
        {
            fields = fields.Where(f => f.FieldType == typeof(int) || f.FieldType == typeof(string) || f.FieldType == typeof(byte[])).ToArray();
            return fields;
        }

        public static string ReadToken(TextReader tr)
        {
            StringBuilder sb = new StringBuilder();

            //eat white space
            int c = tr.Peek();
            if (c == -1)
            {
                return null;
            }
            while (Char.IsWhiteSpace((char)c))
            {
                c = tr.Read();
                c = tr.Peek();
                if (c == -1)
                {
                    return null;
                }
            }
            if (c == '=' || c == ',')
            {
                c = tr.Read();
                sb.Append((char)c);
            }
            else if (c == '"')
            {
                c = tr.Read();
                //read a quoted string
                while (true)
                {
                    c = tr.Read();
                    if (c == '\\')
                    {
                        c = tr.Read();
                        switch (c)
                        {
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            default:
                                sb.Append((char)c);
                                break;
                        }
                    }
                    else if (c == '"')
                    {
                        break;
                    }
                    else if (c == -1)
                    {
                        return null;
                    }
                    sb.Append((char)c);
                }
            }
            else if (c >= '0' && c <= '9')
            {
                while (c >= '0' && c <= '9')
                {
                    c = tr.Read();
                    sb.Append((char)c);
                    c = tr.Peek();
                }
            }
            else
            {
                while (!Char.IsWhiteSpace((char)c) && !(c == ',' || c == '=') && c != -1)
                {
                    c = tr.Read();
                    sb.Append((char)c);
                    c = tr.Peek();
                }
            }
            return sb.ToString();
        }

        public static bool ParseComment(object obj, string comment)
        {
            if (comment == null) return false;
            var sr = new StringReader(comment);
            var fields = GetIntStringAndByteArrayFields(obj.GetType().GetFields());
            var dic = new Dictionary<string, FieldInfo>();
            foreach (var field in fields)
            {
                dic.Add(field.Name.ToUpperInvariant(), field);
            }

            FieldInfo currentField = null;
            for (string token = ReadToken(sr); token != null; token = ReadToken(sr))
            {
                if (dic.ContainsKey(token.ToUpperInvariant()))
                {
                    currentField = dic[token.ToUpperInvariant()];
                }
                else if (token == "=")
                {
                    token = ReadToken(sr);
                    if (token == null) return false;
                    if (currentField == null) return false;
                    if (currentField.FieldType == typeof(int))
                    {
                        int intValue;
                        if (int.TryParse(token, out intValue))
                        {
                            currentField.SetValue(obj, intValue);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (currentField.FieldType == typeof(string))
                    {
                        currentField.SetValue(obj, token);
                    }
                    else if (currentField.FieldType == typeof(byte[]))
                    {
                        var bytes = GetBytes(token);
                        if (bytes == null)
                        {
                            return false;
                        }
                        currentField.SetValue(obj, bytes);
                    }
                    currentField = null;
                }
                else if (token == ",")
                {

                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static byte[] GetBytes(string token)
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

        public static FreeImageBitmap LoadVsp(byte[] bytes)
        {
            try
            {
                return Vsp.LoadImage(bytes);
            }
            catch
            {
                return null;
            }
        }

        public static FreeImageBitmap LoadPms(byte[] bytes)
        {
            if (Debugger.IsAttached)
            {
                return Pms.LoadImage(bytes);
            }

            try
            {
                return Pms.LoadImage(bytes);
            }
            catch
            {
                return null;
            }
        }

        public static byte[] SkipXcfHeader(byte[] bytes)
        {
            var ms1 = new MemoryStream(bytes);
            var br1 = new BinaryReader(ms1);
            while (br1.BaseStream.Position < br1.BaseStream.Length)
            {
                var tag = (new Tag()).ReadTag(br1);
                if (tag.TagName == "pcgd" || tag.TagName == "dcgd")
                    return tag.TagData;
            }
            //br1.BaseStream.Position = ((br1.BaseStream.Position - 1) | 3) + 1;
            return null;
        }

        public static FreeImageBitmap LoadXcf(byte[] bytes)
        {
            try
            {
                return LoadQnt(SkipXcfHeader(bytes));
            }
            catch
            {
                return null;
            }
        }

        public static FreeImageBitmap LoadQnt(byte[] bytes)
        {
            if (bytes == null) return null;
            try
            {
                return Qnt.LoadImage(bytes);
            }
            catch
            {
                return null;
            }
        }

        public static QntHeader LoadQntHeader(Stream stream)
        {
            var qntHeader = Qnt.GetQntHeader(stream);
            if (qntHeader == null || !qntHeader.Validate())
            {
                return null;
            }
            return qntHeader;
        }

        public static void SaveVsp(Stream stream, FreeImageBitmap bitmap)
        {
            Vsp.SaveImage(stream, bitmap);
        }

        public static void SavePms(Stream stream, FreeImageBitmap bitmap)
        {
            Pms.SaveImage(stream, bitmap);
        }

        public static void SaveQnt(Stream stream, FreeImageBitmap bitmap)
        {
            Qnt.SaveImage(stream, bitmap);
        }

        static class Pms
        {
            public static FreeImageBitmap LoadImage(byte[] bytes)
            {
                var pmsHeader = GetHeader(bytes);
                if (pmsHeader.xLocation < 0 || pmsHeader.yLocation < 0 || pmsHeader.width < 0 || pmsHeader.height < 0)
                {
                    return null;
                }
                int firstData = Math.Min(pmsHeader.addressOfPalette, pmsHeader.addressOfData);
                if (firstData == 0) firstData = Math.Max(pmsHeader.addressOfPalette, pmsHeader.addressOfData);
                int lastHeaderAddress = Math.Max(firstData, pmsHeader.headerSize);
                if (lastHeaderAddress < 44 ||
                    (pmsHeader.addressOfData < lastHeaderAddress && pmsHeader.shadowDepth == 0) ||
                    pmsHeader.addressOfData > bytes.Length ||
                    pmsHeader.addressOfPalette > bytes.Length)
                {
                    return null;
                }
                if (pmsHeader.colorDepth == 16)
                {
                    //return null;
                    ushort[] imageData = GetImageData16Bit(pmsHeader, bytes);
                    byte[] shadowImageData = null;
                    if (pmsHeader.addressOfPalette != 0)
                    {
                        if (pmsHeader.shadowDepth == 8 || pmsHeader.shadowDepth == 0)
                        {
                            shadowImageData = GetImageData8Bit(pmsHeader, bytes, pmsHeader.addressOfPalette);
                        }
                    }
                    FreeImageBitmap bitmap;
                    unsafe
                    {
                        int[] imageData2 = new int[pmsHeader.width * pmsHeader.height];

                        fixed (int* img32 = imageData2)
                        {
                            fixed (ushort* img16 = imageData)
                            {
                                int length = imageData2.Length;
                                if (shadowImageData == null)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        int p = img16[i];
                                        byte a = 0xFF;
                                        unchecked
                                        {
                                            int r = ((p >> 0) & 0x1F);
                                            int g = ((p >> 5) & 0x3F);
                                            int b = ((p >> 11) & 0x1F);

                                            r = (r * 0x839CE + 0x8000) >> 16;
                                            g = (g * 0x40C30 + 0x8000) >> 16;
                                            b = (b * 0x839CE + 0x8000) >> 16;

                                            p = (r << 0) |
                                                (g << 8) |
                                                (b << 16) |
                                                (a << 24);
                                        }
                                        img32[i] = p;
                                    }
                                }
                                else
                                {
                                    fixed (byte* img8 = shadowImageData)
                                    {
                                        for (int i = 0; i < length; i++)
                                        {
                                            int p = img16[i];
                                            byte a = img8[i];
                                            unchecked
                                            {
                                                int r = ((p >> 0) & 0x1F);
                                                int g = ((p >> 5) & 0x3F);
                                                int b = ((p >> 11) & 0x1F);

                                                r = (r * 0x839CE + 0x8000) >> 16;
                                                g = (g * 0x40C30 + 0x8000) >> 16;
                                                b = (b * 0x839CE + 0x8000) >> 16;

                                                p = (r << 0) |
                                                    (g << 8) |
                                                    (b << 16) |
                                                    (a << 24);
                                            }
                                            img32[i] = p;
                                        }
                                    }
                                }
                                bitmap = new FreeImageBitmap(pmsHeader.width, pmsHeader.height, pmsHeader.width * 4, 32, FREE_IMAGE_TYPE.FIT_BITMAP, (IntPtr)img32);

                                //bitmap = new FreeImageBitmap(pmsHeader.width, pmsHeader.height, pmsHeader.width * 2, 16, FREE_IMAGE_TYPE.FIT_BITMAP, (IntPtr)img16);
                            }
                        }
                    }
                    bitmap.Tag = pmsHeader;
                    bitmap.Comment = pmsHeader.GetComment();
                    return bitmap;
                }
                else
                {
                    byte[] imageData;
                    imageData = GetImageData8Bit(pmsHeader, bytes);
                    FreeImageBitmap bitmap = new FreeImageBitmap(pmsHeader.width, pmsHeader.height, pmsHeader.width, 8, FREE_IMAGE_TYPE.FIT_BITMAP, imageData);
                    GetPalette(bitmap.Palette, pmsHeader, bytes);
                    bitmap.Tag = pmsHeader;
                    bitmap.Comment = pmsHeader.GetComment();
                    return bitmap;
                }

            }

            public static void SaveImage(Stream stream, FreeImageBitmap bitmap)
            {
                string comment = bitmap.Comment;
                var pmsHeader = new PmsHeader();
                bool readComment = false;
                if (!string.IsNullOrEmpty(comment))
                {
                    if (pmsHeader.ParseComment(comment))
                    {
                        readComment = true;
                    }
                }

                if (pmsHeader.colorDepth == 0)
                {
                    pmsHeader.colorDepth = 8;
                }

                bool is8Bit = !readComment || pmsHeader.colorDepth == 8;

                if (!readComment)
                {
                    pmsHeader.version = 1;
                    pmsHeader.headerSize = 48;
                }
                if (pmsHeader.signature == 0)
                {
                    pmsHeader.signature = 0x00004d50;
                }
                if (pmsHeader.version == 2 && pmsHeader.headerSize == 0)
                {
                    pmsHeader.headerSize = 64;
                }
                if (pmsHeader.headerSize < 48)
                {
                    pmsHeader.headerSize = 48;
                }
                if (pmsHeader.headerSize > 64)
                {
                    pmsHeader.headerSize = 64;
                }
                pmsHeader.addressOfComment = 0;
                pmsHeader.height = bitmap.Height;
                pmsHeader.width = bitmap.Width;

                if (is8Bit)
                {
                    if (bitmap.ColorDepth > 8)
                    {
                        if (bitmap.ColorDepth == 32)
                        {
                            bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_24_BPP);
                        }
                        bitmap.Quantize(FREE_IMAGE_QUANTIZE.FIQ_WUQUANT, 256);
                        //throw new ArgumentException("image must be 8-bit");
                    }

                    pmsHeader.addressOfPalette = pmsHeader.headerSize;
                    pmsHeader.addressOfData = pmsHeader.addressOfPalette + 768;
                    pmsHeader.colorDepth = 8;

                    SaveHeader(stream, pmsHeader);
                    SavePalette(stream, bitmap.Palette);
                    SaveImageData8Bit(stream, bitmap);
                }
                else
                {
                    bool hasAlphaChannel = false;
                    var existingAlphaChannel = bitmap.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                    if (existingAlphaChannel != null)
                    {
                        bool allPixelsOpaque = AllPixelsOpaque(existingAlphaChannel);
                        hasAlphaChannel = !allPixelsOpaque;
                    }
                    if (bitmap.ColorDepth != 32)
                    {
                        bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                    }

                    bool usingAlphaChannel = pmsHeader.shadowDepth == 8 || (pmsHeader.shadowDepth == 0 && hasAlphaChannel);

                    pmsHeader.addressOfPalette = 0;
                    pmsHeader.addressOfData = pmsHeader.headerSize;
                    pmsHeader.colorDepth = 16;

                    ushort[] image16 = new ushort[pmsHeader.width * pmsHeader.height];
                    byte[] image8 = null;
                    if (usingAlphaChannel)
                    {
                        image8 = new byte[pmsHeader.width * pmsHeader.height];
                    }
                    int o = 0;
                    for (int y = 0; y < pmsHeader.height; y++)
                    {
                        var scanline = bitmap.GetScanlineFromTop32Bit(y);

                        if (image8 == null)
                        {
                            for (int x = 0; x < pmsHeader.width; x++)
                            {
                                unchecked
                                {
                                    int p = scanline[x];
                                    int b = (p >> 0) & 0xFF;
                                    int g = (p >> 8) & 0xFF;
                                    int r = (p >> 16) & 0xFF;
                                    //int a = (p >> 24) & 0xFF;
                                    b = (b * 0x1F1F + 0x8000) >> 16;
                                    g = (g * 0x3F3F + 0x8000) >> 16;
                                    r = (r * 0x1F1F + 0x8000) >> 16;
                                    p = (b << 0) |
                                        (g << 5) |
                                        (r << 11);
                                    image16[o] = (ushort)p;
                                    o++;
                                }
                            }
                        }
                        else
                        {
                            for (int x = 0; x < pmsHeader.width; x++)
                            {
                                unchecked
                                {
                                    int p = scanline[x];
                                    int b = (p >> 0) & 0xFF;
                                    int g = (p >> 8) & 0xFF;
                                    int r = (p >> 16) & 0xFF;
                                    int a = (p >> 24) & 0xFF;
                                    b = (b * 0x1F1F + 0x8000) >> 16;
                                    g = (g * 0x3F3F + 0x8000) >> 16;
                                    r = (r * 0x1F1F + 0x8000) >> 16;
                                    p = (b << 0) |
                                        (g << 5) |
                                        (r << 11);
                                    image16[o] = (ushort)p;
                                    image8[o] = (byte)a;
                                    o++;
                                }
                            }
                        }
                    }
                    MemoryStream ms = new MemoryStream();

                    SaveImageData16Bit(ms, image16, pmsHeader.width, pmsHeader.height);
                    int imageSize = (int)ms.Length;
                    if (usingAlphaChannel)
                    {
                        pmsHeader.addressOfPalette = imageSize + pmsHeader.addressOfData;
                    }
                    SaveHeader(stream, pmsHeader);
                    ms.WriteTo(stream);
                    ms.SetLength(0);
                    if (usingAlphaChannel)
                    {
                        SaveImageData8Bit(stream, image8, pmsHeader.width, pmsHeader.height);
                    }
                }
            }

            private static bool AllPixelsOpaque(FreeImageBitmap existingAlphaChannel)
            {
                //all pixels opaque?
                for (int y = 0; y < existingAlphaChannel.Height; y++)
                {
                    var scanline = existingAlphaChannel.GetScanlineFromTop8Bit(y);
                    for (int x = 0; x < existingAlphaChannel.Width; x++)
                    {
                        if (scanline[x] != 255)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            internal static void SavePalette(Stream stream, Palette palette)
            {
                var bw = new BinaryWriter(stream);
                for (int i = 0; i < 256; i++)
                {
                    int r = palette[i].rgbRed;
                    int g = palette[i].rgbGreen;
                    int b = palette[i].rgbBlue;

                    bw.Write((byte)r);
                    bw.Write((byte)g);
                    bw.Write((byte)b);
                }
            }

            public static PmsHeader GetHeader(byte[] bytes)
            {
                if (bytes.Length < 44)
                {
                    throw new InvalidDataException("PMS header is too short");
                }

                var ms = new MemoryStream(bytes);
                var br = new BinaryReader(ms);

                PmsHeader pms = new PmsHeader();
                pms.signature = br.ReadUInt16();
                pms.version = br.ReadUInt16();
                pms.headerSize = br.ReadUInt16();
                pms.colorDepth = br.ReadByte();
                pms.shadowDepth = br.ReadByte();
                pms.isSprite = br.ReadUInt16();
                pms.paletteBank = br.ReadUInt16();
                pms.word0C = br.ReadInt32();
                pms.xLocation = br.ReadInt32();
                pms.yLocation = br.ReadInt32();
                pms.width = br.ReadInt32();
                pms.height = br.ReadInt32();
                pms.addressOfData = br.ReadInt32();
                pms.addressOfPalette = br.ReadInt32();
                pms.addressOfComment = br.ReadInt32();

                int lastHeaderAddress = Math.Max(pms.headerSize, pms.addressOfPalette);
                int lastHeaderAddress2 = Math.Max(pms.headerSize, pms.addressOfData);
                lastHeaderAddress = Math.Min(lastHeaderAddress, lastHeaderAddress2);
                if (lastHeaderAddress > bytes.Length)
                {
                    lastHeaderAddress = bytes.Length;
                }

                int extraDataSize = lastHeaderAddress - (int)ms.Position;
                if (extraDataSize >= 1024 * 1024)
                {
                    throw new InvalidDataException("Too much extra data for PMS file");
                }

                if (extraDataSize >= 0)
                {
                    pms.extraData = br.ReadBytes(extraDataSize);
                }
                /*
                if (pms.addressOfComment != 0)
                {
                   ms.Position = pms.addressOfComment;
                   pms.comment = br.ReadStringNullTerminated();
                }
                */

                return pms;
            }

            public static void SaveHeader(Stream stream, PmsHeader pms)
            {
                long startPosition = stream.Position;

                var bw = new BinaryWriter(stream);
                bw.Write((ushort)pms.signature);
                //bw.Write(ASCIIEncoding.ASCII.GetBytes("PM"));
                bw.Write((ushort)pms.version);
                bw.Write((ushort)pms.headerSize);
                bw.Write((byte)pms.colorDepth);
                bw.Write((byte)pms.shadowDepth);
                bw.Write((ushort)pms.isSprite);
                bw.Write((ushort)pms.paletteBank);
                bw.Write((int)pms.word0C);
                bw.Write((int)pms.xLocation);
                bw.Write((int)pms.yLocation);
                bw.Write((int)pms.width);
                bw.Write((int)pms.height);
                bw.Write((int)pms.addressOfData);
                bw.Write((int)pms.addressOfPalette);
                bw.Write((int)pms.addressOfComment);

                long endPosition = stream.Position;
                int currentLength = (int)(endPosition - startPosition);
                if (currentLength < pms.headerSize)
                {
                    int extraDataLength = pms.headerSize - currentLength;
                    if (pms.extraData != null && pms.extraData.Length >= extraDataLength)
                    {
                        stream.Write(pms.extraData, 0, extraDataLength);
                    }
                    else if (pms.extraData != null)
                    {
                        //shouldn't ever happen
                        int lengthFromArray = pms.extraData.Length;
                        stream.Write(pms.extraData, 0, lengthFromArray);
                        stream.WriteZeroes(extraDataLength - lengthFromArray);
                    }
                    else
                    {
                        stream.WriteZeroes(extraDataLength);
                    }
                }
            }

            /*
             * Get palette from raw data
             *   pal: palette to be stored
             *   b  : raw data (pointer to palette)
            */
            internal static void GetPalette(Palette palette, PmsHeader pmsHeader, byte[] bytes)
            {
                int address = pmsHeader.addressOfPalette;
                int i;

                for (i = 0; i < 256; i++)
                {
                    palette[i] = Color.FromArgb(bytes[address + i * 3 + 0], bytes[address + i * 3 + 1], bytes[address + i * 3 + 2]);
                }
            }

            static void memcpy(byte[] dest, int destIndex, byte[] src, int srcIndex, int length)
            {
                if (length + destIndex > dest.Length)
                {
                    length = dest.Length - destIndex;
                }
                Array.Copy(src, srcIndex, dest, destIndex, length);
                /*
                for (int i = 0; i < length; i++)
                {
                   dest[destIndex + i] = src[srcIndex + i];
                }
                */
            }

            static void memset(byte[] bytes, int index, byte value, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    bytes[index + i] = value;
                }
            }


            /*
             * Do extract 8bit pms image
             *   pms: pms header information
             *   pic: pixel to be stored
             *   b  : raw data (pointer to pixel)
            */
            internal static byte[] GetImageData8Bit(PmsHeader pms, byte[] bytes)
            {
                return GetImageData8Bit(pms, bytes, pms.addressOfData);
            }
            internal static byte[] GetImageData8Bit(PmsHeader pms, byte[] bytes, int addressOfData)
            {
                byte[] pic = new byte[pms.width * pms.height];
                int address = addressOfData;
                int c0, c1;
                int x, y, loc, l, i;
                int scanline = pms.width;

                for (y = 0; y < pms.height; y++)
                {
                    for (x = 0; x < pms.width; )
                    {
                        int a0 = address;
                        loc = y * scanline + x;
                        c0 = bytes[address++];
                        if (c0 <= 0xf7)
                        {
                            //literal
                            pic[loc] = (byte)c0; x++;
                        }
                        else if (c0 == 0xff)
                        {
                            //copy N+3 bytes from previous scanline
                            l = bytes[address] + 3; x += l; address++;
                            memcpy(pic, loc, pic, loc - scanline, l);
                        }
                        else if (c0 == 0xfe)
                        {
                            //copy N+3 bytes from two scanlines ago
                            l = bytes[address] + 3; x += l; address++;
                            memcpy(pic, loc, pic, loc - scanline * 2, l);
                        }
                        else if (c0 == 0xfd)
                        {
                            //fill with N+4 bytes of the next value
                            l = bytes[address] + 4; x += l; address++;
                            c0 = bytes[address++];
                            memset(pic, loc, (byte)c0, l);
                        }
                        else if (c0 == 0xfc)
                        {
                            //fill with alternating bytes N+3 times
                            l = (bytes[address] + 3) * 2; x += l; address++;
                            c0 = bytes[address++]; c1 = bytes[address++];
                            for (i = 0; i < l; i += 2)
                            {
                                pic[loc + i] = (byte)c0;
                                pic[loc + i + 1] = (byte)c1;
                            }
                        }
                        else
                        {
                            //copy byte
                            pic[loc] = bytes[address++]; x++;
                        }
                        if (x > pms.width)
                        {

                        }
                    }
                }
                return pic;
            }

            internal static ushort[] GetImageData16Bit(PmsHeader pmsHeader, byte[] bytes)
            {
                ushort[] pic = new ushort[pmsHeader.width * pmsHeader.height];
                int bytePosition = pmsHeader.addressOfData;

                int c0, c1, pc0, pc1;
                int x, y, i, l, loc;
                int scanline = pmsHeader.width;

                for (y = 0; y < pmsHeader.height; y++)
                {
                    for (x = 0; x < pmsHeader.width; )
                    {
                        loc = y * scanline + x;
                        c0 = bytes[bytePosition++];
                        if (c0 <= 0xf7)
                        {
                            c1 = bytes[bytePosition++]; x++;
                            pic[loc] = (ushort)(c0 | (c1 << 8));
                        }
                        else if (c0 == 0xff)
                        {
                            l = bytes[bytePosition] + 2; x += l; bytePosition++;
                            for (i = 0; i < l; i++)
                            {
                                pic[loc + i] = pic[loc + i - scanline];
                            }
                        }
                        else if (c0 == 0xfe)
                        {
                            l = bytes[bytePosition] + 2; x += l; bytePosition++;
                            for (i = 0; i < l; i++)
                            {
                                pic[loc + i] = pic[loc + i - scanline * 2];
                            }
                        }
                        else if (c0 == 0xfd)
                        {
                            l = bytes[bytePosition] + 3; x += l; bytePosition++;
                            c0 = bytes[bytePosition++]; c1 = bytes[bytePosition++];
                            pc0 = c0 | (c1 << 8);
                            for (i = 0; i < l; i++)
                            {
                                pic[loc + i] = (ushort)pc0;
                            }
                        }
                        else if (c0 == 0xfc)
                        {
                            l = (bytes[bytePosition] + 2) * 2; x += l; bytePosition++;
                            c0 = bytes[bytePosition++]; c1 = bytes[bytePosition++]; pc0 = c0 | (c1 << 8);
                            c0 = bytes[bytePosition++]; c1 = bytes[bytePosition++]; pc1 = c0 | (c1 << 8);
                            for (i = 0; i < l; i += 2)
                            {
                                pic[loc + i] = (ushort)pc0;
                                pic[loc + i + 1] = (ushort)pc1;
                            }
                        }
                        else if (c0 == 0xfb)
                        {
                            x++;
                            pic[loc] = pic[loc - scanline - 1];
                        }
                        else if (c0 == 0xfa)
                        {
                            x++;
                            pic[loc] = pic[loc - scanline + 1];
                        }
                        else if (c0 == 0xf9)
                        {
                            l = bytes[bytePosition] + 1; x += l; bytePosition++;
                            c0 = bytes[bytePosition++]; c1 = bytes[bytePosition++];
                            pc0 = ((c0 & 0xe0) << 8) + ((c0 & 0x18) << 6) + ((c0 & 0x07) << 2);
                            pc1 = ((c1 & 0xc0) << 5) + ((c1 & 0x3c) << 3) + (c1 & 0x03);
                            pic[loc] = (ushort)(pc0 + pc1);
                            for (i = 1; i < l; i++)
                            {
                                c1 = bytes[bytePosition++];
                                pc1 = ((c1 & 0xc0) << 5) + ((c1 & 0x3c) << 3) + (c1 & 0x03);
                                pic[loc + i] = (ushort)(pc0 | pc1);
                            }
                        }
                        else
                        {
                            c0 = bytes[bytePosition++]; c1 = bytes[bytePosition++]; x++;
                            pic[loc] = (ushort)(c0 | (c1 << 8));
                        }
                    }
                }
                return pic;
                //ushort[] pic = new ushort[pmsHeader.width * pmsHeader.height];
                //int c0, c1, pc0, pc1;
                //int x, y, i, l, loc;
                //int position = 0;
                //int scanline = pmsHeader.width;

                //for (y = 0; y < pmsHeader.height; y++)
                //{
                //    for (x = 0; x < pmsHeader.width; )
                //    {
                //        loc = y * scanline + x;
                //        c0 = bytes[position++];
                //        if (c0 <= 0xf7)
                //        {
                //            c1 = bytes[position++]; x++;
                //            pic[loc] = (ushort)(c0 | (c1 << 8));
                //        }
                //        else if (c0 == 0xff)
                //        {
                //            c1 = bytes[position++];
                //            //if (loc - scanline < 0)
                //            //{
                //            //    pic[loc] = (ushort)(c0 | (c1 << 8));
                //            //    x++;
                //            //}
                //            //else
                //            {
                //                l = c1 + 2; x += l; position++;
                //                for (i = 0; i < l; i++)
                //                {
                //                    pic[loc + i] = pic[loc + i - scanline];
                //                }
                //            }
                //        }
                //        else if (c0 == 0xfe)
                //        {
                //            l = bytes[position] + 2; x += l; position++;
                //            for (i = 0; i < l; i++)
                //            {
                //                pic[loc + i] = pic[loc + i - scanline * 2];
                //            }
                //        }
                //        else if (c0 == 0xfd)
                //        {
                //            l = bytes[position] + 3; x += l; position++;
                //            c0 = bytes[position++]; c1 = bytes[position++];
                //            pc0 = c0 | (c1 << 8);
                //            for (i = 0; i < l; i++)
                //            {
                //                pic[loc + i] = (ushort)pc0;
                //            }
                //        }
                //        else if (c0 == 0xfc)
                //        {
                //            l = (bytes[position] + 2) * 2; x += l; position++;
                //            c0 = bytes[position++]; c1 = bytes[position++]; pc0 = c0 | (c1 << 8);
                //            c0 = bytes[position++]; c1 = bytes[position++]; pc1 = c0 | (c1 << 8);
                //            for (i = 0; i < l; i += 2)
                //            {
                //                pic[loc + i] = (ushort)pc0;
                //                pic[loc + i + 1] = (ushort)pc1;
                //            }
                //        }
                //        else if (c0 == 0xfb)
                //        {
                //            x++;
                //            pic[loc] = pic[loc - scanline - 1];
                //        }
                //        else if (c0 == 0xfa)
                //        {
                //            x++;
                //            pic[loc] = pic[loc - scanline + 1];
                //        }
                //        else if (c0 == 0xf9)
                //        {
                //            l = bytes[position] + 1; x += l; position++;
                //            c0 = bytes[position++]; c1 = bytes[position++];
                //            pc0 = ((c0 & 0xe0) << 8) + ((c0 & 0x18) << 6) + ((c0 & 0x07) << 2);
                //            pc1 = ((c1 & 0xc0) << 5) + ((c1 & 0x3c) << 3) + (c1 & 0x03);
                //            pic[loc] = (ushort)(pc0 + pc1);
                //            for (i = 1; i < l; i++)
                //            {
                //                c1 = bytes[position++];
                //                pc1 = ((c1 & 0xc0) << 5) + ((c1 & 0x3c) << 3) + (c1 & 0x03);
                //                pic[loc + i] = (ushort)(pc0 | pc1);
                //            }
                //        }
                //        else
                //        {
                //            c0 = bytes[position++]; c1 = bytes[position++]; x++;
                //            pic[loc] = (ushort)(c0 | (c1 << 8));
                //        }
                //    }
                //}
                //return pic;
            }

            internal static void SaveImageData8Bit(Stream stream, FreeImageBitmap bitmap)
            {
                int width = bitmap.Width;
                int height = bitmap.Height;

                byte[] pic = new byte[width * height];

                int o = 0;
                for (int y = 0; y < height; y++)
                {
                    var scanline = bitmap.GetScanlineFromTop8Bit(y);
                    for (int x = 0; x < width; x++)
                    {
                        pic[o++] = scanline[x];
                    }
                }
                SaveImageData8Bit(stream, pic, width, height);
            }

            internal static void SaveImageData8Bit(Stream stream, byte[] pic, int width, int height)
            {
                int[] sizes = new int[4];

                for (int i = 0; i < pic.Length; i++)
                {
                    //find one of these: RLE run, previous scanline run, two scanlines ago run, alternating RLE run
                    int rleLength1 = MeasureRleRun(pic, i, width);
                    int rleLength2 = MeasureRleRun2(pic, i, width);
                    int prevScanlineLength = MeasurePreviousScanlineRun(pic, i, width, width);
                    int prevScanlineLength2 = MeasurePreviousScanlineRun(pic, i, width, width * 2);

                    if (rleLength1 < 4) rleLength1 = -1;
                    if (rleLength2 < 6) rleLength2 = -1;
                    if (prevScanlineLength < 3) prevScanlineLength = -1;
                    if (prevScanlineLength2 < 3) prevScanlineLength2 = -1;

                    sizes[0] = rleLength1;
                    sizes[1] = rleLength2;
                    sizes[2] = prevScanlineLength;
                    sizes[3] = prevScanlineLength2;

                    int maxIndex;
                    int max = sizes.Max(out maxIndex);

                    if (max < 3)
                    {
                        maxIndex = -1;
                    }

                    byte b = pic[i];

                    switch (maxIndex)
                    {
                        case 0:
                            {
                                stream.WriteByte(0xFD);
                                stream.WriteByte((byte)(max - 4));
                                stream.WriteByte(b);
                                i += max;
                            }
                            break;
                        case 1:
                            {
                                stream.WriteByte(0xFC);
                                stream.WriteByte((byte)((max - 6) / 2));
                                stream.WriteByte(b);
                                stream.WriteByte(pic[i + 1]);
                                i += max;
                            }
                            break;
                        case 2:
                            {
                                stream.WriteByte(0xFF);
                                stream.WriteByte((byte)(max - 3));
                                i += max;
                            }
                            break;
                        case 3:
                            {
                                stream.WriteByte(0xFE);
                                stream.WriteByte((byte)(max - 3));
                                i += max;
                            }
                            break;
                        default:
                            {
                                if (b <= 0xF7)
                                {
                                    stream.WriteByte(b);
                                }
                                else
                                {
                                    stream.WriteByte(0xF8);
                                    stream.WriteByte(b);
                                }
                                i++;
                            }
                            break;
                    }
                    i--;
                }
            }

            static int MeasureRleRun(byte[] pic, int i, int width)
            {
                int maxLength = 259;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
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

            static int MeasureRleRun2(byte[] pic, int i, int width)
            {
                int maxLength = 516;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
                }

                int i0 = i;
                if (i + 1 >= pic.Length) return 0;
                if (i + 2 == pic.Length) return 2;
                if (maxLength < 6) return 0;
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

            static int MeasurePreviousScanlineRun(byte[] pic, int i, int width, int offset)
            {
                int maxLength = 258;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
                }

                int i0 = i;
                if (i - offset < 0)
                {
                    return 0;
                }
                while (i < pic.Length && pic[i] == pic[i - offset])
                {
                    if (i - i0 >= maxLength) break;
                    i++;
                }
                return i - i0;
            }

            internal static void SaveImageData16Bit(Stream stream, ushort[] pic, int width, int height)
            {
                /*
                <F7 = raw byte
                FF: length = b+2, from previous scanline
                FE: length = b+2, from two scanlines ago
                FD: length = b+3, RLE run
                FC: length = (b+2)*2, alternating RLE run
                FB: one from previous scanline - 1
                FA: one from previous scanline + 1
                F9: length = b+1, 3 high bits for B, 2 high bits for G, 3 high bits for R,
                                 read 2 low bits for B, 4 low bits for G, 3 low bits for R
                F8: literal
                */

                int[] sizes = new int[4];

                //first pass: encode anything that isn't a run as F8
                MemoryStream ms = new MemoryStream();
                int i = 0;
                for (i = 0; i < pic.Length; i++)
                {
                    //find one of these: RLE run, previous scanline run, two scanlines ago run, alternating RLE run
                    int rleLength1 = MeasureRleRun(pic, i, width);
                    int rleLength2 = MeasureRleRun2(pic, i, width);
                    int prevScanlineLength = MeasurePreviousScanlineRun(pic, i, width, width);
                    int prevScanlineLength2 = MeasurePreviousScanlineRun(pic, i, width, width * 2);

                    if (rleLength1 < 3) rleLength1 = -1;
                    if (rleLength2 < 4) rleLength2 = -1;
                    if (prevScanlineLength < 2) prevScanlineLength = -1;
                    if (prevScanlineLength2 < 2) prevScanlineLength2 = -1;

                    sizes[0] = rleLength1;
                    sizes[1] = rleLength2;
                    sizes[2] = prevScanlineLength;
                    sizes[3] = prevScanlineLength2;

                    int maxIndex;
                    int max = sizes.Max(out maxIndex);

                    if (max < 3)
                    {
                        maxIndex = -1;
                    }

                    ushort b = pic[i];

                    switch (maxIndex)
                    {
                        case 0:
                            {
                                //RLE
                                ms.WriteByte(0xFD);
                                ms.WriteByte((byte)(max - 3));
                                //stream.WriteByte(b);
                                i += max;
                            }
                            break;
                        case 1:
                            {
                                //Alternating RLE
                                ms.WriteByte(0xFC);
                                ms.WriteByte((byte)((max - 4) / 2));
                                //stream.WriteByte(b);
                                //stream.WriteByte(pic[i + 1]);
                                i += max;
                            }
                            break;
                        case 2:
                            {
                                ms.WriteByte(0xFF);
                                ms.WriteByte((byte)(max - 2));
                                i += max;
                            }
                            break;
                        case 3:
                            {
                                ms.WriteByte(0xFE);
                                ms.WriteByte((byte)(max - 2));
                                i += max;
                            }
                            break;
                        default:
                            {
                                ms.WriteByte(0xF8);
                                i++;
                            }
                            break;
                    }
                    i--;
                }
                BinaryWriter bw = new BinaryWriter(stream);

                ////JUNK PASS: all literals
                /*
                for (i = 0; i < pic.Length; i++)
                {
                   bw.Write((byte)0xF8);
                   bw.Write((ushort)pic[i]);
                }
                return;
                */

                //pass #2
                ms.Position = 0;
                i = 0;
                while (ms.Position < ms.Length)
                {
                    int literalCount = 0;
                    int b = ms.PeekByte();
                    if (b == 0xF8)
                    {
                        while (true)
                        {
                            literalCount++;
                            b = ms.ReadByte();
                            b = ms.PeekByte();
                            if (b != 0xF8)
                            {
                                break;
                            }
                        }
                        //process a bunch of literals
                        int literalsRemaining = literalCount;
                        while (literalsRemaining > 0)
                        {
                            int commonCount = GetCommonCount(pic, i, width);
                            if (commonCount > literalsRemaining) commonCount = literalsRemaining;
                            if (commonCount >= 3)
                            {
                                bw.Write((byte)0xF9);
                                bw.Write((byte)(commonCount - 1));

                                //const int mask2 = 0x19E3;
                                int p, r, g;
                                p = pic[i];
                                b = (p >> 0) & 0x1F;
                                g = (p >> 5) & 0x3F;
                                r = (p >> 11) & 0x1F;

                                int highRGB = ((b >> 2) << 0) |
                                    ((g >> 4) << 3) |
                                    ((r >> 2) << 5);

                                bw.Write((byte)highRGB);

                                for (int c = 0; c < commonCount; c++)
                                {
                                    p = pic[i];
                                    b = (p >> 0) & 0x1F;
                                    g = (p >> 5) & 0x3F;
                                    r = (p >> 11) & 0x1F;
                                    int lowRGB = ((b & 0x03) << 0) |
                                        ((g & 0x0F) << 2) |
                                        ((r & 0x03) << 6);
                                    bw.Write((byte)lowRGB);

                                    i++;
                                }

                                literalsRemaining -= commonCount;
                            }
                            else
                            {
                                int p = pic[i];
                                int x = i % width;
                                if (x > 0 && i - width - 1 >= 0 && pic[i - width - 1] == p)
                                {
                                    bw.Write((byte)0xFB);
                                }
                                else if (x < width && i - width + 1 >= 0 && pic[i - width + 1] == p)
                                {
                                    bw.Write((byte)0xFA);
                                }
                                else
                                {
                                    if ((p & 0xFF) < 0xF8)
                                    {
                                        bw.Write((ushort)p);
                                    }
                                    else
                                    {
                                        bw.Write((byte)0xF8);
                                        bw.Write((ushort)p);
                                    }
                                }
                                i++;
                                literalsRemaining--;
                            }
                        }
                    }
                    else
                    {
                        b = ms.ReadByte();
                        //process the tag
                        int lengthByte = ms.ReadByte();
                        switch (b)
                        {
                            case 0xFD:
                                //RLE
                                bw.Write((byte)b);
                                bw.Write((byte)lengthByte);
                                bw.Write((ushort)pic[i]);
                                i += lengthByte + 3;
                                break;
                            case 0xFC:
                                //Alternating RLE
                                bw.Write((byte)b);
                                bw.Write((byte)lengthByte);
                                bw.Write((ushort)pic[i]);
                                bw.Write((ushort)pic[i + 1]);
                                i += lengthByte * 2 + 4;
                                break;
                            case 0xFF:
                                //From previous scanline
                                bw.Write((byte)b);
                                bw.Write((byte)lengthByte);
                                i += lengthByte + 2;
                                break;
                            case 0xFE:
                                //from two scanlines ago
                                bw.Write((byte)b);
                                bw.Write((byte)lengthByte);
                                i += lengthByte + 2;
                                break;
                        }
                    }
                }
            }

            static int MeasureRleRun(ushort[] pic, int i, int width)
            {
                int maxLength = 258;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
                }

                ushort b = pic[i];
                int i0 = i;
                i++;
                while (i < pic.Length && pic[i] == b)
                {
                    if (i - i0 >= maxLength) break;
                    i++;
                }
                return i - i0;
            }

            static int MeasureRleRun2(ushort[] pic, int i, int width)
            {
                int maxLength = 514;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
                }

                int i0 = i;
                if (i + 1 >= pic.Length) return 0;
                if (i + 2 == pic.Length) return 2;
                if (maxLength < 6) return 0;
                ushort b1 = pic[i];
                ushort b2 = pic[i + 1];
                i += 2;
                while (i + 1 < pic.Length && pic[i] == b1 && pic[i + 1] == b2)
                {
                    if (i - i0 + 1 >= maxLength) break;
                    i += 2;
                }
                return i - i0;
            }

            static int MeasurePreviousScanlineRun(ushort[] pic, int i, int width, int offset)
            {
                int maxLength = 257;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
                }

                int i0 = i;
                if (i - offset < 0)
                {
                    return 0;
                }
                while (i < pic.Length && pic[i] == pic[i - offset])
                {
                    if (i - i0 >= maxLength) break;
                    i++;
                }
                return i - i0;
            }

            private static int GetCommonCount(ushort[] pic, int i, int width)
            {
                const int mask = 0xE61C;
                int maxLength = 256;
                int x = i % width;
                if (maxLength > width - x)
                {
                    maxLength = width - x;
                }
                int i0 = i;

                int first = pic[i] & mask;
                while (i < pic.Length && ((pic[i] & mask) == first))
                {
                    if (i - i0 >= maxLength) break;
                    i++;
                }
                return i - i0;
            }


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
                var blue = image.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
                var red = image.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                if (alphaPixels != null)
                {
                    image.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                }
                image.SetChannel(blue, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                image.SetChannel(red, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);

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
                    var ms = new MemoryStream(inputBytes, qntHeader.headerSize, qntHeader.pixelSize);
                    //ms.Position = qntHeader.headerSize;

                    var rawMs = ZLibCompressor.DeCompress(ms);
                    var raw = rawMs.ToArray();
                    int endPosition = (int)ms.Position;
                    //int length = (int)ms.Position - qntHeader.headerSize;
                    //if (length != qntHeader.pixelSize + qntHeader.alphaSize)
                    //{

                    //}

                    int w = qntHeader.width;
                    int h = qntHeader.height;
                    if (raw.Length < w * h * 3)
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
                var ms = new MemoryStream(inputBytes);
                ms.Position = qntHeader.headerSize + qntHeader.pixelSize;

                var rawMs = ZLibCompressor.DeCompress(ms);
                var raw = rawMs.ToArray();

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

                MemoryStream ms = new MemoryStream(raw);
                var br = new BinaryReader(ms);
                return br.ReadBytes(j);

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

        static class Ajp
        {
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
                alpha = null;
                if (pmsFile != null)
                {
                    FreeImageBitmap pmsImage = Pms.LoadImage(pmsFile.ToArray());
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

                if (pmsFile != null)
                {
                    pmsFile.Position = 0;
                    FreeImageBitmap pmsImage = Pms.LoadImage(pmsFile.ToArray());
                    jpegImage.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                    jpegImage.SetChannel(pmsImage, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                    pmsImage.Dispose();
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
                {
                    return;
                }
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

        public static FreeImageBitmap LoadAjp(byte[] bytes)
        {
            try
            {
                return Ajp.LoadImage(bytes);
            }
            catch
            {
                return null;
            }
        }

        public static FreeImageBitmap LoadAjp(Stream stream)
        {
            try
            {
                return Ajp.LoadImage(stream);
            }
            catch
            {
                return null;
            }
        }

        public static AjpHeader LoadAjpHeader(Stream stream)
        {
            long oldPosition = stream.Position;
            var br = new BinaryReader(stream);
            var ajpHeader = Ajp.ReadAjpHeader(br);
            stream.Position = oldPosition;
            return ajpHeader;
        }

        public static void LoadAjp(byte[] bytes, out byte[] jpegFile, out FreeImageBitmap alpha, out AjpHeader ajpHeader)
        {
            Ajp.LoadImage(bytes, out jpegFile, out alpha, out ajpHeader);
        }

        public static void LoadAjp(Stream stream, out byte[] jpegFile, out FreeImageBitmap alpha, out AjpHeader ajpHeader)
        {
            Ajp.LoadImage(stream, out jpegFile, out alpha, out ajpHeader);
        }

        public static bool PaletteMatches(Palette palette1, Palette palette2, int startColor, int endColor)
        {
            for (int i = startColor; i < endColor; i++)
            {
                if ((palette1[i].uintValue & 0xFFFFFF) != (palette2[i].uintValue & 0xFFFFFF))
                {
                    return false;
                }
            }
            return true;
        }

        public static void RemapPalette(FreeImageBitmap bitmap, Palette newPalette)
        {
            if (bitmap.ColorDepth > 8)
            {
                bitmap.Quantize(FREE_IMAGE_QUANTIZE.FIQ_WUQUANT, 256, newPalette);
            }

            var sourcePal = bitmap.Palette.AsArray.Select(c => c.uintValue).ToArray();
            var destPal = newPalette.AsArray.Select(c => c.uintValue).ToArray();
            int[] srcToDest = new int[256];
            for (int i = 0; i < 256; i++)
            {
                srcToDest[i] = -1;
            }

            //first map identical colors
            {
                Dictionary<int, int> rgbToPaletteIndex = new Dictionary<int, int>();
                for (int i = 0; i < 256; i++)
                {
                    int c = (int)(destPal[i] & 0xFFFFFF);
                    if (!rgbToPaletteIndex.ContainsKey(c))
                    {
                        rgbToPaletteIndex.Add(c, i);
                    }
                }
                for (int i = 0; i < 256; i++)
                {
                    int c = (int)(sourcePal[i] & 0xFFFFFF);
                    if (rgbToPaletteIndex.ContainsKey(c))
                    {
                        srcToDest[i] = rgbToPaletteIndex[c];
                    }
                }
            }

            //map remaining colors
            {
                for (int i = 0; i < 256; i++)
                {
                    if (srcToDest[i] == -1)
                    {
                        int c = (int)(sourcePal[i] & 0xFFFFFF);
                        int minDistance = int.MaxValue;
                        int minIndex = -1;
                        for (int j = 0; j < 256; j++)
                        {
                            int c2 = (int)(destPal[j] & 0xFFFFFF);
                            int distance = GetDistance(c, c2);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                minIndex = j;
                            }
                        }
                        srcToDest[i] = minIndex;
                    }
                }
            }
            byte[] sequence = new byte[256];
            byte[] srcToDestByte = new byte[256];
            {
                for (int i = 0; i < 256; i++)
                {
                    sequence[i] = (byte)i;
                    srcToDestByte[i] = (byte)srcToDest[i];
                }
            }

            bitmap.ApplyPaletteIndexMapping(sequence, srcToDestByte, 256, false);
            bitmap.Palette.AsArray = newPalette.AsArray;
        }

        public static unsafe void RemapPalette(FreeImageBitmap bitmap, FreeImageBitmap referenceImage, int numberOfColors)
        {
            using (FreeImageBitmap bitmap32 = bitmap.GetColorConvertedInstance(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP))
            {
                if (referenceImage.Height < bitmap.Height || referenceImage.Width < bitmap.Width)
                {
                    int h2 = referenceImage.Height;
                    if (h2 < bitmap.Height)
                    {
                        h2 = bitmap.Height;
                    }
                    int w2 = referenceImage.Width;
                    if (w2 < bitmap.Width)
                    {
                        w2 = bitmap.Width;
                    }
                    referenceImage.EnlargeCanvas<byte>(0, 0, w2 - referenceImage.Width, h2 - referenceImage.Height, 0);
                }

                //FreeImageBitmap reference32 = bitmap.GetColorConvertedInstance(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);

                if (bitmap.ColorDepth != 8)
                {
                    bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP);
                }
                int* newPalette = (int*)(referenceImage.Palette.BaseAddress);

                int h = bitmap.Height;
                int w = bitmap.Width;

                //Ties are broken in ColorToIndex by popularity in the original image
                Dictionary<int, int> ColorToIndex = new Dictionary<int, int>();
                int[] ColorHistogram = new int[256];
                for (int y = 0; y < h; y++)
                {
                    byte* srcScanline = (byte*)(referenceImage.GetScanlineFromTop8Bit(y).BaseAddress);
                    for (int x = 0; x < w; x++)
                    {
                        int c = srcScanline[x];
                        ColorHistogram[c]++;
                    }
                }

                //add each palette color to the dictionary
                for (int i = 0; i < numberOfColors; i++)
                {
                    int c = newPalette[i] & 0xFFFFFF;
                    if (ColorToIndex.ContainsKey(c))
                    {
                        int otherIndex = ColorToIndex[c];
                        if (ColorHistogram[i] > ColorHistogram[otherIndex])
                        {
                            ColorToIndex[c] = i;
                        }
                    }
                    else
                    {
                        ColorToIndex[c] = i;
                    }
                }

                for (int y = 0; y < h; y++)
                {
                    int* trueColorScanline = (int*)(bitmap32.GetScanlineFromTop32Bit(y).BaseAddress);
                    byte* srcScanline = (byte*)(bitmap.GetScanlineFromTop8Bit(y).BaseAddress);
                    byte* referenceScanline = (byte*)(referenceImage.GetScanlineFromTop8Bit(y).BaseAddress);

                    for (int x = 0; x < w; x++)
                    {
                        int srcColor = trueColorScanline[x] & 0xFFFFFF;
                        int refIndex = referenceScanline[x];
                        int refColor = newPalette[refIndex] & 0xFFFFFF;
                        if (srcColor == refColor)
                        {
                            srcScanline[x] = (byte)refIndex;
                        }
                        else
                        {
                            if (ColorToIndex.ContainsKey(srcColor))
                            {
                                int newColorIndex = ColorToIndex[srcColor];
                                if (refColor == (newPalette[newColorIndex] & 0xFFFFFF))
                                {
                                    srcScanline[x] = (byte)refIndex;
                                }
                                else
                                {
                                    srcScanline[x] = (byte)newColorIndex;
                                }
                            }
                            else
                            {
                                float minDistance = float.MaxValue;
                                int minIndex = 0;
                                for (int i = 0; i < numberOfColors; i++)
                                {
                                    int c = newPalette[i] & 0xFFFFFF;
                                    float distance = GetDistanceF(srcColor, c);
                                    if (distance < minDistance)
                                    {
                                        minIndex = i;
                                        minDistance = distance;
                                    }
                                    else if (distance == minDistance && ColorHistogram[i] > ColorHistogram[minIndex])
                                    {
                                        minIndex = i;
                                    }
                                }
                                if (refColor == (newPalette[minIndex] & 0xFFFFFF))
                                {
                                    srcScanline[x] = (byte)refIndex;
                                }
                                else
                                {
                                    srcScanline[x] = (byte)minIndex;
                                }
                                ColorToIndex.Add(srcColor, minIndex);
                            }
                        }
                    }
                }
                bitmap.Palette.AsArray = referenceImage.Palette.AsArray;
            }
        }

        private static uint[] PaletteToArray(Palette newPalette)
        {
            var destPal = newPalette.AsArray.Select(c => c.uintValue).ToArray();
            return destPal;
        }

        static float GetDistanceF(int c1, int c2)
        {
            int r1 = c1 & 0xFF;
            int g1 = (c1 & 0xFF00) >> 8;
            int b1 = (c1 & 0xFF0000) >> 16;

            int r2 = c2 & 0xFF;
            int g2 = (c2 & 0xFF00) >> 8;
            int b2 = (c2 & 0xFF0000) >> 16;

            float Y1 = 0.299f * r1 + 0.587f * g1 + 0.114f * b1 + 0;
            float Cb1 = -0.169f * r1 - 0.331f * g1 + 0.499f * b1 + 128;
            float Cr1 = 0.499f * r1 - 0.418f * g1 - 0.0813f * b1 + 128;

            float Y2 = 0.299f * r2 + 0.587f * g2 + 0.114f * b2 + 0;
            float Cb2 = -0.169f * r2 - 0.331f * g2 + 0.499f * b2 + 128;
            float Cr2 = 0.499f * r2 - 0.418f * g2 - 0.0813f * b2 + 128;

            float yDistance = Y2 - Y1;
            float cbDistance = Cb1 - Cb2;
            float crDistance = Cr1 - Cr2;

            return yDistance * yDistance + cbDistance * cbDistance + crDistance * crDistance;
        }
        static int GetDistance(int c1, int c2)
        {
            int rDistance = ((c1 & 0xFF) - (c2 & 0xFF));
            int gDistance = (((c1 & 0xFF00) >> 8) - (c2 & 0xFF00) >> 8);
            int bDistance = (((c1 & 0xFF0000) >> 16) - (c2 & 0xFF0000) >> 16);
            return rDistance * rDistance + gDistance * gDistance + bDistance * bDistance;
        }

        public static void SaveAjp(Stream stream, FreeImageBitmap bitmap)
        {
            Ajp.SaveImage(stream, bitmap);
        }

        public static void SaveAjp(Stream stream, byte[] jpegFile, FreeImageBitmap alpha, AjpHeader ajpHeader)
        {
            Ajp.SaveImage(stream, jpegFile, alpha, ajpHeader);
        }
    }

    public static partial class Extensions
    {
        public static Scanline<byte> GetScanlineFromTop8Bit(this FreeImageBitmap bitmap, int y)
        {
            return bitmap.GetScanline<byte>(bitmap.Height - 1 - y);
        }
        public static Scanline<RGBTRIPLE> GetScanlineFromTop24Bit(this FreeImageBitmap bitmap, int y)
        {
            return bitmap.GetScanline<RGBTRIPLE>(bitmap.Height - 1 - y);
        }
        public static Scanline<int> GetScanlineFromTop32Bit(this FreeImageBitmap bitmap, int y)
        {
            return bitmap.GetScanline<int>(bitmap.Height - 1 - y);
        }
        public static Scanline<FI4BIT> GetScanlineFromTop4Bit(this FreeImageBitmap bitmap, int y)
        {
            return bitmap.GetScanline<FI4BIT>(bitmap.Height - 1 - y);
        }
    }

    public static class SwfToAffConverter
    {
        static byte[] xorKey = new byte[] { 0xC8, 0xBB, 0x8F, 0xB7, 0xED, 0x43, 0x99, 0x4A, 0xA2, 0x7E, 0x5B, 0xB0, 0x68, 0x18, 0xF8, 0x88, 0x53 };

        public static byte[] ConvertSwfToAff(byte[] swfBytes)
        {
            MemoryStream ms = new MemoryStream();
            ConvertSwfToAff(swfBytes, ms);
            return ms.ToArray();
        }

        public static void ConvertSwfToAff(byte[] swfBytes, Stream outputStream)
        {
            var bw = new BinaryWriter(outputStream);

            //"AFF\0", 1, filesize, 0x4D2

            bw.Write(ASCIIEncoding.ASCII.GetBytes("AFF"));
            bw.Write((byte)0);
            bw.Write((int)1);
            bw.Write((int)swfBytes.Length + 16);
            bw.Write((int)0x4D2);

            //screw around with first 0x40 bytes of SWF file
            int count = Math.Min(swfBytes.Length, 0x40);

            for (int i = 0; i < count; i++)
            {
                int i2 = i & 0x0F;
                swfBytes[i] ^= xorKey[i2];
            }

            bw.Write(swfBytes);
            bw.Flush();
        }

        public static void ConvertSwfToAff(string swfFileName, string affFileName)
        {
            var swfBytes = File.ReadAllBytes(swfFileName);

            using (FileStream fs = new FileStream(affFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                ConvertSwfToAff(swfBytes, fs);

                fs.Flush();
                fs.Close();
            }
        }

        public static byte[] ConvertAffToSwf(byte[] bytes)
        {
            return ConvertAffToSwf(new MemoryStream(bytes));
        }

        public static byte[] ConvertAffToSwf(Stream inputStream)
        {
            var br = new BinaryReader(inputStream);
            byte[] affHeader = br.ReadBytes(16);
            byte[] swfBytes = br.ReadBytes((int)br.BaseStream.Length - 16);
            int count = Math.Min(swfBytes.Length, 0x40);
            for (int i = 0; i < count; i++)
            {
                int i2 = i & 0x0F;
                swfBytes[i] ^= xorKey[i2];
            }
            return swfBytes;
        }

        public static void ConvertAffToSwf(string affFileName, string swfFileName)
        {
            //not yet tested
            using (FileStream fs = new FileStream(affFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var swfBytes = ConvertAffToSwf(fs);
                File.WriteAllBytes(swfFileName, swfBytes);
            }
        }
    }
}
