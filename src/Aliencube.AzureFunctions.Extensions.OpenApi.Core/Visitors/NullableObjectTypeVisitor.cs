using System;
using System.Collections.Generic;
using System.Linq;

using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Abstractions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Extensions;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Serialization;

namespace Aliencube.AzureFunctions.Extensions.OpenApi.Core.Visitors
{
    /// <summary>
    /// This represents the type visitor for <see cref="object"/>.
    /// </summary>
    public class NullableObjectTypeVisitor : TypeVisitor
    {
        /// <inheritdoc />
        public override bool IsVisitable(Type type)
        {
            var isVisitable = this.IsVisitable(type, TypeCode.Object) && type.IsOpenApiNullable();

            return isVisitable;
        }

        /// <inheritdoc />
        public override void Visit(IAcceptor acceptor, KeyValuePair<string, Type> type, NamingStrategy namingStrategy, params Attribute[] attributes)
        {
            var instance = acceptor as OpenApiSchemaAcceptor;
            if (instance.IsNullOrDefault())
            {
                return;
            }

            // Gets the schema for the underlying type.
            type.Value.IsOpenApiNullable(out var underlyingType);

            var types = new Dictionary<string, Type>()
            {
                { type.Key, underlyingType }
            };
            var schemas = new Dictionary<string, OpenApiSchema>();

            var subAcceptor = new OpenApiSchemaAcceptor()
            {
                Types = types,
                Schemas = schemas,
            };

            var collection = VisitorCollection.CreateInstance();
            subAcceptor.Accept(collection, namingStrategy);

            // Adds the schema for the underlying type.
            var name = subAcceptor.Schemas.First().Key;
            var schema = subAcceptor.Schemas.First().Value;
            schema.Nullable = true;

            // Adds the visibility property.
            if (attributes.Any())
            {
                var visibilityAttribute = attributes.OfType<OpenApiSchemaVisibilityAttribute>().SingleOrDefault();
                if (!visibilityAttribute.IsNullOrDefault())
                {
                    var extension = new OpenApiString(visibilityAttribute.Visibility.ToDisplayName());

                    schema.Extensions.Add("x-ms-visibility", extension);
                }
            }

            instance.Schemas.Add(name, schema);
        }

        /// <inheritdoc />
        public override bool IsParameterVisitable(Type type)
        {
            var isVisitable = this.IsVisitable(type);

            return isVisitable;
        }

        /// <inheritdoc />
        public override OpenApiSchema ParameterVisit(Type type, NamingStrategy namingStrategy)
        {
            type.IsOpenApiNullable(out var underlyingType);
            var collection = VisitorCollection.CreateInstance();
            var schema = collection.ParameterVisit(underlyingType, namingStrategy);

            schema.Nullable = true;

            return schema;
        }

        /// <inheritdoc />
        public override bool IsPayloadVisitable(Type type)
        {
            var isVisitable = this.IsVisitable(type);

            return isVisitable;
        }

        /// <inheritdoc />
        public override OpenApiSchema PayloadVisit(Type type, NamingStrategy namingStrategy)
        {
            type.IsOpenApiNullable(out var underlyingType);
            var collection = VisitorCollection.CreateInstance();
            var schema = collection.PayloadVisit(underlyingType, namingStrategy);

            schema.Nullable = true;

            return schema;
        }
    }
}
