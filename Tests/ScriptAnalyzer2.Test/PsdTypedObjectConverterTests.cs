
using Microsoft.PowerShell.ScriptAnalyzer.Configuration.Psd;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;
using Xunit;

namespace ScriptAnalyzer2.Test
{
    public class PsdTypedObjectConverterTests
    {
        private readonly PsdTypedObjectConverter _converter;

        public PsdTypedObjectConverterTests()
        {
            _converter = new PsdTypedObjectConverter();
        }

        [Fact]
        public void TestSimpleFieldObject()
        {
            var obj = _converter.Convert<SimpleFieldObject>("@{ Field = 'Banana' }");
            Assert.Equal("Banana", obj.Field);
        }

        [Fact]
        public void TestSimplePropertyObject()
        {
            var obj = _converter.Convert<SimplePropertyObject>("@{ Property = 'Goose' }");
            Assert.Equal("Goose", obj.Property);
        }

        [Fact]
        public void TestSimpleReadOnlyFieldObject()
        {
            var obj = _converter.Convert<SimpleReadOnlyFieldObject>("@{ ReadOnlyField = 'Goose' }");
            Assert.Equal("Goose", obj.ReadOnlyField);
        }

        [Fact]
        public void TestSimpleReadOnlyPropertyObject()
        {
            var obj = _converter.Convert<SimpleReadOnlyPropertyObject>("@{ ReadOnlyProperty = 'Goose' }");
            Assert.Equal("Goose", obj.ReadOnlyProperty);
        }

        [Fact]
        public void TestNumerics()
        {
            var obj = _converter.Convert<NumericObject>(@"
@{
    SByte   = 3
    Byte    = 4
    Short   = 5
    Int     = 6
    Long    = 7
    UShort  = 8
    UInt    = 9
    ULong   = 10
    Decimal = 11.1
    Float   = 12.2
    Double  = 13.3
}");
            Assert.Equal((sbyte)3, obj.SByte);
            Assert.Equal((byte)4, obj.Byte);
            Assert.Equal((short)5, obj.Short);
            Assert.Equal(6, obj.Int);
            Assert.Equal(7, obj.Long);
            Assert.Equal((ushort)8u, obj.UShort);
            Assert.Equal(9u, obj.UInt);
            Assert.Equal(10u, obj.ULong);
            Assert.Equal(11.1m, obj.Decimal);
            Assert.Equal(12.2f, obj.Float);
            Assert.Equal(13.3, obj.Double);
        }

        [Fact]
        public void DateTimeTest()
        {
            var expected = new DateTime(ticks: 637191090850000000);
            var obj = _converter.Convert<DateTimeObject>("@{ DateTime = '6/03/2020 4:31:25 PM' }");
            Assert.Equal(expected, obj.DateTime);
        }

        [Fact]
        public void DataContractTest()
        {
            var obj = _converter.Convert<DataContractObject>("@{ FirstName = 'Raymond'; FamilyName = 'Yacht' }");
            Assert.Equal("Raymond", obj.FirstName);
            Assert.Equal("Yacht", obj.LastName);
            Assert.Null(obj.MiddleName);
        }

        [Fact]
        public void DataContractThrowsTest()
        {
            Assert.Throws<ArgumentException>(() => _converter.Convert<DataContractObject>("@{ FirstName = 'Raymond'; MiddleName = 'Luxury'; FamilyName = 'Yacht' }"));
        }

        [Fact]
        public void PartiallySetObjectTest()
        {
            var obj = _converter.Convert<PartiallySettableObject>("@{ Name = 'Moose'; Count = 4 }");
            Assert.Equal("Moose", obj.Name);
            Assert.Equal(4, obj.Count);
        }

        [Fact]
        public void PartiallySetObjectDefaultTest()
        {
            var obj = _converter.Convert<PartiallySettableObject>("@{ Name = 'Moose' }");
            Assert.Equal("Moose", obj.Name);
            Assert.Equal(1, obj.Count);
        }

        [Fact]
        public void JsonObjectTest()
        {
            var obj = _converter.Convert<JsonObject>("@{ FullName = 'Moo'; Count = 10 }");
            Assert.Equal("Moo", obj.Name);
            Assert.Equal(10, obj.Count);
        }


        [Fact]
        public void JsonObjectBadMemberTest()
        {
            Assert.Throws<ArgumentException>(() => _converter.Convert<JsonObject>("@{ FullName = 'Moo'; ShortName = 'M'; Count = 10 }"));
        }

        [Fact]
        public void TestJsonObjectOptOut()
        {
            var obj = _converter.Convert<JsonObject2>("@{ Count = 2; ShortName = 'F'; FullName = 'Farquad' }");
            Assert.Equal(2, obj.Count);
            Assert.Equal("F", obj.ShortName);
            Assert.Equal("Farquad", obj.Name);
        }


        [Fact]
        public void TestJsonObjectOptOutThrows()
        {
            Assert.Throws<ArgumentException>(() => _converter.Convert<JsonObject2>("@{ Count = 2; Sign = 'Libra'; ShortName = 'F'; FullName = 'Farquad' }"));
        }

        [Fact]
        public void TestCompositeObject()
        {
            var obj = _converter.Convert<CompositeObject>("@{ Name = 'Thing'; Simple = @{ Field = 'Moo' }; SubObject = @{ Count = 3; FullName = 'X' } }");

            Assert.Equal("Thing", obj.Name);
            Assert.Equal("Moo", obj.Simple.Field);
            Assert.Equal(3, obj.SubObject.Count);
            Assert.Equal("X", obj.SubObject.Name);
        }

        [Fact]
        public void TestReadOnlyCompositeObject()
        {
            var obj = _converter.Convert<InjectedCompositeObject>("@{ Name = 'Thing'; Simple = @{ Field = 'Moo' }; SubObject = @{ Count = 3; FullName = 'X' } }");

            Assert.Equal("Thing", obj.Name);
            Assert.Equal("Moo", obj.Simple.Field);
            Assert.Equal(3, obj.SubObject.Count);
            Assert.Equal("X", obj.SubObject.Name);
        }

        [Fact]
        public void TestDictionary()
        {
            var obj = _converter.Convert<Dictionary<string, SimpleFieldObject>>("@{ '1' = @{ Field = 'x' }; '2' = @{ Field = 'y' } }");

            Assert.Equal("x", obj["1"].Field);
            Assert.Equal("y", obj["2"].Field);
        }

        [Fact]
        public void TestIDictionary()
        {
            var obj = _converter.Convert<IDictionary<string, SimpleFieldObject>>("@{ '1' = @{ Field = 'x' }; '2' = @{ Field = 'y' } }");

            Assert.Equal("x", obj["1"].Field);
            Assert.Equal("y", obj["2"].Field);
        }

        [Fact]
        public void TestReadOnlyDictionary()
        {
            var obj = _converter.Convert<IReadOnlyDictionary<string, SimpleFieldObject>>("@{ '1' = @{ Field = 'x' }; '2' = @{ Field = 'y' } }");

            Assert.Equal("x", obj["1"].Field);
            Assert.Equal("y", obj["2"].Field);
        }

        [Fact]
        public void TestHashtable()
        {
            var obj = _converter.Convert<Hashtable>("@{ 1 = @{ X = 'x' }; 'hi there' = 2 }");

            Assert.Equal("x", ((Hashtable)obj[1])["X"]);
            Assert.Equal(2, obj["hi there"]);
        }

        [Fact]
        public void TestArray()
        {
            var obj = _converter.Convert<double[]>("@( 2.1, 3.8, 4.0 )");
            Assert.Equal(new double[] { 2.1, 3.8, 4.0 }, obj);
        }

        [Fact]
        public void TestList()
        {
            var obj = _converter.Convert<List<string>>("'hi','THERE','Friend'");
            Assert.Equal(new List<string> { "hi", "THERE", "Friend" }, obj);
        }

        [Fact]
        public void TestGenericIEnumerable()
        {
            var obj = _converter.Convert<IEnumerable<string>>("@('hi','THERE','Friend')");
            Assert.Equal(new[] { "hi", "THERE", "Friend" }, obj);
        }

        [Fact]
        public void TestNonGenericIEnumerable()
        {
            var obj = _converter.Convert<IEnumerable>("'hi', 7, $true");
            Assert.Equal(new object[] { "hi", 7, true }, obj);
        }

        [Fact]
        public void TestArrayOfObject()
        {
            var obj = _converter.Convert<PartiallySettableObject[]>("@{ Name = 'a'; Count = 1 }, @{ Name = 'b'; Count = 2 }");
            Assert.Equal("a", obj[0].Name);
            Assert.Equal(1, obj[0].Count);
            Assert.Equal("b", obj[1].Name);
            Assert.Equal(2, obj[1].Count);
        }

        [Fact]
        public void TestBool()
        {
            var obj = _converter.Convert<bool>("$false");
            Assert.False(obj);
        }

        [Fact]
        public void TestNull()
        {
            var obj = _converter.Convert<object>("$null");
            Assert.Null(obj);
        }

        [Fact]
        public void TestTypedNull()
        {
            var obj = _converter.Convert<SimpleFieldObject>("$null");
            Assert.Null(obj);
        }

        [Fact]
        public void TestNullableValue()
        {
            var obj = _converter.Convert<int?>("4");
            Assert.Equal(4, obj);
        }

        [Fact]
        public void TestNullableValue_Null()
        {
            var obj = _converter.Convert<int?>("$null");
            Assert.Null(obj);
        }

        [Fact]
        public void TestNonNullableValue()
        {
            Assert.Throws<ArgumentException>(() => _converter.Convert<int>("$null"));
        }

        [Fact]
        public void TestConversionToObject_Bool()
        {
            var obj = _converter.Convert<object>("$true");
            Assert.Equal(true, obj);
        }

        [Fact]
        public void TestConversionToObject_Array()
        {
            var obj = _converter.Convert<object>("1, $true, 'x'");
            Assert.Equal(new object[] { 1, true, "x" }, (object[])obj);
        }
        
        [Fact]
        public void TestConversionToObject_Hashtable()
        {
            var obj = _converter.Convert<object>("@{ 1 = 'a',5; 'b' = @{ t = $false } }");

            Assert.IsType<Hashtable>(obj);

            var hashtable = (Hashtable)obj;

            Assert.Equal(new object[] { "a", 5 }, hashtable[1]);
            Assert.IsType<Hashtable>(hashtable["b"]);

            var subtable = (Hashtable)hashtable["b"];

            Assert.Equal(false, subtable["t"]);
        }

        [Fact]
        public void TestPartialInstantiation()
        {
            var obj = _converter.Convert<HashtableAst>("@{ x = 1; y = 2 }");
            Assert.Equal(1, ((ConstantExpressionAst)GetExpressionAstFromStatementAst(obj.KeyValuePairs[0].Item2)).Value);
            Assert.Equal(2, ((ConstantExpressionAst)GetExpressionAstFromStatementAst(obj.KeyValuePairs[1].Item2)).Value);
        }

        [Fact]
        public void TestPartialInstantiation_ReadOnlyDict()
        {
            var obj = _converter.Convert<IReadOnlyDictionary<string, ExpressionAst>>("@{ x = 1; y = 2 }");
            Assert.Equal(1, ((ConstantExpressionAst)obj["x"]).Value);
            Assert.Equal(2, ((ConstantExpressionAst)obj["y"]).Value);
        }

        [Fact]
        public void TestPartialInstantiation_ReadOnlyHashtableDict()
        {
            var obj = _converter.Convert<IReadOnlyDictionary<string, HashtableAst>>("@{ x = @{ m = 'hi' }; y = @{ m = 'bye' } }");
            var x = _converter.Convert<IReadOnlyDictionary<string, string>>(obj["x"]);
            var y = _converter.Convert<IReadOnlyDictionary<string, string>>(obj["y"]);
            Assert.Equal("hi", x["m"]);
            Assert.Equal("bye", y["m"]);
        }

        [Fact]
        public void TestPartialInstantiation_IEnumerable()
        {
            var obj = _converter.Convert<IReadOnlyList<ExpressionAst>>("1, 'hi', $true");
            var one = _converter.Convert<int>(obj[0]);
            var two = _converter.Convert<string>(obj[1]);
            var three = _converter.Convert<bool>(obj[2]);
            Assert.Equal(1, one);
            Assert.Equal("hi", two);
            Assert.True(three);
        }

        [Fact]
        public void TestEnum_Undecorated()
        {
            var one = _converter.Convert<UndecoratedEnum>("'one'");
            var twoThree = _converter.Convert<UndecoratedEnum[]>("'two','three'");

            Assert.Equal(UndecoratedEnum.One, one);
            Assert.Equal(new[] { UndecoratedEnum.Two, UndecoratedEnum.Three }, twoThree);
        }

        [Fact]
        public void TestEnum_WithEnumMember()
        {
            var goose = _converter.Convert<DecoratedEnum>("'goose'");
            var dog = _converter.Convert<DecoratedEnum>("'dog'");

            Assert.Equal(DecoratedEnum.Duck, goose);
            Assert.Equal(DecoratedEnum.Dog, dog);
            Assert.Throws<ArgumentException>(() => _converter.Convert<DecoratedEnum>("'missing'"));
        }

        private ExpressionAst GetExpressionAstFromStatementAst(StatementAst statementAst)
        {
            return ((PipelineAst)statementAst).GetPureExpression();
        }
    }

    public enum UndecoratedEnum
    {
        One,
        Two,
        Three
    }

    public enum DecoratedEnum
    {
        [EnumMember(Value = "goose")]
        Duck,

        Missing,

        [EnumMember]
        Dog,
    }

    public class SimpleFieldObject
    {
        public string Field;
    }

    public class SimplePropertyObject
    {
        public string Property { get; set; }
    }

    public class SimpleReadOnlyFieldObject
    {
        public SimpleReadOnlyFieldObject(string readOnlyField)
        {
            ReadOnlyField = readOnlyField;
        }

        public readonly string ReadOnlyField;
    }

    public class SimpleReadOnlyPropertyObject
    {
        public SimpleReadOnlyPropertyObject(string readOnlyProperty)
        {
            ReadOnlyProperty = readOnlyProperty;
        }

        public string ReadOnlyProperty { get; }
    }

    public class PartiallySettableObject
    {
        public PartiallySettableObject(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public int Count { get; set; } = 1;
    }

    public class NumericObject
    {
        public sbyte SByte;

        public byte Byte;

        public short Short;

        public int Int;

        public long Long;

        public ushort UShort;

        public uint UInt;

        public ulong ULong;

        public decimal Decimal;

        public float Float;

        public double Double;
    }

    public class DateTimeObject
    {
        public DateTime DateTime;
    }

    [DataContract]
    public class DataContractObject
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember(Name = "FamilyName")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class JsonObject
    {
        [JsonConstructor]
        public JsonObject(string name)
        {
            Name = name;
        }

        [JsonProperty]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "FullName")]
        public string Name { get; }

        public string ShortName { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class JsonObject2
    {
        [JsonConstructor]
        public JsonObject2(string name)
        {
            Name = name;
        }

        [JsonProperty]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "FullName")]
        public string Name { get; }

        public string ShortName { get; set; }

        [JsonIgnore]
        public string Sign { get; set; }
    }

    public class CompositeObject
    {
        public string Name { get; set; }

        public SimpleFieldObject Simple { get; set; }

        public JsonObject SubObject { get; set; }
    }


    public class InjectedCompositeObject
    {
        [JsonConstructor]
        public InjectedCompositeObject(string name, SimpleFieldObject simple, JsonObject subObject)
        {
            Name = name;
            Simple = simple;
            SubObject = subObject;
        }

        public string Name { get; }

        public SimpleFieldObject Simple { get; }

        public JsonObject SubObject { get; }
    }
}
