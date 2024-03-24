using MainStage.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface IResourceAllocator
{
    /// <summary>
    /// After every instruction checks if any interrupts happened.
    /// </summary>
    public void Test(VirtualMachine machine);
    
    /// <summary>
    /// Asks parrent machine to provide it with more memory.
    /// </summary>
    /// <param name="machine"></param>
    public void ProvideMemory(VirtualMachine machine);


    /// <summary>
    /// Dispose of the current virtual machine.
    /// </summary>
    /// <param name="machine"></param>
    public void Dispose(VirtualMachine machine);

    public void SetInterrupt(int interruptCode);

}
