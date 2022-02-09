using System;
using System.Collections.Generic;

namespace WebCore.Collections
{
    public class ConcurrentList<T>
    {
		public int Count
		{
			get
			{
				return ReadList.Count;
			}
		}

		protected List<T> ReadList
		{
			get
			{
				if (dirty)
				{
					object obj = modifyLock;
					lock (obj)
					{
						readList = new List<T>(writeList);
					}
				}
				return readList;
			}
		}

		public T this[int key]
		{
			get
			{
				return ReadList[key];
			}
			set
			{
				object obj = modifyLock;
				lock (obj)
				{
					writeList[key] = value;
					readList[key] = value;
				}
			}
		}

		public void Add(T value)
		{
			object obj = modifyLock;
			lock (obj)
			{
				writeList.Add(value);
				dirty = true;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ReadList.GetEnumerator();
		}

		public void Remove(T value)
		{
			object obj = modifyLock;
			lock (obj)
			{
				writeList.Remove(value);
				dirty = true;
			}
		}

		public void Insert(int idx, T value)
		{
			object obj = modifyLock;
			lock (obj)
			{
				writeList.Insert(idx, value);
				dirty = true;
			}
		}

		public void RemoveAt(int idx)
		{
			object obj = modifyLock;
			lock (obj)
			{
				writeList.RemoveAt(idx);
				dirty = true;
			}
		}

		private List<T> writeList = new List<T>();

		private List<T> readList = new List<T>();

		private object modifyLock = new object();

		private bool dirty;
	}
}
