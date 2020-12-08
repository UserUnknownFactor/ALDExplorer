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
    public class XcfHeader : ICloneable
    {
        public List<Tag> tags;
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
            var xcf = this;
            return true;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
}