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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace NanoServerCompliance
{
    public class ReflectionTypeAnalysis
    {
        public Dictionary<string, List<string>> typeMethod { get; set; }
        public Dictionary<string, List<string>> typeProperties { get; set; }
        public Dictionary<string, List<string>> typeFields { get; set; } 

        private static object syncRoot = new Object();
        private static ReflectionTypeAnalysis instance = null;

        /// <summary>
        /// The helper instance that handles utility functions
        /// </summary>
        public static ReflectionTypeAnalysis Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ReflectionTypeAnalysis();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Get available types from reading the assembly
        /// </summary>
        /// <param name="assemblyDir"></param>
        public void GetTypeFromAssembly(string assemblyDir)
        {
            //Initalize the type dictionaries
            typeMethod = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            typeProperties = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            typeFields = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string filePath in Directory.GetFiles(assemblyDir))
            {
                if (!filePath.EndsWith(".METADATA_DLL", StringComparison.OrdinalIgnoreCase))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, "Unexpected file path");
                    //throw new InvalidOperationException(message);
                }

                using (Stream stream = File.OpenRead(filePath))
                using (PEReader peReader = new PEReader(stream))
                {
                    MetadataReader metadataReader = peReader.GetMetadataReader();
                    foreach (TypeDefinitionHandle typeHandle in metadataReader.TypeDefinitions)
                    {
                        // We only care about public types
                        TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeHandle);
                        // The visibility mask is used to mask out the bits that contain the visibility.
                        // The visibilities are not combineable, e.g. you can't be both public and private, which is why these aren't independent powers of two.

                        TypeAttributes visibilityBits = typeDefinition.Attributes & TypeAttributes.VisibilityMask;
                        if (visibilityBits != TypeAttributes.Public && visibilityBits != TypeAttributes.NestedPublic)
                        {
                            continue;
                        }
                        string typeName = GetTypeFullName(metadataReader, typeDefinition);

                        List<string> methodNames = GetTypeMethod(typeDefinition, metadataReader);
                        List<string> propertyNames = GetTypeProperties(typeDefinition, metadataReader);
                        List<string> fieldNames = GetTypeFields(typeDefinition, metadataReader);
                        if (typeMethod.ContainsKey(typeName))
                        {
                            List<string> typeMethodNames = typeMethod[typeName];
                            typeMethodNames.AddRange(methodNames);
                            List<string> typePropertyNames = typeProperties[typeName];
                            typePropertyNames.AddRange(propertyNames);
                            List<string> typeFieldNames = typeFields[typeName];
                            typeFieldNames.AddRange(fieldNames);
                        }
                        else
                        {
                            typeMethod.Add(typeName, methodNames);
                            typeProperties.Add(typeName, propertyNames);
                            typeFields.Add(typeName, fieldNames);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Get the full name of a Type.
        /// </summary>
        private string GetTypeFullName(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            string fullName = String.Empty;
            string typeName = metadataReader.GetString(typeDefinition.Name);
            string nsName = metadataReader.GetString(typeDefinition.Namespace);

            // Get the enclosing type if the type is nested
            TypeDefinitionHandle declaringTypeHandle = typeDefinition.GetDeclaringType();
            if (declaringTypeHandle.IsNil)
            {
                fullName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nsName, typeName);
            }
            else
            {
                fullName = typeName;
                while (!declaringTypeHandle.IsNil)
                {
                    TypeDefinition declaringTypeDef = metadataReader.GetTypeDefinition(declaringTypeHandle);
                    declaringTypeHandle = declaringTypeDef.GetDeclaringType();
                    if (declaringTypeHandle.IsNil)
                    {
                        fullName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}+{2}",
                                                 metadataReader.GetString(declaringTypeDef.Namespace),
                                                 metadataReader.GetString(declaringTypeDef.Name),
                                                 fullName);
                    }
                    else
                    {
                        fullName = string.Format(CultureInfo.InvariantCulture, "{0}+{1}",
                                                 metadataReader.GetString(declaringTypeDef.Name),
                                                 fullName);
                    }
                }
            }

            return fullName;
        }

        /// <summary>
        /// Get all public methods from a given type
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="metadataReader"></param>
        /// <returns></returns>
        public List<string> GetTypeMethod(TypeDefinition typeDefinition, MetadataReader metadataReader)
        {
            List<string> methodNames = new List<string>();
            MethodDefinitionHandleCollection methods = typeDefinition.GetMethods();
            foreach (MethodDefinitionHandle method in methods)
            {
                MethodDefinition methodDef = metadataReader.GetMethodDefinition(method);
                StringHandle methodName = methodDef.Name;
                string name = metadataReader.GetString(methodName);
                methodNames.Add(name);
            }
            return methodNames;
        }

        /// <summary>
        /// Get all properties from a type
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="metadataReader"></param>
        /// <returns></returns>
        public List<string> GetTypeProperties(TypeDefinition typeDefinition, MetadataReader metadataReader)
        {
            List<string> propertyNames = new List<string>();
            PropertyDefinitionHandleCollection properties = typeDefinition.GetProperties();
            foreach (PropertyDefinitionHandle property in properties)
            {
                PropertyDefinition propertyDef = metadataReader.GetPropertyDefinition(property);
                StringHandle propertyName = propertyDef.Name;
                string name = metadataReader.GetString(propertyName);
                propertyNames.Add(name);
            }
            return propertyNames;
        }
        
        /// <summary>
        /// Get all fields from a given type
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="metadataReader"></param>
        /// <returns></returns>
        public List<string> GetTypeFields(TypeDefinition typeDefinition, MetadataReader metadataReader)
        {
            List<string> fieldNames = new List<string>();
            FieldDefinitionHandleCollection fields = typeDefinition.GetFields();
            foreach (FieldDefinitionHandle field in fields)
            {
                FieldDefinition fieldDef = metadataReader.GetFieldDefinition(field);
                StringHandle fieldName = fieldDef.Name;
                string name = metadataReader.GetString(fieldName);
                fieldNames.Add(name);
            }
            return fieldNames;
        }
    }
}
