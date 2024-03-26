using MainStage.Enumerators;
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
    /// Dispose of the current virtual machine.
    /// </summary>
    /// <param name="machine"></param>
    public void Dispose(VirtualMachine machine);

    public void ProvideMemory(VirtualMachine machine);
    public void DisposeMemory(VirtualMachine machine);

    public void SetInterrupt(InterruptType interrupt);

    public void UpdateMemory(VirtualMachine machine, string firstWord, string secondWord);
}
