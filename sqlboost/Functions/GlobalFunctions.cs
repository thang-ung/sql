using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;

using System.IO;
using SQLBoost.TSQL.Master.RowStruct;
using SQLBoost.TSQL.Master.Classes;
using System.IO.Compression;
using System.Text;
using System.Linq;


namespace SQLBoost.TSQL.Master{


public partial class UserDefinedFunctions
{

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static SqlString csformatPipe(SqlString pattern, SqlString args){
		return new SqlString(string.Format(pattern.ToString(), args.ToString().Split('|')));
	}//end csformatPipe

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static string csformat(string pattern,
		string arg00,
		string arg01 =null,
		string arg02 =null,
		string arg03 =null,
		string arg04 =null,
		string arg05 =null,
		string arg06 =null,
		string arg07 =null,
		string arg08 =null,
		string arg09 =null){
		return string.Format(new NullFmt(), pattern, arg00, arg01, arg02, arg03, arg04, arg05, arg06, arg07, arg08, arg09);
	}//end csformat

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static string RegexReplace( SqlString xstr, SqlString pattern, SqlString replacement ){
		var m =regxParts.parseRegex( pattern.ToString() );
		return Regex.Replace( xstr.ToString(), m.pattern, replacement.ToString(), m.options );
	}//end RegexReplace

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static SqlInt32 Matches( string xstr, string pattern ){
		var m =regxParts.parseRegex( pattern );
		return new SqlInt32( Regex.Matches( xstr, m.pattern, m.options ).Count );
	}//end Matches

	[Microsoft.SqlServer.Server.SqlFunction(
		IsDeterministic = true,
		IsPrecise = true,
		DataAccess = DataAccessKind.None,
		SystemDataAccess = SystemDataAccessKind.None)]
	public static SqlInt32 matchGroup(string xstr, string pattern, int k){
		var m =regxParts.parseRegex( pattern );
		var	jmats =Regex.Matches(xstr, m.pattern, m.options);
		try
		{
			foreach (Match mat in jmats)
				if (mat.Groups[k].Length > 0)
					return SqlInt32.Parse(mat.Groups[k].Value);
		}catch(Exception){}

		return SqlInt32.Null;	//not found
	}//end matchGroup

	[Microsoft.SqlServer.Server.SqlFunction(
		IsDeterministic = true,
		IsPrecise = true,
		DataAccess = DataAccessKind.None,
		SystemDataAccess = SystemDataAccessKind.None)]
	public static string matchGroupStr(string xstr, string pattern, int k){
		var m =regxParts.parseRegex( pattern );
		var jmats = Regex.Matches(xstr, m.pattern, m.options);
		foreach(Match mat in jmats)
			if(mat.Groups[k].Length > 0)
				return mat.Groups[k].Value;

		return null;
	}//end matchGroupStr

		[Microsoft.SqlServer.Server.SqlFunction(
		IsDeterministic = true,
		IsPrecise = true,
		DataAccess = DataAccessKind.None,
		SystemDataAccess = SystemDataAccessKind.None)]
	public static string matchGroupEx(string xstr, string pattern, int k){
		var m =regxParts.parseRegex( pattern );
		var jmats = Regex.Matches(xstr, m.pattern, m.options);
		foreach(Match mat in jmats)
			if(mat.Groups[k].Length == 0)
				return null;
			else if( mat.Groups[k].Captures.Count > 1 ){	//repeating group
				StringBuilder	sb =new StringBuilder();
				foreach(var cap in mat.Groups[k].Captures)
					sb.Append( cap.ToString() );
				return sb.ToString();
			}
			else
				return mat.Groups[k].Value;

		return null;
	}//end matchGroupEx

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static Byte[] Deflate( string xstr ){
		using( var output =new MemoryStream() )
		using(var zstream =new DeflateStream(output, CompressionMode.Compress) ){
			using(var writer =new StreamWriter(zstream, System.Text.Encoding.UTF8) ){
				writer.Write(xstr);
			}

			return output.ToArray();
		}
	}//end Deflate

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static string Enflate( Byte[] deflated ){
		var output = new MemoryStream();
		using(var compressedStream = new MemoryStream(deflated))
		using(var zstream = new DeflateStream(compressedStream, CompressionMode.Decompress)){
#if( NET35 )
			using(StreamWriter writer =new StreamWriter(zstream, System.Text.Encoding.UTF8) ){
				writer.Write(deflated);
				return output.ToString();
			}
#else
			zstream.CopyTo(output);
			zstream.Close();
			output.Position = 0;
			return Encoding.UTF8.GetString( output.ToArray() );
#endif
		}
	}//end Enflate

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static string Compress(string xstr)
	{
		var bytes = Encoding.UTF8.GetBytes(xstr);

		using (var msin = new MemoryStream(bytes))
		using (var msout = new MemoryStream()){
			using (var gzip = new GZipStream(msout, CompressionMode.Compress)){
#if(NET35)
				gzip.Write(bytes, 0, bytes.Length);
#else
				msin.CopyTo(gzip);
#endif
			}

			return Convert.ToBase64String(msout.ToArray());
		}
	}//end Compress

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true,
		IsPrecise = true,
		DataAccess = DataAccessKind.None,
		SystemDataAccess = SystemDataAccessKind.None)]
	public static string Decompress(string deflated){
		byte[] gzBuffer = Convert.FromBase64String(deflated);
		using (var ms = new MemoryStream())
		{
			ms.Write(gzBuffer, 0, gzBuffer.Length);
			ms.Position = 0;
#if(NET35)
			using (var zip = new GZipStream(ms, CompressionMode.Decompress))
			using (var reader = new StreamReader(zip, Encoding.UTF8)){
				return reader.ReadToEnd();
			}
#else
			using(var reader = new MemoryStream() )
			using(var zip =new GZipStream(ms, CompressionMode.Decompress)){
					zip.CopyTo(reader, 8192);
				return Encoding.UTF8.GetString(reader.ToArray());
			}
#endif
		}
	}//end Decompress

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static Byte[] MD5(  string vbin ){
		var output =System.Security.Cryptography.MD5.Create();
		return output.ComputeHash(Encoding.UTF8.GetBytes(vbin));
	}//end MD5

	[Microsoft.SqlServer.Server.SqlFunction(IsDeterministic =true,
		IsPrecise =true,
		DataAccess=DataAccessKind.None,
		SystemDataAccess=SystemDataAccessKind.None)]
	public static string evalFactoradic( string factorads ){
		var factoradic =factorads.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
		var order =factoradic.Length;

		var perm = new int[order];
		perm[order - 1] = 1;  // right-most value is set to 1.
		for (int i = order - 2; i >= 0; --i)
		{
			perm[i] = int.Parse(factoradic[i]);
			for (int j = i + 1; j < order; ++j)
			{
				if (perm[j] >= perm[i]) ++perm[j];
			}
		}
		return string.Join(",", perm.Select( x => x.ToString() ).ToArray());
	}//end evalFactoradic

}

}//end SQLBoost.TSQL.Master
