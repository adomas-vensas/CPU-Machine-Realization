using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Components
{
    public class CentralProcessingUnit
    {
        private uint PTR;
        private uint R;
        private ushort IC;
        private byte C;
        private byte MODE;
        private uint PI;
        private uint SI;
        private uint TI;
        public uint[] Memory = new uint[300];


        public CentralProcessingUnit()
        {

        }

        public void Load(byte x, byte y)
        {
            R = Memory[x * 10 + y];
        }

        public void Store(byte x, byte y)
        {
            Memory[x * 10 + y] = R;
        }

        public void Add(byte x, byte y)
        {
            R += Memory[x * 10 + y];
        }

        public void Flip(byte x, byte y)
        {
            C = R == Memory[x * 10 + y] ? (byte)1 : (byte)0;
        }

        public void Count(byte x, byte y)
        {
            IC = C == 1 ? C : IC;
        }

        public void Read(byte x)
        {
            //What could be considered an input stream?
        }

        public void Send(byte x)
        {
            //What could be considered an output stream?
        }

        public void Not(byte x, byte y)
        {
            IC = C == 1 ? (ushort)(x * y) : IC;
        }

        public void Go(byte x, byte y)
        {
            IC = (ushort)(x * y);
        }

        public void Halt()
        {
            //How do we end?
        }

    }
}
