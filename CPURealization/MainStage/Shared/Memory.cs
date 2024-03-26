using MainStage.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage;

public class Memory<TKey, TValue> : Dictionary<TKey, TValue>
{
    public event EventHandler MaxSizeReached;
    public event EventHandler MemoryOutOfRange;  

    private readonly TValue _defaultValue;

    private readonly Dictionary<int, Action<int>> _interruptTable;

    #region Properties

    public int MaxSize { get; set; }
    public int BlockSize { get; set; }

    public new TValue this[TKey key]
    {
        get
        {
            if(!base.ContainsKey(key))
            {
                MemoryOutOfRange?.Invoke(this, EventArgs.Empty);
                return default(TValue);
            }

            return base[key];
        }
        set
        {
            if (!base.ContainsKey(key) && this.Count >= MaxSize && MaxSizeReached == null)
            {
                throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
            }
            else if(!base.ContainsKey(key) && this.Count >= MaxSize)
            {
                MaxSizeReached?.Invoke(this, new EventArgs());

                if (!base.ContainsKey(key) && this.Count >= MaxSize)
                {
                    throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
                }
            }

            base[key] = value;
        }
    }

    public Dictionary<int, Action<int>> InterruptTable
    {
        get => _interruptTable;

//        _interruptTable = new Dictionary<int, Action<int>>
//        {
//            { 1, new Action<int>(_os.GiveControl)
//},
//            { 2, new Action<int>(_os.GiveControl) },
//            { 3, new Action<int>(_os.GiveControl) },
//            { 4, new Action<int>(_os.GiveControl) },
//            { 5, new Action<int>(_os.GiveControl) },
//            { 6, new Action<int>(_os.GiveControl) },
//            { 7, new Action<int>(_os.GiveControl) },
//            { 8, new Action<int>(_os.GiveControl) }
//        };
    }

    #endregion Properties

    #region Constructors

    public Memory(int maxSize, int blockSize, Func<int, TKey> keyGenerator,
        TValue initialValue)
    {
        if(maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max Size must be greater than 0");
        }

        MaxSize = maxSize;
        BlockSize = blockSize;
        _defaultValue = initialValue;


        for (int i = 0; i < MaxSize; ++i)
        {
            this.Add(keyGenerator(i), initialValue);
        }

    }

    #endregion Constructors

    public new void Add(TKey key, TValue value)
    {
        if(this.Count >= MaxSize && MaxSizeReached == null)
        {
            throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
        }
        else if(this.Count >= MaxSize)
        {
            MaxSizeReached?.Invoke(this, new EventArgs());

            if(this.Count >= MaxSize)
            {
                throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
            }
        }

        base.Add(key, value);
    }

    public new bool Remove(TKey key)
    {
        if(!this.ContainsKey(key))
        {
            return false;
        }

        this[key] = _defaultValue;

        return true;
    }

}
