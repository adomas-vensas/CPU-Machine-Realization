﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface IVMRegisters
{
    public string R { get; set; }
    public string IC { get; set; }
    public bool C { get; set; }
}
