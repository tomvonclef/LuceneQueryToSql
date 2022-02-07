using System.Collections.Generic;
using System.Collections.Immutable;

namespace LuceneQueryToSql
{
    public class ParameterizedSql
    {
        public string Sql { get; private set; }
        public ImmutableDictionary<string, string> UserInputVariables { get; private set; }

        public ParameterizedSql(string sql, IDictionary<string, string> userInputVariables)
        {
            Sql = sql;
            UserInputVariables = userInputVariables.ToImmutableDictionary();
        }
    }
}
