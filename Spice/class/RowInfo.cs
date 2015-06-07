using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spice
{
    class RowInfo {
	public static readonly int ROW_NORMAL = 0;  // ordinary value
	public static readonly int ROW_CONST  = 1;  // value is constant
	public static readonly int ROW_EQUAL  = 2;  // value is equal to another value
	public int nodeEq, type, mapCol, mapRow;
	public double value;
    public bool rsChanges; // row's right side changes
    public bool lsChanges; // row's left side changes
    public bool dropRow;   // row is not needed in matrix
	public RowInfo() { type = ROW_NORMAL; }
    }
}
