using CRM_API.Options;
using CRMApi.Domain.Models;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;

namespace CRMApi.Tests.TokenGenerationTests
{
    public class TokenServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDatabase> _redisDbMock;

        private readonly TokenService _sut;

        public TokenServiceTests()
        {
            // UserManager mock
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            // Redis mock
            _redisDbMock = new Mock<IDatabase>();
            _redisMock = new Mock<IConnectionMultiplexer>();

            _redisMock
                .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_redisDbMock.Object);

            // JWT options
            var jwtOptions = Options.Create(new JwtOptions
            {
                Secret = "Your_Super_Secret_Key_For_Testing",
                Issuer = "test",
                Audience = "test"
            });

            // SUT
            _sut = new TokenService(_userManagerMock.Object, _redisMock.Object, jwtOptions);

        }

        [Fact]  
        public async Task GenerateTokenPairAsync_WithValidUser_ReturnsTwoTokens()
        {           
            //Arange
            var user = new ApplicationUser()
            {
                FirstName = "Sei",
                LastName = "Godwin",
                Email = "test@gmail.com"
            };

           _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                            .ReturnsAsync(user);

            //Act
            var results = await _sut.GenerateTokenPairAsync(user);

            //Assert
            results.Should().NotBeNull();
            results.RefreshToken.Should().NotBeNullOrEmpty();
            results.AccessToken.Should().NotBeNullOrEmpty();
            results.AccessToken.Should().Contain(".");
        }
    }
}