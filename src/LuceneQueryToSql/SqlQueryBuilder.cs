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

using Lucene.Net.Index;
using Lucene.Net.Search;
using System.Collections.Generic;

namespace LuceneQueryToSql
{
    public class SqlQueryBuilder: QueryBuilder
    {
        private const string FieldPlaceholder = "{{COLUMN}}";

        protected override ParameterizedSql BuildQuery(FuzzyQuery query)
        {
            // FuzzyQuery are to be treated as TermQueries. No actual fuzzy search.
            return BuildQuery(new TermQuery(new Term(FieldPlaceholder, query.Term.Text)));
        }

        protected override ParameterizedSql BuildQuery(WildcardQuery wildcardQuery)
        {
            var termText = EscapeForSql(wildcardQuery.Term.Text);
            termText = termText.Replace("*", "%");
            termText = termText.Replace("?", "_");

            var userVariables = new Dictionary<string, string> {{"field1", termText}};

            var sql = FieldPlaceholder + " LIKE '%' + @field1 + '%'";
            return new ParameterizedSql(sql, userVariables);
        }

        protected override ParameterizedSql BuildQuery(TermQuery termQuery)
        {
            var termText = EscapeForSql(termQuery.Term.Text);
            var userVariables = new Dictionary<string, string> {{"field1", termText}};

            var sql = FieldPlaceholder + " LIKE '%' + @field1 + '%'";
            return new ParameterizedSql(sql, userVariables);
        }

        protected override ParameterizedSql BuildQuery(TermRangeQuery termRangeQuery)
        {
            // Not handling the TermRangeQuery.
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
