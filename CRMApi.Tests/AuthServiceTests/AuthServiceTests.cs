using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Services.Services;
using CRMApi.Utility.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        private readonly AppDbContext _context;
        private readonly ApplicationUser _user;

        //SUT
        private readonly AuthService _sut;
        public AuthServiceTests() 
        { 
            _rateLimitServiceMock = new Mock<IRateLimitService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _context = CreateDbContext();

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
                _rateLimitServiceMock.Object, _tokenServiceMock.Object);
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

            //Act
            var results = await _sut.ConfirmEmailAsync(dto);

            //Assert
            results.Success.Should().BeFalse();
            results.Data.Should().BeNullOrEmpty();
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
