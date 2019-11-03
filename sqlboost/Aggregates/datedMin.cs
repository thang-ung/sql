using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Runtime.InteropServices;

namespace SQLBoost.TSQL.Master.Aggregates{

	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	[SqlUserDefinedAggregate(
		Format.Native, //use clr serialization to serialize the intermediate result
		IsInvariantToNulls = true, //optimizer property
		IsInvariantToDuplicates = false, //optimizer property
		IsInvariantToOrder = false) //optimizer property
	]
	public class datedMin
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private SqlDateTime	_mind;
		private SqlInt32	_ret;

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		public void Init(){}

		/// <summary>
		/// Accumulate the next value, not if the value is null
		/// </summary>
		/// <param name="value"></param>
		public void Accumulate(SqlDateTime value, SqlInt32 returner){
			if( _mind.IsNull || value.CompareTo(_mind) < 0 ){
				_mind	=value;
				_ret	=returner;
			}
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(datedMin other){
			_mind =other._mind;
			_ret =other._ret;
		}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		public SqlInt32 Terminate(){
			return _ret;
		}

	}

}