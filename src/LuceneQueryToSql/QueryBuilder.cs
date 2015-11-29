// Copyright (c) 2015 Tom von Clef

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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

            return Build(parser.Parse(luceneQuery.ToUpper()));
        }

        public ParameterizedSql BuildSqlWhereClause(string luceneQuery, IEnumerable<string> fields)
        {
            var sqlWhereClause = BuildSqlWhereClause(luceneQuery);

            var parameterizedSqlObjs = fields.Select(field => 
                                             new ParameterizedSql(sqlWhereClause.Sql.Replace(FieldPlaceholder, field), 
                                                                  sqlWhereClause.UserInputVariables))
                                             .ToList();

            return CombineParameterizedSql(parameterizedSqlObjs);
        }
        public ParameterizedSql BuildSqlStatement(string luceneQuery, NotUserInputString table,
                                                  IList<NotUserInputString> fieldsToSearch, 
                                                  IList<NotUserInputString> fieldsToReturn)
        {

            // Escape double quotes, surround with double quotes
            var trustedFieldsToSearch = fieldsToSearch
                    .Select(f => "\"" + f.Str.Replace("\"", "\\\"") + "\"").ToList();

            var trustedFieldsToReturn = fieldsToReturn
                    .Select(f => "\"" + f.Str.Replace("\"", "\\\"") + "\"").ToList();

            var trustedTable = "\"" + table.Str.Replace("\"", "\\\"") + "\"";

            var sqlWhereClause = BuildSqlWhereClause(luceneQuery, trustedFieldsToSearch);

            var returnSql =
                   "SELECT " + string.Join(", ", trustedFieldsToReturn) + "\n" +
                   "FROM " + trustedTable + "\n" +
                   "WHERE " + sqlWhereClause.Sql + ";";

            return new ParameterizedSql(returnSql, sqlWhereClause.UserInputVariables);
        }

        private ParameterizedSql Build(Query query)
        {
            if (query is BooleanQuery)
            {
                return BuildBoolean((BooleanQuery) query);
            }
            else if (query is FuzzyQuery)
            {
                return BuildFuzzy((FuzzyQuery) query);
            }
            else if (query is PhraseQuery)
            {
                return BuildPhrase((PhraseQuery) query);
            }
            else if (query is PrefixQuery)
            {
                return BuildPrefix((PrefixQuery) query);
            }
            else if (query is TermQuery)
            {
                return BuildTerm((TermQuery) query);
            }
            else if (query is TermRangeQuery)
            {
                return BuildTermRange((TermRangeQuery) query);
            }
            else if (query is WildcardQuery)
            {
                return BuildWildcard((WildcardQuery) query);
            }
            else
            {
                throw new Exception("Invalid Build object.");
            }
        }

        protected abstract ParameterizedSql BuildQuery(TermQuery query);

        protected abstract ParameterizedSql BuildQuery(TermRangeQuery query);

        protected abstract ParameterizedSql BuildQuery(FuzzyQuery query);

        protected abstract ParameterizedSql BuildQuery(WildcardQuery query);

        private ParameterizedSql CombineParameterizedSql(IList<ParameterizedSql> parameterizedSqlObjs)
        {
            var queryStringBuilder = new StringBuilder();
            var segmentsAdded = 0;
            var currentParamNumber = 1;
            var combinedUserInputVariables = new Dictionary<string, string>();

            foreach (var parameterizedSql in parameterizedSqlObjs)
            {
                if (parameterizedSql.Sql == null || parameterizedSql.UserInputVariables == null)
                {
                    continue;
                }

                if (segmentsAdded > 0)
                {
                    queryStringBuilder.Append(" OR ");
                }

                var sqlToAppend = parameterizedSql.Sql;

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

            var areMustClauses = booleanQuery.GetClauses().Any(c => c.Occur == Occur.MUST);

            clauses.AddRange(booleanQuery.GetClauses().Where(c => c.Occur == Occur.MUST).ToList());

            // SHOULD clauses are only relevant when there are no MUST clauses.
            // See: https://lucene.apache.org/core/3_0_3/api/core/org/apache/lucene/search/BooleanClause.Occur.html
            if (areMustClauses == false)
            {
                clauses.AddRange(booleanQuery.GetClauses().Where(c => c.Occur == Occur.SHOULD).ToList());
            }

            clauses.AddRange(booleanQuery.GetClauses().Where(c => c.Occur == Occur.MUST_NOT).ToList());

            foreach (var clause in clauses)
            {
                var subQuery = Build(clause.Query);

                if (subQuery == null || subQuery.Sql == null) continue;

                
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

                var sqlToAppend = subQuery.Sql;

                // All sub-query sql contains parameters starting with "field1", "field2", etc and 
                // are here given unique names ("field5", "field6", etc.).
                // They match user input variables with also contain keys that start with "field1", "field2", etc and
                // need to be given matching unique names.
                if (subQuery.UserInputVariables.Count > 0)
                {
                    int numUserInputVariables = subQuery.UserInputVariables.Count;
                    var param = currentParamNumber;

                    for (var i = 1; i <= numUserInputVariables; i++)
                    {
                        combinedUserInputVariables.Add("field" + param, subQuery.UserInputVariables["field" + i]);

                        param++;
                    }

                    param = currentParamNumber;
                    for (var i = numUserInputVariables; i > 0; i--)
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
            if (term != null)
            {
                return BuildQuery(new FuzzyQuery(term, fuzzyQuery.MinSimilarity, fuzzyQuery.PrefixLength));
            }

            return null;
        }

        private ParameterizedSql BuildPhrase(PhraseQuery phraseQuery)
        {
            var termsAdded = 0;
            var field = "";
            var queryStringBuilder = new StringBuilder();

            foreach (var term in phraseQuery.GetTerms())
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
            var queryString = queryStringBuilder.ToString();

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
            if (field != null)
            {
                return BuildQuery(new WildcardQuery(new Term(field, prefixQuery.Prefix.Text + "*")));
            }

            return null;
        }

        private ParameterizedSql BuildTerm(TermQuery termQuery)
        {
            Term term = CopyTerm(termQuery.Term);
            if (term != null)
            {
                return BuildQuery(new TermQuery(term));
            }

            return null;
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
            if (term != null)
            {
                return BuildQuery(new WildcardQuery(term));
            }

            return null;
        }

        private Term CopyTerm(Term term)
        {
            if (term.Field != null)
            {
                return new Term(term.Field, term.Text);
            }

            return null;
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
