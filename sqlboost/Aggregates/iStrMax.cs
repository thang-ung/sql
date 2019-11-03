using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace SQLBoost.TSQL.Master.Aggregates{

	[Serializable]
	[SqlUserDefinedAggregate(
		Format.UserDefined, //use clr serialization to serialize the intermediate result
		IsInvariantToNulls = true, //optimizer property
		IsInvariantToDuplicates = false, //optimizer property
		IsInvariantToOrder = false, //optimizer property
		MaxByteSize = -1) //maximum size in bytes of persisted value
	]
	public class iStrMax : IBinarySerialize
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private int?		_max =null;
		private SqlString	_ret;

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		public void Init(){}

		/// <summary>
		/// Accumulate the next value, not if the value is null
		/// </summary>
		/// <param name="value"></param>
		public void Accumulate(int value, SqlString returner){
			if( _max == null || value.CompareTo(_max) > 0 ){
				_max =value;
				_ret =returner;
			}
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(iStrMax other){}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		[return: SqlFacet(MaxSize = -1)]
		public SqlString Terminate(){
			return _ret;
		}

		// IBinarySerialize
		public void Write(BinaryWriter w){
			var	s =Terminate();
			if( !s.IsNull )	w.Write( s.ToString() );
		}

		public void Read(BinaryReader r){
			if( r.BaseStream.Length > 0 )
				_ret =r.ReadString();
		}

	}

}