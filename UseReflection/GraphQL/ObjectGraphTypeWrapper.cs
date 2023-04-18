using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UseReflection.GraphQL
{
    internal class ObjectGraphTypeHelper<T> : ObjectGraphType<T>
    {
        public ObjectGraphTypeHelper()
        {
            BuildObjectGraphType(typeof(T));

            var attr = typeof(T).GetCustomAttribute<GraphQLMetadataAttribute>();
            Name = attr?.Name ?? typeof(T).Name;
        }

        private void BuildObjectGraphType(Type type)
        {
            foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var name = GetFieldName(fieldInfo);
                AddField(fieldInfo.FieldType, name, fieldInfo.GetValue);
            }

            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var name = GetFieldName(propertyInfo);
                var isSkip = Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute));
                if (!isSkip)
                {
                    AddField(propertyInfo.PropertyType, name, propertyInfo.GetValue);
                }
            }

            foreach (var methodInfo in type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => !m.IsSpecialName))
            {
                AddField(methodInfo);
            }
        }

        private void AddField(Type type, string name, Func<object, object> valueGetter)
        {
            var graphType = GetGraphType(type);
            if (graphType == null) return;
            Field(graphType, name, resolve: context => valueGetter(context.Source));
        }


        private void AddField(MethodInfo methodInfo)
        {
            var methodAttr = methodInfo.GetCustomAttribute<GraphQLMetadataAttribute>();

            if (methodAttr == null)
            {
                return;
            }

            var name = methodAttr.Name ?? methodInfo.Name;

            var parameters = methodInfo.GetParameters();
            var contextType = typeof(ResolveFieldContext<>).MakeGenericType(methodInfo.DeclaringType); // ? 
            var arguments = BuildArguments(parameters, contextType);

            var isAsync = false;
            Type returnType;

            if (methodInfo.ReturnType.IsSubclassOf(typeof(Task)))
            {
                isAsync = true;
                returnType = methodInfo.ReturnType.GetGenericArguments()[0];
            }
            else
            {
                returnType = methodInfo.ReturnType;
            }

            var graphType = GetGraphType(returnType);
            if (graphType == null)
                return;

            if (isAsync)
            {
                FieldAsync(graphType, name,
                    arguments: new QueryArguments(arguments),
                    resolve: async context =>
                    {
                        var task = (Task)methodInfo.Invoke(context.Source,
                            BuildParameters(parameters, contextType, context));
                        await task.ConfigureAwait(false);
                        var resultProperty = task.GetType().GetProperty("Result");
                        return resultProperty?.GetValue(task);
                    });
            }
            else
            {
                Field(graphType, name,
                    arguments: new QueryArguments(arguments),
                    resolve: context =>
                        methodInfo.Invoke(context.Source, BuildParameters(parameters, contextType, context)));
            }

        }

        private static IEnumerable<QueryArgument> BuildArguments(ParameterInfo[] parameters, Type contextType)
        {
            var arguments = new List<QueryArgument>();
            foreach (var parameterInfo in parameters)
            {
                var paraType = parameterInfo.ParameterType;
                if (paraType == contextType)
                {
                    continue;
                }
                var graphType = GetGraphType(paraType);
                if (graphType == null) continue;
                var attr = paraType.GetCustomAttribute<GraphQLMetadataAttribute>();
                var argumentType = attr?.IsTypeOf ?? graphType;
                arguments.Add(new QueryArgument(argumentType) { Name = parameterInfo.Name });
            }

            return arguments;
        }

        private static object[] BuildParameters(ParameterInfo[] parameters, Type contextType,
    ResolveFieldContext<T> context)
        {
            var paras = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == contextType)
                {
                    paras[i] = context;
                }
                else
                {
                    paras[i] = context.GetArgument(parameters[i].ParameterType, parameters[i].Name);

                }
            }

            return paras;
        }

        private static string GetFieldName(MemberInfo methodInfo)
        {
            var methodAttr = methodInfo.GetCustomAttribute<JsonPropertyAttribute>();
            return methodAttr?.PropertyName ?? methodInfo.Name;
        }

        private static Type GetGraphType(Type type)
        {
            var graphType = GetPrimitiveGraphType(type);
            if (graphType != null)
            {
                return graphType;
            }

            if (type.IsClass && !IsEnumerableType(type))
            {
                return GetObjectGraphType(type);
            }

            if (IsEnumerableType(type) || type.IsArray)
            {
                var args = type.GetGenericArguments();
                if (args.Length == 0)
                    return null;
                var itemType = args[0];
                return GetListGraphType(itemType);
            }

            throw new NotSupportedException($"The type {type.FullName} is not supported in GraphQL API.");
        }

        private static bool IsEnumerableType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static Type GetListGraphType(Type itemType)
        {
            var itemGraphType = GetPrimitiveGraphType(itemType);
            if (itemGraphType == null)
            {
                itemGraphType = GetObjectGraphType(itemType);
            }

            return typeof(ListGraphType<>).MakeGenericType(itemGraphType);
        }

        private static Type GetObjectGraphType(Type type)
        {
            return typeof(ObjectGraphTypeHelper<>).MakeGenericType(type);
        }

        private static Type GetPrimitiveGraphType(Type type)
        {
            var primitiveType = type;
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                primitiveType = underlyingType;
            }
            return GraphTypeTypeRegistry.Get(primitiveType);
        }
    }
}
