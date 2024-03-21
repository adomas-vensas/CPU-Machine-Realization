using MainStage.Enumerators;
using MainStage.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MainStage;

public class RealMachine : IRMRegisters, IResourceAllocator, INotifyPropertyChanged
{
    #region Fields

    private List<VirtualMachine> _virtualMachines = new List<VirtualMachine>();
    private Dictionary<int, int> _memory = new Dictionary<int, int>();

    #endregion Fields

    #region Properties

    private int _ptr = 0;
    public int PTR
    {
        get => _ptr;
        set
        {
            _ptr = value % 10000;
            OnPropertyChanged();
        }
    }

    public ModeType MODE { get; set; }

    private int _pi = 0;
    public int PI
    {
        get => _pi;
        set
        {
            _pi = value % 10;
            OnPropertyChanged();
        }
    }

    private int _si = 0;
    public int SI
    {
        get => _si;
        set
        {
            _si = value % 10;
            OnPropertyChanged();
        }
    }

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

    public RealMachine()
    { }

    #endregion Constructors

    #region Methods

    public void Test(VirtualMachine machine)
    {

    }

    public void ProvideMemory(VirtualMachine machine)
    {

    }

    public void Dispose(VirtualMachine machine)
    {

    }

    #endregion Methods

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
    }

    
}
