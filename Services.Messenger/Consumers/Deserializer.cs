// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace Econolite.Ode.Services.Messenger.Consumers;

/// <summary>
/// Deserializer
/// </summary>
/// <typeparam name="T"></typeparam>
public class Deserializer<T> : IDeserializer<T>
{
    /// <summary>
    /// Deserialize
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isNull"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        try
        {
            var str = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        catch
        {
            return default;
        }
    }
}
