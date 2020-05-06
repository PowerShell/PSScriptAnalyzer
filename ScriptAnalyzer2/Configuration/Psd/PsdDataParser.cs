using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration.Psd
{
    internal class PsdDataParser
    {
        /// <summary>
        /// Evaluates all statically evaluable, side-effect-free expressions under an
        /// expression AST to return a value.
        /// Throws if an expression cannot be safely evaluated.
        /// Attempts to replicate the GetSafeValue() method on PowerShell AST methods from PSv5.
        /// </summary>
        /// <param name="exprAst">The expression AST to try to evaluate.</param>
        /// <returns>The .NET value represented by the PowerShell expression.</returns>
        public object ConvertAstValue(ExpressionAst exprAst)
        {
            switch (exprAst)
            {
                case ConstantExpressionAst constExprAst:
                    // Note, this parses top-level command invocations as bareword strings
                    // However, forbidding this causes hashtable parsing to fail
                    // It is probably not worth the complexity to isolate this case
                    return constExprAst.Value;

                case VariableExpressionAst varExprAst:
                    // $true and $false are VariableExpressionAsts, so look for them here
                    switch (varExprAst.VariablePath.UserPath.ToLowerInvariant())
                    {
                        case "true":
                            return true;

                        case "false":
                            return false;

                        case "null":
                            return null;

                        default:
                            throw CreateInvalidDataExceptionFromAst(varExprAst);
                    }

                case ArrayExpressionAst arrExprAst:

                    // Most cases are handled by the inner array handling,
                    // but we may have an empty array
                    if (arrExprAst.SubExpression?.Statements == null)
                    {
                        throw CreateInvalidDataExceptionFromAst(arrExprAst);
                    }

                    if (arrExprAst.SubExpression.Statements.Count == 0)
                    {
                        return new object[0];
                    }

                    var listComponents = new List<object>();
                    // Arrays can either be array expressions (1, 2, 3) or array literals with statements @(1 `n 2 `n 3)
                    // Or they can be a combination of these
                    // We go through each statement (line) in an array and read the whole subarray
                    // This will also mean that @(1; 2) is parsed as an array of two elements, but there's not much point defending against this
                    foreach (StatementAst statement in arrExprAst.SubExpression.Statements)
                    {
                        if (!(statement is PipelineAst pipelineAst))
                        {
                            throw CreateInvalidDataExceptionFromAst(arrExprAst);
                        }

                        ExpressionAst pipelineExpressionAst = pipelineAst.GetPureExpression();
                        if (pipelineExpressionAst == null)
                        {
                            throw CreateInvalidDataExceptionFromAst(arrExprAst);
                        }

                        object arrayValue = ConvertAstValue(pipelineExpressionAst);
                        // We might hit arrays like @(\n1,2,3\n4,5,6), which the parser sees as two statements containing array expressions
                        if (arrayValue is object[] subArray)
                        {
                            listComponents.AddRange(subArray);
                            continue;
                        }

                        listComponents.Add(arrayValue);
                    }
                    return listComponents.ToArray();


                case ArrayLiteralAst arrLiteralAst:
                    return ConvertAstValue(arrLiteralAst);

                case HashtableAst hashtableAst:
                    return ConvertAstValue(hashtableAst);

                default:
                    // Other expression types are too complicated or fundamentally unsafe
                    throw CreateInvalidDataExceptionFromAst(exprAst);
            }
        }

        /// <summary>
        /// Process a PowerShell array literal with statically evaluable/safe contents
        /// into a .NET value.
        /// </summary>
        /// <param name="arrLiteralAst">The PowerShell array AST to turn into a value.</param>
        /// <returns>The .NET value represented by PowerShell syntax.</returns>
        public object[] ConvertAstValue(ArrayLiteralAst arrLiteralAst)
        {
            if (arrLiteralAst == null)
            {
                throw new ArgumentNullException(nameof(arrLiteralAst));
            }

            if (arrLiteralAst.Elements == null)
            {
                throw CreateInvalidDataExceptionFromAst(arrLiteralAst);
            }

            var elements = new List<object>();
            foreach (ExpressionAst exprAst in arrLiteralAst.Elements)
            {
                elements.Add(ConvertAstValue(exprAst));
            }

            return elements.ToArray();
        }

        /// <summary>
        /// Create a hashtable value from a PowerShell AST representing one,
        /// provided that the PowerShell expression is statically evaluable and safe.
        /// </summary>
        /// <param name="hashtableAst">The PowerShell representation of the hashtable value.</param>
        /// <returns>The Hashtable as a hydrated .NET value.</returns>
        public Hashtable ConvertAstValue(HashtableAst hashtableAst)
        {
            if (hashtableAst == null)
            {
                throw new ArgumentNullException(nameof(hashtableAst));
            }

            if (hashtableAst.KeyValuePairs == null)
            {
                throw CreateInvalidDataExceptionFromAst(hashtableAst);
            }

            // Enforce string keys, since that's always what we want
            var hashtable = new Hashtable();
            foreach (Tuple<ExpressionAst, StatementAst> entry in hashtableAst.KeyValuePairs)
            {
                object key = ConvertAstValue(entry.Item1);

                // Get the value
                ExpressionAst valueExprAst = (entry.Item2 as PipelineAst)?.GetPureExpression();
                if (valueExprAst == null)
                {
                    throw CreateInvalidDataExceptionFromAst(entry.Item2);
                }

                // Add the key/value entry into the hydrated hashtable
                hashtable[key] = ConvertAstValue(valueExprAst);
            }

            return hashtable;
        }

        private static InvalidDataException CreateInvalidDataExceptionFromAst(Ast ast)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            return CreateInvalidDataException(ast.Extent);
        }

        private static InvalidDataException CreateInvalidDataException(IScriptExtent extent)
        {
            return new InvalidDataException(
                $"Invalid PSD setting '{extent.Text}' at line {extent.StartLineNumber}, column {extent.StartColumnNumber} in file '{extent.File}'");
        }
    }
}
