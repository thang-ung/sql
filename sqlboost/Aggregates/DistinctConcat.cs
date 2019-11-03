using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SQLBoost.TSQL.Master.Aggregates
{

	[Serializable]
	[SqlUserDefinedAggregate(
		Format.UserDefined, //use clr serialization to serialize the intermediate result
		IsInvariantToNulls =true, //optimizer property
		IsInvariantToDuplicates =false, //optimizer property
		IsInvariantToOrder =false, //optimizer property
		MaxByteSize =-1) //maximum size in bytes of persisted value
	]
	public class DistinctConcat : IBinarySerialize
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private List<string>	_sb;
		private string			_delimit=null;
		private Regex			_regx;

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		public void Init(){
			_sb = new List<string>();
		}

		/// <summary>
		/// Accumulate the next value, not if the value is null
		/// </summary>
		/// <param name="value"></param>
		public void Accumulate(SqlString value, string delimit = "&"){
			if(_delimit == null) _delimit = delimit;
			if(!(value.IsNull || value.ToString().Length == 0 || _sb.Contains(value.ToString())))
				_sb.Add( value.ToString() );
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(DistinctConcat other){
//			_sb.AddRange(other._sb);
		}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		[return: SqlFacet(MaxSize = -1)]
		public SqlString Terminate()
		{
			return new SqlString(String.Join(_delimit, _sb.ToArray()));
		}

		// IBinarySerialize
		public void Read(BinaryReader r){
			_sb = new List<string>(){r.ReadString()};
		}

		public void Write(BinaryWriter w){
			w.Write(Terminate().ToString());
		}

	}

}