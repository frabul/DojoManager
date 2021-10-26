using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DojoManagerApi
{
    public class LinkedObservableCollection<T> : ObservableCollection<T>
    {
        public IList<T> Origin;
        bool Synching = false;
        public LinkedObservableCollection(IList<T> li) : base()
        {
            Origin = li;
            foreach (var it in li)
                this.Add(it);
            Synching = true;
        }

        protected override void ClearItems()
        {
            Origin.Clear();
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (Synching)
                Origin.Insert(index, item);
            var wrapped = (T)EntityWrapper.Wrap(item);
            base.InsertItem(index, wrapped);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        protected override void RemoveItem(int index)
        {
            if (Synching)
                Origin.RemoveAt(index);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (Synching)
                Origin[index] = item;
            var wrapped = (T)EntityWrapper.Wrap(item);
            base.SetItem(index, wrapped);
        }

    }
}
