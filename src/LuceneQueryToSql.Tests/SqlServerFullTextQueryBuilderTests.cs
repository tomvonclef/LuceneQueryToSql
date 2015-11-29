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
    public class SqlServerFullTextQueryBuilderTests
    {
        [Test]
        public void BuildTermQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc");

            // Assert
            Assert.AreEqual("CONTAINS({{COLUMN}}, @field1)", parameterizedSql.Sql);
        }

        [Test]
        public void BuildTermQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildTermQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc");

            // Assert
            Assert.AreEqual("ABC", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildPrefixQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc*");

            // Assert
            Assert.AreEqual("CONTAINS({{COLUMN}}, \"@field1\")", parameterizedSql.Sql);
        }

        [Test]
        public void BuildPrefixQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc*");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildPrefixQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("abc*");

            // Assert
            Assert.AreEqual("ABC%", parameterizedSql.UserInputVariables["field1"]);
        }


        [Test]
        public void BuildTermRangeQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("{a TO B}");

            // Assert
            Assert.AreEqual(null, parameterizedSql);
        }

        [Test]
        public void BuildTermRangeQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("{a TO B}");

            // Assert
            Assert.AreEqual(null, parameterizedSql);
        }

        [Test]
        public void BuildTermRangeQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("{a TO B}");

            // Assert
            Assert.AreEqual(null, parameterizedSql);
        }

        [Test]
        public void BuildWildcardQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("Ab?Cd*");

            // Assert
            Assert.AreEqual("CONTAINS({{COLUMN}}, \"@field1\")", parameterizedSql.Sql);
        }

        [Test]
        public void BuildWildcardQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("Ab?Cd*");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildWildcardPhraseQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"Ab?Cd* dog\"");

            // Assert
            Assert.AreEqual("CONTAINS({{COLUMN}}, \"@field1\")", parameterizedSql.Sql);
        }

        [Test]
        public void BuildWildcardPhraseQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"Ab?Cd* dog\"");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildWildcardPhraseQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"Ab?Cd* dog\"");

            // Assert
            Assert.AreEqual("AB_CD% DOG", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildWildcardQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("Ab?Cd*");

            // Assert
            Assert.AreEqual("AB_CD%", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildFuzzyQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("Able~");

            // Assert
            Assert.AreEqual("CONTAINS({{COLUMN}}, @field1)", parameterizedSql.Sql);
        }

        [Test]
        public void BuildFuzzyQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("Able~");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildFuzzyQuery_ValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("Able~");

            // Assert
            Assert.AreEqual("ABLE", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildPhraseQuery_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"abc def\"");

            // Assert
            Assert.AreEqual("CONTAINS({{COLUMN}}, @field1)", parameterizedSql.Sql);
        }

        [Test]
        public void BuildPhraseQuery_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"abc def\"");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildPhraseQuery_OneValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("\"abc def\"");

            // Assert
            Assert.AreEqual("ABC DEF", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildBooleanQuery_OneValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("+foo");

            // Assert
            Assert.AreEqual("((CONTAINS({{COLUMN}}, @field1)))", parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_OneValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("+foo");

            // Assert
            Assert.AreEqual(1, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_OneValidInput_ValidParameter()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("+foo");

            // Assert
            Assert.AreEqual("FOO", parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildBooleanQuery_TwoValidInputs_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var sqlOutput = "((CONTAINS({{COLUMN}}, @field1)) AND (CONTAINS({{COLUMN}}, @field2)))";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("foo AND bar");

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_TwoValidInputs_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("foo AND bar");

            // Assert
            Assert.AreEqual(2, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_TwoValidInputs_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("foo AND bar");

            // Assert
            Assert.AreEqual("FOO", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("BAR", parameterizedSql.UserInputVariables["field2"]);
        }

        [Test]
        public void BuildBooleanQuery_ThreeValidInputs_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var sqlOutput = "((CONTAINS({{COLUMN}}, @field1)) AND (CONTAINS({{COLUMN}}, @field2)))";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("cat OR dog AND muzzle");

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_ThreeValidInputs_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("cat OR dog AND muzzle");

            // Assert
            Assert.AreEqual(2, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_ThreeValidInputs_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("cat OR dog AND muzzle");

            // Assert
            Assert.AreEqual("DOG", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("MUZZLE", parameterizedSql.UserInputVariables["field2"]);
        }

        [Test]
        public void BuildBooleanQuery_FourValidInputs_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var sqlOutput = "((((CONTAINS({{COLUMN}}, @field1)) OR (CONTAINS({{COLUMN}}, @field2)))) AND "
                          + "(((CONTAINS({{COLUMN}}, @field3)) OR (CONTAINS({{COLUMN}}, @field4)))))";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle Or leash)");

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_FourValidInputs_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle Or leash)");

            // Assert
            Assert.AreEqual(4, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_FourValidInputs_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle Or leash)");

            // Assert
            Assert.AreEqual("CAT", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("DOG", parameterizedSql.UserInputVariables["field2"]);
            Assert.AreEqual("MUZZLE", parameterizedSql.UserInputVariables["field3"]);
            Assert.AreEqual("LEASH", parameterizedSql.UserInputVariables["field4"]);
        }

        [Test]
        public void BuildBooleanQuery_FiveValidInputs_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var sqlOutput = "((((CONTAINS({{COLUMN}}, @field1)) OR (CONTAINS({{COLUMN}}, @field2)))) AND "
                          + "(((CONTAINS({{COLUMN}}, @field3)) OR (CONTAINS({{COLUMN}}, @field4)) OR "
                          + "(CONTAINS({{COLUMN}}, @field5)))))";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle Or leash or toy)");

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_FiveValidInputs_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle Or leash or toy)");

            // Assert
            Assert.AreEqual(5, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_FiveValidInputs_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle Or leash or toy)");

            // Assert
            Assert.AreEqual("CAT", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("DOG", parameterizedSql.UserInputVariables["field2"]);
            Assert.AreEqual("MUZZLE", parameterizedSql.UserInputVariables["field3"]);
            Assert.AreEqual("LEASH", parameterizedSql.UserInputVariables["field4"]);
            Assert.AreEqual("TOY", parameterizedSql.UserInputVariables["field5"]);
        }

        [Test]
        public void BuildBooleanQuery_SixValidInputs_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var sqlOutput = "((((CONTAINS({{COLUMN}}, @field1)) OR (CONTAINS({{COLUMN}}, @field2)))) AND "
                          + "(((CONTAINS({{COLUMN}}, @field3)) AND (CONTAINS({{COLUMN}}, @field4)) AND "
                          + "(((CONTAINS({{COLUMN}}, @field5))) AND (NOT (CONTAINS({{COLUMN}}, @field6)))))))";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle aNd leash (-toy +treat))");

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildBooleanQuery_SixValidInputs_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle aNd leash (-toy +treat))");

            // Assert
            Assert.AreEqual(6, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildBooleanQuery_SixValidInputs_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause("(cat Or dog) And (muzzle aNd leash (-toy +treat))");

            // Assert
            Assert.AreEqual("CAT", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("DOG", parameterizedSql.UserInputVariables["field2"]);
            Assert.AreEqual("MUZZLE", parameterizedSql.UserInputVariables["field3"]);
            Assert.AreEqual("LEASH", parameterizedSql.UserInputVariables["field4"]);
            Assert.AreEqual("TREAT", parameterizedSql.UserInputVariables["field5"]);
            Assert.AreEqual("TOY", parameterizedSql.UserInputVariables["field6"]);
        }
        [Test]
        public void BuildQuery_SqlWildcardInput_EscapedOutput()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "\"5% of coders \\{\\{FOO\\}\\} are cod\\[ing\\] all_night_long\"";
            var sqlOutput = "5[%] OF CODERS [{][{]FOO}} ARE COD[[]ING] ALL[_]NIGHT[_]LONG";
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery);

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.UserInputVariables["field1"]);
        }

        [Test]
        public void BuildSqlWhereClause_ValidInput_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "fruit AND (veg OR cheese)";
            var fields = new[] {"name", "desc", "code"};
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery, fields);

            // Assert
            Assert.AreEqual("FRUIT", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("VEG", parameterizedSql.UserInputVariables["field2"]);
            Assert.AreEqual("CHEESE", parameterizedSql.UserInputVariables["field3"]);
            Assert.AreEqual("FRUIT", parameterizedSql.UserInputVariables["field4"]);
            Assert.AreEqual("VEG", parameterizedSql.UserInputVariables["field5"]);
            Assert.AreEqual("CHEESE", parameterizedSql.UserInputVariables["field6"]);
            Assert.AreEqual("FRUIT", parameterizedSql.UserInputVariables["field7"]);
            Assert.AreEqual("VEG", parameterizedSql.UserInputVariables["field8"]);
            Assert.AreEqual("CHEESE", parameterizedSql.UserInputVariables["field9"]);
        }

        [Test]
        public void BuildSqlWhereClause_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "fruit AND (veg OR cheese)";
            var fields = new[] {"name", "desc", "code"};
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery, fields);

            // Assert
            Assert.AreEqual(9, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildSqlWhereClause_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "fruit AND (veg OR cheese)";
            var fields = new[] {"name", "desc", "code"};
            var sqlOutput = "(((CONTAINS(name, @field1)) AND (((CONTAINS(name, @field2)) OR (CONTAINS(name, @field3)))))) OR "
                          + "(((CONTAINS(desc, @field4)) AND (((CONTAINS(desc, @field5)) OR (CONTAINS(desc, @field6)))))) OR "
                          + "(((CONTAINS(code, @field7)) AND (((CONTAINS(code, @field8)) OR (CONTAINS(code, @field9))))))";
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery, fields);

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }

        [Test]
        public void BuildSqlWhereClause2_ValidInput_ValidParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "fruit AND veg OR cheese";
            var fields = new[] {"name", "desc", "code"};
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery, fields);

            // Assert
            Assert.AreEqual("FRUIT", parameterizedSql.UserInputVariables["field1"]);
            Assert.AreEqual("FRUIT", parameterizedSql.UserInputVariables["field2"]);
            Assert.AreEqual("FRUIT", parameterizedSql.UserInputVariables["field3"]);
        }

        [Test]
        public void BuildSqlWhereClause2_ValidInput_ValidNumParameters()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "fruit AND veg OR cheese";
            var fields = new[] {"name", "desc", "code"};
            
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery, fields);

            // Assert
            Assert.AreEqual(3, parameterizedSql.UserInputVariables.Count);
        }

        [Test]
        public void BuildSqlWhereClause2_ValidInput_ValidSql()
        {
            // Arrange
            var sqlQueryBuilder = new SqlServerFullTextQueryBuilder();
            var luceneQuery = "fruit AND veg OR cheese";
            var fields = new[] {"name", "desc", "code"};
            var sqlOutput = "(((CONTAINS(name, @field1)))) OR "
                          + "(((CONTAINS(desc, @field2)))) OR "
                          + "(((CONTAINS(code, @field3))))";
            // Act
            var parameterizedSql = sqlQueryBuilder.BuildSqlWhereClause(luceneQuery, fields);

            // Assert
            Assert.AreEqual(sqlOutput, parameterizedSql.Sql);
        }
    }
}
