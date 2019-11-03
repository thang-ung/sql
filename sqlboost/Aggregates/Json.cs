using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Collections.Generic;

namespace SQLBoost.TSQL.Master.Aggregates{

	[Serializable]
	[SqlUserDefinedAggregate(
		Format.UserDefined, //use clr serialization to serialize the intermediate result
		IsInvariantToNulls =true, //optimizer property
		IsInvariantToDuplicates =false, //optimizer property
		IsInvariantToOrder =false, //optimizer property
		MaxByteSize =-1) //maximum size in bytes of persisted value
	]
	public class Json : IBinarySerialize
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private List<string> _sb;
		private string		_fmt =null;

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
		public void Accumulate(string format ="value:\"{0}\"", string val00 =null,
				string val01 =null,
				string val02 = null,
				string val03 = null,
				string val04 = null,
				string val05 = null,
				string val06 = null,
				string val07 = null,
				string val08 = null,
				string val09 =null){
			if(_fmt ==null) _fmt = format;
			try{
				_sb.Add(string.Format(format, val00,
						val01, val02, val03,
						val04, val05, val06,
						val07, val08, val09) );
			}
			catch{}
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(Json other){
//			_sb.AddRange(other._sb);
		}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		[return: SqlFacet(MaxSize = -1)]
		public SqlString Terminate()
		{
			return new SqlString(string.Join(",", _sb.ToArray()));
		}

		// IBinarySerialize
		public void Read(BinaryReader r){
			_sb = new List<string>() { r.ReadString() };
		}

		public void Write(BinaryWriter w){
			w.Write("["+ Terminate().ToString() +"]");
		}
	}

}