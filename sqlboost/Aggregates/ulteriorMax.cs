using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SQLBoost.TSQL.Master{

	public class ulteriorMax<T,R> : IBinarySerialize
	where T: System.ValueType, INullable
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private SortedDictionary<T,R> _sb;

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		public void Init(){
			_sb = new SortedDictionary<T,R>(new ReverseComparer<T>());
		}

		/// <summary>
		/// Accumulate the next value, not if the value is null
		/// </summary>
		/// <param name="value"></param>
		public void Accumulate(T value, R returner){
			if(!value.IsNull)
				_sb[value] =returner;
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(ulteriorMax<T,R> other){}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		[return: SqlFacet(MaxSize = -1)]
		public R Terminate(){
			foreach(var k in _sb.Keys)
				return _sb[ k ];

			return 0;
		}

		// IBinarySerialize
		public void Read(BinaryReader r){
			_sb = new SortedDictionary<T,R>(new ReverseComparer<T>());
			if( r.BaseStream.Length > 0 )
				_sb[new T()]=r.ReadString();
		}

		public void Write(BinaryWriter w){
			var	s =Terminate().ToString();
			if(s !=null)	w.Write(s);
		}

	}

}