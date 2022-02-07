using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Index;

namespace LuceneQueryToSql
{
    public abstract class QueryBuilder
    {
        protected const string FieldPlaceholder = "{{COLUMN}}";

        // Private Class variables
        private const Lucene.Net.Util.Version LUCENE_VERSION = Lucene.Net.Util.Version.LUCENE_30;
        private static readonly Analyzer ANALYZER = new WhitespaceAnalyzer();

        public ParameterizedSql BuildSqlWhereClause(string luceneQuery)
        {
            var parser = new QueryParser(LUCENE_VERSION, FieldPlaceholder, ANALYZER)
            {
                DefaultOperator = QueryParser.Operator.AND,
                LowercaseExpandedTerms = false
            };

            Query luceneQueryParsed = parser.Parse(luceneQuery.ToUpper());

            return Build(luceneQueryParsed);
        }

        public ParameterizedSql BuildSqlWhereClause(string luceneQuery, IEnumerable<string> fields)
        {
            ParameterizedSql sqlWhereClause = BuildSqlWhereClause(luceneQuery);

            var parameterizedSqlObjs = fields.Select(field => 
                                             new ParameterizedSql(sqlWhereClause.Sql.Replace(FieldPlaceholder, field), 
                                                                  sqlWhereClause.UserInputVariables))
                                             .ToList();

            return CombineParameterizedSql(parameterizedSqlObjs);
        }
        public ParameterizedSql BuildSqlStatement(string luceneQuery, string table,
                                                  IList<string> fieldsToSearch, 
                                                  IList<string> fieldsToReturn)
        {

            // Escape double quotes, surround with double quotes
            var trustedFieldsToSearch = fieldsToSearch.Select(f => "\"" + f.Replace("\"", "\\\"") + "\"").ToList();
            var trustedFieldsToReturn = fieldsToReturn.Select(f => "\"" + f.Replace("\"", "\\\"") + "\"").ToList();
            string trustedTable = "\"" + table.Replace("\"", "\\\"") + "\"";

            ParameterizedSql sqlWhereClause = BuildSqlWhereClause(luceneQuery, trustedFieldsToSearch);

            string returnSql = "SELECT " + string.Join(", ", trustedFieldsToReturn) + "\n" +
                               "FROM " + trustedTable + "\n" +
                               "WHERE " + sqlWhereClause.Sql + ";";

            return new ParameterizedSql(returnSql, sqlWhereClause.UserInputVariables);
        }

        private ParameterizedSql Build(Query query)
        {
            switch (query)
            {
                case BooleanQuery booleanQuery:
                    return BuildBoolean(booleanQuery);
                case FuzzyQuery fuzzyQuery:
                    return BuildFuzzy(fuzzyQuery);
                case PhraseQuery phraseQuery:
                    return BuildPhrase(phraseQuery);
                case PrefixQuery prefixQuery:
                    return BuildPrefix(prefixQuery);
                case TermQuery termQuery:
                    return BuildTerm(termQuery);
                case TermRangeQuery rangeQuery:
                    return BuildTermRange(rangeQuery);
                case WildcardQuery wildcardQuery:
                    return BuildWildcard(wildcardQuery);
                default:
                    throw new Exception("Invalid Build object.");
            }
        }

        protected abstract ParameterizedSql BuildQuery(TermQuery query);

        protected abstract ParameterizedSql BuildQuery(TermRangeQuery query);

        protected abstract ParameterizedSql BuildQuery(FuzzyQuery query);

        protected abstract ParameterizedSql BuildQuery(WildcardQuery query);

        private static ParameterizedSql CombineParameterizedSql(IList<ParameterizedSql> parameterizedSqlObjs)
        {
            var queryStringBuilder = new StringBuilder();
            var segmentsAdded = 0;
            var currentParamNumber = 1;
            var combinedUserInputVariables = new Dictionary<string, string>();

            foreach (ParameterizedSql parameterizedSql in parameterizedSqlObjs)
            {
                if (parameterizedSql.Sql == null || parameterizedSql.UserInputVariables == null)
                {
                    continue;
                }

                if (segmentsAdded > 0)
                {
                    queryStringBuilder.Append(" OR ");
                }

                string sqlToAppend = parameterizedSql.Sql;

                // All sub-query sql contains parameters starting with "field1", "field2", etc and 
                // are here given unique names ("field5", "field6", etc.).
                // They match user input variables with also contain keys that start with "field1", "field2", etc and
                // need to be given matching unique names.
                for (var i = 1; i <= parameterizedSql.UserInputVariables.Count; i++)
                {
                    sqlToAppend = Regex.Replace(sqlToAppend, 
                                                "@field" + i + "([^0-9])",
                                                "@field" + currentParamNumber + "$1");

                    combinedUserInputVariables.Add("field" + currentParamNumber, 
                                                   parameterizedSql.UserInputVariables["field" + i]);

                    currentParamNumber++;
                }

                queryStringBuilder.Append("(" + sqlToAppend + ")");

                segmentsAdded++;
            }

            return new ParameterizedSql(queryStringBuilder.ToString(), combinedUserInputVariables);
        }

        private ParameterizedSql BuildBoolean(BooleanQuery booleanQuery)
        {
            var queryStringBuilder = new StringBuilder();
            var segmentsAdded = 0;
            var segmentsAddedOccur = 0;
            var currentParamNumber = 1;
            var combinedUserInputVariables = new Dictionary<string, string>();
            Occur? currentOccur = null;

            var clauses = new List<BooleanClause>();

            bool areMustClauses = booleanQuery.GetClauses().Any(c => c.Occur == Occur.MUST);

            clauses.AddRange(booleanQuery.GetClauses().Where(c => c.Occur == Occur.MUST).ToList());

            // SHOULD clauses are only relevant when there are no MUST clauses.
            // See: https://lucene.apache.org/core/3_0_3/api/core/org/apache/lucene/search/BooleanClause.Occur.html
            if (areMustClauses == false)
            {
                clauses.AddRange(booleanQuery.GetClauses().Where(c => c.Occur == Occur.SHOULD).ToList());
            }

            clauses.AddRange(booleanQuery.GetClauses().Where(c => c.Occur == Occur.MUST_NOT).ToList());

            foreach (BooleanClause clause in clauses)
            {
                ParameterizedSql subQuery = Build(clause.Query);

                if (subQuery?.Sql == null) continue;

                if (currentOccur == null) // if first clause
                {
                    queryStringBuilder.Append("(");
                    currentOccur = clause.Occur;
                }
                // if switch from Occur.MUST clauses to Occur.SHOULD clauses, 
                // or if from Occur.SHOULD clauses to Occur.MUST_NOT clauses
                else if(currentOccur != clause.Occur)
                {
                    queryStringBuilder.Append(") AND (");
                    currentOccur = clause.Occur;
                    segmentsAddedOccur = 0;
                }

                if (segmentsAddedOccur > 0)
                {
                    if (clause.Occur == Occur.MUST)
                    {
                        queryStringBuilder.Append(" AND ");
                    }
                    else if (clause.Occur == Occur.SHOULD)
                    {
                        queryStringBuilder.Append(" OR ");
                    }
                    else if (clause.Occur == Occur.MUST_NOT)
                    {
                        queryStringBuilder.Append(" AND NOT ");
                    }
                }
                else if (clause.Occur == Occur.MUST_NOT)
                {
                    queryStringBuilder.Append("NOT ");
                }

                string sqlToAppend = subQuery.Sql;

                // All sub-query sql contains parameters starting with "field1", "field2", etc and 
                // are here given unique names ("field5", "field6", etc.).
                // They match user input variables with also contain keys that start with "field1", "field2", etc and
                // need to be given matching unique names.
                if (subQuery.UserInputVariables.Count > 0)
                {
                    int numUserInputVariables = subQuery.UserInputVariables.Count;
                    int param = currentParamNumber;

                    for (var i = 1; i <= numUserInputVariables; i++)
                    {
                        combinedUserInputVariables.Add("field" + param, subQuery.UserInputVariables["field" + i]);

                        param++;
                    }

                    param = currentParamNumber;
                    for (int i = numUserInputVariables; i > 0; i--)
                    {
                        sqlToAppend = Regex.Replace(sqlToAppend, 
                                                    "@field" + i + "([^0-9])",
                                                    "@field" + (param + i - 1) + "$1");

                        currentParamNumber++;
                    }
                }

                queryStringBuilder.Append("(" + sqlToAppend + ")");

                // If the last clause, close the parens around MUST/SHOULD/MUST_NOT sections
                if (segmentsAdded == clauses.Count - 1)
                {
                    queryStringBuilder.Append(")");
                }

                segmentsAdded++;
                segmentsAddedOccur++;
            }

            return new ParameterizedSql(queryStringBuilder.ToString(), combinedUserInputVariables);
        }

        private ParameterizedSql BuildFuzzy(FuzzyQuery fuzzyQuery)
        {
            Term term = CopyTerm(fuzzyQuery.Term);
            return term == null ? null : BuildQuery(new FuzzyQuery(term, fuzzyQuery.MinSimilarity, fuzzyQuery.PrefixLength));
        }

        private ParameterizedSql BuildPhrase(PhraseQuery phraseQuery)
        {
            int termsAdded = 0;
            string field = "";
            var queryStringBuilder = new StringBuilder();

            foreach (Term term in phraseQuery.GetTerms())
            {
                if (termsAdded == 0)
                {
                    field = term.Field;
                }
                else if (termsAdded > 0)
                {
                    queryStringBuilder.Append(" ");
                }

                queryStringBuilder.Append(term.Text);

                termsAdded++;
            }

            Query query;
            string queryString = queryStringBuilder.ToString();

            if (queryString.Contains("?") || queryString.Contains("*"))
            {
                query = new WildcardQuery(new Term(field, queryString));
            }
            else
            {
                query = new TermQuery(new Term(field, queryString));
            }

            return Build(query);
        }

        private ParameterizedSql BuildPrefix(PrefixQuery prefixQuery)
        {
            string field = prefixQuery.Prefix.Field;
            return field == null ? null : BuildQuery(new WildcardQuery(new Term(field, prefixQuery.Prefix.Text + "*")));
        }

        private ParameterizedSql BuildTerm(TermQuery termQuery)
        {
            Term term = CopyTerm(termQuery.Term);
            return term == null ? null : BuildQuery(new TermQuery(term));
        }

        private ParameterizedSql BuildTermRange(TermRangeQuery termRangeQuery)
        {
            string field = termRangeQuery.Field;
            var q = termRangeQuery;
            if (field != null)
            {
                return BuildQuery(new TermRangeQuery(field, q.LowerTerm, q.UpperTerm, q.IncludesLower, 
                                                     q.IncludesUpper, q.Collator));
            }

            return null;
        }

        private ParameterizedSql BuildWildcard(WildcardQuery wildcardQuery)
        {
            Term term = CopyTerm(wildcardQuery.Term);
            return term == null ? null : BuildQuery(new WildcardQuery(term));
        }

        private static Term CopyTerm(Term term)
        {
            return term.Field == null ? null : new Term(term.Field, term.Text);
        }

        private DateTime? ParseDate(string str)
        {
            var dateFormats = new [] {"M/d/yyyy", "yyyyMMdd", "dd-MMM-yyyy", "yyyy-M-d"};

            foreach (var format in dateFormats)
            {
                try
                {
                    return DateTime.ParseExact(str, format, new CultureInfo("en-US"));
                }
                catch (ArgumentNullException)
                {
                    // Ignore
                }
                catch (FormatException)
                {
                    // Ignore
                }
            }

            return null;
        }

        private string FormatDate(DateTime date)
        {
            return date.ToString("yyyyMMddHHmmss");
        }
    }
}
