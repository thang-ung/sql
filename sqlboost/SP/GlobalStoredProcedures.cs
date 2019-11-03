using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Threading;

using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Text;

namespace SeniorLinkLib.TSQL.Master{

public partial class StoredProcedures
{
	public static int SqlSafeParmsEx( ref SqlCommand cmd, SqlXml values ){
		int		n = 0;

		if (!values.IsNull){
			XPathDocument	xp = new XPathDocument(values.CreateReader());
			XPathNavigator	xnav = xp.CreateNavigator();
			string			strArg = null;
/*
			foreach(XPathNodeIterator k in xnav.Select("//values/value")){
				k.Current;
			}
*/
			n = (int)(double)xnav.Evaluate("count(/values/value)");
			for( int r = 1; r <= n; r++ ){
				strArg = string.Format("@arg{0}", r);
				XPathNavigator	xcurr = xnav.SelectSingleNode(string.Format("/values/value[{0}]", r));

				string		val;

				if( xcurr.SelectSingleNode("*[1]") != null ){	//xml
					XmlDocument	xd = new XmlDocument();
					XmlNode	root = xd.CreateElement("root");
					root.InnerXml = xcurr.InnerXml;

					cmd.Parameters.Add(new SqlParameter(strArg,
						new SqlXml(new XmlNodeReader(root.SelectSingleNode("/*")))));
				}
				else if( xcurr.Value == null || (val = xcurr.Value).Length == 0 ){
					SqlParameter parm = new SqlParameter(strArg, SqlDbType.VarChar, 10);
					parm.IsNullable = true;
					parm.SqlValue = SqlString.Null;
					cmd.Parameters.Add(parm);
				}
				else if(val.StartsWith("N'")){
					cmd.Parameters.Add(new SqlParameter(strArg, SqlDbType.NVarChar, val.Length ));
					cmd.Parameters[strArg].Value = val.TrimStart(new char[] { 'N' }).Trim(new char[] { '\'' });
				}
				else if(val.StartsWith("'")){
					cmd.Parameters.Add(new SqlParameter(strArg, SqlDbType.VarChar, val.Length ));
					cmd.Parameters[strArg].Value = val.Substring(1,val.Length-2);
				}
				else{
					try{
						cmd.Parameters.Add(new SqlParameter(strArg, SqlDecimal.Parse(val)));

						if (cmd.CommandText.Contains(strArg + " OUTPUT"))
							cmd.Parameters[strArg].Direction = ParameterDirection.Output;
					}
					catch(Exception){
						cmd.Parameters.Add(new SqlParameter(strArg, SqlDbType.VarChar, 10));
						cmd.Parameters[strArg].IsNullable = true;
						cmd.Parameters[strArg].SqlValue = SqlString.Null;
					}
				}
			}
		}
		return n;
	}//end SqlSafeParmsEx

	[Microsoft.SqlServer.Server.SqlProcedure]
	public static int SqlSafeScalar( SqlString sqlcmd, SqlXml values ){
		int			    n = 0;
		SqlConnection	conn = new SqlConnection("context connection=true");
		SqlCommand		cmd = new SqlCommand(sqlcmd.ToString(), conn);
		conn.Open();
		cmd.CommandType = CommandType.Text;
		cmd.CommandTimeout = 0;

		n = SqlSafeParmsEx( ref cmd, values );
		try{
			n = (int)cmd.ExecuteScalar();
		}
		catch (Exception ed){
			throw new Exception(ed.Message + "\n" +
					@sqlcmd.ToString() );
		}
		conn.Close();
		return n;
	}//end SqlSafeScalar

	[Microsoft.SqlServer.Server.SqlProcedure]
	public static int SqlSafeScalarContext( SqlString sqlcmd, SqlXml values,
			string context="context connection=true" ){
		int			n = 0;
		SqlConnection	conn = new SqlConnection(context);
		SqlCommand		cmd = new SqlCommand(sqlcmd.ToString(), conn);
		conn.Open();
		cmd.CommandType = CommandType.Text;
		cmd.CommandTimeout = 0;

		n = SqlSafeParmsEx( ref cmd, values );
		try{
			n = (int)cmd.ExecuteScalar();
		}
		catch (Exception ed){
			throw new Exception(ed.Message + "\n" +
					@sqlcmd.ToString() );
//					xnav.SelectSingleNode(string.Format("/values/value[{0}]", 19)).Value);
		}
		conn.Close();
		return n;
	}//end SqlSafeScalarContext
	
	[Microsoft.SqlServer.Server.SqlProcedure]
	public static int SqlSafeExecute( SqlString sqlcmd, SqlXml values ){
		int			n = 0;
		SqlConnection	conn = new SqlConnection("context connection=true");
		SqlCommand		cmd = new SqlCommand(sqlcmd.ToString(), conn);
		conn.Open();
		cmd.CommandType = CommandType.Text;
		cmd.CommandTimeout = 0;

		n=SqlSafeParmsEx( ref cmd, values );
		try{
			n = cmd.ExecuteNonQuery();
		}
		catch (Exception ed){
			throw new Exception(ed.Message + "\n" +
					@sqlcmd.ToString() );
//					xnav.SelectSingleNode(string.Format("/values/value[{0}]", 19)).Value);
		}
		conn.Close();
		return n;
	}//end SqlSafeExecute

	[Microsoft.SqlServer.Server.SqlProcedure]
	public static int SqlSafeExecuteQry( SqlString sqlcmd, SqlXml values, out SqlXml xout ){
		return SqlSafeExecuteQryContext( sqlcmd, "context connection=true", ref values, out xout );
	}

	[Microsoft.SqlServer.Server.SqlProcedure]
	public static int SqlSafeExecuteQryContext( SqlString sqlcmd, SqlString sqlcontext, ref SqlXml values,
			out SqlXml xout ){
		int				n = 0, i = 0;
		SqlConnection	conn = new SqlConnection(sqlcontext.IsNull ? "context connection=true": sqlcontext.ToString());
		SqlCommand		cmd = new SqlCommand(sqlcmd.ToString().Replace("exec ","exec @rr="), conn);
		conn.Open();
		cmd.CommandType = CommandType.Text;
		cmd.CommandTimeout = 0;

		SqlSafeParmsEx(ref cmd, values);

		try{
			cmd.Parameters.Add( new SqlParameter("@rr",new SqlInt32(0)) );
			cmd.Parameters["@rr"].Direction = ParameterDirection.InputOutput;

			SqlDataReader	sr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			n = (int)(SqlInt32)cmd.Parameters["@rr"].SqlValue;
			if( n == 0 && sr.HasRows && sr.Read() ){
//				n = sr.RecordsAffected;
				xout = sr.GetSqlXml(0);
			}
			else
				xout = values;
			sr.Close();

			foreach( SqlParameter parm in cmd.Parameters ){
				i++;
				if( parm.Direction == ParameterDirection.Output ){
					XPathDocument	xp = new XPathDocument(values.CreateReader());

					XmlDocument	xd = new XmlDocument();
					XmlNode	root = xd.CreateElement("root");
					root.InnerXml = xp.CreateNavigator().InnerXml;

					root.SelectSingleNode(string.Format("/values/value[{0}]", i)).InnerXml = parm.Value.ToString();
					values = new SqlXml(new XmlNodeReader(root.SelectSingleNode("/*")));
				}
			}
		}
		catch (Exception ed){
			throw new Exception(ed.Message + "\n" +
					@sqlcmd.ToString() );
//					xnav.SelectSingleNode(string.Format("/values/value[{0}]", 19)).Value);
		}
//		conn.Close();
		return n;
	}//end SqlSafeExecuteQry

	[Microsoft.SqlServer.Server.SqlProcedure]
	public static int AsyncSqlExec( string sqlcmd, SqlXml args
		, string serverInstance, string db_name
		, string usr ="" ){
		string context;

		if(string.IsNullOrEmpty(usr))
			context =String.Format(@"Server=.\{0};Database={1};Integrated Security=True;"
				, serverInstance, db_name);
		else
			context =String.Format(@"Server=.\{0};Database={1};User ID={2};Password={2};"
					, serverInstance, db_name, usr);

//		SqlConnection    conn =new SqlConnection("context connection=true;");	//async=true;
		SqlConnection    conn =new SqlConnection(context);
		SqlCommand        cmd =new SqlCommand(sqlcmd, conn);

		conn.Open();
		cmd.CommandType = CommandType.StoredProcedure;
		cmd.CommandTimeout =240;	//four minutes

		var ds = new DataSet();
		if(!args.IsNull){
			XPathDocument    xd =new XPathDocument(args.CreateReader());
			XPathNavigator    xnav =xd.CreateNavigator();

			xnav.MoveToRoot();
			var    xi =xnav.SelectChildren(string.Empty, string.Empty);    //all children
			while(xi.MoveNext() ==true){
				string	contentType =xi.Current.GetAttribute("contentType",""),
						nom =xi.Current.GetAttribute("name","");
				switch(contentType){
				case "nvarchar":
				case "string":{
					var	parm =cmd.Parameters.Add(nom, SqlDbType.NVarChar,255);
					parm.Value =xi.Current.Value;
					break;
				}
				case "integer":
				case "long":
				case "tinyint":
				case "smallint":{
					var	parm =cmd.Parameters.Add(nom, SqlDbType.Int);
					int	numval;
					int.TryParse( xi.Current.Value, out numval );
					parm.Value =numval;
					break;
				}
				case "table":{
					var ms = new MemoryStream(Encoding.UTF8.GetBytes(xi.Current.InnerXml.ToCharArray()));

					ms.Position = 0;
					ds.ReadXml(ms);
					ds.Tables[ds.Tables.Count-1].TableName =xi.Current.GetAttribute("name","");

					var	parm =cmd.Parameters.Add(nom, SqlDbType.Structured);
					parm.SqlValue =ds.Tables[ds.Tables.Count-1];
					break;
				}
				default:{
					var	parm =cmd.Parameters.Add(nom, SqlDbType.Variant);
					parm.Value =xi.Current.Value;
					break;
				}
				}
			}
		}//end if

		//context is lost (closed) on the thread spun, after current ends.
		var async =new Thread(() =>{
			try{
				cmd.ExecuteNonQuery();
			}
			catch(SqlException err){
				if(err.Number ==-2	//log timeout and sp not found
				 || err.Number ==0xafc){
					cmd.CommandTimeout =0;
					//log time out
				}
				else throw(err);
			}
			});
		async.Start();
		return 0;
	}

};

}//end SeniorLinkLib.TSQL.Master