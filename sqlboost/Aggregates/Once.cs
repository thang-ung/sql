using System;
using Microsoft.SqlServer.Server;
using System.IO;

namespace SQLBoost.TSQL.Master.Aggregates{

	[Serializable]
	[SqlUserDefinedAggregate(
		Format.UserDefined, //use clr serialization to serialize the intermediate result
		IsInvariantToNulls = true, //optimizer property
		IsInvariantToDuplicates =true, //optimizer property
		IsInvariantToOrder =true, //optimizer property
		MaxByteSize = -1) //maximum size in bytes of persisted value
	]
	public struct Once : IBinarySerialize
	{
		public void Init(){}

		public void Accumulate(string Value){
			if( _var ==null ) _var =Value;
		}

		public void Merge(Once Group){
			_var =Group._var;
		}

		[return: SqlFacet(MaxSize = -1)]
		public string Terminate(){
			return _var;
		}

		// This is a place-holder member field
		private string _var;

			// IBinarySerialize
			public void Read(BinaryReader r){
				if( r.BaseStream.Length > 0 )
					_var=r.ReadString();
			}

			public void Write(BinaryWriter w){
				w.Write(Terminate());
			}
	}//end Once
}
