using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;

namespace ALDExplorer
{
    class DisposableCollection<T> : Collection<T>, IDisposable where T : class, IDisposable
    {
        public DisposableCollection()
            : base()
        {

        }

        public DisposableCollection(IEnumerable<T> sequence)
            : this()
        {
            AddRange(sequence);
        }

        public void AddRange(IEnumerable<T> sequence)
        {
            var list = this.Items as List<T>;
            if (list != null)
            {
                list.AddRange(sequence);
            }
            else
            {
                foreach (var item in sequence)
                {
                    Add(item);
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in this)
            {
                if (item != null)
                {
                    item.Dispose();
                }
            }
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                if (item != null)
                {
                    item.Dispose();
                }
            }
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            var oldItem = this[index];
            if (oldItem != null)
            {
                oldItem.Dispose();
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            if (oldItem != null)
            {
                oldItem.Dispose();
            }
            base.SetItem(index, item);
        }
    }
}
