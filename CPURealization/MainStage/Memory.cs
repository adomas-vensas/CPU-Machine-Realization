﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage;

public class Memory<TKey, TValue> : Dictionary<TKey, TValue>
{
    public EventHandler MaxSizedReached;

    private readonly TValue _defaultValue;

    #region Properties

    public int MaxSize { get; set; }
    public int BlockSize { get; set; }

    public new TValue this[TKey key]
    {
        get => base[key];
        set
        {
            if (!base.ContainsKey(key) && this.Count >= MaxSize && MaxSizedReached == null)
            {
                throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
            }
            else if(!base.ContainsKey(key) && this.Count >= MaxSize)
            {
                MaxSizedReached?.Invoke(this, new EventArgs());

                if (!base.ContainsKey(key) && this.Count >= MaxSize)
                {
                    throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
                }
            }

            base[key] = value;
        }
    }

    #endregion Properties

    #region Constructors

    public Memory(int maxSize, int blockSize, Func<int, TKey> keyGenerator, TValue initialValue)
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
        if(this.Count >= MaxSize && MaxSizedReached == null)
        {
            throw new InvalidOperationException($"Cannot add more than {MaxSize} elements.");
        }
        else if(this.Count >= MaxSize)
        {
            MaxSizedReached?.Invoke(this, new EventArgs());

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
