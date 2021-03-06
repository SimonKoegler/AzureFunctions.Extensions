using System;
using System.Collections.Generic;
using System.Linq;

using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Abstractions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Enums;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Extensions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Tests.Fakes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Visitors;

using FluentAssertions;

using Microsoft.OpenApi.Any;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Serialization;

namespace Aliencube.AzureFunctions.Extensions.OpenApi.Core.Tests.Visitors
{
    [TestClass]
    public class Int32EnumTypeVisitorTests
    {
        private IVisitor _visitor;
        private NamingStrategy _strategy;

        [TestInitialize]
        public void Init()
        {
            this._visitor = new Int32EnumTypeVisitor();
            this._strategy = new CamelCaseNamingStrategy();
        }

        [DataTestMethod]
        [DataRow(typeof(FakeIntEnum), false)]
        public void Given_Type_When_IsNavigatable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsNavigatable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(FakeIntEnum), true)]
        [DataRow(typeof(int), false)]
        public void Given_Type_When_IsVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(FakeIntEnum), true)]
        [DataRow(typeof(int), false)]
        public void Given_Type_When_IsParameterVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsParameterVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(FakeIntEnum), true)]
        [DataRow(typeof(int), false)]
        public void Given_Type_When_IsPayloadVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsPayloadVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("integer", "int32", typeof(FakeIntEnum))]
        public void Given_Type_When_Visit_Invoked_Then_It_Should_Return_Result(string dataType, string dataFormat, Type enumType)
        {
            var name = "hello";
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, typeof(FakeIntEnum));
            var enums = enumType.ToOpenApiInt32Collection();

            this._visitor.Visit(acceptor, type, this._strategy);

            acceptor.Schemas.Should().ContainKey(name);
            acceptor.Schemas[name].Type.Should().Be(dataType);
            acceptor.Schemas[name].Format.Should().Be(dataFormat);

            for (var i = 0; i < acceptor.Schemas[name].Enum.Count; i++)
            {
                var @enum = acceptor.Schemas[name].Enum[i];
                @enum.Should().BeOfType<OpenApiInteger>();
                (@enum as OpenApiInteger).Value.Should().Be((enums[i] as OpenApiInteger).Value);
            }

            (acceptor.Schemas[name].Default as OpenApiInteger).Value.Should().Be((enums.First() as OpenApiInteger).Value);
        }

        [DataTestMethod]
        [DataRow(OpenApiVisibilityType.Advanced)]
        [DataRow(OpenApiVisibilityType.Important)]
        [DataRow(OpenApiVisibilityType.Internal)]
        public void Given_Attribute_When_Visit_Invoked_Then_It_Should_Return_Result(OpenApiVisibilityType visibility)
        {
            var name = "hello";
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, typeof(FakeIntEnum));
            var attribute = new OpenApiSchemaVisibilityAttribute(visibility);

            this._visitor.Visit(acceptor, type, this._strategy, attribute);

            acceptor.Schemas[name].Extensions.Should().ContainKey("x-ms-visibility");
            acceptor.Schemas[name].Extensions["x-ms-visibility"].Should().BeOfType<OpenApiString>();
            (acceptor.Schemas[name].Extensions["x-ms-visibility"] as OpenApiString).Value.Should().Be(visibility.ToDisplayName(this._strategy));
        }

        [DataTestMethod]
        [DataRow("integer", "int32", typeof(FakeIntEnum))]
        public void Given_Type_When_ParameterVisit_Invoked_Then_It_Should_Return_Result(string dataType, string dataFormat, Type enumType)
        {
            var enums = enumType.ToOpenApiInt32Collection();

            var result = this._visitor.ParameterVisit(typeof(FakeIntEnum), this._strategy);

            result.Type.Should().Be(dataType);
            result.Format.Should().Be(dataFormat);

            for (var i = 0; i < result.Enum.Count; i++)
            {
                var @enum = result.Enum[i];
                @enum.Should().BeOfType<OpenApiInteger>();
                (@enum as OpenApiInteger).Value.Should().Be((enums[i] as OpenApiInteger).Value);
            }

            (result.Default as OpenApiInteger).Value.Should().Be((enums.First() as OpenApiInteger).Value);
        }

        [DataTestMethod]
        [DataRow("integer", "int32", typeof(FakeIntEnum))]
        public void Given_Type_When_PayloadVisit_Invoked_Then_It_Should_Return_Result(string dataType, string dataFormat, Type enumType)
        {
            var enums = enumType.ToOpenApiInt32Collection();

            var result = this._visitor.PayloadVisit(typeof(FakeIntEnum), this._strategy);

            result.Type.Should().Be(dataType);
            result.Format.Should().Be(dataFormat);

            for (var i = 0; i < result.Enum.Count; i++)
            {
                var @enum = result.Enum[i];
                @enum.Should().BeOfType<OpenApiInteger>();
                (@enum as OpenApiInteger).Value.Should().Be((enums[i] as OpenApiInteger).Value);
            }

            (result.Default as OpenApiInteger).Value.Should().Be((enums.First() as OpenApiInteger).Value);
        }
    }
}
