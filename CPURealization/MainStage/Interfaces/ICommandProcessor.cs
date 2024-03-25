using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface ICommandProcessor
{
    public void ParseInput(string commandString);

}
