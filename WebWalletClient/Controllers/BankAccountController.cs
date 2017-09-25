using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebWalletClient.Models;
using WebWalletClient.Models.BankAccountViewModel;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace WebWalletClient.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _client;

        private static async Task<List<BankAccount>> GetBankAccounts(HttpResponseMessage response, List<BankAccount> bankAccounts)
        {
            if (response.IsSuccessStatusCode)
            {
                bankAccounts = await response.Content.ReadAsAsync<List<BankAccount>>();
            }

            return bankAccounts;
        }

        private static async Task<BankAccount> GetBankAccount(HttpResponseMessage response, BankAccount bankAccount)
        {
            if (response.IsSuccessStatusCode)
            {
                bankAccount = await response.Content.ReadAsAsync<BankAccount>();
            }

            return bankAccount;
        }

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
            HttpResponseMessage response = await _client.GetAsync($"api/bankaccount");
            List<BankAccount> bankAccounts = null;
            bankAccounts = await GetBankAccounts(response, bankAccounts);
            if (bankAccounts == null)
            {
                return NotFound();
            }
            Guid ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) { ownUserId = new Guid(_userManager.GetUserId(User)); }
            bankAccounts = bankAccounts.Where(o => o.OwnerId == ownUserId).ToList();
            var bankAccountsViewModels = new List<BankAccountViewModel>();
            foreach (var bankAccount in bankAccounts)
            {
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
            }
            return View(bankAccountsViewModels);
        }

        // GET: BankAccount/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            HttpResponseMessage response = await _client.GetAsync($"api/bankaccount/" + id.ToString());
            BankAccount bankAccount = null;
            bankAccount = await GetBankAccount(response, bankAccount);
            if (bankAccount == null)
            {
                return NotFound();
            }

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
            Guid ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) { ownUserId = new Guid(_userManager.GetUserId(User)); }
            bankAccount.OwnerId = ownUserId;
            if (ModelState.IsValid)
            {
                bankAccount.Id = Guid.NewGuid();
                var bankAccountContent = JsonConvert.SerializeObject(bankAccount);
                var buffer = System.Text.Encoding.UTF8.GetBytes(bankAccountContent);
                var bankAccountByteContent = new ByteArrayContent(buffer);
                bankAccountByteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await _client.PostAsync($"api/bankaccount/", bankAccountByteContent);

                return RedirectToAction(nameof(Index));
            }
            return View(bankAccount);
        }

        // GET: BankAccount/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HttpResponseMessage response = await _client.GetAsync($"api/bankaccount/" + id.ToString());
            BankAccount bankAccount = null;
            bankAccount = await GetBankAccount(response, bankAccount);

            if (bankAccount == null)
            {
                return NotFound();
            }
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
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CreationTime,InterestRate,Comment,Balance")] BankAccountViewModel bankAccountViewModel)
        {
            if (id != bankAccountViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                HttpResponseMessage response = await _client.GetAsync($"api/bankaccount/" + id.ToString());
                BankAccount oldBankAccount = null;
                oldBankAccount = await GetBankAccount(response, oldBankAccount);
                if (oldBankAccount == null)
                {
                    return NotFound();
                }
                oldBankAccount.Comment = bankAccountViewModel.Comment;
                oldBankAccount.InterestRate = bankAccountViewModel.InterestRate;

                var bankAccountContent = JsonConvert.SerializeObject(oldBankAccount);
                var buffer = System.Text.Encoding.UTF8.GetBytes(bankAccountContent);
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
            {
                return NotFound();
            }

            HttpResponseMessage response = await _client.GetAsync($"api/bankaccount/" + id.ToString());
            BankAccount bankAccount = null;
            bankAccount = await GetBankAccount(response, bankAccount);
            if (bankAccount == null)
            {
                return NotFound();
            }
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            HttpResponseMessage response = await _client.DeleteAsync($"api/bankaccount/" + id.ToString());
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BankAccountExists(Guid id)
        {
            HttpResponseMessage response = await _client.GetAsync($"api/bankaccount");
            List<BankAccount> bankAccounts = null;
            bankAccounts = await GetBankAccounts(response, bankAccounts);
            return bankAccounts.Any(e => e.Id == id);
        }
    }
}
