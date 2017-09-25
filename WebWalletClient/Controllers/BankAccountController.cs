using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebWalletClient.Models;
using WebWalletClient.Models.BankAccountViewModel;

namespace WebWalletClient.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly HttpClient _client;
        private readonly UserManager<ApplicationUser> _userManager;

        public BankAccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:54411//")
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        // GET: BankAccount
        private string GetUserNameFromId(Guid id)
        {
            try
            {
                var users = _userManager.Users.ToList();
                return users.FirstOrDefault(o => o.Id == id.ToString())?.UserName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync($"api/bankaccount");
            var bankAccounts = await Utils.GetItems<BankAccount>(response);
            if (bankAccounts == null)
                return NotFound();
            var ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));
            bankAccounts = bankAccounts.Where(o => o.OwnerId == ownUserId).ToList();
            var bankAccountsViewModels = new List<BankAccountViewModel>();
            foreach (var bankAccount in bankAccounts)
                bankAccountsViewModels.Add(new BankAccountViewModel
                {
                    Id = bankAccount.Id,
                    Username = GetUserNameFromId(bankAccount.OwnerId),
                    Balance = bankAccount.Balance,
                    Comment = bankAccount.Comment,
                    CreationTime = bankAccount.CreationTime,
                    InterestRate = bankAccount.InterestRate,
                    Transactions = bankAccount.Transactions
                });
            return View(bankAccountsViewModels);
        }

        // GET: BankAccount/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();
            var response = await _client.GetAsync($"api/bankaccount/" + id);
            var bankAccount = await Utils.GetItem<BankAccount>(response);
            if (bankAccount == null)
                return NotFound();

            var bankAccountsViewModel = new BankAccountViewModel
            {
                Id = bankAccount.Id,
                Username = GetUserNameFromId(bankAccount.OwnerId),
                Balance = bankAccount.Balance,
                Comment = bankAccount.Comment,
                CreationTime = bankAccount.CreationTime,
                InterestRate = bankAccount.InterestRate,
                Transactions = bankAccount.Transactions
            };


            return View(bankAccountsViewModel);
        }

        // GET: BankAccount/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BankAccount/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InterestRate,Comment,Balance")] BankAccount bankAccount)
        {
            var ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));
            bankAccount.OwnerId = ownUserId;
            if (ModelState.IsValid)
            {
                bankAccount.Id = Guid.NewGuid();
                var bankAccountContent = JsonConvert.SerializeObject(bankAccount);
                var buffer = Encoding.UTF8.GetBytes(bankAccountContent);
                var bankAccountByteContent = new ByteArrayContent(buffer);
                bankAccountByteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await _client.PostAsync($"api/bankaccount/", bankAccountByteContent);

                return RedirectToAction(nameof(Index));
            }
            return View(bankAccount);
        }

        // GET: BankAccount/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var response = await _client.GetAsync($"api/bankaccount/" + id);
            var bankAccount = await Utils.GetItem<BankAccount>(response);

            if (bankAccount == null)
                return NotFound();
            var bankAccountsViewModel = new BankAccountViewModel
            {
                Id = bankAccount.Id,
                Username = GetUserNameFromId(bankAccount.OwnerId),
                Balance = bankAccount.Balance,
                Comment = bankAccount.Comment,
                CreationTime = bankAccount.CreationTime,
                InterestRate = bankAccount.InterestRate,
                Transactions = bankAccount.Transactions
            };


            return View(bankAccountsViewModel);
        }

        // POST: BankAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("Id,CreationTime,InterestRate,Comment,Balance")] BankAccountViewModel bankAccountViewModel)
        {
            if (id != bankAccountViewModel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var response = await _client.GetAsync($"api/bankaccount/" + id);
                var oldBankAccount = await Utils.GetItem<BankAccount>(response);
                if (oldBankAccount == null)
                    return NotFound();
                oldBankAccount.Comment = bankAccountViewModel.Comment;
                oldBankAccount.InterestRate = bankAccountViewModel.InterestRate;

                var bankAccountContent = JsonConvert.SerializeObject(oldBankAccount);
                var buffer = Encoding.UTF8.GetBytes(bankAccountContent);
                var bankAccountByteContent = new ByteArrayContent(buffer);
                bankAccountByteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _client.PutAsync($"api/bankaccount/0", bankAccountByteContent);

                return RedirectToAction(nameof(Index));
            }
            return View(bankAccountViewModel);
        }

        // GET: BankAccount/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var response = await _client.GetAsync($"api/bankaccount/" + id);
            var bankAccount = await Utils.GetItem<BankAccount>(response);
            if (bankAccount == null)
                return NotFound();
            var bankAccountsViewModel = new BankAccountViewModel
            {
                Id = bankAccount.Id,
                Username = GetUserNameFromId(bankAccount.OwnerId),
                Balance = bankAccount.Balance,
                Comment = bankAccount.Comment,
                CreationTime = bankAccount.CreationTime,
                InterestRate = bankAccount.InterestRate,
                Transactions = bankAccount.Transactions
            };

            return View(bankAccountsViewModel);
        }

        // POST: BankAccount/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var response = await _client.DeleteAsync($"api/bankaccount/" + id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BankAccountExists(Guid id)
        {
            var response = await _client.GetAsync($"api/bankaccount");
            var bankAccounts = await Utils.GetItems<BankAccount>(response);
            return bankAccounts.Any(e => e.Id == id);
        }
    }
}