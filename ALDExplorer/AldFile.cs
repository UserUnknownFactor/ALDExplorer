using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using FreeImageAPI;
using System.Drawing;
using System.Windows.Forms;
using ZLibNet;

namespace ALDExplorer
{
    using Node = AldFileSubimages.SubImageFinder.Node;
    using SubImageFinder = AldFileSubimages.SubImageFinder;
    //using DDW.Swf;

    public class AldFileCollection
    {
        public string AldFileName
        {
            get
            {
                string firstFileName = knownFileName;
                var firstAldFile = this.AldFiles.FirstOrDefault();
                if (firstAldFile != null)
                {
                    firstFileName = firstAldFile.AldFileName;
                }
                return firstFileName;
            }
        }
        public AldFileType FileType
        {
            get
            {
                if (AldFiles != null && AldFiles.Count >= 1)
                {
                    return AldFiles.FirstOrDefault().FileType;
                }
                return AldFileType.Invalid;
            }
        }
        string knownFileName = "";
        public List<AldFile> AldFiles = new List<AldFile>();
        public List<AldFileEntry> FileEntries = new List<AldFileEntry>();
        public Dictionary<int, AldFileEntry> FileEntriesByNumber = new Dictionary<int, AldFileEntry>();

        public void Refresh()
        {
            this.FileEntries.Clear();
            foreach (var aldFile in this.AldFiles.ToArray())
            {
                int fileLetter = aldFile.FileLetter;
                for (int i = 0; i < aldFile.FileEntries.Count; i++)
                {
                    var entry = aldFile.FileEntries[i];
                    if (entry.FileLetter != fileLetter)
                    {
                        aldFile.FileEntries.RemoveAt(i);
                        i--;
                        var destAldFile = GetAldFileByLetter(entry.FileLetter);
                        destAldFile.FileEntries.Add(entry);
                    }
                }
            }

            UpdateIndexes();
            foreach (var aldFile in this.AldFiles)
            {
                this.FileEntries.AddRange(aldFile.FileEntries);
            }
        }

        public void UpdateIndexes()
        {
            this.FileEntriesByNumber.Clear();
            foreach (var aldFile in this.AldFiles)
            {
                for (int i = 0; i < aldFile.FileEntries.Count; i++)
                {
                    var entry = aldFile.FileEntries[i];
                    entry.Index = i;
                    FileEntriesByNumber[entry.FileNumber] = entry;
                }
            }
        }

        public void ReadFile(string firstAldFile)
        {
            this.knownFileName = firstAldFile;
            string extension = Path.GetExtension(firstAldFile).ToLowerInvariant();
            if (extension == ".ald")
            {
                string[] allFiles = AldFile.AldUtil.GetAldOtherFiles(firstAldFile).ToArray();
                foreach (var fileName in allFiles)
                {
                    var aldFile = new AldFile();
                    aldFile.ReadFile(fileName);
                    AldFiles.Add(aldFile);
                    //this.FileEntries.AddRange(aldFile.FileEntries);
                }
            }
            else if (extension == ".dat")
            {
                string[] allFiles = AldFile.AldUtil.GetDatOtherFiles(firstAldFile).ToArray();
                foreach (var fileName in allFiles)
                {
                    var aldFile = new AldFile();
                    aldFile.ReadFile(fileName);
                    AldFiles.Add(aldFile);
                    //this.FileEntries.AddRange(aldFile.FileEntries);
                }
            }
            else
            {
                var aldFile = new AldFile();
                aldFile.ReadFile(firstAldFile);
                AldFiles.Add(aldFile);
                //this.FileEntries.AddRange(aldFile.FileEntries);
            }
            ReadIndexBlock(AldFiles.FirstOrDefault().IndexBlock);

            Refresh();
        }

        private void ReadIndexBlock(byte[] tableData)
        {
            if (tableData == null || !(this.FileType == AldFileType.AldFile || this.FileType == AldFileType.DatFile))
            {
                return;
            }

            //clear all file numbers
            foreach (var aldFile in AldFiles)
            {
                foreach (var entry in aldFile.FileEntries)
                {
                    entry.FileNumber = 0;
                }
            }

            var tableSize = tableData.Length;
            if (this.FileType == AldFileType.AldFile)
            {
                int maxFileNumber = tableSize / 3;

                for (int rawFileNumber = 0; rawFileNumber < maxFileNumber; rawFileNumber++)
                {
                    int fileNumber = rawFileNumber + 1;
                    int entryFileLetter = tableData[rawFileNumber * 3 + 0];
                    int rawFileIndex = tableData[rawFileNumber * 3 + 1] + tableData[rawFileNumber * 3 + 2] * 256;
                    if (rawFileIndex != 0)
                    {
                        var aldFile = GetAldFileByLetter(entryFileLetter, false);
                        if (aldFile != null)
                        {
                            int aldFileIndex = rawFileIndex - 1;
                            if (aldFileIndex < aldFile.FileEntries.Count)
                            {
                                aldFile.FileEntries[aldFileIndex].FileNumber = fileNumber;
                            }
                        }
                    }
                }
            }
            else if (this.FileType == AldFileType.DatFile)
            {
                int maxFileNumber = tableSize / 2;

                for (int rawFileNumber = 0; rawFileNumber < maxFileNumber; rawFileNumber++)
                {
                    int fileNumber = rawFileNumber + 1;
                    int entryFileLetter = tableData[rawFileNumber * 2 + 0];
                    int rawFileIndex = tableData[rawFileNumber * 2 + 1];
                    if (rawFileIndex != 0)
                    {
                        var aldFile = GetAldFileByLetter(entryFileLetter, false);
                        if (aldFile != null)
                        {
                            int aldFileIndex = rawFileIndex - 1;
                            if (aldFileIndex < aldFile.FileEntries.Count)
                            {
                                aldFile.FileEntries[aldFileIndex].FileNumber = fileNumber;
                            }
                        }
                    }
                }
            }
        }

        public AldFile GetAldFileByLetter(int fileLetter)
        {
            return GetAldFileByLetter(fileLetter, true);
        }

        public AldFile GetAldFileByLetter(int fileLetter, bool create)
        {
            string firstFileName = this.AldFileName;
            if (String.IsNullOrEmpty(firstFileName))
            {
                //throw new InvalidOperationException();
            }

            var aldFile = this.AldFiles.Where(f => f.FileLetter == fileLetter).FirstOrDefault();
            if (aldFile == null && create)
            {
                aldFile = new AldFile();
                aldFile.FileType = this.FileType;
                aldFile.FileLetter = fileLetter;
                aldFile.AldFileName = GetAldFileName(firstFileName, fileLetter);
                this.AldFiles.Add(aldFile);
                this.AldFiles.Sort((f1, f2) => f1.FileLetter - f2.FileLetter);
            }
            return aldFile;
        }

        public string GetAldFileName(string fileName, int fileLetter)
        {
            var fileType = this.FileType;
            if (fileType == AldFileType.AldFile)
            {
                return AldFile.AldUtil.GetAldFileName(fileName, fileLetter);
            }
            else if (fileType == AldFileType.DatFile)
            {
                return AldFile.AldUtil.GetDatFileName(fileName, fileLetter);
            }
            return fileName;
        }

        public void SaveFile(string fileName)
        {
            SaveFile(fileName, false);
        }

        public void CreatePatch(int newNumberForA, int numberForZ)
        {
            var fileType = this.FileType;
            if (fileType == AldFileType.AldFile || fileType == AldFileType.DatFile)
            {
                bool renameA = false;
                AldFile aFile = null, mFile = null, zFile = null;
                aFile = GetAldFileByLetter(1, false);
                mFile = GetAldFileByLetter(newNumberForA, false);
                zFile = GetAldFileByLetter(numberForZ, true);

                if (mFile == null)
                {
                    if (aFile != null)
                    {
                        //move AFILE to MFILE
                        aFile.FileLetter = newNumberForA;
                        foreach (var entry in aFile.FileEntries)
                        {
                            entry.FileLetter = newNumberForA;
                        }
                        mFile = aFile;
                        aFile = null;
                        renameA = true;
                    }
                }
                if (aFile == null)
                {
                    aFile = GetAldFileByLetter(1);
                }
                SetPatchFileEntries(zFile);

                zFile.UpdateInformation();
                zFile.SaveTempFile();

                mFile.AldFileName = GetAldFileName(mFile.AldFileName, newNumberForA);
                aFile.AldFileName = GetAldFileName(aFile.AldFileName, 1);

                aFile.BuildIndexBlock(GetEntries());
                aFile.UpdateInformation();
                aFile.SaveTempFile();

                zFile.CommitTempFile();
                if (renameA)
                {
                    string mFileName = mFile.AldFileName;
                    string aFileName = aFile.AldFileName;
                    File.Move(aFileName, mFileName);
                }
                aFile.CommitTempFile();
            }
        }

        public bool CreatePatch2(string outputFileName)
        {
            //for AFA or ALK files, just output files that have changed.

            AldFile outputFile = new AldFile();
            outputFile.AldFileName = outputFileName;
            outputFile.FileType = this.FileType;
            var thisFile = this.AldFiles.FirstOrDefault();
            if (thisFile == null) return false;
            foreach (var entry in thisFile.FileEntries)
            {
                if (entry.HasReplacementData())
                {
                    outputFile.FileEntries.Add(entry.Clone());
                }
                else if (entry.HasSubImages() && entry.alreadyLookedForSubImages)
                {
                    foreach (var subentry in entry.GetSubImages())
                    {
                        if (subentry.HasReplacementData())
                        {
                            outputFile.FileEntries.Add(entry.Clone());
                            break;
                        }
                    }
                }
            }
            if (thisFile.FileEntries.Count == 0)
            {
                return false;
            }

            outputFile.SaveFileAndCommit();
            return true;
        }

        public void SaveFile(string fileName, bool createPatch)
        {
            if (createPatch)
            {
                CreatePatch(13, 26);
                return;
            }

            //foreach (var aldFile in this.AldFiles)
            //{
            //    //aldFile.AldFileName = AldFile.AldUtil.GetFileName(fileName, aldFile.FileLetter);
            //}

            AldFile aFile = this.AldFiles.FirstOrDefault();
            if (aFile.FileType == AldFileType.AldFile || aFile.FileType == AldFileType.DatFile)
            {
                aFile = GetAldFileByLetter(1);
            }
            foreach (var aldFile in this.AldFiles)
            {
                aldFile.UpdateInformation();
            }

            if (createPatch)
            {
                var patchFile = GetAldFileByLetter(24);
                patchFile.AldFileName = aFile.AldFileName;
                SetPatchFileEntries(patchFile);
                patchFile.UpdateInformation();
            }


            aFile.BuildIndexBlock(GetEntries());
            string[] newFileNames;
            string[] tempFileNames;
            GetOutputAndTempFilenames(fileName, aFile.FileType, out newFileNames, out tempFileNames);

            for (int i = 0; i < this.AldFiles.Count; i++)
            {
                var aldFile = this.AldFiles[i];
                string newFileName = newFileNames[i];
                string tempFile = tempFileNames[i];
                if (aldFile.FileType == AldFileType.DatFile)
                {
                    //build full index block for every file for DAT files
                    aldFile.BuildIndexBlock(GetEntries());
                }
                aldFile.SaveToFile(tempFile);
            }

            for (int i = 0; i < this.AldFiles.Count; i++)
            {
                var aldFile = this.AldFiles[i];
                string newFileName = newFileNames[i];
                string tempFile = tempFileNames[i];
                aldFile.CommitTempFile(newFileName, tempFile);
            }

            //else
            //{
            //    var atFile = GetOrAddNew(0);
            //    var patchFile = GetOrAddNew(25); //Y
            //    SetPatchFileEntries(patchFile);
            //    patchFile.UpdateInformation();
            //    patchFile.SaveFileAndCommit();
            //    this.FileEntries = GetEntries().ToList();
            //    atFile.BuildIndexBlock(FileEntries);
            //}
        }

        private void GetOutputAndTempFilenames(string fileName, AldFileType fileType, out string[] newFileNames, out string[] tempFileNames)
        {
            newFileNames = AldFiles.Select(f => GetAldFileName(fileName, f.FileLetter)).ToArray();
            //if (fileType == AldFileType.AldFile)
            //{
            //    newFileNames = AldFiles.Select(f => AldFile.AldUtil.GetAldFileName(fileName, f.FileLetter)).ToArray();
            //}
            //else if (fileType == AldFileType.DatFile)
            //{
            //    newFileNames = AldFiles.Select(f => AldFile.AldUtil.GetDatFileName(fileName, f.FileLetter)).ToArray();
            //}
            //else  // if (fileType == AldFileType.AlkFile)
            //{
            //    newFileNames = AldFiles.Select(f => fileName).ToArray();
            //}
            tempFileNames = newFileNames.Select(f => AldFile.GetTempFileName(f)).ToArray();
        }

        private IEnumerable<AldFileEntry> GetEntries()
        {
            foreach (var aldFile in this.AldFiles)
            {
                foreach (var entry in aldFile.FileEntries)
                {
                    yield return entry;
                }
            }
        }

        private void SetPatchFileEntries(AldFile patchFile)
        {
            Dictionary<int, AldFileEntry> knownFileEntries = new Dictionary<int, AldFileEntry>();
            foreach (var entry in patchFile.FileEntries)
            {
                knownFileEntries[entry.FileNumber] = entry;
            }
            foreach (var entry in this.FileEntries)
            {
                if (entry.HasReplacementData())
                {
                    int fileNumber = entry.FileNumber;
                    var entry2 = entry.Clone();
                    entry2.Parent = entry.Parent;
                    entry2.FileLetter = patchFile.FileLetter;
                    knownFileEntries[fileNumber] = entry2;
                }
            }
            //merge patchFile with knownFileEntries
            Dictionary<int, AldFileEntry> patchFileEntries = new Dictionary<int, AldFileEntry>();
            foreach (var entry in patchFile.FileEntries)
            {
                if (entry.FileNumber > 0)
                {
                    patchFileEntries[entry.FileNumber] = entry;
                }
            }
            foreach (var entryPair in knownFileEntries)
            {
                var entry = entryPair.Value;
                patchFileEntries[entry.FileNumber] = entry;
            }
            var newPatchEntries = patchFileEntries.Select(p => p.Value).OrderBy(e => e.FileNumber).ToArray();
            patchFile.FileEntries.Clear();
            patchFile.FileEntries.AddRange(newPatchEntries);

            //SetFileEntries(patchFile, knownFileEntries);
        }

        //private static void SetFileEntries(AldFile patchFile, Dictionary<int, AldFileEntry> knownFileEntries)
        //{
        //    var newPatchEntries = knownFileEntries.Select(p => p.Value).OrderBy(v => v.FileNumber);

        //    patchFile.FileEntries.Clear();
        //    patchFile.FileEntries.AddRange(newPatchEntries);
        //}

    }

    public class AldFile
    {
        //public bool IsDatFile
        //{
        //    get
        //    {
        //        return this.fileType == AldFileType.DatFile;
        //    }
        //}

        public AldFileType FileType
        {
            get
            {
                return fileType;
            }
            internal set
            {
                fileType = value;
            }
        }

        AldFileType fileType;

        public string AldFileName;
        public byte[] Footer;
        public byte[] IndexBlock;
        public AldFileEntryCollection FileEntries = new AldFileEntryCollection();
        public int FileLetter;

        public static bool AlwaysRemapImages;

        public void ReadFile(string fileName)
        {
            this.fileType = AldUtil.CheckFileType(fileName);
            this.AldFileName = fileName;

            long[] fileAddresses;
            byte[][] fileHeaders;
            string[] fileNames;
            long[] fileLengths;
            int[] fileNumbers;

            if (fileType == AldFileType.AldFile)
            {
                int fileLetter = AldUtil.GetAldFileLetter(fileName);
                this.FileLetter = fileLetter;

                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileAddresses = AldUtil.GetAldFileAddresses(fs);
                    fileHeaders = AldUtil.GetAldFileHeaders(fs, fileAddresses);
                    fileNames = AldUtil.GetAldFileNames(fileHeaders);
                    fileLengths = AldUtil.GetAldFileLengths(fileHeaders, fileAddresses, fs.Length);
                    IndexBlock = AldUtil.GetAldIndexBlock(fs);
                    fileNumbers = AldUtil.GetAldFileNumbers(fs, AldUtil.GetAldFileLetter(fileName));
                }
                this.Footer = null;

                this.FileEntries.Clear();

                for (int i = 0; i < fileAddresses.Length; i++)
                {
                    //ALD files generated by arc_conv are invalid and do not have the footer
                    if (i == fileAddresses.Length - 1 && fileHeaders[i] != null && fileHeaders[i].Length == 16)
                    {
                        this.Footer = fileHeaders[i];
                    }
                    else
                    {
                        var fileEntry = new AldFileEntry();
                        fileEntry.Parent = this;
                        fileEntry.FileAddress = fileAddresses[i] + ((fileHeaders[i] != null) ? (fileHeaders[i].Length) : (0));
                        fileEntry.FileHeader = fileHeaders[i];
                        fileEntry.FileName = fileNames[i];
                        fileEntry.FileNumber = fileNumbers.GetOrDefault(i, 0);
                        fileEntry.FileSize = fileLengths[i];
                        fileEntry.FileLetter = fileLetter;
                        fileEntry.HeaderAddress = fileAddresses[i];
                        fileEntry.Index = i;
                        FileEntries.Add(fileEntry);
                    }
                }
            }
            else if (fileType == AldFileType.DatFile)
            {
                int fileLetter = AldUtil.GetDatFileLetter(fileName);
                this.FileLetter = fileLetter;

                long fileSize;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileSize = fs.Length;
                    fileAddresses = AldUtil.GetDatFileAddresses(fs);
                    IndexBlock = AldUtil.GetDatIndexBlock(fs);
                    fileNumbers = AldUtil.GetDatFileNumbers(fs, AldUtil.GetDatFileLetter(fileName));
                }
                this.Footer = null;

                this.FileEntries.Clear();

                string filenameBase = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
                string filenameExtension = ".vsp";
                if (filenameBase.Length >= 2)
                {
                    filenameBase = filenameBase.Substring(1);
                }
                if (filenameBase.Length < 3)
                {
                    filenameBase = filenameBase.PadRight(3, '_');
                }
                else
                {
                    filenameBase = filenameBase.Substring(0, 3);
                }
                if (filenameBase == "dis")
                {
                    filenameExtension = ".sco";
                }
                else if (filenameBase == "mus")
                {
                    filenameExtension = ".mus";
                }
                else if (filenameBase == "map")
                {
                    filenameExtension = ".map";
                }

                for (int i = 0; i < fileAddresses.Length - 1; i++)
                {
                    var fileEntry = new AldFileEntry();
                    fileEntry.Parent = this;
                    fileEntry.FileAddress = fileAddresses[i];
                    fileEntry.FileHeader = null;
                    fileEntry.FileNumber = fileNumbers.GetOrDefault(i, 0);
                    //FIXME - find out actual file type



                    fileEntry.FileName = filenameBase + fileEntry.FileNumber.ToString("0000") + filenameExtension;

                    long nextFileAddress;

                    if (i + 1 < fileAddresses.Length)
                    {
                        nextFileAddress = fileAddresses[i + 1];
                    }
                    else
                    {
                        nextFileAddress = fileSize;
                    }
                    fileEntry.FileSize = nextFileAddress - fileAddresses[i];
                    fileEntry.FileLetter = fileLetter;
                    fileEntry.HeaderAddress = fileAddresses[i];
                    fileEntry.Index = i;
                    FileEntries.Add(fileEntry);
                }
            }
            else if (fileType == AldFileType.AlkFile)
            {
                this.FileLetter = 0;

                long[] fileSizes;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    BinaryReader br = new BinaryReader(fs);
                    fileAddresses = AldUtil.GetAlkFileAddresses(fs);
                    fileSizes = AldUtil.GetAlkFileSizes(fs);

                    this.Footer = null;
                    this.FileEntries.Clear();
                    for (int i = 0; i < fileAddresses.Length; i++)
                    {
                        var fileEntry = new AldFileEntry();
                        fileEntry.Parent = this;
                        fileEntry.FileAddress = fileAddresses[i];
                        fileEntry.FileSize = fileSizes[i];
                        string extension = ".bin";

                        fs.Position = fileEntry.FileAddress;
                        var filePeek = br.ReadBytes(16);
                        var ms = new MemoryStream(filePeek);
                        var br2 = new BinaryReader(ms);
                        do
                        {
                            ms.Position = 0;
                            if (br2.ReadStringFixedSize(2) == "PM")
                            {
                                extension = ".pms";
                                break;
                            }
                            ms.Position = 0;
                            if (br2.ReadStringFixedSize(3) == "QNT")
                            {
                                extension = ".qnt";
                                break;
                            }
                            ms.Position = 0;
                            if (br2.ReadStringFixedSize(3) == "AJP")
                            {
                                extension = ".ajp";
                                break;
                            }
                            ms.Position = 6;
                            if (br2.ReadStringFixedSize(4) == "JFIF")
                            {
                                extension = ".jpg";
                                break;
                            }
                            ms.Position = 0;
                            if (br2.ReadStringFixedSize(4) == "RIFF")
                            {
                                extension = ".wav";
                                break;
                            }
                            ms.Position = 0;
                            if (br2.ReadStringFixedSize(4) == "OggS")
                            {
                                extension = ".ogg";
                                break;
                            }
                            ms.Position = 0;
                            if (br2.ReadByte() == 0x89 && br2.ReadStringFixedSize(3) == "PNG")
                            {
                                extension = ".png";
                                break;
                            }
                        } while (false);


                        fileEntry.FileName = "FILE" + i.ToString("0000") + extension;
                        if (fileEntry.FileSize == 0)
                        {
                            fileEntry.FileName = "--";
                        }

                        fileEntry.FileNumber = i;
                        fileEntry.FileHeader = null;
                        fileEntry.HeaderAddress = fileAddresses[i];
                        fileEntry.Index = i;
                        FileEntries.Add(fileEntry);
                    }
                }
            }
            else if (fileType == AldFileType.AFA1File || fileType == AldFileType.AFA2File)
            {
                bool isVersion2 = fileType == AldFileType.AFA2File;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var entries = AldUtil.GetAfaFileEntries(fs, ref isVersion2);
                    int i = 0;
                    foreach (var entry in entries)
                    {
                        entry.Parent = this;
                        entry.Index = i;
                        entry.FileNumber = i;
                        entry.HeaderAddress = entry.FileAddress;
                        FileEntries.Add(entry);
                        i++;
                    }
                }
            }
            else
            {
                throw new InvalidDataException("ALD file is invalid");
            }
        }

        internal static class AldUtil
        {
            public static long[] GetAldFileAddresses(Stream fs)
            {
                long oldPosition = fs.Position;
                try
                {
                    long fsLength = fs.Length;
                    if (fsLength > uint.MaxValue || fsLength < 512)
                    {
                        //file size is wrong - over 4GB or under 512 bytes
                        return null;
                    }
                    long fileLength = fsLength;

                    var br = new BinaryReader(fs);

                    var first3Bytes = br.ReadBytes(3);
                    fs.Position = 0;
                    if (first3Bytes[2] > 0 || first3Bytes[1] > 3)
                    {
                        //limit of 65536 files, too many files, invalid ALD file.
                    }
                    int headerSize = (first3Bytes[0] << 8) | (first3Bytes[1] << 16) | (first3Bytes[2] << 24);
                    if (headerSize > 196611)
                    {
                        //limit of 65536 files, too many files, invalid ALD file.
                        return null;
                    }
                    if (headerSize > fileLength)
                    {
                        //invalid ALD file - header length is out of bounds
                        return null;
                    }
                    var header = br.ReadBytes(headerSize);
                    fs.Position = 0;

                    int filesLimit = headerSize / 3;
                    List<long> FileAddresses = new List<long>(filesLimit);
                    long lastAddress = 0;
                    bool sawZero = false;
                    for (int i = 1; i < filesLimit; i++)
                    {
                        long address = ((uint)header[i * 3 + 0] << 8) | ((uint)header[i * 3 + 1] << 16) | ((uint)header[i * 3 + 2] << 24);
                        if (address == 0)
                        {
                            sawZero = true;
                            continue;
                        }
                        if (address < lastAddress)
                        {
                            //invalid ALD file - file addresses are not strictly increasing
                            return null;
                        }
                        if (sawZero)
                        {
                            //invalid ALD file - contains nonzero values after a zero
                            return null;
                        }
                        if (address > fileLength)
                        {
                            //invalid ALD file - file address is out of bounds
                            return null;
                        }
                        lastAddress = address;
                        FileAddresses.Add(address);
                    }

                    if (FileAddresses.Count < 1)
                    {
                        //invalid ALD file - no files found inside
                        return null;
                    }
                    if (FileAddresses[0] - headerSize > 196611)
                    {
                        //invalid ALD file - file index table is too big
                        return null;
                    }

                    return FileAddresses.ToArray();
                }
                finally
                {
                    fs.Position = oldPosition;
                }
            }

            public static int[][] GetAldFileNumbers(string fileName)
            {
                List<int[]> fileNumbersList = new List<int[]>();
                string path = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName.Length >= 2)
                {
                    baseName = baseName.Substring(0, baseName.Length - 1);
                    for (int i = 0; i < 27; i++)
                    {
                        char c = (i == 0) ? '@' : ((char)('A' + i - i));
                        string aldFileName = Path.Combine(path, baseName + c + ".ald");
                        if (File.Exists(aldFileName))
                        {
                            using (var fs = File.OpenRead(aldFileName))
                            {
                                fileNumbersList.Add(GetAldFileNumbers(fs, i));
                            }
                        }
                        else
                        {
                            fileNumbersList.Add(new int[0]);
                        }
                    }
                }
                return fileNumbersList.ToArray();
            }

            public static byte[] GetAldIndexBlock(Stream fs)
            {
                int tableAddress = 0;
                return GetAldIndexBlock(fs, out tableAddress);
            }

            public static byte[] GetAldIndexBlock(Stream fs, out int tableAddress)
            {
                long oldPosition = fs.Position;
                try
                {
                    var br = new BinaryReader(fs);
                    var first3Bytes = br.ReadBytes(3);
                    var firstFileAddress3Bytes = br.ReadBytes(3);
                    tableAddress = (first3Bytes[0] << 8) | (first3Bytes[1] << 16) | (first3Bytes[2] << 24);
                    int firstFileAddress = (firstFileAddress3Bytes[0] << 8) | (firstFileAddress3Bytes[1] << 16) | (firstFileAddress3Bytes[2] << 24);
                    int tableSize = firstFileAddress - tableAddress;
                    fs.Position = tableAddress;

                    List<int> fileNumbersList = new List<int>();

                    var tableData = br.ReadBytes(tableSize);
                    return tableData;
                }
                finally
                {
                    fs.Position = oldPosition;
                }
            }

            public static int[] GetAldFileNumbers(Stream fs, int fileLetter)
            {
                int tableAddress;
                var tableData = GetAldIndexBlock(fs, out tableAddress);
                int tableSize = tableData.Length;

                //long oldPosition = fs.Position;
                //var br = new BinaryReader(fs);
                //var first3Bytes = br.ReadBytes(3);
                //var firstFileAddress3Bytes = br.ReadBytes(3);
                //int tableAddress = (first3Bytes[0] << 8) | (first3Bytes[1] << 16) | (first3Bytes[2] << 24);
                //int firstFileAddress = (firstFileAddress3Bytes[0] << 8) | (firstFileAddress3Bytes[1] << 16) | (firstFileAddress3Bytes[2] << 24);
                //int tableSize = firstFileAddress - tableAddress;
                //fs.Position = tableAddress;

                List<int> fileNumbersList = new List<int>();

                //var tableData = br.ReadBytes(tableSize);
                int maxFileIndex = tableAddress / 3 - 1;
                int maxFileNumber = tableSize / 3;

                for (int fileNumberIndex = 0; fileNumberIndex < maxFileNumber; fileNumberIndex++)
                {
                    int fileNumber = fileNumberIndex + 1;
                    int entryFileLetter = tableData[fileNumberIndex * 3 + 0];
                    int fileIndex = tableData[fileNumberIndex * 3 + 1] + tableData[fileNumberIndex * 3 + 2] * 256;
                    if (fileIndex != 0)
                    {
                        if (fileIndex < maxFileIndex && entryFileLetter == fileLetter)
                        {
                            fileNumbersList.SetOrAdd(fileIndex - 1, fileNumber);
                        }
                    }
                }

                //fs.Position = oldPosition;
                return fileNumbersList.ToArray();
            }

            public static byte[][] GetAldFileHeaders(Stream fs, long[] fileAddresses)
            {
                long oldPosition = fs.Position;
                var br = new BinaryReader(fs);
                List<byte[]> headers = new List<byte[]>(fileAddresses.Length);
                for (int i = 0; i < fileAddresses.Length; i++)
                {
                    long address = fileAddresses[i];
                    fs.Position = address;
                    long headerLength = br.ReadUInt32();
                    if (headerLength != 0x20)
                    {
                        if (headerLength < 16 || headerLength > 256)
                        {
                            headerLength = -1;
                        }
                        long remainingBytes = fs.Length - address;
                        if (remainingBytes == 16 && headerLength == -1)
                        {
                            headerLength = remainingBytes;
                        }
                        if (headerLength > remainingBytes)
                        {
                            headerLength = remainingBytes;
                        }
                    }
                    fs.Position = address;
                    byte[] header;
                    if (headerLength != -1)
                    {
                        header = br.ReadBytes((int)headerLength);
                    }
                    else
                    {
                        header = null;
                    }
                    headers.Add(header);
                }
                fs.Position = oldPosition;
                return headers.ToArray();
            }

            static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

            public static string[] GetAldFileNames(byte[][] fileHeaders)
            {
                List<string> fileNames = new List<string>(fileHeaders.Length);
                for (int i = 0; i < fileHeaders.Length; i++)
                {
                    byte[] header = fileHeaders[i];
                    string fileName;
                    if (header == null)
                    {
                        fileName = null;
                    }
                    else if (header.Length < 32)
                    {
                        fileName = "";
                    }
                    else
                    {
                        int nameLength = 0;
                        int maxNameLength = header.Length - 16;
                        for (nameLength = 0; nameLength < maxNameLength; nameLength++)
                        {
                            if (header[16 + nameLength] == 0)
                            {
                                break;
                            }
                        }

                        fileName = shiftJis.GetString(header, 16, nameLength);
                        try
                        {
                            string dummy = Path.GetExtension(fileName);
                        }
                        catch (ArgumentException ex)
                        {
                            fileName = null;
                        }
                    }
                    fileNames.Add(fileName);
                }
                return fileNames.ToArray();
            }

            public static long[] GetAldFileLengths(byte[][] fileHeaders, long[] fileAddresses, long totalSize)
            {
                List<long> fileLengths = new List<long>(fileHeaders.Length);
                for (int i = 0; i < fileHeaders.Length; i++)
                {
                    long fileAddress = fileAddresses.GetOrNull(i);
                    long nextAddress = fileAddresses.GetOrNull(i + 1);
                    if (nextAddress == 0)
                    {
                        nextAddress = totalSize;
                    }
                    long physicalSize = nextAddress - fileAddress;
                    if (physicalSize < 0)
                    {

                    }

                    byte[] header = fileHeaders[i];
                    long length = 0;
                    if (header != null && header.Length >= 4)
                    {
                        length = BitConverter.ToUInt32(header, 4);
                        if (length > physicalSize || length < 0)
                        {
                            length = physicalSize - header.Length;
                        }
                    }
                    else
                    {
                        length = physicalSize;
                    }
                    fileLengths.Add(length);
                }
                return fileLengths.ToArray();
            }

            public static int GetDatFileLetter(string fileName)
            {
                if (fileName == null)
                {
                    return 1;
                }
                string baseName = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
                if (baseName.Length > 1)
                {
                    char c = baseName[0];
                    if (c == '@') return 0;
                    if (c >= 'A' && c <= 'Z') return (c - 'A' + 1);
                    return 1;
                }
                else
                {
                    return 1;
                }
            }

            public static bool DatFileHasLetter(string fileName)
            {
                if (fileName == null)
                {
                    return false;
                }
                string baseName = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
                if (baseName.Length > 1)
                {
                    char c = baseName[0];
                    if (c == '@') return true;
                    if (c >= 'A' && c <= 'Z') return true;
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public static int GetAldFileLetter(string fileName)
            {
                if (fileName == null)
                {
                    return 1;
                }
                string baseName = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
                if (baseName.Length > 2)
                {
                    baseName = baseName.Substring(baseName.Length - 2);
                    char c = baseName[1];
                    if (c == '@') return 0;
                    if (c >= 'A' && c <= 'Z') return (c - 'A' + 1);
                    return 1;
                }
                else
                {
                    return 1;
                }
            }

            public static bool AldFileHasLetter(string fileName)
            {
                if (fileName == null)
                {
                    return false;
                }
                string baseName = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
                if (baseName.Length > 2)
                {
                    baseName = baseName.Substring(baseName.Length - 2);
                    char c = baseName[1];
                    if (c == '@') return true;
                    if (c >= 'A' && c <= 'Z') return true;
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public static string GetAldFileName(string fileName, int fileLetter)
            {
                if (String.IsNullOrEmpty(fileName))
                {
                    char c = (fileLetter == 0) ? '@' : ((char)('A' + fileLetter - 1));
                    return c + ".ald";
                }
                string path = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName.Length >= 2)
                {
                    baseName = baseName.Substring(0, baseName.Length - 1);
                    char c = (fileLetter == 0) ? '@' : ((char)('A' + fileLetter - 1));
                    return Path.Combine(path, baseName + c + ".ald");
                }
                return fileName;
            }

            public static string GetDatFileName(string fileName, int fileLetter)
            {
                string path = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName.Length >= 2)
                {
                    baseName = baseName.Substring(1);
                    char c = (fileLetter == 0) ? '@' : ((char)('A' + fileLetter - 1));
                    return Path.Combine(path, c + baseName + ".dat");
                }
                return fileName;
            }

            public static IEnumerable<string> GetAldOtherFiles(string fileName)
            {
                string path = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName.Length >= 2)
                {
                    baseName = baseName.Substring(0, baseName.Length - 1);
                    for (int i = 0; i < 27; i++)
                    {
                        char c = (i == 0) ? '@' : ((char)('A' + i - 1));
                        string aldFileName = Path.Combine(path, baseName + c + ".ald");
                        if (File.Exists(aldFileName))
                        {
                            yield return aldFileName;
                        }
                    }
                }
            }

            public static AldFileType CheckFileType(string fileName)
            {
                using (var fs = File.OpenRead(fileName))
                {
                    var aldFileAddresses = AldUtil.GetAldFileAddresses(fs);
                    if (aldFileAddresses != null)
                    {
                        return AldFileType.AldFile;
                    }

                    var datFileAddresses = AldUtil.GetDatFileAddresses(fs);
                    if (datFileAddresses != null)
                    {
                        return AldFileType.DatFile;
                    }

                    var alkFileAddresses = AldUtil.GetAlkFileAddresses(fs);
                    if (alkFileAddresses != null)
                    {
                        return AldFileType.AlkFile;
                    }

                    bool isVersion2 = false;
                    var afa1FileEntries = AldUtil.GetAfaFileEntries(fs, ref isVersion2);
                    if (afa1FileEntries != null)
                    {
                        if (!isVersion2)
                        {
                            return AldFileType.AFA1File;
                        }
                        else
                        {
                            return AldFileType.AFA2File;
                        }
                    }
                }
                return AldFileType.Invalid;
            }

            public static AldFileEntry[] GetAfaFileEntries(FileStream fs, ref bool isVersion2)
            {
                isVersion2 = false;

                long oldPosition = fs.Position;
                try
                {
                    byte[] afahSignature;
                    int headerLength;
                    byte[] alicArchSignature;
                    int version, unknown3;
                    long dataBase;

                    byte[] infoTagSignature;
                    int infoTagSize;
                    int tocLengthCompressed, tocLengthDecompressed, entryCount;

                    byte[] dataSignature;
                    long dataLength;

                    long fileLength = fs.Length;

                    if (fileLength < 64 || fileLength > uint.MaxValue)
                    {
                        return null;
                    }

                    var br = new BinaryReader(fs);
                    br.BaseStream.Position = 0;

                    afahSignature = br.ReadBytes(4);
                    headerLength = br.ReadInt32();
                    alicArchSignature = br.ReadBytes(8);

                    if (ASCIIEncoding.ASCII.GetString(afahSignature) != "AFAH")
                    {
                        return null;
                    }
                    if (ASCIIEncoding.ASCII.GetString(alicArchSignature) != "AlicArch")
                    {
                        return null;
                    }
                    if (headerLength != 0x1C)
                    {
                        return null;
                    }
                    version = br.ReadInt32();
                    unknown3 = br.ReadInt32();
                    dataBase = br.ReadUInt32();

                    if (version != 1 || unknown3 != 1)
                    {
                        //these are usually 1, don't really care though
                    }

                    if (dataBase + 8 > fileLength)
                    {
                        return null;
                    }

                    infoTagSignature = br.ReadBytes(4);
                    infoTagSize = br.ReadInt32();
                    tocLengthCompressed = infoTagSize - 16;
                    tocLengthDecompressed = br.ReadInt32();
                    entryCount = br.ReadInt32();

                    if (ASCIIEncoding.ASCII.GetString(infoTagSignature) != "INFO")
                    {
                        return null;
                    }

                    if (tocLengthCompressed < 0)
                    {
                        return null;
                    }
                    if (infoTagSize + headerLength + 4 > dataBase)
                    {
                        return null;
                    }
                    if (tocLengthDecompressed < 0)
                    {
                        return null;
                    }
                    if (entryCount < 0)
                    {
                        return null;
                    }
                    //minimum entry length is 6.5 words long
                    if (tocLengthDecompressed < entryCount * (6 * 4 + 2))
                    {
                        return null;
                    }

                    long pos2 = br.BaseStream.Position;
                    //validate the DATA tag
                    br.BaseStream.Position = dataBase;
                    dataSignature = br.ReadBytes(4);
                    if (ASCIIEncoding.ASCII.GetString(dataSignature) != "DATA")
                    {
                        return null;
                    }

                    dataLength = br.ReadUInt32();
                    if (dataLength + dataBase > fileLength)
                    {
                        return null;
                    }
                    if (dataLength + dataBase != fileLength)
                    {
                        //usually it matches the file size exactly, but if it doesn't?
                    }

                    br.BaseStream.Position = pos2;

                    byte[] compressedToc = br.ReadBytes(tocLengthCompressed);
                    byte[] decompressedToc = Decompress(compressedToc, tocLengthDecompressed);
                    if (decompressedToc == null)
                    {
                        return null;
                    }

                    //try version 1 and version 2 file
                    AldFileEntry[] entries = null;
                    if (version != 2)
                    {
                        //try version 1 first
                        entries = GetAfaFileEntries(decompressedToc, dataBase, entryCount, fileLength, false);
                        if (entries != null)
                        {
                            return entries;
                        }
                        entries = GetAfaFileEntries(decompressedToc, dataBase, entryCount, fileLength, true);
                        if (entries != null)
                        {
                            isVersion2 = true;
                            return entries;
                        }
                    }
                    else
                    {
                        //try version 2 first
                        entries = GetAfaFileEntries(decompressedToc, dataBase, entryCount, fileLength, true);
                        if (entries != null)
                        {
                            isVersion2 = true;
                            return entries;
                        }
                        entries = GetAfaFileEntries(decompressedToc, dataBase, entryCount, fileLength, false);
                        if (entries != null)
                        {
                            return entries;
                        }
                    }
                    return null;
                }
                finally
                {
                    fs.Position = oldPosition;
                }
            }

            static Encoding shiftJisWithThrow = Encoding.GetEncoding("shift-jis", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

            private static AldFileEntry[] GetAfaFileEntries(byte[] decompressedToc, long dataBase, int entryCount, long fileSize, bool isVersion2)
            {
                try
                {
                    List<AldFileEntry> list = new List<AldFileEntry>();
                    var ms = new MemoryStream(decompressedToc);
                    var br = new BinaryReader(ms);
                    int index = 0;
                    while (ms.Position < ms.Length)
                    {
                        int fileNameLength = br.ReadInt32();
                        int fileNameLengthPadded = br.ReadInt32();
                        if (fileNameLength <= 0 || fileNameLength > 512)
                        {
                            return null;
                        }
                        if (fileNameLengthPadded <= 0 || fileNameLengthPadded > 512)
                        {
                            return null;
                        }
                        if (fileNameLength > fileNameLengthPadded)
                        {
                            return null;
                        }
                        var fileNameBytes = br.ReadBytes(fileNameLengthPadded);
                        //if file name bytes contains any nulls, invalid filename
                        if (fileNameBytes.Take(fileNameLength).AnyEqualTo((byte)0))
                        {
                            return null;
                        }
                        string fileName = shiftJisWithThrow.GetString(fileNameBytes, 0, fileNameLength);
                        int unknown1, unknown2, unknown3 = 0;
                        long offset, length;

                        unknown1 = br.ReadInt32();
                        unknown2 = br.ReadInt32();
                        if (!isVersion2)
                        {
                            unknown3 = br.ReadInt32();
                        }

                        offset = br.ReadUInt32();
                        length = br.ReadUInt32();

                        if (offset + dataBase + length > fileSize)
                        {
                            return null;
                        }

                        AldFileEntry entry = new AldFileEntry();
                        entry.FileAddress = offset + dataBase;
                        entry.FileSize = length;
                        entry.FileName = fileName;
                        entry.Index = index;
                        entry.FileNumber = -1;
                        entry.FileHeader = null;
                        entry.FileLetter = 0;
                        entry.HeaderAddress = -1;
                        entry.ReplacementFileName = null;
                        entry.ReplacementBytes = null;
                        entry.Parent = null;

                        list.Add(entry);
                        index++;
                    }
                    if (list.Count != entryCount)
                    {
                        return null;
                    }
                    return list.ToArray();
                }
                catch
                {
                    return null;
                }
            }

            public static void WriteAfaHeader(FileStream fs, IList<AldFileEntry> aldFiles, long dataBase, bool isVersion2)
            {
                //this is called after the AldFiles have been updated to the correct data
                BinaryWriter bw = new BinaryWriter(fs);
                bw.WriteStringFixedSize("AFAH", 4);
                bw.Write((int)0x1C);
                bw.WriteStringFixedSize("AlicArch", 8);
                bw.Write((int)(isVersion2 ? 2 : 1)); //unknown2
                bw.Write((int)1); //unknown3
                bw.Write((uint)dataBase);

                bw.WriteStringFixedSize("INFO", 4);
                var tocData = CreateAfaToc(aldFiles, dataBase, isVersion2);
                var compressedTocData = Compress(tocData);
                //var test1 = Decompress(compressedTocData.Take(compressedTocData.Length - 4).ToArray(), tocData.Length);
                //var test2 = Decompress(compressedTocData.Take(compressedTocData.Length - 5).ToArray(), tocData.Length);
                //var test3 = Decompress(compressedTocData.Take(compressedTocData.Length - 6).ToArray(), tocData.Length);
                //var test4 = Decompress(compressedTocData.Take(compressedTocData.Length - 7).ToArray(), tocData.Length);
                //var test5 = Decompress(compressedTocData.Take(compressedTocData.Length - 8).ToArray(), tocData.Length);
                //var test6 = Decompress(compressedTocData.Take(compressedTocData.Length - 20).ToArray(), tocData.Length);
                //var test7 = Decompress(compressedTocData.Take(compressedTocData.Length - 24).ToArray(), tocData.Length);



                int tocLengthCompressed = compressedTocData.Length;
                int tocLengthDecompressed = tocData.Length;
                int entryCount = aldFiles.Count;
                int infoTagSize = tocLengthCompressed + 16;
                bw.Write((int)infoTagSize);
                bw.Write((int)tocLengthDecompressed);
                bw.Write((int)entryCount);
                bw.Write(compressedTocData);

                long dummyLength = (dataBase - bw.BaseStream.Position);
                if (dummyLength >= 8)
                {
                    bw.WriteStringFixedSize("DUMM", 4); //arc_conv writes a DUMM tag to pad the header
                    bw.Write((uint)dummyLength);
                    bw.BaseStream.WriteZeroes(dummyLength - 8);
                }
                else if (dummyLength < 8 && dummyLength >= 0)
                {
                    bw.BaseStream.WriteZeroes(dummyLength);
                }
                else
                {
                    //should not happen
                }
                bw.WriteStringFixedSize("DATA", 4);
                long dataSize = bw.BaseStream.Length - dataBase;
                bw.Write((uint)dataSize);
            }


            public static int EstimateAfaHeaderSize(IList<AldFileEntry> aldFiles, bool isVersion2)
            {
                int headerSize = 0x1C + 0x10;
                int minEntrySize = 6 * 4 + (isVersion2 ? 0 : 4);
                int textLength = aldFiles.Sum(f => shiftJis.GetByteCount(f.FileName) + 1);
                return headerSize + minEntrySize * aldFiles.Count + textLength;
            }

            //public static byte[] CreateAfaHeader(IList<AldFileEntry> aldFiles, bool isVersion2)
            //{
            //    throw new NotImplementedException();
            //    MemoryStream ms = new MemoryStream();
            //    BinaryWriter bw = new BinaryWriter(ms);

            //    byte[] tocBytes = CreateAfaToc(aldFiles, isVersion2);




            //}

            private static byte[] CreateAfaToc(IList<AldFileEntry> aldFiles, long dataBase, bool isVersion2)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                foreach (var entry in aldFiles)
                {
                    byte[] fileNameBytes = shiftJis.GetBytes(entry.FileName);
                    int fileNameLengthPadded = (fileNameBytes.Length) | 3 + 1;

                    bw.Write((int)fileNameBytes.Length);
                    bw.Write((int)(fileNameLengthPadded));
                    bw.Write(fileNameBytes);
                    int paddingByteCount = fileNameLengthPadded - fileNameBytes.Length;
                    for (int i = 0; i < paddingByteCount; i++)
                    {
                        bw.Write((byte)0);
                    }
                    bw.Write((int)0x2d99e180); //unknown1  0x2d99e180
                    bw.Write((int)0x01cf5475); //unknown2  0x01cf5475
                    if (!isVersion2)
                    {
                        bw.Write((int)0); //unknown3
                    }
                    bw.Write((uint)(entry.FileAddress - dataBase));
                    bw.Write((uint)(entry.FileSize));
                }
                return ms.ToArray();
            }

            private static byte[] Decompress(byte[] compressedData, int decompressedLength)
            {
                var ms = new MemoryStream(compressedData);
                var zlibStream = new ZLibStream(ms, CompressionMode.Decompress);
                try
                {
                    var br = new BinaryReader(zlibStream);
                    var bytes = br.ReadBytes(decompressedLength);
                    return bytes;
                }
                catch
                {
                    return null;
                }
            }

            private static byte[] Compress(byte[] uncompressedData)
            {
                var ms = new MemoryStream();
                var zlibStream = new ZLibStream(ms, CompressionMode.Compress, CompressionLevel.Level9);
                try
                {
                    var bw = new BinaryWriter(zlibStream);
                    bw.Write(uncompressedData);
                    zlibStream.Flush();
                    zlibStream.Close();
                    return ms.ToArray();
                }
                catch
                {
                    return null;
                }
            }

            public static long[] GetAlkFileAddresses(Stream fs)
            {
                long oldPosition = fs.Position;
                try
                {
                    long fsLength = fs.Length;
                    long fileLength = fsLength;

                    var br = new BinaryReader(fs);

                    string signature = br.ReadStringFixedSize(4);
                    if (signature != "ALK0") return null;

                    int fileCount = br.ReadInt32();
                    if (fileCount < 0 || fileCount * 8 > fileLength)
                    {
                        return null;
                    }

                    int headerSize = fileCount * 8;
                    if (headerSize > fileLength)
                    {
                        //invalid DAT file - header size is too big for the container file size
                        return null;
                    }
                    var header = br.ReadBytes(headerSize);
                    int minAddress = headerSize + 8;

                    List<long> FileAddresses = new List<long>(fileCount);
                    long lastAddress = 0;
                    for (int i = 0; i < fileCount; i++)
                    {
                        long address = BitConverter.ToUInt32(header, i * 8);
                        long fileSize = BitConverter.ToUInt32(header, i * 8 + 4);
                        if (address < minAddress)
                        {
                            return null;
                        }
                        if (address < lastAddress)
                        {
                            return null;
                        }
                        if (address > fileLength)
                        {
                            return null;
                        }
                        lastAddress = address + fileSize;
                        FileAddresses.Add(address);
                    }

                    if (FileAddresses.Count < 1)
                    {
                        return null;
                    }

                    return FileAddresses.ToArray();
                }
                finally
                {
                    fs.Position = oldPosition;
                }
            }

            public static long[] GetAlkFileSizes(Stream fs)
            {
                long oldPosition = fs.Position;
                try
                {
                    long fsLength = fs.Length;
                    long fileLength = fsLength;

                    var br = new BinaryReader(fs);

                    string signature = br.ReadStringFixedSize(4);
                    if (signature != "ALK0") return null;

                    int fileCount = br.ReadInt32();
                    if (fileCount < 0 || fileCount * 8 > fileLength)
                    {
                        return null;
                    }

                    int headerSize = fileCount * 8;
                    if (headerSize > fileLength)
                    {
                        //invalid DAT file - header size is too big for the container file size
                        return null;
                    }
                    var header = br.ReadBytes(headerSize);
                    int minAddress = headerSize + 8;

                    List<long> FileSizes = new List<long>(fileCount);
                    long lastAddress = 0;
                    for (int i = 0; i < fileCount; i++)
                    {
                        long address = BitConverter.ToUInt32(header, i * 8);
                        long fileSize = BitConverter.ToUInt32(header, i * 8 + 4);
                        if (address < minAddress)
                        {
                            return null;
                        }
                        if (address < lastAddress)
                        {
                            return null;
                        }
                        if (address > fileLength)
                        {
                            return null;
                        }
                        lastAddress = address + fileSize;
                        FileSizes.Add(fileSize);
                    }

                    if (FileSizes.Count < 1)
                    {
                        return null;
                    }

                    return FileSizes.ToArray();
                }
                finally
                {
                    fs.Position = oldPosition;
                }
            }

            public static long[] GetDatFileAddresses(Stream fs)
            {
                long oldPosition = fs.Position;
                try
                {
                    long fsLength = fs.Length;
                    if (fsLength > 16 * 1024 * 1024 || fsLength < 512)
                    {
                        //file is a bad size - over 16MB or under 512 bytes
                        return null;
                    }
                    long fileLength = fsLength;

                    var br = new BinaryReader(fs);

                    int headerSize = (br.ReadUInt16() - 1) * 256;
                    if (headerSize > 512)
                    {
                        //limit of 255 files, too many files, invalid DAT file.
                        return null;
                    }
                    if (headerSize > fileLength)
                    {
                        //invalid DAT file - header size is too big for the container file size
                        return null;
                    }
                    fs.Position = 0;
                    var header = br.ReadBytes(headerSize);

                    int filesLimit = headerSize / 2;
                    List<long> FileAddresses = new List<long>(filesLimit);
                    long lastAddress = 0;
                    bool sawZero = false;
                    for (int i = 1; i < filesLimit; i++)
                    {
                        long address = BitConverter.ToUInt16(header, i * 2);
                        if (address == 0)
                        {
                            sawZero = true;
                            continue;
                        }
                        address = (address - 1) * 256;
                        if (address < lastAddress)
                        {
                            //invalid DAT file - addresses are not strictly increasing
                            return null;
                        }
                        if (sawZero)
                        {
                            //invalid DAT file - nonzero number present after zero
                            return null;
                        }
                        if (address > fileLength)
                        {
                            //invalid DAT file - file address is too big for the container file size
                            return null;
                        }
                        lastAddress = address;
                        FileAddresses.Add(address);
                    }

                    if (FileAddresses.Count < 1)
                    {
                        //invalid DAT file - no files inside
                        return null;
                    }

                    return FileAddresses.ToArray();
                }
                finally
                {
                    fs.Position = oldPosition;
                }
            }

            public static int[][] GetDatFileNumbers(string fileName)
            {
                List<int[]> fileNumbersList = new List<int[]>();
                string path = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName.Length >= 2)
                {
                    baseName = baseName.Substring(1);
                    for (int i = 0; i < 27; i++)
                    {
                        char c = (i == 0) ? '@' : ((char)('A' + i - i));
                        string aldFileName = Path.Combine(path, c + baseName + ".dat");
                        if (File.Exists(aldFileName))
                        {
                            using (var fs = File.OpenRead(aldFileName))
                            {
                                fileNumbersList.Add(GetDatFileNumbers(fs, i));
                            }
                        }
                        else
                        {
                            fileNumbersList.Add(new int[0]);
                        }
                    }
                }
                return fileNumbersList.ToArray();
            }

            public static int[] GetDatFileNumbers(Stream fs, int fileLetter)
            {
                long oldPosition = fs.Position;
                fs.Position = 0;
                var br = new BinaryReader(fs);
                int tableAddress = (br.ReadUInt16() - 1) * 256;
                int firstFileAddress = (br.ReadUInt16() - 1) * 256;
                int tableSize = firstFileAddress - tableAddress;
                fs.Position = tableAddress;

                List<int> fileNumbersList = new List<int>();

                var tableData = br.ReadBytes(tableSize);
                int maxFileIndex = tableAddress / 2;
                int maxFileNumber = tableSize / 2;

                for (int fileNumberIndex = 0; fileNumberIndex < maxFileNumber; fileNumberIndex++)
                {
                    int fileNumber = fileNumberIndex + 1;
                    int entryFileLetter = tableData[fileNumberIndex * 2 + 0];
                    int fileIndex = tableData[fileNumberIndex * 2 + 1];
                    if (fileIndex != 0)
                    {
                        if (fileIndex < maxFileIndex && entryFileLetter == fileLetter)
                        {
                            fileNumbersList.SetOrAdd(fileIndex - 1, fileNumber);
                        }
                    }
                }

                fs.Position = oldPosition;
                return fileNumbersList.ToArray();
            }

            public static byte[] GetDatIndexBlock(Stream fs)
            {
                long oldPosition = fs.Position;
                fs.Position = 0;
                var br = new BinaryReader(fs);
                int tableAddress = (br.ReadUInt16() - 1) * 256;
                int firstFileAddress = (br.ReadUInt16() - 1) * 256;
                int tableSize = firstFileAddress - tableAddress;
                fs.Position = tableAddress;

                var tableData = br.ReadBytes(tableSize);

                fs.Position = oldPosition;
                return tableData;
            }


            public static IEnumerable<string> GetDatOtherFiles(string fileName)
            {
                string path = Path.GetDirectoryName(fileName);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName.Length >= 2)
                {
                    baseName = baseName.Substring(1);
                    for (int i = 0; i < 27; i++)
                    {
                        char c = (i == 0) ? '@' : ((char)('A' + i - 1));
                        string aldFileName = Path.Combine(path, c + baseName + ".dat");
                        if (File.Exists(aldFileName))
                        {
                            yield return aldFileName;
                        }
                    }
                }
            }
        }

        public static int PadToLength(int value, int padSize)
        {
            return (value + (padSize - 1)) & ~(padSize - 1);
        }

        public static long PadToLength(long value, long padSize)
        {
            return (value + (padSize - 1)) & ~(padSize - 1);
        }

        byte[] CreateAldIndexBlock(int fileLetter)
        {
            int highestFileNumber = 1;
            if (this.FileEntries.Count > 0)
            {
                highestFileNumber = this.FileEntries.Max(e => e.FileNumber);
            }
            int size = PadToLength((highestFileNumber - 1) * 3, 256);

            byte[] indexBlock = new byte[size];
            for (int i = 0; i < this.FileEntries.Count; i++)
            {
                var entry = this.FileEntries[i];
                int fileNumberIndex = entry.FileNumber - 1;

                if (fileNumberIndex >= 0)
                {
                    indexBlock[fileNumberIndex * 3 + 0] = (byte)(fileLetter);
                    indexBlock[fileNumberIndex * 3 + 1] = (byte)((i + 1) & 0xFF);
                    indexBlock[fileNumberIndex * 3 + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                }
            }

            return indexBlock;
        }

        //not used
        byte[] CreateDatIndexBlock(int fileLetter)
        {
            int highestFileNumber = 1;
            if (this.FileEntries.Count > 0)
            {
                highestFileNumber = this.FileEntries.Max(e => e.FileNumber);
            }
            int size = PadToLength((highestFileNumber - 1) * 2, 256);

            byte[] indexBlock = new byte[size];
            for (int i = 0; i < this.FileEntries.Count; i++)
            {
                var entry = this.FileEntries[i];
                int fileNumberIndex = entry.FileNumber - 1;

                if (fileNumberIndex >= 0)
                {
                    indexBlock[fileNumberIndex * 2 + 0] = (byte)(fileLetter);
                    indexBlock[fileNumberIndex * 2 + 1] = (byte)((i + 1) & 0xFF);
                }
            }

            return indexBlock;
        }

        static void SetStreamLength(Stream stream, int newSize)
        {
            stream.Position = stream.Length;
            if (newSize < stream.Length)
            {
                stream.SetLength(newSize);
            }
            else
            {
                if (stream.Length < newSize)
                {
                    stream.WriteZeroes(newSize - (int)stream.Length);
                }
            }
        }

        static void SetStreamLength(Stream stream, long newSize)
        {
            stream.Position = stream.Length;
            if (newSize < stream.Length)
            {
                stream.SetLength(newSize);
            }
            else
            {
                if (stream.Length < newSize)
                {
                    stream.WriteZeroes(newSize - stream.Length);
                }
            }
        }

        public void SaveFileAndCommit()
        {
            SaveFileAndCommit(this.AldFileName);
        }

        public void SaveFileAndCommit(string newFileName)
        {
            string tempFile = GetTempFileName(newFileName);
            SaveToFile(tempFile);
            CommitTempFile(newFileName, tempFile);
        }

        public static string GetTempFileName(string newFileName)
        {
            string tempFile = Path.ChangeExtension(newFileName, ".$$$");
            return tempFile;
        }

        public void CommitTempFile()
        {
            CommitTempFile(this.AldFileName, GetTempFileName(this.AldFileName));
        }

        public void CommitTempFile(string newFileName, string tempFile)
        {
            this.AldFileName = newFileName;
            if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }
            File.Move(tempFile, newFileName);
        }

        public void SaveTempFile()
        {
            SaveToFile(GetTempFileName(this.AldFileName));
        }

        public void SaveToFile(string outputFileName)
        {
            SaveToFile(outputFileName, false);
        }

        public void SaveToFile(string outputFileName, bool keepFileLetter)
        {
            byte[] indexBlock;
            bool dontPad = false;
            if (this.fileType == AldFileType.AFA1File || this.fileType == AldFileType.AFA2File || this.fileType == AldFileType.AlkFile)
            {
                dontPad = true;
            }

            if (this.IndexBlock == null)
            {
                //???
                int fileLetter = this.FileLetter;
                if (!keepFileLetter && fileLetter == 0)
                {
                    fileLetter = AldUtil.GetAldFileLetter(this.AldFileName);
                    this.FileLetter = fileLetter;
                }

                if (this.fileType == AldFileType.AldFile)
                {
                    indexBlock = CreateAldIndexBlock(fileLetter);
                }
                else if (this.fileType == AldFileType.DatFile)
                {
                    indexBlock = CreateDatIndexBlock(fileLetter);
                }
                else if (this.fileType == AldFileType.AlkFile)
                {
                    indexBlock = new byte[0];
                    //indexBlock = CreateAlkIndexBlock(this.FileEntries.Count);
                }
                else if (this.FileType == AldFileType.AFA1File || this.FileType == AldFileType.AFA2File)
                {
                    indexBlock = new byte[0];
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                indexBlock = this.IndexBlock;
            }

            int numberOfFiles = this.FileEntries.Count;
            int sizeOfFilesBlock;
            if (this.fileType == AldFileType.AldFile)
            {
                sizeOfFilesBlock = PadToLength((numberOfFiles + 2) * 3, 256);
            }
            else if (this.fileType == AldFileType.DatFile)
            {
                sizeOfFilesBlock = PadToLength((numberOfFiles + 1) * 2, 256);
            }
            else if (this.fileType == AldFileType.AlkFile)
            {
                sizeOfFilesBlock = (numberOfFiles + 1) * 8;
            }
            else if (this.fileType == AldFileType.AFA1File || this.fileType == AldFileType.AFA2File)
            {
                int estimatedHeaderSize = AldUtil.EstimateAfaHeaderSize(this.FileEntries, this.FileType == AldFileType.AFA2File);
                int headerSize = estimatedHeaderSize;
                headerSize += 64;
                headerSize += 4095;
                headerSize /= 4096;
                headerSize *= 4096;

                headerSize += 8;


                sizeOfFilesBlock = headerSize;
            }
            else
            {
                throw new InvalidOperationException();
            }

            List<long> fileAddresses = new List<long>();

            using (var fs = File.OpenWrite(outputFileName))
            {
                var bw = new BinaryWriter(fs);
                SetStreamLength(fs, sizeOfFilesBlock);
                fileAddresses.Add(fs.Position);
                bw.Write(indexBlock);

                for (int i = 0; i < this.FileEntries.Count; i++)
                {
                    var entry = this.FileEntries[i];
                    entry.Index = i;
                    long headerPosition = fs.Position;

                    if (this.FileType == AldFileType.AldFile)
                    {
                        entry.UpdateFileHeader();
                        fs.WriteZeroes(entry.FileHeader.Length);
                    }

                    long filePosition = fs.Position;
                    entry.WriteDataToStream(fs);
                    long afterFile = fs.Position;
                    entry.FileSize = afterFile - filePosition;

                    //do this AFTER writing the file data
                    entry.Parent = this;

                    if (this.fileType == AldFileType.AldFile)
                    {
                        fs.Position = headerPosition;
                        entry.UpdateFileHeader();
                        bw.Write(entry.FileHeader);
                    }
                    entry.FileAddress = filePosition;
                    entry.HeaderAddress = headerPosition;

                    fileAddresses.Add(headerPosition);

                    if (!dontPad)
                    {
                        long newPosition = PadToLength(afterFile, 256);
                        SetStreamLength(fs, newPosition);
                    }
                }
                fileAddresses.Add(fs.Position);
                if (this.fileType == AldFileType.AldFile)
                {
                    UpdateFooter();
                    bw.Write(Footer);
                }

                fs.Position = 0;
                if (this.fileType == AldFileType.AldFile)
                {
                    for (int i = 0; i < fileAddresses.Count; i++)
                    {
                        uint address = (uint)(fileAddresses[i]);
                        bw.Write((byte)((address >> 8) & 0xFF));
                        bw.Write((byte)((address >> 16) & 0xFF));
                        bw.Write((byte)((address >> 24) & 0xFF));
                    }
                }
                else if (this.fileType == AldFileType.DatFile)
                {
                    for (int i = 0; i < fileAddresses.Count; i++)
                    {
                        uint address = (uint)(fileAddresses[i]);
                        bw.Write((ushort)((address + 256) >> 8));
                    }
                }
                else if (this.fileType == AldFileType.AlkFile)
                {
                    fs.Position = 0;
                    bw.WriteStringFixedSize("ALK0", 4);
                    bw.Write(this.FileEntries.Count);
                    for (int i = 1; i < fileAddresses.Count - 1; i++)
                    {
                        long address = fileAddresses[i];
                        long nextAddress = fileAddresses[i + 1];
                        long length = nextAddress - address;
                        bw.Write((uint)address);
                        bw.Write((uint)length);
                    }
                }
                else if (this.fileType == AldFileType.AFA1File || this.fileType == AldFileType.AFA2File)
                {
                    fs.Position = 0;
                    AldUtil.WriteAfaHeader(fs, this.FileEntries, sizeOfFilesBlock - 8, this.FileType == AldFileType.AFA2File);


                }

                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }

        private void UpdateFooter()
        {
            if (this.Footer == null)
            {
                this.Footer = new byte[16];
                var ms = new MemoryStream(this.Footer);
                var bw = new BinaryWriter(ms);

                bw.Write((byte)'N');
                bw.Write((byte)'L');
                bw.Write((byte)0x01);
                bw.Write((byte)0x00);
                bw.Write((int)0x10);
            }
            {
                var ms = new MemoryStream(this.Footer);
                var bw = new BinaryWriter(ms);
                ms.Position = 8;
                bw.Write((byte)0x01);
                bw.Write((short)this.FileEntries.Count);
            }
        }

        public void UpdateInformation()
        {
            for (int i = 0; i < this.FileEntries.Count; i++)
            {
                var entry = this.FileEntries[i];
                entry.UpdateFileHeader();
                entry.Index = i;
            }
        }

        public void BuildIndexBlock(IEnumerable<AldFileEntry> fileEntries)
        {
            if (this.FileType == AldFileType.AldFile)
            {
                this.IndexBlock = CreateAldIndexBlock(fileEntries);
            }
            else if (this.FileType == AldFileType.DatFile)
            {
                this.IndexBlock = CreateDatIndexBlock(fileEntries);
            }
            else if (this.FileType == AldFileType.AlkFile)
            {
                //do nothing
            }
            else if (this.FileType == AldFileType.AFA1File || this.FileType == AldFileType.AFA2File)
            {
                //do nothing
            }
        }

        private static byte[] CreateDatIndexBlock(IEnumerable<AldFileEntry> fileEntries)
        {
            int highestFileNumber = 1;
            if (fileEntries.FirstOrDefault() != null)
            {
                highestFileNumber = fileEntries.Max(e => e.FileNumber);
            }
            int size = PadToLength((highestFileNumber - 1) * 2, 256);

            byte[] indexBlock = new byte[size];
            foreach (var entry in fileEntries)
            {
                int fileLetter = entry.FileLetter;
                int fileNumberIndex = entry.FileNumber - 1;
                int i = entry.Index;

                if (fileNumberIndex >= 0)
                {
                    indexBlock[fileNumberIndex * 2 + 0] = (byte)(fileLetter);
                    indexBlock[fileNumberIndex * 2 + 1] = (byte)((i + 1) & 0xFF);
                }
            }
            return indexBlock;
        }

        private static byte[] CreateAldIndexBlock(IEnumerable<AldFileEntry> fileEntries)
        {
            int highestFileNumber = 1;
            if (fileEntries.FirstOrDefault() != null)
            {
                highestFileNumber = fileEntries.Max(e => e.FileNumber);
            }
            int size = PadToLength((highestFileNumber - 1) * 3, 256);

            byte[] indexBlock = new byte[size];
            foreach (var entry in fileEntries)
            {
                int fileLetter = entry.FileLetter;
                int fileNumberIndex = entry.FileNumber - 1;
                int i = entry.Index;

                if (fileNumberIndex >= 0)
                {
                    indexBlock[fileNumberIndex * 3 + 0] = (byte)(fileLetter);
                    indexBlock[fileNumberIndex * 3 + 1] = (byte)((i + 1) & 0xFF);
                    indexBlock[fileNumberIndex * 3 + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                }
            }

            return indexBlock;
        }
    }

    public class AldFileEntryCollection : Collection<AldFileEntry>
    {

    }

    public class AldFileEntry : IWithIndex, IWithParent<AldFile>
    {
        public AldFileEntry Clone()
        {
            var clone = (AldFileEntry)MemberwiseClone();
            if (clone.FileHeader != null)
            {
                clone.FileHeader = (byte[])clone.FileHeader.Clone();
            }
            return clone;
        }
        public AldFileEntry()
        {

        }
        public AldFile Parent
        {
            get;
            set;
        }
        public string FileName
        {
            get;
            set;
        }
        public int FileLetter
        {
            get;
            set;
        }
        //public FileType FileType
        //{
        //    get;
        //    set;
        //}
        public int Index
        {
            get;
            set;
        }
        public int FileNumber
        {
            get;
            set;
        }
        public long FileSize
        {
            get;
            set;
        }
        public long FileAddress
        {
            get;
            set;
        }
        public long HeaderAddress
        {
            get;
            set;
        }
        public byte[] FileHeader
        {
            get;
            set;
        }
        public byte[] GetFileData()
        {
            return GetFileData(false);
        }

        public byte[] GetFileData(bool doNotConvert)
        {
            var ms = new MemoryStream();
            WriteDataToStream(ms, doNotConvert);
            return ms.ToArray();
        }

        public object Tag
        {
            get;
            set;
        }

        internal AldFileEntry[] subImages = null;
        internal bool alreadyLookedForSubImages = false;

        public override string ToString()
        {
            return "FileNumber: " + this.FileNumber + " FileName: " + this.FileName + " Index: " + this.Index;
        }

        static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        public void UpdateFileHeader()
        {
            if (Parent != null && Parent.FileType != AldFileType.AldFile)
            {
                return;
            }

            if (FileHeader == null)
            {
                var fileNameBytes = shiftJis.GetBytes(this.FileName ?? " ");
                int headerSize = AldFile.PadToLength(16 + fileNameBytes.Length, 16);
                if (headerSize == 16) headerSize = 32;

                FileHeader = new byte[headerSize];
                var ms = new MemoryStream(FileHeader);
                var bw = new BinaryWriter(ms);
                bw.Write((int)headerSize);
                bw.Write((int)0);
                bw.Write((uint)0x8E5C4430); //???
                bw.Write((int)0x01C9F639); //???
                bw.Write((int)0);
                bw.Write((int)0);
                bw.Write((int)0);
                bw.Write((int)0);
            }

            {
                var ms = new MemoryStream(FileHeader);
                var bw = new BinaryWriter(ms);

                byte[] fileNameBytes = shiftJis.GetBytes(this.FileName);
                ms.Position = 4;
                bw.Write((int)this.FileSize);
                ms.Position = 16;
                int maxFileNameLength = FileHeader.Length - 16;

                if (fileNameBytes.Length < maxFileNameLength)
                {
                    fileNameBytes = fileNameBytes.Concat(Enumerable.Repeat((byte)0, maxFileNameLength - fileNameBytes.Length)).ToArray();
                }
                else if (fileNameBytes.Length > maxFileNameLength)
                {
                    fileNameBytes = fileNameBytes.Take(maxFileNameLength).ToArray();
                }
                bw.Write(fileNameBytes);
            }
        }


        public void WriteDataToStream(Stream stream)
        {
            WriteDataToStream(stream, false);
        }

        public void WriteDataToStream(Stream stream, bool doNotConvert)
        {
            if (this.HasSubImages())
            {
                if (this.subImages == null && !this.alreadyLookedForSubImages)
                {
                    WriteDataToStream2(stream, doNotConvert);
                    return;
                }

                bool anyDirty = false;
                //are subimages clean?  Return original container
                foreach (var subimage in this.subImages)
                {
                    if (subimage.HasReplacementData())
                    {
                        anyDirty = true;
                        break;
                    }
                }

                if (!anyDirty)
                {
                    WriteDataToStream2(stream, doNotConvert);
                    return;
                }

                byte[] containerBytes;
                {
                    var msContainer = new MemoryStream();
                    WriteDataToStream2(msContainer, doNotConvert);
                    containerBytes = msContainer.ToArray();
                }

                //var subImageFinder = new SubImageFinder(this);
                //var subImages = this.subImages;
                //var nodes = subImages.Select(entry=>entry.Tag as Node).ToArray();
                //if (subImages == null)
                //{
                //    nodes = subImageFinder.GetSubImageNodes(containerBytes);
                //    subImages = nodes.Select(node=>AldFileSubimages.GetDummyEntry(node)).ToArray();
                //    this.subImages = subImages;
                //}

                var subImages = this.GetSubImages();
                List<Node> nodes = new List<Node>();
                foreach (var subImage in subImages)
                {
                    var oldNode = subImage.Tag as Node;
                    if (oldNode != null)
                    {
                        var ms = new MemoryStream();
                        subImage.WriteDataToStream(ms);
                        var newBytes = ms.ToArray();
                        var newNode = oldNode.Clone();
                        newNode.Bytes = newBytes;
                        nodes.Add(newNode);
                    }
                }
                var subImageFinder = new SubImageFinder(this);
                var newFileBytes = subImageFinder.ReplaceSubImageNodes(containerBytes, nodes.ToArray());
                stream.Write(newFileBytes, 0, newFileBytes.Length);
                return;
            }
            else
            {
                WriteDataToStream2(stream, doNotConvert);
            }
        }

        private void WriteDataToStream2(Stream stream, bool doNotConvert)
        {
            long streamStartPosition = stream.Position;
            if (GetReplacementFileData != null)
            {

            }
            else if (!String.IsNullOrEmpty(ReplacementFileName))
            {
                WriteReplacementFile(stream, doNotConvert);
            }
            else if (ReplacementBytes != null)
            {
                stream.Write(this.ReplacementBytes, 0, this.ReplacementBytes.Length);
            }
            else
            {
                var node = this.Tag as Node;
                if (node != null)
                {
                    if (node.Bytes != null)
                    {
                        stream.Write(node.Bytes, 0, node.Bytes.Length);
                        return;
                    }
                }

                if (Parent != null)
                {
                    using (var fs = new FileStream(Parent.AldFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var br = new BinaryReader(fs);
                        fs.Position = this.FileAddress;
                        fs.WriteToStream(stream, FileSize);
                    }
                }
                else
                {
                    //this is a subfile
                    if (node != null)
                    {
                        if (node.Bytes != null)
                        {
                            stream.Write(node.Bytes, 0, node.Bytes.Length);
                            return;
                        }
                        else
                        {
                            var parent = node.Parent;
                            var ms = new MemoryStream();
                            parent.WriteDataToStream(ms, doNotConvert);
                            ms.Position = this.FileAddress;
                            ms.WriteToStream(stream, FileSize);
                        }
                    }
                }
            }
            //long streamEndPosition = stream.Position;
            //this.FileSize = (int)(streamEndPosition - streamStartPosition);
        }

        private void WriteReplacementFile(Stream stream, bool doNotConvert)
        {
            string extension = Path.GetExtension(this.FileName).ToLowerInvariant();
            string replacementFileExtension = Path.GetExtension(ReplacementFileName).ToLowerInvariant();

            //is this a subimage of a flat file?  Then make it a qnt file
            var node = this.Tag as Node;
            if (node != null)
            {
                var parentEntry = node.Parent;
                if (parentEntry != null)
                {
                    string parentExt = Path.GetExtension(parentEntry.FileName).ToLowerInvariant();
                    if (parentExt == ".flat")
                    {
                        if (extension == ".png")
                        {
                            extension = ".qnt";
                        }
                    }
                }
            }

            bool converted = false;

            if (replacementFileExtension != extension && !doNotConvert)
            {
                if (extension == ".vsp")
                {
                    using (FreeImageBitmap referenceImage = GetOriginalImageVSP())
                    {
                        using (FreeImageBitmap imagefile = new FreeImageBitmap(this.ReplacementFileName))
                        {
                            if (referenceImage != null)
                            {
                                var vspHeader = new VspHeader();
                                vspHeader.ParseComment(referenceImage.Comment);
                                bool is8Bit = vspHeader.is8Bit == 1;

                                bool paletteMatches = false;
                                if (imagefile.ColorDepth == 4 && ImageConverter.PaletteMatches(imagefile.Palette, referenceImage.Palette, 0, 16))
                                {
                                    paletteMatches = true;
                                }
                                if (imagefile.ColorDepth == 8 && ImageConverter.PaletteMatches(imagefile.Palette, referenceImage.Palette, 0, 256))
                                {
                                    paletteMatches = true;
                                }
                                if (!paletteMatches || AldFile.AlwaysRemapImages)
                                {
                                    if (!is8Bit)
                                    {
                                        ImageConverter.RemapPalette(imagefile, referenceImage, 16);
                                    }
                                    else
                                    {
                                        ImageConverter.RemapPalette(imagefile, referenceImage, 256);
                                    }
                                    //convert image file to 4-bit
                                    //imagefile.Quantize(FREE_IMAGE_QUANTIZE.FIQ_NNQUANT, 16, 16, referenceImage.Palette);
                                }

                                imagefile.Comment = referenceImage.Comment;
                            }
                            else
                            {
                                var vspHeader = new VspHeader();

                                if (!String.IsNullOrEmpty(imagefile.Comment))
                                {
                                    vspHeader.ParseComment(referenceImage.Comment);
                                    bool is8Bit = vspHeader.is8Bit == 1;
                                    imagefile.Comment = vspHeader.GetComment();
                                }
                            }
                            ImageConverter.SaveVsp(stream, imagefile);
                        }
                    }

                    converted = true;
                }
                if (extension == ".pms")
                {
                    using (FreeImageBitmap referenceImage = GetOriginalImagePMS())
                    {
                        using (FreeImageBitmap imageFile = new FreeImageBitmap(this.ReplacementFileName))
                        {
                            if (referenceImage != null)
                            {
                                bool paletteMatches = false;
                                if (imageFile.ColorDepth == 8 && ImageConverter.PaletteMatches(imageFile.Palette, referenceImage.Palette, 0, 256))
                                {
                                    paletteMatches = true;
                                }
                                if (!paletteMatches || AldFile.AlwaysRemapImages)
                                {
                                    ImageConverter.RemapPalette(imageFile, referenceImage, 256);
                                }
                                //var pmsHeader = referenceImage.Tag as PmsHeader;
                                //if (pmsHeader != null) imageFile.Tag = pmsHeader.Clone();
                                imageFile.Comment = referenceImage.Comment;
                                referenceImage.Dispose();
                            }
                            ImageConverter.SavePms(stream, imageFile);
                            converted = true;
                        }
                    }
                }
                if (extension == ".qnt")
                {
                    var qntHeader = GetOriginalImageHeaderQNT();
                    using (FreeImageBitmap imageFile = new FreeImageBitmap(this.ReplacementFileName))
                    {
                        if (qntHeader != null)
                        {
                            imageFile.Comment = qntHeader.GetComment();
                        }
                        ImageConverter.SaveQnt(stream, imageFile);
                        converted = true;
                    }
                }
                if (extension == ".ajp")
                {
                    var ajpHeader = GetOriginalImageHeaderAJP();
                    if (Path.GetExtension(this.ReplacementFileName).ToLowerInvariant() == ".jpg")
                    {
                        string alphaFileDirectory = Path.Combine(Path.GetDirectoryName(this.ReplacementFileName), "SP");
                        string alphaImageName = Path.GetFileNameWithoutExtension(this.ReplacementFileName) + ".bmp";
                        string alphaFileName = Path.Combine(alphaFileDirectory, alphaImageName);
                        bool alphaFileExists = false;
                        alphaFileExists = File.Exists(alphaFileName);
                        if (!alphaFileExists)
                        {
                            alphaImageName = Path.ChangeExtension(alphaImageName, ".png");
                            alphaFileName = Path.Combine(alphaFileDirectory, alphaImageName);
                            alphaFileExists = File.Exists(alphaFileName);
                        }

                        if (ajpHeader == null)
                        {
                            if (alphaFileExists)
                            {
                                FreeImageBitmap alphaImage = new FreeImageBitmap(alphaFileName);
                                alphaImage.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP | FREE_IMAGE_COLOR_DEPTH.FICD_FORCE_GREYSCALE | FREE_IMAGE_COLOR_DEPTH.FICD_REORDER_PALETTE);
                                var jpegFile = File.ReadAllBytes(this.ReplacementFileName);
                                ImageConverter.SaveAjp(stream, jpegFile, alphaImage, ajpHeader);
                            }
                            else
                            {
                                var jpegFile = File.ReadAllBytes(this.ReplacementFileName);
                                ImageConverter.SaveAjp(stream, jpegFile, null, ajpHeader);
                            }
                            converted = true;
                            goto after;
                        }
                        else
                        {
                            if (ajpHeader.HasAlpha && alphaFileExists)
                            {
                                FreeImageBitmap alphaImage = new FreeImageBitmap(alphaFileName);
                                alphaImage.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP | FREE_IMAGE_COLOR_DEPTH.FICD_FORCE_GREYSCALE | FREE_IMAGE_COLOR_DEPTH.FICD_REORDER_PALETTE);
                                var jpegFile = File.ReadAllBytes(this.ReplacementFileName);
                                ImageConverter.SaveAjp(stream, jpegFile, alphaImage, ajpHeader);
                                converted = true;
                                goto after;
                            }
                            else if (!ajpHeader.HasAlpha)
                            {
                                var jpegFile = File.ReadAllBytes(this.ReplacementFileName);
                                ImageConverter.SaveAjp(stream, jpegFile, null, ajpHeader);
                                converted = true;
                                goto after;
                            }
                        }
                    }
                    using (FreeImageBitmap imageFile = new FreeImageBitmap(this.ReplacementFileName))
                    {
                        if (ajpHeader != null)
                        {
                            imageFile.Comment = ajpHeader.GetComment();
                        }
                        ImageConverter.SaveAjp(stream, imageFile);
                    }
                after:
                    ;
                }
            }
            if ((extension == ".swf" || extension == ".aff") && !doNotConvert)
            {
                bool wantAff = true;

                var referenceData = this.GetOriginalBytes(3);
                if (referenceData != null)
                {
                    string sig = ASCIIEncoding.ASCII.GetString(referenceData, 0, 3);
                    if (sig == "FWS" || sig == "CWS")
                    {
                        wantAff = false;
                    }
                }
                if (referenceData == null && extension == ".swf" && replacementFileExtension == ".swf")
                {
                    wantAff = false;
                }

                var swfBytes = File.ReadAllBytes(ReplacementFileName);
                bool isSwf = true;
                string sig2 = ASCIIEncoding.ASCII.GetString(swfBytes, 0, 3);
                if (sig2 == "AFF")
                {
                    isSwf = false;
                }

                if (isSwf && wantAff)
                {
                    //swfBytes = SwfToAffConverter.ConvertSwfToAff(swfBytes);
                }
                if (!isSwf && !wantAff)
                {
                    //swfBytes = SwfToAffConverter.ConvertAffToSwf(swfBytes);
                }

                stream.Write(swfBytes, 0, swfBytes.Length);
                converted = true;
            }

            if (!converted)
            {
                //no conversion?
                using (var fs = new FileStream(ReplacementFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.WriteToStream(stream);
                }
            }
        }

        private QntHeader GetOriginalImageHeaderQNT()
        {
            return CallFunctionOnFile((fs) => ImageConverter.LoadQntHeader(fs));
        }

        private AjpHeader GetOriginalImageHeaderAJP()
        {
            return CallFunctionOnFile((fs) => ImageConverter.LoadAjpHeader(fs));
        }

        private FreeImageBitmap GetOriginalImageQNT()
        {
            byte[] originalBytes = GetOriginalBytes();
            FreeImageBitmap referenceImage = null;
            if (originalBytes != null)
            {
                referenceImage = ImageConverter.LoadQnt(originalBytes);
            }
            return referenceImage;
        }

        private FreeImageBitmap GetOriginalImageAJP()
        {
            byte[] originalBytes = GetOriginalBytes();
            FreeImageBitmap referenceImage = null;
            if (originalBytes != null)
            {
                referenceImage = ImageConverter.LoadAjp(originalBytes);
            }
            return referenceImage;
        }

        private FreeImageBitmap GetOriginalImageVSP()
        {
            byte[] originalBytes = GetOriginalBytes();
            FreeImageBitmap referenceImage = null;
            if (originalBytes != null)
            {
                referenceImage = ImageConverter.LoadVsp(originalBytes);
            }
            return referenceImage;
        }

        private FreeImageBitmap GetOriginalImagePMS()
        {
            byte[] originalBytes = GetOriginalBytes();
            FreeImageBitmap referenceImage = null;
            if (originalBytes != null)
            {
                referenceImage = ImageConverter.LoadPms(originalBytes);
            }
            return referenceImage;
        }

        private byte[] GetOriginalBytes()
        {
            return GetOriginalBytes((int)this.FileSize);
        }

        private byte[] GetOriginalBytes(int byteCount)
        {
            return CallFunctionOnFile((fs) =>
            {
                var br = new BinaryReader(fs);
                return br.ReadBytes(byteCount);
            });
        }

        private T CallFunctionOnFile<T>(Func<Stream, T> actionToTake)
        {
            if (this.FileSize > 0 && this.FileAddress > 0)
            {
                using (var fs = new FileStream(Parent.AldFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Position = this.FileAddress;
                    return actionToTake(fs);
                }
            }
            else
            {
                return default(T);
            }
        }

        public event EventHandler GetReplacementFileData;
        string _replacementFileName;
        public string ReplacementFileName
        {
            get
            {
                return _replacementFileName;
            }
            set
            {
                if (_replacementFileName != value)
                {
                    _replacementFileName = value;
                    this.subImages = null;
                    this.alreadyLookedForSubImages = false;
                }
            }
        }

        byte[] _replacementBytes;
        public byte[] ReplacementBytes
        {
            get
            {
                return _replacementBytes;
            }
            set
            {
                _replacementBytes = value;
            }
        }

        #region IWithParent Members

        object IWithParent.Parent
        {
            get
            {
                return Parent;
            }
            set
            {
                Parent = value as AldFile;
            }
        }

        #endregion

        public bool HasReplacementData()
        {
            return (!String.IsNullOrEmpty(this.ReplacementFileName)) || (this.GetReplacementFileData != null) || (this.ReplacementBytes != null);
        }
    }

    public static partial class Extensions
    {
        static readonly byte[] zeroes = new byte[4096];
        public static void WriteZeroes(this Stream stream, int count)
        {
            while (count > 0)
            {
                int numberToWrite = count;
                if (numberToWrite > zeroes.Length)
                {
                    numberToWrite = zeroes.Length;
                }
                stream.Write(zeroes, 0, numberToWrite);

                count -= numberToWrite;
            }
        }
        public static void WriteZeroes(this Stream stream, long count)
        {
            while (count > 0)
            {
                long numberToWrite = count;
                if (numberToWrite > zeroes.Length)
                {
                    numberToWrite = zeroes.Length;
                }
                stream.Write(zeroes, 0, (int)numberToWrite);

                count -= numberToWrite;
            }
        }
    }

    public enum AldFileType
    {
        AldFile = 0,
        DatFile,
        AlkFile,
        AFA1File,
        AFA2File,
        Invalid = -1,
    }

    //public class SubStream : Stream
    //{
    //    long offset, length;
    //    long streamLength;

    //    long position;

    //    Stream stream;

    //    public SubStream(Stream stream, long offset, long length)
    //    {
    //        streamLength = stream.Length;
    //        this.stream = stream;

    //    }


    //    public override long Length
    //    {
    //        get
    //        {
    //            return length;
    //        }
    //    }

    //    public override long Position
    //    {
    //        get
    //        {
    //            return stream.Position - offset;
    //        }
    //        set
    //        {
    //            long newPos = value - offset;
    //            if (newPos < 0)
    //            {
    //                throw new ArgumentOutOfRangeException("Position");
    //            }
    //            stream.Position = newPos;
    //        }
    //    }

    //    public override bool CanRead
    //    {
    //        get
    //        {
    //            return stream.CanRead;
    //        }
    //    }

    //    public override bool CanSeek
    //    {
    //        get
    //        {
    //            return stream.CanSeek;
    //        }
    //    }

    //    public override bool CanWrite
    //    {
    //        get
    //        {
    //            return stream.CanWrite;
    //        }
    //    }

    //    public override void Flush()
    //    {
    //        stream.Flush();
    //    }

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        //bounds checking
    //        if (offset < 0 || offset > buffer.Length)
    //        {
    //            throw new ArgumentOutOfRangeException("offset", "Offset is out of the bounds of the array.");
    //        }
    //        if (count < 0 || offset + count > buffer.Length)
    //        {
    //            throw new ArgumentOutOfRangeException("count", "offset + count would exceed the array bounds");
    //        }





    //        if (this.Position < 0)
    //        {
    //            throw new ArgumentOutOfRangeException("Position", "stream position is negative");

    //            long behindBy = 0 - this.position;
    //            if (count <= behindBy)
    //            {
    //                for (int i = offset; i < offset + count; i++)
    //                {
    //                    buffer[i] = (byte)0;
    //                }
    //            }
    //            else
    //            {
    //                int count2 = count - (int)behindBy;
    //                for (int i = offset; i < behindBy; i++)
    //                {
    //                    buffer[i] = (byte)0;
    //                }
    //            }
    //        }


    //        stream.Read(buffer, offset, count);
    //    }

    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        if (origin == SeekOrigin.Begin)
    //        {
    //            this.Position = offset;
    //        }
    //        else if (origin == SeekOrigin.Current)
    //        {
    //            this.Position += offset;
    //        }
    //        else if (origin == SeekOrigin.End)
    //        {
    //            this.Position = length;
    //        }
    //        return this.Position;
    //    }

    //    public override void SetLength(long value)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {


    //        throw new NotImplementedException();
    //    }
    //}


    public static class AldFileSubimages
    {
        public static bool HasSubImages(this AldFileEntry entry)
        {
            var node = entry.Tag as Node;
            if (node != null) return false;

            string ext = Path.GetExtension(entry.FileName).ToLowerInvariant();
            if (ext == ".swf" || ext == ".aff" || ext == ".flat")
            {
                return true;
            }
            return false;
        }

        public static AldFileEntry[] GetSubImages(this AldFileEntry entry)
        {
            if (!entry.HasSubImages())
            {
                return null;
            }
            if (entry.subImages != null)
            {
                return entry.subImages;
            }
            if (entry.alreadyLookedForSubImages)
            {
                return null;
            }

            var subImageFinder = new SubImageFinder(entry);
            var nodes = subImageFinder.GetSubImageNodes();
            entry.alreadyLookedForSubImages = true;
            if (nodes != null)
            {
                List<AldFileEntry> entriesList = new List<AldFileEntry>();
                foreach (var node in nodes)
                {
                    var dummyEntry = GetDummyEntry(node);
                    entriesList.Add(dummyEntry);
                }
                var subImages = entriesList.ToArray();
                entry.subImages = subImages;
                return subImages;
            }
            return null;
        }

        internal static AldFileEntry GetDummyEntry(Node node)
        {
            var dummyEntry = new AldFileEntry();
            dummyEntry.Index = -1;
            dummyEntry.Parent = node.Parent.Parent;
            dummyEntry.FileAddress = node.Parent.FileAddress + node.Offset;
            dummyEntry.FileSize = node.Bytes.Length;
            dummyEntry.FileName = node.FileName;
            dummyEntry.Tag = node;
            return dummyEntry;
        }

        internal class SubImageFinder
        {
            AldFileEntry entry;
            public SubImageFinder(AldFileEntry entry)
            {
                this.entry = entry;
            }

            public Node[] GetSubImageNodes()
            {
                var bytes = entry.GetFileData();

                return GetSubImageNodes(bytes);
            }

            public Node[] GetSubImageNodes(byte[] bytes)
            {
                string sig = ASCIIEncoding.ASCII.GetString(bytes, 0, 3);
                if (sig == "FLA")
                {
                    return GetSubImageNodesFlat(bytes);
                }
                if (sig == "AFF")
                {
                    //return GetSubImageNodesAff(bytes);
                }
                if (sig == "FWS" || sig == "CWS")
                {
                    //return GetSubImageNodesSwf(bytes);
                }
                return null;
            }

            public byte[] ReplaceSubImageNodes(byte[] bytes, Node[] nodes)
            {
                string sig = ASCIIEncoding.ASCII.GetString(bytes, 0, 3);
                if (sig == "FLA")
                {
                    return ReplaceSubImageNodesFlat(bytes, nodes);
                }
                if (sig == "AFF")
                {
                    //return ReplaceSubImageNodesAff(bytes, nodes);
                }
                if (sig == "FWS" || sig == "CWS")
                {
                    //return ReplaceSubImageNodesSwf(bytes, nodes);
                }
                return null;
            }

            class Tag
            {
                public byte[] TagData;
                public string TagName;
                public int TagLength;
            }

            public class Node : ICloneable
            {
                public string FileName;
                public byte[] Bytes;
                public long Offset;
                public AldFileEntry Parent;

                public Node Clone()
                {
                    return (Node)this.MemberwiseClone();
                }

                #region ICloneable Members

                object ICloneable.Clone()
                {
                    return Clone();
                }

                #endregion
            }

            Tag ReadTag(BinaryReader br)
            {
                var tag = new Tag();
                var tagNameBytes = br.ReadBytes(4);
                tag.TagName = ASCIIEncoding.ASCII.GetString(tagNameBytes);
                tag.TagLength = br.ReadInt32();
                if (tag.TagLength < 0 || tag.TagLength > br.BaseStream.Length - br.BaseStream.Position)
                {
                    return null;
                }
                tag.TagData = br.ReadBytes(tag.TagLength);
                return tag;
            }

            private void WriteTag(BinaryWriter bw, Tag tag)
            {
                bw.WriteStringFixedSize(tag.TagName, 4);
                bw.Write((int)tag.TagData.Length);
                bw.Write(tag.TagData);
            }

            static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

            public Node[] GetSubImageNodesFlat(byte[] bytes)
            {
                List<Node> list = new List<Node>();
                var ms1 = new MemoryStream(bytes);
                var br1 = new BinaryReader(ms1);

                while (br1.BaseStream.Position < br1.BaseStream.Length)
                {
                    long offset1 = br1.BaseStream.Position;
                    var tag = ReadTag(br1);
                    if (tag.TagName == "LIBL")
                    {
                        var dataBytes = tag.TagData;
                        var ms = new MemoryStream(dataBytes);
                        var br = new BinaryReader(ms);
                        int fileCount = br.ReadInt32(); //9
                        for (int fileNumber = 0; fileNumber < fileCount; fileNumber++)
                        {
                            int fileNameLength = br.ReadInt32(); //8
                            var fileNameBytes = br.ReadBytes(fileNameLength);
                            string fileName = shiftJis.GetString(fileNameBytes);
                            br.BaseStream.Position = ((br.BaseStream.Position - 1) | 3) + 1;
                            int unknown1 = br.ReadInt32();  //
                            int dataLength = br.ReadInt32();
                            int unknown2 = br.ReadInt32();
                            byte[] imageBytes; ;
                            long offset2 = br.BaseStream.Position + offset1 + 8;
                            if (unknown2 == 0x00544E51) //QNT
                            {
                                br.BaseStream.Position -= 4;
                                offset2 -= 4;
                                imageBytes = br.ReadBytes(dataLength);
                            }
                            else
                            {
                                imageBytes = br.ReadBytes(dataLength - 4);
                            }

                            list.Add(new Node() { Bytes = imageBytes, FileName = fileName, Offset = offset2, Parent = this.entry });
                            br.BaseStream.Position = ((br.BaseStream.Position - 1) | 3) + 1;
                        }
                    }
                }
                return list.ToArray();
            }

            public byte[] ReplaceSubImageNodesFlat(byte[] bytes, Node[] nodes)
            {
                var ms1 = new MemoryStream(bytes);
                var br1 = new BinaryReader(ms1);

                var msOutput = new MemoryStream();
                var bw = new BinaryWriter(msOutput);

                while (br1.BaseStream.Position < br1.BaseStream.Length)
                {
                    var tag = ReadTag(br1);
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
                            string fileName = shiftJis.GetString(fileNameBytes);
                            long lastPosition = br.BaseStream.Position;
                            br.BaseStream.Position = ((br.BaseStream.Position - 1) | 3) + 1;
                            int paddingCount1 = (int)(br.BaseStream.Position - lastPosition);

                            int unknown1 = br.ReadInt32();
                            int dataLength = br.ReadInt32();
                            int unknown2 = br.ReadInt32();
                            byte[] imageBytes;
                            if (unknown2 == 0x00544E51) //QNT
                            {
                                br.BaseStream.Position -= 4;
                                imageBytes = br.ReadBytes(dataLength);
                            }
                            else
                            {
                                imageBytes = br.ReadBytes(dataLength - 4);
                            }
                            lastPosition = br.BaseStream.Position;
                            br.BaseStream.Position = ((br.BaseStream.Position - 1) | 3) + 1;
                            int paddingCount2 = (int)(br.BaseStream.Position - lastPosition);

                            var node = nodes[fileNumber];
                            //write replacement node
                            var newFileNameBytes = shiftJis.GetBytes(node.FileName);

                            long outputPosition = bw.BaseStream.Position;

                            bw.Write((int)newFileNameBytes.Length);
                            bw.Write(newFileNameBytes);

                            int outputPadding1 = (int)(((((bw.BaseStream.Position - outputPosition) - 1) | 3) + 1) - (bw.BaseStream.Position - outputPosition));
                            for (int i = 0; i < outputPadding1; i++)
                            {
                                bw.Write((byte)0);
                            }

                            bw.Write((int)unknown1);
                            if (unknown2 != 0x00544E51)
                            {
                                bw.Write((int)node.Bytes.Length + 4);
                                bw.Write((int)unknown2);
                            }
                            else
                            {
                                bw.Write((int)node.Bytes.Length);
                            }
                            bw.Write(node.Bytes);

                            int outputPadding2 = (int)(((((bw.BaseStream.Position - outputPosition) - 1) | 3) + 1) - (bw.BaseStream.Position - outputPosition));

                            for (int i = 0; i < outputPadding2; i++)
                            {
                                bw.Write((byte)0);
                            }
                        }

                        long lastPos = bw.BaseStream.Position;
                        bw.BaseStream.Position = tagLengthPos;
                        int tagLength = (int)(lastPos - (tagLengthPos + 4));
                        bw.Write((int)tagLength);
                        bw.BaseStream.Position = lastPos;
                    }
                    else
                    {
                        WriteTag(bw, tag);
                    }

                }
                return msOutput.ToArray();
            }


            /*
            public Node[] GetSubImageNodesAff(byte[] bytes)
            {
                return GetSubImageNodesSwf(SwfToAffConverter.ConvertAffToSwf(bytes));
            }
            public Node[] GetSubImageNodesSwf(byte[] bytes)
            {
                List<Node> nodes = new List<Node>();

                SwfTagList2 swfTags = new SwfTagList2();
                swfTags.ReadSwf(bytes);

                foreach (var tag in swfTags)
                {
                    if (tag.TagType == TagType.DefineBitsLossless || tag.TagType == TagType.DefineBitsLossless2)
                    {
                        var node = new Node();
                        node.Bytes = tag.Data;
                        node.FileName = "img" + tag.DefineId.ToString("0000") + ".png";
                        node.Parent = this.entry;
                        nodes.Add(node);
                    }
                    if (tag.TagType == TagType.DefineSound)
                    {
                        var defineSoundTag = tag.Tag as DefineSoundTag;
                        var node = new Node();
                        var ms = new MemoryStream();
                        var bw = new BinaryWriter(ms);

                        if (defineSoundTag.SoundFormat == SoundCompressionType.MP3)
                        {
                            node.Bytes = defineSoundTag.SoundData;
                            node.FileName = "sound" + tag.DefineId.ToString("0000") + ".mp3";
                        }
                        else if (defineSoundTag.SoundFormat == SoundCompressionType.UncompressedLE)
                        {
                            int channels = defineSoundTag.IsStereo ? 2 : 1;
                            var soundData = defineSoundTag.SoundData;
                            int sampleRate = (int)defineSoundTag.SoundRate;

                            WriteWaveFile(bw, channels, soundData, sampleRate);
                            node.FileName = "sound" + tag.DefineId.ToString("0000") + ".wav";
                            node.Bytes = ms.ToArray();
                        }

                        node.Parent = this.entry;
                        nodes.Add(node);
                    }
                }

                return nodes.ToArray();
            }*/

            private static void WriteWaveFile(BinaryWriter bw, int channels, byte[] soundData, int sampleRate)
            {
                bw.WriteStringFixedSize("RIFF", 4);
                bw.Write(0x24 + soundData.Length);
                bw.WriteStringFixedSize("WAVE", 4);
                bw.WriteStringFixedSize("fmt ", 4);
                bw.Write(0x10);
                bw.Write((short)1);
                bw.Write((short)channels);
                bw.Write(sampleRate);
                bw.Write((int)(sampleRate * channels * 2));
                bw.Write((short)(channels * 2));
                bw.Write((short)16);
                bw.WriteStringFixedSize("data", 4);
                bw.Write(soundData.Length);
                bw.Write(soundData);
            }

            /*public byte[] ReplaceSubImageNodesAff(byte[] bytes, Node[] nodes)
            {
                return SwfToAffConverter.ConvertSwfToAff(ReplaceSubImageNodesSwf(SwfToAffConverter.ConvertAffToSwf(bytes), nodes));
            }

            public byte[] ReplaceSubImageNodesSwf(byte[] bytes, Node[] nodes)
            {
                Dictionary<int, byte[]> TagIdToBytes = new Dictionary<int, byte[]>();
                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    string fileName = node.FileName;
                    int num = GetNumberFromString(fileName);
                    if (num == -1) { num = i; }
                    TagIdToBytes[num] = node.Bytes;
                }

                SwfTagList2 swfTags = new SwfTagList2();
                swfTags.ReadSwf(bytes);

                for (int i = 0; i < swfTags.Count; i++)
                {
                    var tag = swfTags[i];
                    int defineId = tag.DefineId;
                    var nodeBytes = TagIdToBytes.GetOrNull(defineId);

                    if (nodeBytes != null)
                    {
                        if (tag.TagType == TagType.DefineBitsLossless || tag.TagType == TagType.DefineBitsLossless2)
                        {
                            FreeImageBitmap bitmap = null;
                            bitmap = Form1.GetImage(nodeBytes);
                            var imageTag = tag.Tag as DefineBitsLosslessTag;
                            imageTag.SetBitmap(bitmap.ToBitmap());
                            swfTags[i] = new TagWrapper(imageTag);
                        }
                        if (tag.TagType == TagType.DefineSound)
                        {
                            var defineSoundTag = tag.Tag as DefineSoundTag;

                            if (defineSoundTag.SoundFormat == SoundCompressionType.MP3)
                            {
                                defineSoundTag.SoundData = nodeBytes;
                            }
                            else if (defineSoundTag.SoundFormat == SoundCompressionType.UncompressedLE)
                            {
                                var ms = new MemoryStream(nodeBytes);
                                var br = new BinaryReader(ms);
                                //skip to DATA tag of riff wav
                                string riff = br.ReadStringFixedSize(4);
                                int fileLength = br.ReadInt32();
                                string wave = br.ReadStringFixedSize(4);
                                
                                string tagName;
                                int tagLength;

                                while (ms.Position < ms.Length)
                                {
                                    tagName = br.ReadStringFixedSize(4);
                                    tagLength = br.ReadInt32();
                                    if (tagName == "data")
                                    {
                                        defineSoundTag.SoundData = br.ReadBytes(tagLength);
                                        break;
                                    }
                                    else
                                    {
                                        br.BaseStream.Position += tagLength;
                                    }
                                }
                                swfTags[i] = new TagWrapper(defineSoundTag);
                            }
                        }
                    }
                }

                MemoryStream output = new MemoryStream();
                swfTags.WriteSwf(output);
                return output.ToArray();
            }*/

            private static int GetNumberFromString(string fileName)
            {
                //find a digit
                int i = 0;
                for (i = 0; i < fileName.Length; i++)
                {
                    char c = fileName[i];
                    if (c >= '0' && c <= '9')
                    {
                        break;
                    }
                }
                if (i >= fileName.Length)
                {
                    return -1;
                }
                int firstDigitIndex = i;

                //find a non-digit
                for (; i < fileName.Length; i++)
                {
                    char c = fileName[i];
                    if (c >= '0' && c <= '9')
                    {

                    }
                    else
                    {
                        break;
                    }
                }
                int firstNonDigitIndex = i;

                string digits = fileName.Substring(firstDigitIndex, firstNonDigitIndex - firstDigitIndex);
                int value;
                if (int.TryParse(digits, out value))
                {
                    return value;
                }
                return -1;
            }

        }
    }
}
