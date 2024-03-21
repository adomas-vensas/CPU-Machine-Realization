using MainStage.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MainStage;

public class VirtualMachine : IInstructions, IVMRegisters, INotifyPropertyChanged
{
    #region Fields

    private readonly IResourceAllocator _parent;

    #endregion Fields

    #region Properties
    protected Dictionary<int, int> Memory { get; set; }
    //protected RealMachine Parent => _parent;

    private int _r = 0;
    public int R
    {
        get => _r;
        set
        {
            _r = value % 10000;
            OnPropertyChanged();
        }
    }

    private int _ic = 0;
    public int IC
    {
        get => _ic;
        set
        {
            _ic = value % 100;
            OnPropertyChanged();
        }
    }

    private bool _c = false;
    public bool C
    {
        get => _c;
        set
        {
            _c = value;
            OnPropertyChanged();
        }
    }

    #endregion Properties

    #region Constructors

    protected VirtualMachine(IResourceAllocator machine, int memorySize)
    {
        _parent = machine;
        Memory = Enumerable.Range(0, memorySize).ToDictionary(x => x, y => 0);
    }

    #endregion Constructors

    public void Add(int x, int y)
    {
        int address = GetMemAddress(x, y);
        int value = Memory[address];

        R += value;

        _parent.Test(this);
    }

    public void Count(int x, int y)
    {
        if(C)
        {
            int address = GetMemAddress(x, y);
            IC = Memory[address];
        }

        _parent.Test(this);
    }

    public void Flip(int x, int y)
    {
        int address = GetMemAddress(x, y);

        C = (R == Memory[address]);

        _parent.Test(this);
    }

    public void Go(int x, int y)
    {
        IC = GetMemAddress(x, y);

        _parent.Test(this);
    }

    public void Halt()
    {
        _parent.Dispose(this);
    }

    public void Load(int x, int y)
    {
        int address = GetMemAddress(x, y);
        int value = Memory[address];

        R = value;

        _parent.Test(this);
    }

    public void Not(int x, int y)
    {
        if(!C)
        {
            IC = GetMemAddress(x, y);
        }

        _parent.Test(this);
    }

    public void Read(int x)
    {
        throw new NotImplementedException();
    }

    public void Send(int x)
    {
        throw new NotImplementedException();
    }

    public void Store(int x, int y)
    {
        int address = GetMemAddress(x, y);

        Memory[address] = R;

        _parent.Test(this);
    }

    private int GetMemAddress(int x, int y)
    {
        return x * 10 + y;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
