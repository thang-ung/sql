using System;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace SQLBoost.TSQL.Master.RowStruct{
	public struct RowMatchGroup{

		public SqlInt32		igroup;
		public SqlString	matched;

		public RowMatchGroup( SqlInt32 n, SqlString smatched ){
			igroup	=n;
			matched	=smatched;
		}
	}

	public struct regxParts{
		public RegexOptions	options;
		public string		pattern;

		public static regxParts parseRegex(string pattern){
			Match mats = Regex.Match(pattern.ToString(), "^/(.+)/([igms]*)$", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
			RegexOptions options = RegexOptions.None;

			foreach (char x in mats.Groups[2].Value.ToCharArray())
				switch (x){
				case 'i':
					options |= RegexOptions.IgnoreCase;
					break;
				case 'm':
					options |= RegexOptions.Multiline;
					break;
				case 's':
					options |= RegexOptions.Singleline;
					break;
				case 'g':
					break;
				}
			return new regxParts(){options=options, pattern=mats.Groups[1].Value};
		}

	}

}
