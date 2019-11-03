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
	public class DateStrMax : IBinarySerialize
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private SqlDateTime	_mind;
		private string	_ret;

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		public void Init(){}

		/// <summary>
		/// Accumulate the next value, not if the value is null
		/// </summary>
		/// <param name="value"></param>
		public void Accumulate(SqlDateTime value, string returner){
			if( _mind.IsNull || value.CompareTo(_mind) > 0 ){
				_mind	=value;
				_ret	=returner;
			}
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(DateStrMax other){}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		[return: SqlFacet(MaxSize = -1)]
		public string Terminate(){
			return _ret;
		}

		// IBinarySerialize
		public void Write(BinaryWriter w){
			var	s =Terminate();
			if(s !=null)	w.Write(s);
		}

		public void Read(BinaryReader r){
			if( r.BaseStream.Length > 0 )
				_ret =r.ReadString();
		}

	}

}