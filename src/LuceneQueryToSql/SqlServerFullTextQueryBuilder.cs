using Lucene.Net.Index;
using Lucene.Net.Search;
using System.Collections.Generic;

namespace LuceneQueryToSql
{
    public class SqlServerFullTextQueryBuilder: QueryBuilder
    {
        protected override ParameterizedSql BuildQuery(FuzzyQuery query)
        {
            // FuzzyQuery are to be treated as TermQueries. No actual fuzzy search.
            return BuildQuery(new TermQuery(new Term(FieldPlaceholder, query.Term.Text)));
        }

        protected override ParameterizedSql BuildQuery(WildcardQuery wildcardQuery)
        {
            var termText = EscapeForSql(wildcardQuery.Term.Text);
            // termText = termText.Replace("*", "%");
            termText = termText.Replace("?", "_");

            var userVariables = new Dictionary<string, string> {{"field1", "\"" + termText + "\""}};

            var sql = "CONTAINS(" + FieldPlaceholder + ", @field1)";
            return new ParameterizedSql(sql, userVariables);
        }

        protected override ParameterizedSql BuildQuery(TermQuery termQuery)
        {
            var termText = EscapeForSql(termQuery.Term.Text);
            var userVariables = new Dictionary<string, string> {{"field1", termText}};

            var sql = "CONTAINS(" + FieldPlaceholder + ", @field1)";
            return new ParameterizedSql(sql, userVariables);
        }

        protected override ParameterizedSql BuildQuery(TermRangeQuery termRangeQuery)
        {
            // Not handling the TermRangeQuery. Discarding the search term.
            return null;
        }

        private string EscapeForSql(string termText)
        {
            string returnVal = termText;

            // This replacement needs to be first.
            returnVal = returnVal.Replace("[", "[[]");

            returnVal = returnVal.Replace("%", "[%]");
            returnVal = returnVal.Replace("_", "[_]");

            // This is only needed so I can use "{{COLUMN}}" as text to be replaced perfectly safely.
            // I.E. if anyone uses "{{COLUMN}}" as a search term, it will still work.
            returnVal = returnVal.Replace("{", "[{]");

            return returnVal;
        }
    }
}
