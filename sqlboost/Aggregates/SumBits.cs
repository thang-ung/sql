using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
[Serializable]
[SqlUserDefinedAggregate(
        Format.Native, //use clr serialization to serialize the intermediate result
        IsInvariantToNulls = true, //optimizer property
        IsInvariantToDuplicates = false, //optimizer property
        IsInvariantToOrder = false) //optimizer property
    ]

public struct SumBits
{
    private SqlInt32 _iVal;
    public void Init()
    {
        _iVal = 0;
    }

    public void Accumulate(SqlInt32 Value)
    {
        _iVal |= Value;
    }

    public void Merge(SumBits Group)
    {
        _iVal = Group._iVal;
    }

    public SqlInt32 Terminate ()
    {
        // Put your code here
        return _iVal;
    }

}
