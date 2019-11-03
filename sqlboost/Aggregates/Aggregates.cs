using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Text;
using System.Text.RegularExpressions;

namespace SQLBoost.TSQL.Master.Aggregates{

[Serializable]
[SqlUserDefinedAggregate(
	Format.UserDefined, //use clr serialization to serialize the intermediate result
	IsInvariantToNulls =true, //optimizer property
	IsInvariantToDuplicates =false, //optimizer property
	IsInvariantToOrder =false, //optimizer property
	MaxByteSize =-1) //maximum size in bytes of persisted value
]
public class VConcat : IBinarySerialize
{
	/// <summary>
	/// The variable that holds the intermediate result of the concatenation
	/// </summary>
	private StringBuilder	_sb;
	private Regex			_regx;
	private string			_delimit="&";

	/// <summary>
	/// Initialize the internal data structures
	/// </summary>
	public void Init(){
		_sb =new StringBuilder();
	}

	/// <summary>
	/// Accumulate the next value, not if the value is null
	/// </summary>
	/// <param name="value"></param>
	public void Accumulate(SqlString value, string delimit ="&"){
//		if( regx.Length > 0 )	_regx =new Regex( regx );
		_delimit =delimit;
		if( !(value.IsNull || value.ToString().Length ==0) )
			_sb.Append(value.ToString()).Append(_delimit);
	}

	/// <summary>
	/// Merge the partially computed aggregate with this aggregate.
	/// </summary>
	/// <param name="other"></param>
	public void Merge(VConcat other){
		_sb.Append(other._sb);
	}

	/// <summary>
	/// Called at the end of aggregation, to return the results of the aggregation.
	/// </summary>
	[return: SqlFacet(MaxSize = -1)] 
	public SqlString Terminate(){
		return new SqlString(_sb.ToString());
	}

	// IBinarySerialize
	public void Read(BinaryReader r){
		_sb =new StringBuilder(r.ReadString());
	}

	public void Write(BinaryWriter w){
		string output =Terminate().ToString();
		//delete the trailing comma, if any
		if( output != null && output.Length > 0 )
			output =output.Substring(0, _sb.Length - _delimit.Length);
		w.Write(output);
	}

}

}