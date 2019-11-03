using System;

namespace SQLBoost.TSQL.Master.Classes
{
	class NullFmt : IFormatProvider, ICustomFormatter
	{
		public object GetFormat(Type service){
			if(service == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}

		public string Format(string fmt, object arg, IFormatProvider provider){
			if(arg == null)
				return "null";

			IFormattable formattable =arg as IFormattable;
			if(formattable != null)
				return formattable.ToString(fmt, provider);

			return arg.ToString();
		}
	}
}
