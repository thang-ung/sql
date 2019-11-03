using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Collections.Generic;

namespace SQLBoost.TSQL.Master.Aggregates{
	public sealed class ReverseComparer<T> : IComparer<T>{
		private readonly IComparer<T> inner;
		public ReverseComparer() : this(null){ }
		public ReverseComparer(IComparer<T> inner){
			this.inner = inner ?? Comparer<T>.Default;
		}
		int IComparer<T>.Compare(T x, T y){ return inner.Compare(y, x); }
	}

	[Serializable]
	[SqlUserDefinedAggregate(
		Format.UserDefined, //use clr serialization to serialize the intermediate result
		IsInvariantToNulls = true, //optimizer property
		IsInvariantToDuplicates = false, //optimizer property
		IsInvariantToOrder = false, //optimizer property
		MaxByteSize = -1) //maximum size in bytes of persisted value
	]
	public class paraMax : IBinarySerialize
	{
		/// <summary>
		/// The variable that holds the intermediate result of the concatenation
		/// </summary>
		private string	_mins;
		private int		_ret;

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		public void Init(){}

		/// <summary>
		/// Accumulate the next value, not if the value is null
		/// </summary>
		/// <param name="value"></param>
		public void Accumulate(SqlString value, int returner){
			if( _mins == null || value.CompareTo(_mins) > 0 ){
				_mins =value.ToString();
				_ret =returner;
			}
		}

		/// <summary>
		/// Merge the partially computed aggregate with this aggregate.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(paraMax other){}

		/// <summary>
		/// Called at the end of aggregation, to return the results of the aggregation.
		/// </summary>
		[return: SqlFacet(MaxSize = -1)]
		public int Terminate(){
			return _ret;
		}

		// IBinarySerialize
		public void Read(BinaryReader r){
			if( r.BaseStream.Length > 0 )
				_ret=r.ReadInt32();
		}

		public void Write(BinaryWriter w){
			w.Write(Terminate());
		}

	}

}