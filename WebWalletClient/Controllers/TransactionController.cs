using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Transaction
        public async Task<IActionResult> Index(string id)
        {
            var bankAccounts = await Utils.GetItems<BankAccount>(_client, "api/bankaccount");
            if (bankAccounts == null)
                return NotFound();
            var ownUserId = Guid.NewGuid(); //To pass unit test where User=null
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
                var bankAccount = await Utils.GetItem<BankAccount>(_client, "api/bankaccount/" + id);

				transactions = await Utils.GetItems<Transaction>(_client, "api/transaction/");
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

            Transaction transaction = null;
            transaction = await Utils.GetItem<Transaction>(_client, "api/transaction/" + id);
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

                var bankAccount = await Utils.GetItem<BankAccount>(_client,"api/bankaccount/" + transactionViewModel.BankAccountId);
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

	            bankAccount = Utils.Put<BankAccount>(_client, "api/bankaccount/0",bankAccount).Result;

	            var transaction = Utils.Post<Transaction>(_client, "api/transaction/", transactionViewModel).Result;


				return RedirectToAction("Index", new {id = transactionViewModel.BankAccountId});
            }
            return View(transactionViewModel);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var transaction = await Utils.GetItem<Transaction>(_client, "api/transaction/" + id);
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
                var oldTransaction = await Utils.GetItem<Transaction>(_client, "api/transaction/" + id);
                oldTransaction.Comment = transactionViewModel.Comment;
	            var transaction = Utils.Put<Transaction>(_client, "api/transaction/0", oldTransaction).Result;

				return RedirectToAction("Index", new {id = transactionViewModel.BankAccountId});
            }
            return View(transactionViewModel);
        }

        private async Task<bool> TransactionExists(Guid id)
        {
            var transactions = await Utils.GetItems<Transaction>(_client, "api/transaction/" + id);
            return transactions.Any(e => e.Id == id);
        }
    }
}