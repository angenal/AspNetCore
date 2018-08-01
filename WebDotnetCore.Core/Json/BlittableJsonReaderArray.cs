using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using WebCore.Collections;
using WebCore.Json.Parsing;

namespace WebCore.Json
{
    public unsafe class BlittableJsonReaderArray : BlittableJsonReaderBase, IEnumerable<object>, IDisposable
    {
        private readonly int _count;
        private readonly byte* _metadataPtr;
        private readonly byte* _dataStart;
        private readonly long _currentOffsetSize;
        private Dictionary<int, Tuple<object, BlittableJsonToken>> _cache;

        public DynamicJsonArray Modifications;

        public BlittableJsonReaderObject Parent => _parent;
      
        public BlittableJsonReaderArray(int pos, BlittableJsonReaderObject parent, BlittableJsonToken type)
            : base (parent._context)
        {
            _parent = parent;

            byte arraySizeOffset;
            _count = parent.ReadVariableSizeInt(pos, out arraySizeOffset);

            _dataStart = parent.BasePointer + pos;
            _metadataPtr = _dataStart + arraySizeOffset;

            // analyze main object type and it's offset and propertyIds flags
            _currentOffsetSize = ProcessTokenOffsetFlags(type);
        }

        public byte* DataStart => _dataStart;
        public int Length => _count;

        public override string ToString()
        {
            using (var memoryStream = new MemoryStream())
            using(var tw = new BlittableJsonTextWriter(_context,memoryStream))
            {
                tw.WriteValue(BlittableJsonToken.StartArray,this);
                tw.Flush();
                memoryStream.Position = 0;

                return new StreamReader(memoryStream).ReadToEnd();
            }
        }
        public void BlittableValidation()
        {
            _parent?.BlittableValidation();
        }
        
        public void Dispose()
        {            
            _parent?.Dispose();
        }

        public BlittableJsonToken GetArrayType()
        {
            var blittableJsonToken = (BlittableJsonToken)(*(_metadataPtr + _currentOffsetSize)) & TypesMask;
            Debug.Assert(blittableJsonToken != 0);
            return blittableJsonToken;
        }

        public object this[int index] => GetValueTokenTupleByIndex(index).Item1;

        public T GetByIndex<T>(int index)
        {
            var obj = GetValueTokenTupleByIndex(index).Item1;
            T result;
            BlittableJsonReaderObject.ConvertType(obj, out result);
            return result;
        }

        public string GetStringByIndex(int index)
        {
            var obj = GetValueTokenTupleByIndex(index).Item1;
            if (obj == null)
                return null;

            if (obj is LazyStringValue lazyStringValue)
                return (string)lazyStringValue;
            if (obj is LazyCompressedStringValue lazyCompressedStringValue)
                return lazyCompressedStringValue;
            BlittableJsonReaderObject.ConvertType(obj, out string result);
            return result;

        }

        public void AddItemsToStream<T>(ManualBlittableJsonDocumentBuilder<T> writer) where T : struct, IUnmanagedWriteBuffer
        {
            for (var i = 0; i < _count; i++)
            {
                var tuple = GetValueTokenTupleByIndex(i);
                writer.WriteValue(tuple.Item2, tuple.Item1);
            }
        }

        public Tuple<object, BlittableJsonToken> GetValueTokenTupleByIndex(int index)
        {
            // try get value from cache, works only with Blittable types, other objects are not stored for now
            Tuple<object, BlittableJsonToken> result;
            if (NoCache == false && _cache != null && _cache.TryGetValue(index, out result))
                return result;

            if (index >= _count || index < 0)
                throw new IndexOutOfRangeException($"Cannot access index {index} when our size is {_count}");

            var itemMetadataStartPtr = _metadataPtr + index * (_currentOffsetSize + 1);
            var offset = ReadNumber(itemMetadataStartPtr, _currentOffsetSize);
            var token = *(itemMetadataStartPtr + _currentOffsetSize);
            result = Tuple.Create(_parent.GetObject((BlittableJsonToken)token,
                (int)(_dataStart - _parent.BasePointer - offset)), (BlittableJsonToken)token & TypesMask);

            if (result.Item1 is BlittableJsonReaderBase blittableJsonReaderBase)
            {
                blittableJsonReaderBase.NoCache = NoCache;
                if (NoCache == false)
                {
                    if (_cache == null)
                    {
                        _cache = new Dictionary<int, Tuple<object, BlittableJsonToken>>(NumericEqualityComparer.BoxedInstanceInt32);
                    }
                    _cache[index] = result;
                }
            }
            return result;
        }

        public IEnumerable<object> Items
        {
            get
            {
                for (int i = 0; i < _count; i++)
                    yield return this[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<object> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var array = obj as BlittableJsonReaderArray;

            if (array != null)
                return Equals(array);

            return false;
        }

        protected bool Equals(BlittableJsonReaderArray other)
        {
            if (_count != other._count)
                return false;

            for (int i = 0; i < _count; i++)
            {
                var x = this[i];
                var y = other[i];

                if (x == null && y == null)
                    continue;

                if ((x?.Equals(y) ?? false) == false)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _count;
        }
    }
}
