/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;


namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class SesameTests
    {
        public static SesameHttpProtocolConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseSesame))
            {
                Assert.Inconclusive("Test Config marks Sesame as unavailable, cannot run test");
            }
            return new SesameHttpProtocolConnector(TestConfigManager.GetSetting(TestConfigManager.SesameServer), TestConfigManager.GetSetting(TestConfigManager.SesameRepository));
        }

        [TestMethod]
        public void StorageSesameSaveLoad()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/SesameTest");

                SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
                sesame.SaveGraph(g);

                //Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                Graph h = new Graph();
                sesame.LoadGraph(h, "http://example.org/SesameTest");
                Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

                Assert.AreEqual(g, h, "Graphs should have been equal");
            }
            finally
            {
                //Options.HttpFullDebugging = false;
                //Options.HttpDebugging = true;
            }
        }

        [TestMethod]
        public void StorageSesameDeleteTriples()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/SesameTest");

            SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
            sesame.SaveGraph(g);

            Console.WriteLine("Graph before deletion");
            TestTools.ShowGraph(g);

            //Delete all Triples about the Ford Fiesta
            sesame.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

            Object results = sesame.Query("ASK WHERE { <http://example.org/vehicles/FordFiesta> ?p ?o }");
            if (results is SparqlResultSet)
            {
                Assert.IsFalse(((SparqlResultSet)results).Result, "There should no longer be any triples about the Ford Fiesta present");
            }

            Graph h = new Graph();
            sesame.LoadGraph(h, g.BaseUri);

            Console.WriteLine("Graph after deletion");
            TestTools.ShowGraph(h);

            Assert.IsFalse(h.IsEmpty, "Graph should not be completely empty");
            Assert.IsTrue(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
            Assert.IsFalse(g.Equals(h), "Graph retrieved should not be equal to original Graph");
        }

        [TestMethod]
        public void StorageSesameCyrillic()
        {
                SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/sesame/cyrillic");
                FileLoader.Load(g, "cyrillic.rdf");
                sesame.SaveGraph(g);

                String ask = "ASK WHERE {?s ?p 'литерал'}";

                Object results = sesame.Query(ask);
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Failed to get a Result Set as expected");
                }
        }

        [TestMethod]
        public void StorageSesameAsk()
        {
                SesameHttpProtocolConnector sesame = SesameTests.GetConnection();

                String ask = "ASK WHERE { ?s ?p ?o }";

                Object results = sesame.Query(ask);
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Failed to get a Result Set as expected");
                }
        }

        [TestMethod]
        public void StorageSesameDescribe()
        {
                SesameHttpProtocolConnector sesame = SesameTests.GetConnection();

                String describe = "DESCRIBE <http://example.org/vehicles/FordFiesta>";

                Object results = sesame.Query(describe);
                if (results is IGraph)
                {
                    TestTools.ShowGraph((IGraph)results);
                }
                else
                {
                    Assert.Fail("Failed to get a Graph as expected");
                }
         }

        [TestMethod]
        public void StorageSesameSparqlUpdate1()
        {
            try
            {
                Options.HttpDebugging = true;

                SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
                sesame.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/sparqlUpdateLoad>");

                Graph orig = new Graph();
                orig.LoadFromUri(new Uri("http://dbpedia.org/resource/Ilkeston"));

                Graph actual = new Graph();
                sesame.LoadGraph(actual, "http://example.org/sparqlUpdateLoad");

                GraphDiffReport diff = orig.Difference(actual);
                if (!diff.AreEqual)
                {
                    TestTools.ShowDifferences(diff);
                }

                Assert.AreEqual(orig, actual, "Graphs should be equal");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageSesameSparqlUpdate2()
        {
            try
            {
                Options.HttpDebugging = true;

                SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/sparqlUpdateDeleteWhere");
                sesame.SaveGraph(g);

                sesame.Update("WITH <http://example.org/sparqlUpdateDeleteWhere> DELETE { ?s a ?type } WHERE { ?s a ?type }");

                Graph h = new Graph();
                sesame.LoadGraph(h, "http://example.org/sparqlUpdateDeleteWhere");
                INode rdfType = h.CreateUriNode("rdf:type");
                Assert.IsFalse(h.GetTriplesWithPredicate(rdfType).Any(), "Should not be any rdf:type triples after SPARQL Update operation");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
}