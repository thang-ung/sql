using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using SQLBoost.TSQL.Master.Classes;
using System.Runtime.InteropServices;

namespace SQLBoost.TSQL.Master.Aggregates{

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    [SqlUserDefinedAggregate(
        Format.UserDefined, //use clr serialization to serialize the intermediate result
        IsInvariantToNulls = true, //optimizer property
        IsInvariantToDuplicates = false, //optimizer property
        IsInvariantToOrder = false, //optimizer property
        MaxByteSize = -1) //maximum size in bytes of persisted value
    ]
    public class TransMax : IBinarySerialize{
        /// <summary>
        /// The variable that holds the intermediate result of the concatenation
        /// </summary>
        private IComparable _mind =null;
        private object      _ret;

        /// <summary>
        /// Initialize the internal data structures
        /// </summary>
        public void Init(){}

        /// <summary>
        /// Accumulate the next value, not if the value is null
        /// </summary>
        /// <param name="value"></param>
        public void Accumulate(object value, object returner){
            if( value.GetType().Name == "DBNull" ) return;
       
            var icmp = (IComparable)value;
            if(_mind == null || icmp.CompareTo(_mind) > 0){
                _mind = icmp;
                _ret = returner;
            }
        }

        /// <summary>
        /// Merge the partially computed aggregate with this aggregate.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(TransMax other){ }

        /// <summary>
        /// Called at the end of aggregation, to return the results of the aggregation.
        /// </summary>
        [return: SqlFacet(MaxSize = -1)]
        public object Terminate(){
            return _ret;
        }

        // IBinarySerialize
        public void Write(BinaryWriter w){
            var s = Terminate();
            if(s != null) w.Write( s.ToString() );
        }

        public void Read(BinaryReader r){
            if(r.BaseStream.Length > 0)
                _ret = r.ReadString();
        }

    }//end TransMax
}