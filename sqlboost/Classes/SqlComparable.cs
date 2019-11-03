using System;

namespace SQLBoost.TSQL.Master.Classes
{
    class SqlComparable<T>:IComparable<T>
        where T: IComparable
    {
        public int CompareTo(T other){
            return this.CompareTo(other);
        }
    }
}
