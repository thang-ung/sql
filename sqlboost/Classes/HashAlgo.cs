using System;
using System.Text;
using System.Security.Cryptography;

namespace SQLBoost.TSQL.Master.Classes
{
	public class HashAlgo<TAlgoProvider>
	where TAlgoProvider : HashAlgorithm{
		private readonly HashAlgorithm	_halgo;

		public HashAlgo(){
			_halgo = Activator.CreateInstance( typeof(TAlgoProvider) ) as HashAlgorithm;
		}

		public byte[] ComputeHash(System.IO.Stream inputStream){
			return _halgo.ComputeHash(inputStream);
		}

		public byte[] ComputeHash(byte[] buffer){
			return _halgo.ComputeHash( buffer );
		}

		public string ToBase64String(string data){
			byte[] bytes = Encoding.UTF8.GetBytes(data.ToString());
			byte[] hashVal =this.ComputeHash( bytes );

			string output = Convert.ToBase64String(hashVal);
			return output;
		}
	}
}
