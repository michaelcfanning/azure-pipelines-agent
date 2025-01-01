// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests
{
    public sealed class HostContextL0
    {
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void CreateServiceReturnsNewInstance()
        {
            // Arrange.
            using (var _hc = Setup())
            {
                // Act.
                var reference1 = _hc.CreateService<IAgentServer>();
                var reference2 = _hc.CreateService<IAgentServer>();

                // Assert.
                Assert.NotNull(reference1);
                Assert.IsType<AgentServer>(reference1);
                Assert.NotNull(reference2);
                Assert.IsType<AgentServer>(reference2);
                Assert.False(object.ReferenceEquals(reference1, reference2));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void GetServiceReturnsSingleton()
        {
            // Arrange.
            using (var _hc = Setup())
            {

                // Act.
                var reference1 = _hc.GetService<IAgentServer>();
                var reference2 = _hc.GetService<IAgentServer>();

                // Assert.
                Assert.NotNull(reference1);
                Assert.IsType<AgentServer>(reference1);
                Assert.NotNull(reference2);
                Assert.True(object.ReferenceEquals(reference1, reference2));
            }
        }

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        // some URLs with secrets to mask
        [InlineData("https://user:pass@example.com/path", "https://***@example.com/path")]
        [InlineData("http://user:pass@example.com/path", "http://***@example.com/path")]
        [InlineData("ftp://user:pass@example.com/path", "ftp://***@example.com/path")]
        [InlineData("https://user:pass@example.com/weird:thing@path", "https://***@example.com/weird:thing@path")]
        [InlineData("https://user:pass@example.com:8080/path", "https://***@example.com:8080/path")]
        [InlineData("https://user:pass@example.com:8080/path\nhttps://user2:pass2@example.com:8080/path", "https://***@example.com:8080/path\nhttps://***@example.com:8080/path")]
        [InlineData("https://user@example.com:8080/path\nhttps://user2:pass2@example.com:8080/path", "https://user@example.com:8080/path\nhttps://***@example.com:8080/path")]
        [InlineData("https://user:pass@example.com:8080/path\nhttps://user2@example.com:8080/path", "https://***@example.com:8080/path\nhttps://user2@example.com:8080/path")]
        // some URLs without secrets to mask
        [InlineData("https://example.com/path", "https://example.com/path")]
        [InlineData("http://example.com/path", "http://example.com/path")]
        [InlineData("ftp://example.com/path", "ftp://example.com/path")]
        [InlineData("ssh://example.com/path", "ssh://example.com/path")]
        [InlineData("https://example.com/@path", "https://example.com/@path")]
        [InlineData("https://example.com:8080/path", "https://example.com:8080/path")]
        [InlineData("https://example.com/weird:thing@path", "https://example.com/weird:thing@path")]
        public void UrlCredentialsAreMaskedOssSecretMasker(string input, string expected)
        {
            // Arrange.

            try
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", "true");
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);

                using (var _hc = Setup(testName: nameof(UrlCredentialsAreMaskedOssSecretMasker)))
                {
                    // Act.
                    var result = _hc.SecretMasker.MaskSecrets(input);

                    // Assert.
                    Assert.Equal(expected, result);
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", "true");
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);
            }
        }

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        // some URLs with secrets to mask
        [InlineData("https://user:pass@example.com/path", "***example.com/path")]
        [InlineData("http://user:pass@example.com/path", "***example.com/path")]
        [InlineData("ftp://user:pass@example.com/path", "***example.com/path")]
        [InlineData("https://user:pass@example.com/weird:thing@path", "***example.com/weird:thing@path")]
        [InlineData("https://user:pass@example.com:8080/path", "***example.com:8080/path")]
        [InlineData("https://user:pass@example.com:8080/path\nhttps://user2:pass2@example.com:8080/path", "***example.com:8080/path\n***example.com:8080/path")]
        [InlineData("https://user@example.com:8080/path\nhttps://user2:pass2@example.com:8080/path", "https://user@example.com:8080/path\n***example.com:8080/path")]
        [InlineData("https://user:pass@example.com:8080/path\nhttps://user2@example.com:8080/path", "***example.com:8080/path\nhttps://user2@example.com:8080/path")]
        // some URLs without secrets to mask
        [InlineData("https://example.com/path", "https://example.com/path")]
        [InlineData("http://example.com/path", "http://example.com/path")]
        [InlineData("ftp://example.com/path", "ftp://example.com/path")]
        [InlineData("ssh://example.com/path", "ssh://example.com/path")]
        [InlineData("https://example.com/@path", "https://example.com/@path")]
        [InlineData("https://example.com:8080/path", "https://example.com:8080/path")]
        [InlineData("https://example.com/weird:thing@path", "https://example.com/weird:thing@path")]
        public void UrlCredentialsAreMaskedBuiltInSecretMasker(string input, string expected)
        {
            // Arrange.

            try
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", null);
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", "true");

                using (var _hc = Setup(testName: nameof(UrlCredentialsAreMaskedBuiltInSecretMasker)))
                {
                    // Act.
                    var result = _hc.SecretMasker.MaskSecrets(input);

                    // Assert.
                    Assert.Equal(expected, result);
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", null);
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);
            }
        }

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        // some URLs with secrets to mask
        [InlineData("https://user:pass@example.com/path", "***example.com/path")]
        [InlineData("http://user:pass@example.com/path", "***example.com/path")]
        [InlineData("ftp://user:pass@example.com/path", "***example.com/path")]
        [InlineData("https://user:pass@example.com/weird:thing@path", "***example.com/weird:thing@path")]
        [InlineData("https://user:pass@example.com:8080/path", "***example.com:8080/path")]
        [InlineData("https://user:pass@example.com:8080/path\nhttps://user2:pass2@example.com:8080/path", "***example.com:8080/path\n***example.com:8080/path")]
        [InlineData("https://user@example.com:8080/path\nhttps://user2:pass2@example.com:8080/path", "https://user@example.com:8080/path\n***example.com:8080/path")]
        [InlineData("https://user:pass@example.com:8080/path\nhttps://user2@example.com:8080/path", "***example.com:8080/path\nhttps://user2@example.com:8080/path")]
        // some URLs without secrets to mask
        [InlineData("https://example.com/path", "https://example.com/path")]
        [InlineData("http://example.com/path", "http://example.com/path")]
        [InlineData("ftp://example.com/path", "ftp://example.com/path")]
        [InlineData("ssh://example.com/path", "ssh://example.com/path")]
        [InlineData("https://example.com/@path", "https://example.com/@path")]
        [InlineData("https://example.com:8080/path", "https://example.com:8080/path")]
        [InlineData("https://example.com/weird:thing@path", "https://example.com/weird:thing@path")]
        public void UrlCredentialsAreMaskedSecretMaskerVSO(string input, string expected)
        {
            // Arrange.

            Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", null);
            Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);

            using (var _hc = Setup(testName: nameof(UrlCredentialsAreMaskedSecretMaskerVSO)))
            {
                // Act.
                var result = _hc.SecretMasker.MaskSecrets(input);

                // Assert.
                Assert.Equal(expected, result);
            }
        }

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        // Some secrets that the scanner SHOULD suppress.
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddeadAPIMdo9bzQ==", "SEC101/181:AQYnVRHEp9bsvtiS75Hw")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaACDbOpqrYA==", "SEC101/160:cgAuNarRt3XE67OyFKtT")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+ABacEmI0Q==", "SEC101/163:hV8JHmDwlzKVQLDQ4aVz")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+AMCIBB+lg==", "SEC101/170:vGkdeeXzDdYpZG/P/N+U")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+AStaCQW6A==", "SEC101/152:iFwwHb6GCjF+WxbWkhIp")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeaddeaddeaddeadAzFuFakD8w==", "SEC101/158:DI3pIolg4mUyaYvnQJ9s")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeaddeaddeadxxAzSeCyiycA", "SEC101/166:ws3fLn9rYjxet8tPxeei")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ACRC5W7f3", "SEC101/176:gfxbCiSbZlGd1NSqkoQg")]
        [InlineData("oy2mdeaddeaddeadeadqdeaddeadxxxezodeaddeadwxuq", "SEC101/031:G47Z8IeLmqos+/TXkWoH")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaAIoTOumzco=", "SEC101/178:oCE/hp1BfeSLXPJgMqTz")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ASbHpHeAI=", "SEC101/171:ujJlDjBUPI6u49AyMCXk")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+AEhG2s/8w=", "SEC101/172:7aH00tlYEZcu0yhnxhm6")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ARmD7h+qo=", "SEC101/173:73UIu7xCGv6ofelm1yqH")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaAzCaJM04l8=", "SEC101/154:Elbi036ZI8k03jlXzG52")]
        [InlineData("xxx8Q~dead.dead.DEAD-DEAD-dead~deadxxxxx", "SEC101/156:vcocI2kI5E2ycoG55kza")]
        [InlineData("npm_deaddeaddeaddeaddeaddeaddeaddeaddead", "SEC101/050:bUOMn/+Dx0jUK71D+nHu")]
        [InlineData("xxx7Q~dead.dead.DEAD-DEAD-dead~deadxx", "SEC101/156:WNRIG2TMMQjdUEGSNRIQ")]
        // Some secrets that the scanner should NOT suppress.
        [InlineData("SSdtIGEgY29tcGxldGVseSBpbm5vY3VvdXMgc3RyaW5nLg==", "SSdtIGEgY29tcGxldGVseSBpbm5vY3VvdXMgc3RyaW5nLg==")]
        [InlineData("The password is knock knock knock", "The password is knock knock knock")]
        public void OtherSecretsAreMaskedOssSecretsMasker(string input, string expected)
        {
            // Arrange.
            try
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", "true");
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);

                using (var _hc = Setup(testName: nameof(OtherSecretsAreMaskedOssSecretsMasker)))
                {
                    // Act.
                    var result = _hc.SecretMasker.MaskSecrets(input);

                    // Assert.
                    Assert.Equal(expected, result);
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", "true");
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);
            }
        }
        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        // Some secrets that the scanner SHOULD suppress.
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddeadAPIMdo9bzQ==", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaACDbOpqrYA==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+ABacEmI0Q==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+AMCIBB+lg==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+AStaCQW6A==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeaddeaddeaddeadAzFuFakD8w==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeaddeaddeadxxAzSeCyiycA", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ACRC5W7f3", "***")]
        [InlineData("oy2mdeaddeaddeadeadqdeaddeadxxxezodeaddeadwxuq", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaAIoTOumzco=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ASbHpHeAI=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+AEhG2s/8w=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ARmD7h+qo=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaAzCaJM04l8=", "***")]
        [InlineData("xxx8Q~dead.dead.DEAD-DEAD-dead~deadxxxxx", "***")]
        [InlineData("npm_deaddeaddeaddeaddeaddeaddeaddeaddead", "***")]
        [InlineData("xxx7Q~dead.dead.DEAD-DEAD-dead~deadxx", "***")]
        // Some secrets that the scanner should NOT suppress.
        [InlineData("SSdtIGEgY29tcGxldGVseSBpbm5vY3VvdXMgc3RyaW5nLg==", "SSdtIGEgY29tcGxldGVseSBpbm5vY3VvdXMgc3RyaW5nLg==")]
        [InlineData("The password is knock knock knock", "The password is knock knock knock")]
        [InlineData("https://user:pass@example.com/path", "***example.com/path")]
        public void OtherSecretsAreMaskedBuiltInSecretsMasker(string input, string expected)
        {
            // Arrange.
            try
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", null);
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", "true");

                using (var _hc = Setup())
                {
                    // Act.
                    var result = _hc.SecretMasker.MaskSecrets(input);

                    // Assert.
                    Assert.Equal(expected, result);
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", "true");
                Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);
            }
        }

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        // Some secrets that the scanner SHOULD suppress.
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddeadAPIMdo9bzQ==", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaACDbOpqrYA==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+ABacEmI0Q==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+AMCIBB+lg==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeadde/dead+deaddeaddeaddeaddeaddeaddeaddeaddead+AStaCQW6A==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeaddeaddeaddeadAzFuFakD8w==", "***")]
        [InlineData("deaddeaddeaddeaddeaddeaddeaddeaddeaddeadxxAzSeCyiycA", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ACRC5W7f3", "***")]
        [InlineData("oy2mdeaddeaddeadeadqdeaddeadxxxezodeaddeadwxuq", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaAIoTOumzco=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ASbHpHeAI=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+AEhG2s/8w=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa+ARmD7h+qo=", "***")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaAzCaJM04l8=", "***")]
        [InlineData("xxx8Q~dead.dead.DEAD-DEAD-dead~deadxxxxx", "***")]
        [InlineData("npm_deaddeaddeaddeaddeaddeaddeaddeaddead", "***")]
        [InlineData("xxx7Q~dead.dead.DEAD-DEAD-dead~deadxx", "***")]
        // Some secrets that the scanner should NOT suppress.
        [InlineData("SSdtIGEgY29tcGxldGVseSBpbm5vY3VvdXMgc3RyaW5nLg==", "SSdtIGEgY29tcGxldGVseSBpbm5vY3VvdXMgc3RyaW5nLg==")]
        [InlineData("The password is knock knock knock", "The password is knock knock knock")]
        public void OtherSecretsAreMaskedSecretsMaskerVSO(string input, string expected)
        {
            // Arrange.

            Environment.SetEnvironmentVariable("AZP_ENABLE_OSS_SECRET_MASKER", null);
            Environment.SetEnvironmentVariable("AZP_ENABLE_NEW_SECRET_MASKER", null);

            using (var _hc = Setup())
            {
                // Act.
                var result = _hc.SecretMasker.MaskSecrets(input);

                // Assert.
                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public void LogFileChangedAccordingToEnvVariable()
        {
            try
            {
                var newPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "logs", Constants.Path.DiagDirectory);
                Environment.SetEnvironmentVariable("AGENT_DIAGLOGPATH", newPath);

                using (var _hc = new HostContext(HostType.Agent))
                {
                    // Act.
                    var diagFolder = _hc.GetDiagDirectory();

                    // Assert
                    Assert.Equal(newPath, diagFolder);
                    Directory.Exists(diagFolder);
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("AGENT_DIAGLOGPATH", null);
            }
        }

        public HostContext Setup([CallerMemberName] string testName = "")
        {
            var hc = new HostContext(
                hostType: HostType.Agent,
                logFile: Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"trace_{nameof(HostContextL0)}_{testName}.log"));
            return hc;
        }
    }
}
