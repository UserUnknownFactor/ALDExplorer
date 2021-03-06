using System.Collections.Generic;
using System.IO;
using System.Text;
using HexDump;

namespace ALDExplorer
{
    using Node = AldFileSubimages.SubImageFinder.Node;

    static class FLAT
    {
        static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        public static byte RotByteL(byte v, int count)
        {
            count &= 7;
            return (byte)(v << count | v >> (8 - count));
        }

        public static T[] Slice<T>(this T[] arr, long indexFrom, long indexTo)
        {
            if (indexFrom > indexTo)
                throw new System.ArgumentOutOfRangeException("indexFrom is bigger than indexTo!");

            if (indexFrom > arr.Length - 1)
                return new T[0];

            if (indexTo > arr.Length - 1)
                indexTo = arr.Length - 1;

            long length = indexTo - indexFrom;
            T[] result = new T[length];
            System.Array.Copy(arr, indexFrom, result, 0, length);

            return result;
        }

        enum DataType
        {
            FLAG_CG = 2,
            FLAT_ZLIB = 5,
        };

        public static Node[] GetNodes(byte[] bytes, string parent_file, AldFileEntry parent)
        {
            List<Node> list = new List<Node>();
            using (var ms1 = new MemoryStream(bytes))
            using (var brfile = new BinaryReader(ms1))
                while (brfile.BaseStream.Position < brfile.BaseStream.Length)
                {
                    long tag_start_offset = brfile.BaseStream.Position;
                    var tag = (new Tag()).ReadTag(brfile);
                    if (tag.TagName == "LIBL" && tag.TagLength > 16)
                    {
                        var dataBytes = tag.TagData;
                        using (var ms = new MemoryStream(dataBytes))
                        using (var brtag = new BinaryReader(ms))
                        {
                            int fileCount = brtag.ReadInt32(); //9
                            int dataLength = 0;
                            string fileName = "alt_image.ajp";
                            for (int fileNumber = 0; fileNumber < fileCount; fileNumber++)
                            {
                                HexView.Debugger(dataBytes.Slice(brtag.BaseStream.Position, brtag.BaseStream.Position + 50));
                                uint fileNameLength = brtag.ReadUInt32(); //8
                                if (fileNameLength > 255) continue;

                                var fileNameBytes = brtag.ReadBytes((int)fileNameLength);
                                brtag.BaseStream.Position = ((brtag.BaseStream.Position - 1) | 3) + 1;

                                fileName = shiftJis.GetString(fileNameBytes);
                                if (parent_file != null)
                                    fileName = Path.GetFileNameWithoutExtension(parent_file) + 
                                        $"_image{fileNumber.ToString().PadLeft(1 + (int)System.Math.Log(fileCount, 10), '0')}";


                                int type = brtag.ReadInt32();
                                dataLength = brtag.ReadInt32();

                                long libl_img_off = brtag.BaseStream.Position + tag_start_offset + 8; // 8 is (tag + size)
                                HexView.Debugger(dataBytes.Slice(libl_img_off, libl_img_off + 20));

                                if (type == (int)DataType.FLAG_CG)
                                {
                                    int unknown1 = brtag.ReadInt32();  // XXX: special case: CG usually (not always!) has extra int32
                                    int maybe_head;
                                    if (unknown1 == 0x00544E51 || unknown1 == 0x00504A41) // QNT or AJP
                                    {
                                        maybe_head = unknown1;
                                    } 
                                    else
                                    {
                                        maybe_head = brtag.ReadInt32();
                                    }

                                    byte[] imageBytes = null;
                                    brtag.BaseStream.Position -= 4;
                                    if (maybe_head == 0x00544E51) //QNT needs the tag
                                    {
                                        libl_img_off -= 4;
                                        imageBytes = brtag.ReadBytes(dataLength);
                                        fileName += ".qnt";
                                    }
                                    else if (maybe_head == 0x00504A41) //AJP needs the tag
                                    {
                                        libl_img_off -= 4;
                                        imageBytes = brtag.ReadBytes(dataLength);
                                        fileName += ".ajp";
                                    }
                                    else { // unknown image
                                        //imageBytes = brtag.ReadBytes(dataLength);
                                        brtag.BaseStream.Position += dataLength;
                                        fileName += ".dat";
                                    }

                                    if (imageBytes != null)
                                        list.Add(new Node() { Bytes = imageBytes, FileName = fileName, Offset = libl_img_off, Parent = parent });
                                }
                                else
                                {
                                    brtag.BaseStream.Position += dataLength;
                                }
                                brtag.BaseStream.Position = ((brtag.BaseStream.Position - 1) | 3) + 1 - 4;
                            } // for
                        }
                    } // LIBL
                    else if (tag.TagName == "TALT" && tag.TagLength > 16)
                    {
                        var dataBytes = tag.TagData;
                        using (var ms = new MemoryStream(dataBytes))
                        using (var brtag = new BinaryReader(ms))
                        {
                            int fileCount = brtag.ReadInt32(); //9
                            int dataLength = 0;
                            string fileName = "alt_image.ajp";
                            for (int fileNumber = 0; fileNumber < fileCount; fileNumber++)
                            {
                                if (parent_file != null)
                                    fileName = Path.GetFileNameWithoutExtension(parent_file) +
                                        $"_alt_image{fileNumber.ToString().PadLeft(System.Math.Max(0, (int)System.Math.Log(10, fileCount)) + 1, '0')}";

                                dataLength = brtag.ReadInt32(); //9
                                //brtag.BaseStream.Position += dataLength;
                                brtag.BaseStream.Position = ((brtag.BaseStream.Position - 1) | 3) + 1;

                                int maybe_head = brtag.ReadInt32();  //
                                long libl_img_off = brtag.BaseStream.Position + tag_start_offset + 8;
                                HexView.Debugger(bytes.Slice(libl_img_off, libl_img_off + 20));

                                byte[] imageBytes = null;
                                brtag.BaseStream.Position -= 4;
                                if (maybe_head == 0x00544E51) //QNT needs the tag
                                {
                                    libl_img_off -= 4;
                                    imageBytes = brtag.ReadBytes(dataLength);
                                    fileName += ".qnt";
                                }
                                if (maybe_head == 0x00504A41) //AJP needs the tag
                                {
                                    libl_img_off -= 4;
                                    imageBytes = brtag.ReadBytes(dataLength);
                                    fileName += ".ajp";
                                }
                                else { // unknown image
                                    //imageBytes = brtag.ReadBytes(dataLength);
                                    brtag.BaseStream.Position += dataLength;
                                    fileName += ".dat";
                                }

                                if (imageBytes != null)
                                    list.Add(new Node() { Bytes = imageBytes, FileName = fileName, Offset = libl_img_off, Parent = parent });
                                brtag.BaseStream.Position = ((brtag.BaseStream.Position - 1) | 3) + 1 - 4;
                            } // for
                        }
                    } // TALT
                } // EndOfStream
            return list.ToArray();
        }

        static private Node FindNodeByNumber(Node[] nodes, int i, int total) {
            foreach (var n in nodes) {
                var expected = $"_image{i.ToString().PadLeft(1 + (int)System.Math.Log(total, 10), '0')}.";
                if (n.FileName.Contains(expected)) return n;
            }
            return null;
        }

        static public byte[] ReplaceNodes(byte[] bytes, Node[] nodes)
        {
            var ms1 = new MemoryStream(bytes);
            var br1 = new BinaryReader(ms1);

            var msOutput = new MemoryStream();
            var bw = new BinaryWriter(msOutput);
            while (br1.BaseStream.Position < br1.BaseStream.Length)
            {
                var tag = (new Tag()).ReadTag(br1);
                if (tag.TagName == "LIBL")
                {
                    bw.WriteStringFixedSize("LIBL", 4);
                    long tagLengthPos = bw.BaseStream.Position;
                    bw.Write((int)0);

                    var dataBytes = tag.TagData;
                    var ms = new MemoryStream(dataBytes);
                    var br = new BinaryReader(ms);
                    int fileCount = br.ReadInt32();
                    bw.Write((int)fileCount);
                    for (int fileNumber = 0; fileNumber < fileCount; fileNumber++)
                    {
                        int fileNameLength = br.ReadInt32();
                        var fileNameBytes = br.ReadBytes(fileNameLength);
                        //string fileName = shiftJis.GetString(fileNameBytes);
                        long lastPosition = br.BaseStream.Position;
                        br.BaseStream.Position = ((br.BaseStream.Position - 1) | 3) + 1;
                        int paddingCount1 = (int)(br.BaseStream.Position - lastPosition);

                        int type = br.ReadInt32();
                        int dataLength = br.ReadInt32();
                        
                        int maybe_head = 0;
                        if (type == (int)DataType.FLAG_CG)
                        {
                            int unknown1 = br.ReadInt32();  // XXX: special case: CG usually (not always!) has extra int32
                            if (unknown1 == 0x00544E51 || unknown1 == 0x00504A41) // QNT or AJP
                            {
                                maybe_head = unknown1;
                            } 
                            else
                            {
                                maybe_head = br.ReadInt32();
                                bw.Write(maybe_head);
                            }
                            br.BaseStream.Position -= 4;
                        }
                        
                        dataBytes = null;
                        if (maybe_head == 0x00544E51 || maybe_head == 0x00504A41) //QNT, AJP
                            br.BaseStream.Position += dataLength; // Don't care about previous content
                        else
                            dataBytes = br.ReadBytes(dataLength);

                        lastPosition = br.BaseStream.Position;
                        br.BaseStream.Position = ((br.BaseStream.Position - 1) | 3) + 1;
                        int paddingCount2 = (int)(br.BaseStream.Position - lastPosition);

                        var node = FindNodeByNumber(nodes, fileNumber, fileCount);
                        var newFileNameBytes = fileNameBytes;//shiftJis.GetBytes(node.FileName);

                        long outputPosition = bw.BaseStream.Position;

                        bw.Write((int)newFileNameBytes.Length);
                        bw.Write(newFileNameBytes);

                        int outputPadding1 = (int)(((((bw.BaseStream.Position - outputPosition) - 1) | 3) + 1) -
                            (bw.BaseStream.Position - outputPosition));
                        for (int i = 0; i < outputPadding1; i++)
                            bw.Write((byte)0);

                        bw.Write((int)type);
                        if (node != null) {
                            bw.Write((int)node.Bytes.Length);
                            bw.Write(node.Bytes);
                        } else if (dataBytes != null) {
                            bw.Write((int)dataBytes.Length);
                            bw.Write(dataBytes);
                        }

                        int outputPadding2 = (int)(((((bw.BaseStream.Position - outputPosition) - 1) | 3) + 1) - 
                            (bw.BaseStream.Position - outputPosition));
                        for (int i = 0; i < outputPadding2; i++)
                            bw.Write((byte)0);
                    }

                    long lastPos = bw.BaseStream.Position;
                    int tagLength = (int)(lastPos - (tagLengthPos + 4));
                    bw.BaseStream.Position = tagLengthPos;
                    bw.Write((int)tagLength);
                    bw.BaseStream.Position = lastPos;
                    
                    ms.Dispose();
                    br.Dispose();
                }
                else
                    tag.WriteTag(bw);
            }

            ms1.Dispose();
            br1.Dispose();

            var ret = msOutput.ToArray();

            bw.Dispose();
            msOutput.Dispose();

            return ret;
        }
    }
}