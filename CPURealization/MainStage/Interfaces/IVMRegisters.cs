using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface IVMRegisters
{
    public int R { get; set; }
    public int IC { get; set; }
    public bool C { get; set; }
}
