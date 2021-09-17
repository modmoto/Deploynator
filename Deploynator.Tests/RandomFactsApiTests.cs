using System;
using System.Threading.Tasks;
using Xunit;

namespace Deploynator.Tests
{
    public class RandomFactsApiTests
    {
        [Fact]
        public async Task GetRandomFactAsync_HappyPath()
        {
            var sut = new RandomFactsApiAdapter();

            var result = await sut.GetRandomFactAsync();

            Assert.NotEmpty(result);
        }
    }
}
