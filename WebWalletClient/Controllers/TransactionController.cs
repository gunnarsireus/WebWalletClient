using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using WebWalletClient.Models;
using WebWalletClient.Models.TransactionViewModel;

namespace WebWalletClient.Controllers
{
    public class TransactionController : Controller
    {
        private readonly HttpClient _client;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:54411//")
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static async Task<List<BankAccount>> GetBankAccounts(HttpResponseMessage response,
            List<BankAccount> bankAccounts)
        {
            if (response.IsSuccessStatusCode)
                bankAccounts = await response.Content.ReadAsAsync<List<BankAccount>>();

            return bankAccounts;
        }

        private static async Task<BankAccount> GetBankAccount(HttpResponseMessage response, BankAccount bankAccount)
        {
            if (response.IsSuccessStatusCode)
                bankAccount = await response.Content.ReadAsAsync<BankAccount>();

            return bankAccount;
        }

        private static async Task<List<Transaction>> GetTransactions(HttpResponseMessage response,
            List<Transaction> transactions)
        {
            if (response.IsSuccessStatusCode)
                transactions = await response.Content.ReadAsAsync<List<Transaction>>();

            return transactions;
        }

        private static async Task<Transaction> GetTransaction(HttpResponseMessage response, Transaction transaction)
        {
            if (response.IsSuccessStatusCode)
                transaction = await response.Content.ReadAsAsync<Transaction>();

            return transaction;
        }

        // GET: Transaction
        public async Task<IActionResult> Index(string id)
        {
            var response = await _client.GetAsync($"api/bankaccount");
            List<BankAccount> bankAccounts = null;
            bankAccounts = await GetBankAccounts(response, bankAccounts);
            if (bankAccounts == null)
                return NotFound();
            var ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));

            bankAccounts = bankAccounts.Where(o => o.OwnerId == ownUserId).ToList();
            if (bankAccounts.Any() && id == null)
                id = bankAccounts[0].Id.ToString();
            var selectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Välj kontonamn",
                    Value = ""
                }
            };
            foreach (var bankAcount in bankAccounts)
                selectList.Add(new SelectListItem
                {
                    Text = bankAcount.Comment,
                    Value = bankAcount.Id.ToString(),
                    Selected = bankAcount.Id.ToString() == id
                });
            var transactions = new List<Transaction>();

            if (id != null)
            {
                response = await _client.GetAsync($"api/bankaccount/" + id);
                BankAccount bankAccount = null;
                bankAccount = await GetBankAccount(response, bankAccount);

                response = await _client.GetAsync($"api/transaction/");
                transactions = await GetTransactions(response, transactions);
                transactions = transactions.Where(o => o.BankAccountId == bankAccount.Id).ToList();
            }


            var transactionListViewModel = new TransactionListViewModel
            {
                BankAccounts = selectList,
                Transactions = transactions
            };

            ViewBag.BankAccountId = id;
            ViewBag.Saldo = bankAccounts.SingleOrDefault(o => o.Id.ToString() == id)?.Balance;
            return View(transactionListViewModel);
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var response = await _client.GetAsync($"api/transaction/" + id);
            Transaction transaction = null;
            transaction = await GetTransaction(response, transaction);
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create(string id)
        {
            if (TempData != null && TempData["CustomError"] != null)
                ModelState.AddModelError(string.Empty, TempData["CustomError"].ToString());
            if (id == null)
                return View();
            var iid = new Guid(id);
            var transactionViewModel = new TransactionViewModel
            {
                BankAccountId = iid,
                Deposition = true
            };
            return View(transactionViewModel);
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BankAccountId,Comment,Deposition,Amount")] TransactionViewModel transactionViewModel)
        {
            if (ModelState.IsValid)
            {
                transactionViewModel.Id = Guid.NewGuid();
                transactionViewModel.BankAccountId = transactionViewModel.BankAccountId;
                transactionViewModel.Comment = transactionViewModel.Comment;
                transactionViewModel.Deposit = transactionViewModel.Deposition ? transactionViewModel.Amount : "";
                transactionViewModel.Withdraw = !transactionViewModel.Deposition ? transactionViewModel.Amount : "";

                var response = await _client.GetAsync($"api/bankaccount/" + transactionViewModel.BankAccountId);
                BankAccount bankAccount = null;
                bankAccount = await GetBankAccount(response, bankAccount);
                if (bankAccount == null)
                    return NotFound();
                var oldBalance = decimal.Parse(bankAccount.Balance, new CultureInfo("se-SV"));
                if (transactionViewModel.Deposition)
                {
                    var deposit = decimal.Parse(transactionViewModel.Amount, new CultureInfo("se-SV"));
                    bankAccount.Balance = (oldBalance + deposit).ToString(new CultureInfo("se-SV"));
                }
                else
                {
                    var withdraw = decimal.Parse(transactionViewModel.Amount, new CultureInfo("se-SV"));
                    bankAccount.Balance = (oldBalance - withdraw).ToString(new CultureInfo("se-SV"));
                    if (decimal.Parse(bankAccount.Balance, new CultureInfo("se-SV")) < 0)
                    {
                        TempData["CustomError"] = "Saldot får ej bli negativt!";
                        return RedirectToAction("Create", new {id = transactionViewModel.BankAccountId});
                    }
                }

                var bankAccountContent = JsonConvert.SerializeObject(bankAccount);
                var buffer = Encoding.UTF8.GetBytes(bankAccountContent);
                var bankAccountByteContent = new ByteArrayContent(buffer);
                bankAccountByteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _client.PutAsync($"api/bankaccount/0", bankAccountByteContent);

                var transactionContent = JsonConvert.SerializeObject(transactionViewModel);
                buffer = Encoding.UTF8.GetBytes(transactionContent);
                var transactionByteContent = new ByteArrayContent(buffer);
                transactionByteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _client.PostAsync($"api/transaction/", transactionByteContent);

                return RedirectToAction("Index", new {id = transactionViewModel.BankAccountId});
            }
            return View(transactionViewModel);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();
            var response = await _client.GetAsync($"api/transaction/" + id);
            Transaction transaction = null;
            transaction = await GetTransaction(response, transaction);
            if (transaction == null)
                return NotFound();
            var transactionViewmodel = new TransactionViewModel
            {
                Id = transaction.Id,
                BankAccountId = transaction.BankAccountId,
                Comment = transaction.Comment,
                CreationTime = transaction.CreationTime,
                Amount = transaction.Deposit == "" ? transaction.Withdraw : transaction.Deposit,
                Deposition = transaction.Deposit != ""
            };
            return View(transactionViewmodel);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("Id,BankAccountId,CreationTime,Comment,Deposit,Withdraw")] TransactionViewModel transactionViewModel)
        {
            if (id != transactionViewModel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var response = await _client.GetAsync($"api/transaction/" + id);
                Transaction oldTransaction = null;
                oldTransaction = await GetTransaction(response, oldTransaction);
                oldTransaction.Comment = transactionViewModel.Comment;
                var transactionContent = JsonConvert.SerializeObject(oldTransaction);
                var buffer = Encoding.UTF8.GetBytes(transactionContent);
                var transactionByteContent = new ByteArrayContent(buffer);
                transactionByteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await _client.PutAsync($"api/transaction/0", transactionByteContent);

                return RedirectToAction("Index", new {id = transactionViewModel.BankAccountId});
            }
            return View(transactionViewModel);
        }

        private async Task<bool> TransactionExists(Guid id)
        {
            var response = await _client.GetAsync($"api/transaction");
            List<Transaction> transactions = null;
            transactions = await GetTransactions(response, transactions);
            return transactions.Any(e => e.Id == id);
        }
    }
}