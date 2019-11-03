using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Runtime.InteropServices;

namespace SQLBoost.TSQL.Master.Aggregates {

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedAggregate(Format.UserDefined,
        IsInvariantToNulls = true, //optimizer property
		IsInvariantToDuplicates = false, //optimizer property
		IsInvariantToOrder = false, //optimizer property
		MaxByteSize = -1)]
    public class TransMin
    {
        object _ret, _curr;
        public void Init(){}

        public void Accumulate(object value, object returner){
            var icmp = (IComparable)value;
            if (_curr == null || icmp.CompareTo(_curr) > 0) {
                _curr = icmp;
                _ret = returner;
            }
        }

        public void Merge(TransMin Group){
            // Put your code here
        }

        public object Terminate() {
            return _ret;
        }

        // This is a place-holder member field
        public int _var1;
    }
}
