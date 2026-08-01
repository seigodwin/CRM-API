using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Services.Services;
using CRMApi.Utility.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRMApi.Tests.AuthServiceTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IRateLimitService> _rateLimitServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AppDbContext _context;
        private readonly ApplicationUser _user;
        private readonly Mock<ILogger<AuthService>> _loggerMock;

        //SUT
        private readonly AuthService _sut;
        public AuthServiceTests() 
        { 
            _rateLimitServiceMock = new Mock<IRateLimitService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _emailServiceMock = new Mock<IEmailService>();
            _context = CreateDbContext();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _user = new ApplicationUser
            {
                FirstName = "Sei",
                LastName = "Godwin",
                Email = "test@gmail.com"
            };

            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object,
                null,null,null,null 
            );

            _sut = new AuthService(_context, _userManagerMock.Object, _roleManagerMock.Object,
                _rateLimitServiceMock.Object, _tokenServiceMock.Object, _emailServiceMock.Object
                , _loggerMock.Object); 
        }

        [Fact]
        public async Task AssignRolesAsync_WithValidData_ReturnsSuccess()
        {
            //Arrange
            var dto = new AssignRoleRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Roles = ["Admin"]
            };

             
            IList<string> userRoles = ["Admin", "Supervisor", "SuperAdmin"];

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(userRoles);
            _userManagerMock.Setup(um => um.AddToRolesAsync(_user, It.IsAny<IEnumerable<string>>()))
                                                               .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            
            //Act
             var results = await  _sut.AssignRolesAsync(dto);
            
            //Assert
            results.Success.Should().BeTrue();
            //results.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task AssignRolesAsync_WithInValidUser_ReturnsUserNotFound()
        {
            //Arrange
            var dto = new AssignRoleRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Roles = [""]
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            //Act
            var results = await _sut.AssignRolesAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNullOrEmpty();
            results.Message.Should().Contain("User not found");
        }

        [Fact]
        public async Task AssignRolesAsync_WithValidUserButInValidRoles_ReturnsFailedInValidRoles()
        {
            //Arrange
            var dto = new AssignRoleRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Roles = ["",""]
            };


            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
          

            //Act
            var results = await _sut.AssignRolesAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNullOrEmpty();
            results.Message.Should().Contain("No valid roles");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidData_ReturnsSuccess()
        {
            //Arrange

            var dto = new ChangePasswordRequestDto
            {
                Email = _user.Email ?? string.Empty,
                CurrentPassword = "CurrentPassword",
                NewPassword = "NewPassword",
                ConFirmNewPassword = "NewPassword",
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _userManagerMock.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>()
                , It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);


            //Act
            var results = await _sut.ChangePasswordAsync(dto);

            //Assert
            results.Success.Should().BeTrue();
            results.Data.Should().BeNullOrEmpty();
            results.Message.Should().Contain("successfully");
        }


        [Fact]
        public async Task ChangePasswordAsync_WithInValidCurrentPassword_ReturnsFalse()
        {
            //Arrange

            var dto = new ChangePasswordRequestDto
            {
                Email = _user.Email ?? string.Empty,
                CurrentPassword = "CurrentPassword",
                NewPassword = "NewPassword",
                ConFirmNewPassword = "NewPassword",
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _userManagerMock.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>()
                , It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(
                                       new IdentityError { Description = "Incorrect user password" }
                    ));

            //Act
            var results = await _sut.ChangePasswordAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNullOrEmpty();
        }


        [Fact]
        public async Task ConfirmEmailAsync_WithInValidDto_ReturnsFalse()
        {
            //Arrange

            var dto = new ConfirmEmailRequestDto
            {
                Email = "",
                Token = ""
            };

            //Act
            var results = await _sut.ConfirmEmailAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNullOrEmpty();
        }


        [Fact]
        public async Task ConfirmEmailAsync_WithValidDto_ReturnsSuccess()
        {
            //Arrange

            var dto = new ConfirmEmailRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Token = "ConfirmEmailToken"
            };

            _userManagerMock.Setup( um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _userManagerMock.Setup(um => um.ConfirmEmailAsync(It.IsAny<ApplicationUser>(),It.IsAny<string>()))
                                            .ReturnsAsync(IdentityResult.Success);

            //Act
            var results = await _sut.ConfirmEmailAsync(dto); 

            //Assert
            results.Success.Should().BeTrue();
            results.Data.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithValidData_ReturnsSuccess()
        {
            //Arange
            var dto = new LoginRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Password = "testpassword"
            };

            var authenticatedUserDto = new AuthenticatedUsertDto
            {
                Id = "testUserId",
                UserName = "seigodwin",
                RefreshToken = "testrefreshtoken",
                AccessToken = "testaccecctoken",
                AccessTokenExpiration = DateTime.Now
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                                        .ReturnsAsync(true);
            _tokenServiceMock.Setup(ts => ts.GenerateTokenPairAsync(It.IsAny<ApplicationUser>()))
                                                            .ReturnsAsync(authenticatedUserDto);

            //Act
            var results = await _sut.LoginAsync(dto);

            //Assert
            results.Success.Should().BeTrue();
            results.Data.Should().NotBeNull();
            results.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
            results.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
            
        }


        [Fact]
        public async Task LoginAsync_WithIncorrectEmail_ReturnsFalse()
        {
            //Arange
            var dto = new LoginRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Password = "testpassword"
            };


            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            
            //Act
            var results = await _sut.LoginAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNull();
        }


        [Fact]
        public async Task LoginAsync_WithIncorrectPassword_ReturnsFalse()
        {
            //Arange
            var dto = new LoginRequestDto
            {
                Email = _user.Email ?? string.Empty,
                Password = "testpassword"
            };


            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                                                                                            .ReturnsAsync(false);

            //Act
            var results = await _sut.LoginAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNull();
            
        }


        private AppDbContext CreateDbContext()

        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

    }
}
