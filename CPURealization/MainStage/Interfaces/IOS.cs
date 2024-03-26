using MainStage.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces
{
    public interface IOS
    {
        public void GiveControl(int interruptCode);

        /// <summary>
        /// Asks parrent machine to provide it with more memory.
        /// </summary>
        /// <param name="machine"></param>
        public void ProvideMemory(VirtualMachine machine);
    }
}
