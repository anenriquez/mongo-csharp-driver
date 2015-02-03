﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Builders
{
    [TestFixture]
    public class FieldsBuilderTypedTests
    {
        public class TestClass
        {
            public int _id;
            public int[] a;
            public SubClass[] a2;
            public int x;
            public string textfield;
            [BsonIgnoreIfDefault]
            public double relevance;
        }

        public class SubClass
        {
            public int b;
        }

        [Test]
        public void TestMetaText()
        {
            if (LegacyTestConfiguration.Server.Primary.Supports(FeatureId.TextSearchQuery))
            {
                var collection = LegacyTestConfiguration.Database.GetCollection<TestClass>("test_meta_text");
                collection.Drop();
                collection.CreateIndex(IndexKeys<TestClass>.Text(x => x.textfield));
                collection.Insert(new TestClass
                {
                    _id = 1,
                    textfield = "The quick brown fox jumped",
                    x = 1
                });
                collection.Insert(new TestClass
                {
                    _id = 2,
                    textfield = "over the lazy brown dog",
                    x = 1
                });

                var query = Query.Text("fox");
                var result = collection.FindOneAs<BsonDocument>(query);
                Assert.AreEqual(1, result["_id"].AsInt32);
                Assert.IsFalse(result.Contains("relevance"));
                Assert.IsTrue(result.Contains("x"));

                var fields = Fields<TestClass>.MetaTextScore(y => y.relevance).Exclude(y => y.x);
                result = collection.FindOneAs<BsonDocument>(new FindOneArgs() { Query = query, Fields = fields });
                Assert.AreEqual(1, result["_id"].AsInt32);
                Assert.IsTrue(result.Contains("relevance"));
                Assert.IsFalse(result.Contains("x"));
            }
        }
    }
}