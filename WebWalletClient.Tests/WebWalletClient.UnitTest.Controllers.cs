using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebWalletClient.Controllers;
using WebWalletClient.Models;
using WebWalletClient.Models.BankAccountViewModel;
using WebWalletClient.Models.TransactionViewModel;
using Xunit;

namespace WebWalletClient.Tests
{
    public class HomeControllerTest
    {
        [Fact]
        public void About()
        {
            // Arrange
            var homeController = new HomeController();

            // Act
            var result = homeController.About() as ViewResult;

            // Assert
            Assert.Equal("Your application description page.", result.ViewData["Message"]);
        }

        [Fact]
        public void Contact()
        {
            // Arrange
            var homeController = new HomeController();

            // Act
            var result = homeController.Contact() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Index()
        {
            // Arrange
            var homeController = new HomeController();

            // Act
            var result = homeController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }
    }


    public class BankAccountControllerTest
    {
        [Fact]
        public void CreateBankAccount()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Arrange
            var bankAccountController = new BankAccountController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));

            // Act
            var result = bankAccountController.Create() as ViewResult;

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateEditDeleteAsync()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Arrange
            var bankAccountController = new BankAccountController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));
            var bankAccount = new BankAccount();
            var result = await bankAccountController.Create(bankAccount);
            // Assert
            Assert.IsType<RedirectToActionResult>(result);


            // Arrange
            var bankAccountId = bankAccount.Id;

            // Act
            result = await bankAccountController.Details(bankAccountId);

            // Assert
            Assert.IsType<ViewResult>(result);

            // Act
            result = await bankAccountController.Edit(bankAccountId);

            // Assert
            Assert.IsType<ViewResult>(result);

            // Act
            var bankAccountViewModel = new BankAccountViewModel
            {
                Id = bankAccountId
            };

            result = await bankAccountController.Edit(bankAccountId, bankAccountViewModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);

            // Act
            result = await bankAccountController.DeleteConfirmed(bankAccountId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task IndexAsync()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Arrange
            var bankAccountController = new BankAccountController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));

            // Act
            var result = await bankAccountController.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }


    public class TransactionControllerTest
    {
        [Fact]
        public async Task CreateEditDeleteAsync()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Arrange
            var bankAccountController = new BankAccountController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));
            var transactionController = new TransactionController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));

            var bankAccount = new BankAccount();
            var result = await bankAccountController.Create(bankAccount);
            // Assert
            Assert.IsType<RedirectToActionResult>(result);


            // Arrange
            var bankAccountId = bankAccount.Id;

            // Act
            var transactionViewModel = new TransactionViewModel
            {
                BankAccountId = bankAccountId,
                Amount = "100",
                IsDeposition = true
            };
            result = await transactionController.Create(transactionViewModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);

            var transactionId = transactionViewModel.Id;
            // Act
            result = await transactionController.Details(transactionId);

            // Assert
            Assert.IsType<ViewResult>(result);

            // Act

            result = await transactionController.Edit(transactionId);

            // Assert
            Assert.IsType<ViewResult>(result);

            // Act
            result = await transactionController.Edit(transactionId, transactionViewModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);

            // Act
            result = await bankAccountController.DeleteConfirmed(bankAccountId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void CreateTransaction()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Arrange
            var transactionController = new TransactionController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));

            // Act
            var bankAccountId = Guid.NewGuid();
            var result = transactionController.Create(bankAccountId.ToString()) as ViewResult;

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task IndexAsync()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Arrange
            var transactionController = new TransactionController(new UserManager<ApplicationUser>(userStoreMock.Object,
                null, null, null, null, null, null, null, null));

            // Act
            var result = await transactionController.Index(null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }

    //    public class WebWalletClientTest
    //    {
    //        private readonly WebWalletClient.Controllers.HomeController _homeController;

    //        public WebWalletClientTest()
    //        {
    //            _homeController = new HomeController();
    //        }

    //        #region Sample_TestCode
    //        [Theory]
    //        [InlineData(-1)]
    //        [InlineData(0)]
    //        [InlineData(1)]
    //        public void ReturnFalseGivenValuesLessThan2(int value)
    //        {
    //            var result = _primeService.IsPrime(value);

    //            Assert.False(result, $"{value} should not be prime");
    //        }
    //        #endregion

    //        [Theory]
    //        [InlineData(2)]
    //        [InlineData(3)]
    //        [InlineData(5)]
    //        [InlineData(7)]
    //        public void ReturnTrueGivenPrimesLessThan10(int value)
    //        {
    //            var result = _homeController.Index();

    //            Assert.True(result, $"{value} should be prime");
    //        }

    //        [Theory]
    //        [InlineData(4)]
    //        [InlineData(6)]
    //        [InlineData(8)]
    //        [InlineData(9)]
    //        public void ReturnFalseGivenNonPrimesLessThan10(int value)
    //        {
    //            var result = _primeService.IsPrime(value);

    //            Assert.False(result, $"{value} should not be prime");
    //        }
    //    }
}