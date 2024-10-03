using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    internal interface IDbConverter<T, TDb>
        where T : class
        where TDb : class
    {
        T? Convert(TDb tout);
        TDb? Convert(T t);
    }
}
