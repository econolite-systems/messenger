// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Confluent.Kafka;
using Econolite.Ode.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Services.Messenger.Extensions;

/// <summary>
/// KafkaConsumerConfigExtensions
/// </summary>
public static class KafkaConsumerConfigExtensions
{
    /// <summary>
    /// AddKafkaConsumerConfig
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaConsumerConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ConsumerConfig>(options =>
        {
            options.GroupId = config.GetSection("Kafka:GroupId")?.Value ?? string.Empty;
            options.EnableAutoCommit = true;
            options.EnableAutoOffsetStore = false;
            options.AutoOffsetReset = AutoOffsetReset.Latest;
            options.SetConfiguration(config);
        });

        return services;
    }

    private static void SaveCert(string path, string cert)
    {
        File.WriteAllText(path, Base64Decode(cert));
    }

    private static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    private static void SetConfiguration(this ClientConfig result, IConfiguration configuration,
        bool legacySupport = false)
    {
        result.Acks = 0;

        if (!legacySupport)
        {
            result.BootstrapServers = configuration["Kafka:bootstrap:servers"];
            var securityProtocol = configuration["Kafka:security:protocol"];
            if (!string.IsNullOrEmpty(securityProtocol))
                result.SecurityProtocol = securityProtocol == "SASL_PLAIN" ? SecurityProtocol.SaslPlaintext :
                    securityProtocol == "SASL_SSL" ? SecurityProtocol.SaslSsl :
                    securityProtocol == "SSL" ? SecurityProtocol.Ssl :
                    SecurityProtocol.Plaintext;

            var saslMechanism = configuration["Kafka:sasl:mechanism"];
            if (!string.IsNullOrEmpty(saslMechanism))
                result.SaslMechanism = saslMechanism == "SCRAM-SHA-512" ? SaslMechanism.ScramSha512 :
                    saslMechanism == "SCRAM-SHA-256" ? SaslMechanism.ScramSha256 :
                    saslMechanism == "GSSAPI" ? SaslMechanism.Gssapi :
                    SaslMechanism.Plain;

            var saslUsername = configuration["Kafka:sasl:username"];
            if (!string.IsNullOrEmpty(saslUsername))
                result.SaslUsername = saslUsername;

            var saslPassword = configuration["Kafka:sasl:password"];
            if (!string.IsNullOrEmpty(saslPassword))
                result.SaslPassword = saslPassword;

            if (!string.IsNullOrWhiteSpace(configuration["Kafka:ssl:ca"]) || !string.IsNullOrWhiteSpace(configuration["Kafka:ssl:certificate"]))
            {
                var sslCaLocation = configuration["Kafka:SslCaLocation"];
                if (!string.IsNullOrEmpty(sslCaLocation))
                    result.SslCaLocation = sslCaLocation;

                var sslCertificateLocation = configuration["Kafka:SslCertLocation"];
                if (!string.IsNullOrEmpty(sslCertificateLocation))
                    result.SslCertificateLocation = sslCertificateLocation;

                var sslCa = configuration["Kafka:ssl:ca"];
                if (sslCa != null)
                {
                    var _caLocation = "./ca.crt";
                    SaveCert(_caLocation, sslCa);
                    result.SslCaLocation = _caLocation;
                }

                var client = configuration["Kafka:ssl:certificate"];
                if (client != null)
                {
                    var _clientLocation = "./client.crt";
                    SaveCert(_clientLocation, client);
                    result.SslCertificateLocation = _clientLocation;
                }
            }
        }
        else
        {
            result.BootstrapServers = configuration["Kafka:LegacyServers"];
        }
    }
}
