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
    }
}
