using System;
using System.Globalization;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using SQLBoost.TSQL.Master.Classes;


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
public class HashSha1 : IBinarySerialize
{
	/// <summary>
	/// The variable that holds the intermediate result of the concatenation
	/// </summary>
	private String								_hashValue;
	private SortedDictionary<SqlInt32,String>	_context;

	/// <summary>
	/// Initialize the internal data structures
	/// </summary>
	public void Init(){
		_context    =new SortedDictionary<SqlInt32, String>();
	}

	/// <summary>
	/// Accumulate the next value, not if the value is null
	/// </summary>
	/// <param name="value"></param>
	public void Accumulate(SqlInt32 datapointId, SqlString value, SqlInt32 tailDatapointId, SqlString tailValue){
		if(_context.Count ==0)  _context[datapointId] =value.ToString();
		if (!(tailValue.IsNull || tailValue.ToString().Length == 0)){
			_context[tailDatapointId]   =tailValue.ToString();
		}
	}

	public void Merge(HashSha1 Group) { }

	/// <summary>
	/// Called at the end of aggregation, to return the results of the aggregation.
	/// </summary>
	[return: SqlFacet(MaxSize = -1)] 
	public SqlString Terminate(){//(3)
		return _hashValue;
	}

	// IBinarySerialize
	public void Write(BinaryWriter w){//(1)
		var		sb = new StringBuilder();
		foreach(var datapt in _context.Keys)
			sb.AppendFormat(CultureInfo.InvariantCulture, "'{0}''{1}'", datapt, _context[datapt] );

		var		hashAlgo	=new HashAlgo<SHA1CryptoServiceProvider>();

		string output =hashAlgo.ToBase64String( sb.ToString() ).Replace('/','_').Replace('+','-');
		w.Write( output );
	}

	public void Read(BinaryReader r){//(2)
		_hashValue = r.ReadString();
	}

}

}