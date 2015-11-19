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

using NUnit.Framework;

namespace LuceneQueryToSql.Tests
{
    [TestFixture]
    public class SqlQueryBuilderTests
    {
        [Test]
        public void BuildTermQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc");

            // Assert
            Assert.AreEqual("{{COLUMN}} LIKE '%' + @field1 + '%'", parameterizedSql.Sql);
        }

        [Test]
        public void BuildTermQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildTermQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc");

            // Assert
            Assert.AreEqual("ABC", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildPhraseQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"abc def\"");

            // Assert
            Assert.AreEqual("{{COLUMN}} LIKE '%' + @field1 + '%'", parameterizedSql.Sql);
        }

        [Test]
        public void BuildPhraseQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"abc def\"");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildPhraseQuery_OneValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"abc def\"");

            // Assert
            Assert.AreEqual("ABC DEF", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildBooleanQuery_OneValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("+foo");

            // Assert
            Assert.AreEqual("{{COLUMN}} LIKE '%' + @field1 + '%'", parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_OneValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("+foo");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_OneValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("+foo");

            // Assert
            Assert.AreEqual("FOO", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildBooleanQuery_TwoValidInputs_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            var sqlOutput = "({{COLUMN}} LIKE '%' + @field1 + '%') AND ({{COLUMN}} LIKE '%' + @field2 + '%')";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("foo AND bar");

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_TwoValidInputs_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("foo AND bar");

            // Assert
            Assert.AreEqual(2, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_TwoValidInputs_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("foo AND bar");

            // Assert
            Assert.AreEqual("FOO", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("BAR", parameterizedSql.UserInputVariables["field2"]);
        }

        [Test]
        public void BuildQuery_SqlWildcardInput_EscapedOutput()
        {
            // Arrange
            var sqlQueryBuilder = new SqlQueryBuilder();
            var luceneQuery = "\"5% of coders \\{\\{FOO\\}\\} are cod\\[ing\\] all_night_long\"";
            var sqlOutput = "5[%] OF CODERS [{][{]FOO}} ARE COD[[]ING] ALL[_]NIGHT[_]LONG";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery);

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.UserInputVariables["field1"]);
        }
    }
}
