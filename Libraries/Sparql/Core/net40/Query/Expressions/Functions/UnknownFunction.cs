/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents an Unknown Function that is not supported by dotNetRDF
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exists as a placeholder class so users may choose to parse Unknown Functions and have them appear in queries even if they cannot be evaluated.  This is useful when you wish to parse a query locally to check syntactic validity before passing it to an external query processor which may understand how to evaluate the function.  Using this placeholder also allows queries containing Unknown Functions to still be formatted properly.
    /// </para>
    /// </remarks>
    public class UnknownFunction
        : BaseNAryExpression
    {
        private readonly Uri _funcUri;

        /// <summary>
        /// Creates a new Unknown Function that has no Arguments
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        public UnknownFunction(Uri funcUri)
            : this(funcUri, Enumerable.Empty<IExpression>()) { }

        /// <summary>
        /// Creates a new Unknown Function that has a Single Argument
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        /// <param name="expr">Argument Expression</param>
        public UnknownFunction(Uri funcUri, IExpression expr)
            : this(funcUri, expr.AsEnumerable()) { }

        /// <summary>
        /// Creates a new Unknown Function that has multiple Arguments
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        /// <param name="exprs">Argument Expressions</param>
        public UnknownFunction(Uri funcUri, IEnumerable<IExpression> exprs)
            : base(exprs)
        {
            this._funcUri = funcUri;
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new UnknownFunction(this._funcUri, args);
        }

        /// <summary>
        /// Gives null as the Value since dotNetRDF does not know how to evaluate Unknown Functions
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            throw new RdfQueryException(string.Format("No implementation for {0} available", this._funcUri));
        }

        /// <summary>
        /// Gets the Function URI of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return this._funcUri.ToString(); 
            }
        }
    }
}
