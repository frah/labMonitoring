using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LabMonitoring
{
    /// <summary>
    /// キーと値のペアを保持するキュー
    /// </summary>
    /// <typeparam name="TKey">ディクショナリ内のキーの型</typeparam>
    /// <typeparam name="TValue">ディクショナリ内の値の型</typeparam>
    public class DictionaryQueue<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private Queue<TKey> _queue;
        private readonly object lockObj = new object();

        /// <summary>
        /// 空で、既定の初期量を備え、キーの型の既定の等値比較演算子を使用する、<b>DictionaryQueue</b> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public DictionaryQueue()
            : base()
        {
            _queue = new Queue<TKey>();
        }

        /// <summary>
        /// 空で、指定した初期量を備え、キーの型の既定の等値比較演算子を使用する、<b>DictionaryQueue</b> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="capacity">DictionaryQueue が格納できる要素数の初期値</param>
        public DictionaryQueue(int capacity)
            : base(capacity)
        {
            _queue = new Queue<TKey>(capacity);
        }

        /// <summary>
        /// 指定したキーと値をディクショナリに追加します。  
        /// </summary>
        /// <param name="key">追加する要素のキー。</param>
        /// <param name="value">追加する要素の値。参照型の場合は、値を null 参照 (Visual Basic では Nothing) にできます。</param>
        new public void Add(TKey key, TValue value)
        {
            lock (lockObj)
            {
                base.Add(key, value);
                _queue.Enqueue(key);
            }
        }

        /// <summary>
        /// 指定したキーを持つ値を <b>DictionaryQueue</b> から削除します。 
        /// </summary>
        /// <param name="key">削除する要素のキー。</param>
        /// <returns>要素が見つかり、正常に削除された場合は <b>true</b>。それ以外の場合は <b>false</b>。このメソッドは、key が <b>DictionaryQueue</b> に見つからない場合、false を返します。</returns>
        new public bool Remove(TKey key)
        {
            lock (lockObj)
            {
                return base.Remove(key);
            }
        }

        /// <summary>
        /// <b>DictionaryQueue</b> からすべてのキーと値を削除します。  
        /// </summary>
        new public void Clear()
        {
            lock (lockObj)
            {
                base.Clear();
                _queue.Clear();
            }
        }

        /// <summary>
        /// <b>DictionaryQueue</b> の先頭にあるオブジェクトを削除せずに返します。 
        /// </summary>
        /// <returns><b>DictionaryQueue</b> の先頭にあるオブジェクト</returns>
        /// <exception cref="System.InvalidOperationException"><b>DictionaryQueue</b> が空です。</exception>
        public TValue Peek()
        {
            lock (lockObj)
            {
                TKey key = default(TKey);
                try
                {
                    key = _queue.Peek();
                    while (!base.ContainsKey(key))
                    {
                        _queue.Dequeue();
                        key = _queue.Peek();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    throw ex;
                }

                try
                {
                    return base[key];
                }
                catch (KeyNotFoundException ex)
                {
                    throw new InvalidOperationException("", ex);
                }
            }
        }

        /// <summary>
        /// <b>DictionaryQueue</b> の先頭にあるオブジェクトを削除し、返します。
        /// </summary>
        /// <returns><b>DictionaryQueue</b> の先頭にあるオブジェクト</returns>
        /// <exception cref="System.InvalidOperationException"><b>DictionaryQueue</b> が空です。</exception>
        public TValue Dequeue()
        {
            try
            {
                TValue val = Peek();
                lock (lockObj)
                {
                    base.Remove(_queue.Dequeue());
                }
                return val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
