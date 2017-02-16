//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    public class Settings
    {
        private string filePath;
        private List<string> includeRules;
        private List<string> excludeRules;
        private List<string> severities;
        private Dictionary<string, Dictionary<string, object>> ruleArguments;

        public string FilePath { get { return filePath; } }
        public IEnumerable<string> IncludeRules { get { return includeRules; } }
        public IEnumerable<string> ExcludeRules { get { return excludeRules; } }
        public IEnumerable<string> Severity { get { return severities; } }
        public Dictionary<string, Dictionary<string, object>> RuleArguments { get { return ruleArguments; } }

        public Settings(object settings, Func<string, string> presetResolver)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            includeRules = new List<string>();
            excludeRules = new List<string>();
            severities = new List<string>();
            ruleArguments = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            var settingsFilePath = settings as string;

            //it can either be a preset or path to a file or a hashtable
            if (settingsFilePath != null)
            {
                if (presetResolver != null)
                {
                    var resolvedFilePath = presetResolver(settingsFilePath);
                    if (resolvedFilePath != null)
                    {
                        settingsFilePath = resolvedFilePath;
                    }
                }

                if (File.Exists(settingsFilePath))
                {
                    filePath = settingsFilePath;
                    parseSettingsFile(settingsFilePath);
                }
                else
                {
                    throw new ArgumentException(String.Format("File does not exist: {0}", settingsFilePath));
                }
            }
            else
            {
                var settingsHashtable = settings as Hashtable;
                if (settingsHashtable != null)
                {
                    parseSettingsHashtable(settingsHashtable);
                }
                else
                {
                    throw new ArgumentException("Input object should either be a string or a hashtable");
                }
            }
        }

        public Settings(object settings) : this(settings, null)
        {
        }

        /// <summary>
        /// Recursively convert hashtable to dictionary
        /// </summary>
        /// <param name="hashtable"></param>
        /// <returns>Dictionary that maps string to object</returns>
        private Dictionary<string, object> GetDictionaryFromHashtable(Hashtable hashtable)
        {
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var obj in hashtable.Keys)
            {
                string key = obj as string;
                if (key == null)
                {
                    // TODO localize
                    throw new InvalidDataException("key not string");
                    // writer.WriteError(
                    //     new ErrorRecord(
                    //         new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.KeyNotString, key)),
                    //         Strings.ConfigurationKeyNotAString,
                    //         ErrorCategory.InvalidData,
                    //         hashtable));
                    // hasError = true;
                }
                var valueHashtableObj = hashtable[obj];
                if (valueHashtableObj == null)
                {
                    throw new InvalidDataException("wrong hash table value");
                    // writer.WriteError(
                    //     new ErrorRecord(
                    //         new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongValueHashTable, valueHashtableObj, key)),
                    //         Strings.WrongConfigurationKey,
                    //         ErrorCategory.InvalidData,
                    //         hashtable));
                    // hasError = true;
                    // return null;
                }
                var valueHashtable = valueHashtableObj as Hashtable;
                if (valueHashtable == null)
                {
                    dictionary.Add(key, valueHashtableObj);
                }
                else
                {
                    dictionary.Add(key, GetDictionaryFromHashtable(valueHashtable));
                }
            }
            return dictionary;
        }

        private bool IsStringOrStringArray(object val)
        {
            if (val is string)
            {
                return true;
            }

            var valArr = val as object[];
            return val == null ? false : valArr.All(x => x is string);
        }

        private List<string> GetData(object val, string key)
        {
            // value must be either string or or an array of strings
            if (val == null)
            {
                throw new InvalidDataException(
                    String.Format(
                        "value should be a string or string array for {0} key",
                        key));
                // writer.WriteError(
                //     new ErrorRecord(
                //         new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongValueHashTable, value, key)),
                //         Strings.WrongConfigurationKey,
                //         ErrorCategory.InvalidData,
                //         profile));
                // hasError = true;
                // break;
            }

            List<string> values = new List<string>();
            var valueStr = val as string;
            if (valueStr != null)
            {
                values.Add(valueStr);
            }
            else
            {
                var valueArr = val as object[];
                if (valueArr != null)
                {
                    foreach (var item in valueArr)
                    {
                        var itemStr = item as string;
                        if (itemStr != null)
                        {
                            values.Add(itemStr);
                        }
                        else
                        {
                            throw new InvalidDataException("array items should be of string type");
                            // writer.WriteError(
                            //     new ErrorRecord(
                            //         new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongValueHashTable, val, key)),
                            //         Strings.WrongConfigurationKey,
                            //         ErrorCategory.InvalidData,
                            //         profile));
                            // hasError = true;
                            // break;
                        }
                    }
                }
                else
                {
                    throw new InvalidDataException("array items should be of string type");
                }
            }

            return values;
        }

        /// <summary>
        /// Sets the arguments for consumption by rules
        /// </summary>
        /// <param name="ruleArgs">A hashtable with rule names as keys</param>
        public Dictionary<string, Dictionary<string, object>> ConvertToRuleArgumentType(object ruleArguments)
        {
            var ruleArgs = ruleArguments as Dictionary<string, object>;
            if (ruleArgs == null)
            {
                throw new ArgumentException(
                    "input should be a dictionary",
                    "ruleArguments");
            }

            if (ruleArgs.Comparer != StringComparer.OrdinalIgnoreCase)
            {
                throw new ArgumentException(
                    "Input dictionary should have OrdinalIgnoreCase comparer.",
                    "ruleArguments");
            }

            var ruleArgsDict = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in ruleArgs.Keys)
            {
                var argsDict = ruleArgs[rule] as Dictionary<string, object>;
                if (argsDict == null)
                {
                    throw new ArgumentException(
                        "input should be a dictionary",
                        "ruleArguments");
                }
                ruleArgsDict[rule] = argsDict;
            }

            return ruleArgsDict;
        }

        private void parseSettingsHashtable(Hashtable settingsHashtable)
        {
            HashSet<string> validKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var settings = GetDictionaryFromHashtable(settingsHashtable);
            foreach (var settingKey in settings.Keys)
            {
                var key = settingKey.ToLower();
                object val = settings[key];
                switch (key)
                {
                    case "severity":
                        severities = GetData(val, key);
                        break;

                    case "includerules":
                        includeRules = GetData(val, key);
                        break;

                    case "excluderules":
                        excludeRules = GetData(val, key);
                        break;

                    case "rules":
                        ruleArguments = ConvertToRuleArgumentType(val);
                        break;

                    default:
                        throw new InvalidDataException(String.Format("Invalid key: {0}", key));
                        // writer.WriteError(
                        //     new ErrorRecord(
                        //         new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongKeyHashTable, key)),
                        //         Strings.WrongConfigurationKey,
                        //         ErrorCategory.InvalidData,
                        //         profile));
                        // hasError = true;
                        // break;
                }
            }
        }

        private void parseSettingsFile(string settingsFilePath)
        {
            Token[] parserTokens = null;
            ParseError[] parserErrors = null;
            Ast profileAst = Parser.ParseFile(settingsFilePath, out parserTokens, out parserErrors);
            IEnumerable<Ast> hashTableAsts = profileAst.FindAll(item => item is HashtableAst, false);

            // no hashtable, raise warning
            if (hashTableAsts.Count() == 0)
            {
                throw new ArgumentException("Given file does not contain a hashtable");
                // writer.WriteError(new ErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidProfile, profile)),
                //     Strings.ConfigurationFileHasNoHashTable, ErrorCategory.ResourceUnavailable, profile));
                // hasError = true;
            }

            HashtableAst hashTableAst = hashTableAsts.First() as HashtableAst;
            Hashtable hashtable;
            try
            {
                hashtable = hashTableAst.SafeGetValue() as Hashtable;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException("input file has invalid hashtable", e);
            }

            if (hashtable == null)
            {
                throw new ArgumentException("input file has invalid hashtable");
            }

            parseSettingsHashtable(hashtable);
        }

        private Hashtable GetValue(HashtableAst hashtableAst)
        {
#if !PSV3
            return hashtableAst.SafeGetValue() as Hashtable;
#else
            return GetHashtableFromHashTableAst(hashTableAst);
#endif
        }

        private Hashtable GetHashtableFromHashTableAst(HashtableAst hashTableAst)
        {
            var output = new Hashtable();
            foreach (var kvp in hashTableAst.KeyValuePairs)
            {
                var keyAst = kvp.Item1 as StringConstantExpressionAst;
                if (keyAst == null)
                {
                    // first item (the key) should be a string
                    ThrowInvalidDataException(kvp.Item1);
                }
                var key = keyAst.Value;

                // parse the item2 as array
                PipelineAst pipeAst = kvp.Item2 as PipelineAst;
                List<string> rhsList = new List<string>();
                if (pipeAst != null)
                {
                    ExpressionAst pureExp = pipeAst.GetPureExpression();
                    if (pureExp is StringConstantExpressionAst)
                    {
                        rhsList.Add(((StringConstantExpressionAst)pureExp).Value);
                    }
                    else if (pureExp is HashtableAst)
                    {
                        output[key] = GetHashtableFromHashTableAst((HashtableAst)pureExp);
                        continue;
                    }
                    else
                    {
                        rhsList = GetArrayFromAst(pureExp);
                    }
                }

                if (rhsList.Count == 0)
                {
                    ThrowInvalidDataException(kvp.Item2);
                }

                output[key] = rhsList;
            }

            return output;
        }

        private List<string> GetArrayFromAst(ExpressionAst exprAst)
        {
            ArrayLiteralAst arrayLitAst = exprAst as ArrayLiteralAst;
            var result = new List<string>();

            if (arrayLitAst == null && exprAst is ArrayExpressionAst)
            {
                ArrayExpressionAst arrayExp = (ArrayExpressionAst)exprAst;
                return arrayExp == null ? null : GetArrayFromArrayExpressionAst(arrayExp);
            }

            if (arrayLitAst == null)
            {
                ThrowInvalidDataException(arrayLitAst);
            }

            foreach (var element in arrayLitAst.Elements)
            {
                var elementValue = element as StringConstantExpressionAst;
                if (elementValue == null)
                {
                    ThrowInvalidDataException(element);
                }

                result.Add(elementValue.Value);
            }

            return result;
        }

        private List<string> GetArrayFromArrayExpressionAst(ArrayExpressionAst arrayExp)
        {
            var result = new List<string>();
            if (arrayExp.SubExpression != null)
            {
                StatementAst stateAst = arrayExp.SubExpression.Statements.FirstOrDefault();
                if (stateAst != null && stateAst is PipelineAst)
                {
                    CommandBaseAst cmdBaseAst = (stateAst as PipelineAst).PipelineElements.FirstOrDefault();
                    if (cmdBaseAst != null && cmdBaseAst is CommandExpressionAst)
                    {
                        CommandExpressionAst cmdExpAst = cmdBaseAst as CommandExpressionAst;
                        if (cmdExpAst.Expression is StringConstantExpressionAst)
                        {
                            return new List<string>()
                            {
                                (cmdExpAst.Expression as StringConstantExpressionAst).Value
                            };
                        }
                        else
                        {
                            // It should be an ArrayLiteralAst at this point
                            return GetArrayFromAst(cmdExpAst.Expression);
                        }
                    }
                }
            }

            return null;
        }

        private void ThrowInvalidDataException(Ast ast)
        {
            ThrowInvalidDataException(ast.Extent);
        }

        private void ThrowInvalidDataException(IScriptExtent extent)
        {
            throw new InvalidDataException(string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.WrongValueFormat,
                                    extent.StartLineNumber,
                                    extent.StartColumnNumber,
                                    extent.File ?? ""));
        }
    }
}
