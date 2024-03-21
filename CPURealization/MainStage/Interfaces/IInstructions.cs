using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface IInstructions
{
    /// <summary>
    /// LOAD x y – loads a number from memory address x*10+y to register R
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Load(int x, int y);

    /// <summary>
    /// 	STORE x y – stores R register’s value to memory address x*10+y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Store(int x, int y);

    /// <summary>
    /// 	ADD x y – add R register’s value and the value at memory address x*10+y. The result is placed in register R
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Add(int x, int y);

    /// <summary>
    /// 	FLIP x y - if R register has value at x*10+y, then assign “true” to register C, otherwise – “false”
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Flip(int x, int y);

    /// <summary>
    /// 	COUNT x y – if register C is equal to “true”, then the value of memory address
    /// [x*10+y] will be assigned to IC
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Count(int x, int y);

    /// <summary>
    /// 	READ x – reads 10 words (40 bytes) from the input stream and places them into memory addresses [x*10+i], where i = [0…9]
    /// </summary>
    /// <param name="x"></param>
    public void Read(int x);

    /// <summary>
    /// 	SEND x – sends 10 words (40 bytes) to the output stream from memory addresses
    ///[x*10+i], where i = [0…9]
    /// </summary>
    /// <param name="x"></param>
    public void Send(int x);

    /// <summary>
    /// 	NOT x y. If C != “true” then IC = xy
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Not(int x, int y);

    /// <summary>
    /// 	GO x y. Assign value xy to register IC
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Go(int x, int y);

    /// <summary>
    /// 	HALT – virtual machine’s program’s end
    /// </summary>
    public void Halt();

}
