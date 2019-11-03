using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;

using SQLBoost.TSQL.Master.RowStruct;
using System.Collections;
using System.Collections.Generic;

namespace SQLBoost.TSQL.Master
{
	public class TableValueFunctions
	{

		[SqlFunction(
			IsDeterministic =true,
			DataAccess =DataAccessKind.None,
			SystemDataAccess =SystemDataAccessKind.None,
			FillRowMethodName = "FillRow_GroupMatch",
			TableDefinition = "igroup int, match nvarchar(max)")]
		public static IEnumerable vmatchGroupStr( SqlString xstr, SqlString pattern ){
			List<RowMatchGroup>	resultCollection =new List<RowMatchGroup>();
			if(xstr ==null) return resultCollection;

			var m =regxParts.parseRegex( pattern.ToString() );
			var jmats = Regex.Matches(xstr.ToString(), m.pattern, m.options);
			if(jmats.Count > 0){
				int	j=jmats[0].Groups.Count;
				foreach(Match mat in jmats)
					for(int k =1; k <= j; k++ )
						if(mat.Groups[k].Length > 0){
							resultCollection.Add(new RowMatchGroup(
								new SqlInt32(k),
								new SqlString(mat.Groups[k].Value)));
//							break;
						}
			}
			return resultCollection;
		}//end vmatchGroupStr

		public static void FillRow_GroupMatch( object ores,
			out SqlInt32 igroup, out SqlString matched ){
			RowMatchGroup result = (RowMatchGroup)ores;

			igroup	=result.igroup;
			matched	=result.matched;
		}

		public static List<string> PermuteWords(string s){
			string[] ss = s.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			bool[] used = new bool[ss.Length];
			string res = "";
			var list = new List<string>();
			permute(ss, used, res, 0, list);
			return list;
		}

		private static void permute(string[] ss, bool[] used, string res, int level, List<string> list){
			if(level == ss.Length && res != ""){
				list.Add(res);
				return;
			}

			for(int i = 0; i < ss.Length; i++){
				if (used[i]) continue;
				used[i] = true;
				permute(ss, used, res + " " + ss[i], level + 1, list);
				used[i] = false;
			}
		}
	}
}
