using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DevLab.AzureAdapter;
using DevLab.AzureAdapter.DTOs;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;
using Environment = DevLab.AzureAdapter.DTOs.Environment;

namespace DevLabs.AzureAdapter.Tests
{
    public class AzureReleaseRepositoryTests
    {
        private MockHttpMessageHandler _mockHttp;
        private HttpClient _httpClient;
        private AzureReleaseRepository _sut;
        private Uri _baseUri;

        public AzureReleaseRepositoryTests()
        {

            _mockHttp = new MockHttpMessageHandler();
            _httpClient = _mockHttp.ToHttpClient();
            _baseUri = new Uri("https://localhost:1234");
            _httpClient.BaseAddress = _baseUri;
            _sut = new AzureReleaseRepository(_httpClient);
        }

        [Fact]
        public void DeployToProd_UnkownReleaseDefinition_NothingDeployed()
        {
            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId=1&$expand=environments&api-version=6.0")
                .Respond("application/json", "{}");

            var actual = _sut.DeployReleasesToProdAsync(new List<ReleaseDefinition> {new() {Id = 1}});

            actual.First().Deployed.Should().BeFalse();
        }

        [Fact]
        public void DeployToProd_NoPotentialProdReleasesFound_NothingDeployed()
        {
            var releaseList = new ReleaseInformationList
            {
                Count = 1,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 1,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            }
                        }
                    }
                }
            };
            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId=1&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList));

            var actual = _sut.DeployReleasesToProdAsync(new List<ReleaseDefinition> {new() {Id = 1}});

            actual.First().Deployed.Should().BeFalse();
        }

        [Fact]
        public void DeployToProd_DeployToStageFailed_FailedResult()
        {
            var releaseList = new ReleaseInformationList
            {
                Count = 1,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 1,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED,
                            },
                        }
                    }
                }
            };

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId=1&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList));

            var actual = _sut.DeployReleasesToProdAsync(new List<ReleaseDefinition> {new() {Id = 1}});

            actual.First().Deployed.Should().BeFalse();
        }

        [Fact]
        public void DeployToProd_GettingApprovalsFailed_FailedResult()
        {
            var releaseList = new ReleaseInformationList
            {
                Count = 1,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 1,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED,
                            },
                        }
                    }
                }
            };

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId=1&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList));

            _mockHttp
                .When(HttpMethod.Patch, $"{_baseUri.AbsoluteUri}release/releases/1/environments/1?api-version=6.0-preview.6")
                .Respond("application/json", "{}");

            var actual = _sut.DeployReleasesToProdAsync(new List<ReleaseDefinition> {new() {Id = 1}});

            actual.First().Deployed.Should().BeFalse();
        }

        [Fact]
        public void DeployToProd_SetApprovalFails_FailedResult()
        {
            var releaseList = new ReleaseInformationList
            {
                Count = 1,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 1,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED,
                            },
                        }
                    }
                }
            };

            var approvelList = new ApprovalList
            {
                Count = 1,
                Value = new[]
                {
                    new Approval{Id = 123}
                }
            };

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId=1&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList));

            _mockHttp
                .When(HttpMethod.Patch, $"{_baseUri.AbsoluteUri}release/releases/1/environments/1?api-version=6.0-preview.6")
                .Respond("application/json", "{}");

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/approvals?releaseIdsFilter=1&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(approvelList));

            var actual = _sut.DeployReleasesToProdAsync(new List<ReleaseDefinition> {new() {Id = 1}});

            actual.First().Deployed.Should().BeFalse();
        }

        [Fact]
        public void DeployToProd_HappyPath()
        {
            var releaseList = new ReleaseInformationList
            {
                Count = 1,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 1,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED,
                            },
                        }
                    }
                }
            };

            var approvelList = new ApprovalList
            {
                Count = 1,
                Value = new[]
                {
                    new Approval{Id = 123}
                }
            };

            var succededReleaseInfo = new ReleaseInformation
            {
                Id = 1,
                Environments = new List<Environment>
                {
                    new()
                    {
                        Id = 0,
                        Name = "INT",
                        Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                    },
                    new()
                    {
                        Id = 1,
                        Name = "PROD",
                        Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED,
                    },
                }
            };

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId=1&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList));

            _mockHttp
                .When(HttpMethod.Patch, $"{_baseUri.AbsoluteUri}release/releases/1/environments/1?api-version=6.0-preview.6")
                .Respond("application/json", "{}");

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/approvals?releaseIdsFilter=1&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(approvelList));

            _mockHttp
                .When(HttpMethod.Patch, $"{_baseUri.AbsoluteUri}release/approvals/123?api-version=6.0")
                .Respond("application/json", "{}");

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases/1?api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(succededReleaseInfo));

            var actual = _sut.DeployReleasesToProdAsync(new List<ReleaseDefinition> {new() {Id = 1}});

            actual.First().Deployed.Should().BeTrue();
            actual.First().ReleaseId.Should().Be(1);
        }

        [Fact]
        public async Task GetReleaseDefinitionsAsync_HappyPath()
        {
            var releaseList1 = new ReleaseInformationList
            {
                Count = 2,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 1,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED,
                            },
                        }
                    }
                }
            };

            var releaseList2 = new ReleaseInformationList
            {
                Count = 2,
                Value = new List<ReleaseInformation> {
                    new()
                    {
                        Id = 3,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_REJECTED,
                            },
                        }
                    },
                    new()
                    {
                        Id = 4,
                        Environments = new List<Environment> {
                            new()
                            {
                                Id = 0,
                                Name = "INT",
                                Status = AzureConstants.ENVIRONMENT_STATUS_SUCCEEDED
                            },
                            new()
                            {
                                Id = 1,
                                Name = "PROD",
                                Status = AzureConstants.ENVIRONMENT_STATUS_NOT_STARTED,
                            },
                        }
                    }
                }
            };

            var expected = new []{new ReleaseDefinition{Id = 1, Name="R1"}, new ReleaseDefinition{Id = 1123, Name="R2"}};
            var releaseDefinitionList = new ReleaseDefinitionList{Count = expected.Length, Value = expected};

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/definitions?api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseDefinitionList));

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId={1}&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList1));

            _mockHttp
                .When(HttpMethod.Get, $"{_baseUri.AbsoluteUri}release/releases?definitionId={1123}&$expand=environments&api-version=6.0")
                .Respond("application/json", JsonSerializer.Serialize(releaseList2));

            var actual = await _sut.GetReleaseDefinitionsAsync();

            actual.Single().Id.Should().Be(1123);
        }
    }
}
