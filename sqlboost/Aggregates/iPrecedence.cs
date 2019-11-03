using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace SQLBoost.TSQL.Master.Aggregates{

	[Serializable]
	[Microsoft.SqlServer.Server.SqlUserDefinedAggregate(Format.Native,
			IsInvariantToDuplicates=true)]
	public struct iPrecedence
	{
		public void Init(){}

		public void Accumulate(SqlInt32 Value, SqlInt32 precedence){
			if( !precedence.IsNull && Value == precedence )
				varmax =Value;
			else if( !precedence.IsNull && varmax == precedence )
				return;
			else if( varmax.IsNull && (!Value.IsNull || Value > varmax) )
				varmax =Value;

			else if( Value > varmax )
				varmax =Value;
		}

		public void Merge(iPrecedence Group){}

		public SqlInt32 Terminate(){
			// Put your code here
			return varmax;
		}

		// This is a place-holder member field
		private SqlInt32 varmax;

	}//end iPrecedence
}