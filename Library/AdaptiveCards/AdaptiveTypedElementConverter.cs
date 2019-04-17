using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveCards
{
    /// <summary>
    ///     This handles using type field to instantiate strongly typed object on deserialization
    /// </summary>
    public class AdaptiveTypedElementConverter : JsonConverter, ILogWarnings
    {
        public List<AdaptiveWarning> Warnings { get; set; } = new List<AdaptiveWarning>();
        private static HashSet<string> Ids { get; set; } = new HashSet<string>();
        public static void BeginCard()
        {
            Ids.Clear();
        }

        public static void EndCard()
        {
            Ids.Clear();
        }

        /// <summary>
        /// Default types to support, register any new types to this list
        /// </summary>
        private static readonly Lazy<Dictionary<string, Type>> TypedElementTypes = new Lazy<Dictionary<string, Type>>(() =>
        {
            // TODO: Should this be a static? It makes it impossible to have diff renderers support different elements
            var types = new Dictionary<string, Type>
            {
                [AdaptiveCard.TypeName] = typeof(AdaptiveCard),
                [AdaptiveTextBlock.TypeName] = typeof(AdaptiveTextBlock),
                [AdaptiveRichTextBlock.TypeName] = typeof(AdaptiveRichTextBlock),
                [AdaptiveImage.TypeName] = typeof(AdaptiveImage),
                [AdaptiveContainer.TypeName] = typeof(AdaptiveContainer),
                [AdaptiveColumn.TypeName] = typeof(AdaptiveColumn),
                [AdaptiveColumnSet.TypeName] = typeof(AdaptiveColumnSet),
                [AdaptiveFactSet.TypeName] = typeof(AdaptiveFactSet),
                [AdaptiveImageSet.TypeName] = typeof(AdaptiveImageSet),
                [AdaptiveTextInput.TypeName] = typeof(AdaptiveTextInput),
                [AdaptiveDateInput.TypeName] = typeof(AdaptiveDateInput),
                [AdaptiveTimeInput.TypeName] = typeof(AdaptiveTimeInput),
                [AdaptiveChoiceSetInput.TypeName] = typeof(AdaptiveChoiceSetInput),
                [AdaptiveNumberInput.TypeName] = typeof(AdaptiveNumberInput),
                [AdaptiveToggleInput.TypeName] = typeof(AdaptiveToggleInput),
                [AdaptiveSubmitAction.TypeName] = typeof(AdaptiveSubmitAction),
                [AdaptiveOpenUrlAction.TypeName] = typeof(AdaptiveOpenUrlAction),
                [AdaptiveShowCardAction.TypeName] = typeof(AdaptiveShowCardAction),
                [AdaptiveMedia.TypeName] = typeof(AdaptiveMedia),
                [AdaptiveToggleVisibilityAction.TypeName] = typeof(AdaptiveToggleVisibilityAction)
            };
            return types;
        });

        public static void RegisterTypedElement<T>(string typeName = null)
            where T : AdaptiveTypedElement
        {
            if (typeName == null)
                typeName = ((AdaptiveTypedElement)Activator.CreateInstance(typeof(T))).Type;

            TypedElementTypes.Value[typeName] = typeof(T);
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(AdaptiveTypedElement).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var typeName = jObject["type"]?.Value<string>() ?? jObject["@type"]?.Value<string>();
            if (typeName == null)
            {
                // Get value of this objectType's "Type" JsonProperty(Required)
                var typeJsonPropertyRequiredValue = objectType.GetRuntimeProperty("Type")
                    .CustomAttributes.Where(a => a.AttributeType == typeof(JsonPropertyAttribute)).FirstOrDefault()?
                    .NamedArguments.Where(a => a.TypedValue.ArgumentType == typeof(Required)).FirstOrDefault()
                    .TypedValue.Value.ToString();

                // If this objectType does not require "Type" attribute, use the objectType's XML "TypeName" attribute
                if (typeJsonPropertyRequiredValue == "0")
                {
                    typeName = objectType
                        .GetRuntimeFields().Where(x => x.Name == "TypeName").FirstOrDefault()?
                        .GetValue("TypeName").ToString();
                }
                else
                {
                    throw new AdaptiveSerializationException("Required property 'type' not found on adaptive card element");
                }
            }

            if (TypedElementTypes.Value.TryGetValue(typeName, out var type))
            {
                if (jObject.Value<string>("id") == null)
                {
                    if (typeof(AdaptiveInput).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    {
                        throw new AdaptiveSerializationException($"Required property 'id' not found on '{typeName}'");
                    }
                }
                else
                {
                    string objectId = jObject.Value<string>("id");
                    if (Ids.Contains(objectId))
                    {
                        throw new AdaptiveSerializationException($"Duplicate 'id' found: '{objectId}'");
                    }
                    else
                    {
                        Ids.Add(objectId);
                    }
                }

                var result = (AdaptiveTypedElement)Activator.CreateInstance(type);
                try
                {
                    serializer.Populate(jObject.CreateReader(), result);
                }
                catch (JsonSerializationException)
                {
                    return result;
                }

                HandleAdditionalProperties(result);
                return result;
            }

            Warnings.Add(new AdaptiveWarning(-1, $"Unknown element '{typeName}'"));
            return null;
        }

        private void HandleAdditionalProperties(AdaptiveTypedElement te)
        {
            // https://stackoverflow.com/questions/34995406/nullvaluehandling-ignore-influences-deserialization-into-extensiondata-despite

            // The default behavior of JsonExtensionData is to include properties if the VALUE could not be set, including abstract properties or default values
            // We don't want to deserialize any properties that exist on the type into AdditionalProperties, so this function removes them

            // Create a list of known property names
            List<String> knownPropertyNames = new List<String>();
            var runtimeProperties = te.GetType().GetRuntimeProperties();
            foreach (var runtimeProperty in runtimeProperties)
            {
                // Check if the property has a JsonPropertyAttribute with the value set
                String jsonPropertyName = null;
                foreach (var attribute in runtimeProperty.CustomAttributes)
                {
                    if (attribute.AttributeType == typeof(Newtonsoft.Json.JsonPropertyAttribute) &&
                        attribute.ConstructorArguments.Count == 1)
                    {
                        jsonPropertyName = attribute.ConstructorArguments[0].Value as String;
                        break;
                    }
                }

                // Add the json property name if present, otherwise use the runtime property name
                knownPropertyNames.Add(jsonPropertyName != null ? jsonPropertyName : runtimeProperty.Name);
            }

            te.AdditionalProperties
                .Select(prop => knownPropertyNames
                    .SingleOrDefault(p => p.Equals(prop.Key, StringComparison.OrdinalIgnoreCase)))
                .Where(p => p != null)
                .ToList()
                .ForEach(p => te.AdditionalProperties.Remove(p));

            foreach (var prop in te.AdditionalProperties)
            {
                Warnings.Add(new AdaptiveWarning(-1, $"Unknown property '{prop.Key}' on '{te.Type}'"));
            }
        }

        public static T CreateElement<T>(string typeName = null)
            where T : AdaptiveTypedElement
        {
            if (typeName == null)
                typeName = ((T)Activator.CreateInstance(typeof(T))).Type;

            if (TypedElementTypes.Value.TryGetValue(typeName, out var type))
            {
                return (T)Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
