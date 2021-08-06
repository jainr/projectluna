using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    /// <summary>
    /// Map data between request (source) -> database entity -> response (target)
    /// </summary>
    /// <typeparam name="S">The source/request type</typeparam>
    /// <typeparam name="T">The target/response type</typeparam>
    /// <typeparam name="D">The database entity type</typeparam>
    public interface IDataMapper<S, T, D>
    {
        D Map(S source);

        T Map(D dbEntity);
    }
}
