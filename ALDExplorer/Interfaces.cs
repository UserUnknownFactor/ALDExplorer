using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ALDExplorer
{
    /// <summary>
    /// An object that has an array Index property.
    /// </summary>
    public interface IWithIndex
    {
        /// <summary>
        /// The array index of the object.
        /// </summary>
        int Index { get; set; }
    }

    /// <summary>
    /// An object that has a parent
    /// </summary>
    public interface IWithParent
    {
        /// <summary>
        /// The parent of this object.
        /// </summary>
        object Parent { get; set; }
    }

    /// <summary>
    /// An object that has a parent
    /// </summary>
    /// <typeparam name="T">The type of the parent</typeparam>
    public interface IWithParent<T> : IWithParent
    {
        /// <summary>
        /// The parent of this object.
        /// </summary>
        new T Parent { get; set; }
    }

}
