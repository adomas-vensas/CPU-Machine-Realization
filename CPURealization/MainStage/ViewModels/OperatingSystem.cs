using MainStage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.ViewModels
{
    public class OperatingSystem : IOS
    {
        public void GiveControl(int interruptCode)
        {
            Console.WriteLine("Control granted with code: {0}", interruptCode);
        }
    }
}
