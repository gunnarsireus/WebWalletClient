using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
		private readonly UserManager<ApplicationUser> _userManager;

		public TransactionController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		// GET: Transaction
		public async Task<IActionResult> Index(string id)
		{
			var bankAccounts = await Utils.Get<List<BankAccount>>("api/bankaccount");
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
			selectList.AddRange(bankAccounts.Select(bankAcount => new SelectListItem
			{
				Text = bankAcount.Comment,
				Value = bankAcount.Id.ToString(),
				Selected = bankAcount.Id.ToString() == id
			}));
			var transactions = new List<Transaction>();

			if (id != null)
			{
				transactions = await Utils.Get<List<Transaction>>("api/transaction/");
				var bankAccountId = new Guid(id);
				transactions = transactions.Where(o => o.BankAccountId == bankAccountId).ToList();
			}

			var transactionListViewModel = new TransactionListViewModel
			{
				BankAccountSelectList = selectList,
				Transactions = transactions
			};

			ViewBag.Saldo = bankAccounts.SingleOrDefault(o => o.Id.ToString() == id)?.Balance;
			ViewBag.BankAccountId = id;
			return View(transactionListViewModel);
		}

		// GET: Transaction/Details/5
		public async Task<IActionResult> Details(Guid? id)
		{
			if (id == null)
				return NotFound();

			var transaction = await Utils.Get<Transaction>("api/transaction/" + id);
			if (transaction == null)
				return NotFound();
			return View(transaction);
		}

		// GET: Transaction/Create
		public IActionResult Create(string id)
		{
			if (TempData?["CustomError"] != null)
				ModelState.AddModelError(string.Empty, TempData["CustomError"].ToString());
			if (id == null)
				return View();
			var bankAccountId = new Guid(id);
			var transactionViewModel = new TransactionViewModel
			{
				BankAccountId = bankAccountId,
				IsDeposition = true
			};
			return View(transactionViewModel);
		}

		// POST: Transaction/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("BankAccountId,Comment,IsDeposition,Amount")] TransactionViewModel transactionViewModel)
		{
			if (!ModelState.IsValid) return View(transactionViewModel);
			transactionViewModel.Id = Guid.NewGuid();
			transactionViewModel.BankAccountId = transactionViewModel.BankAccountId;
			transactionViewModel.Comment = transactionViewModel.Comment;
			transactionViewModel.Deposit = transactionViewModel.IsDeposition ? transactionViewModel.Amount : "";
			transactionViewModel.Withdraw = !transactionViewModel.IsDeposition ? transactionViewModel.Amount : "";

			var bankAccount = await Utils.Get<BankAccount>("api/bankaccount/" + transactionViewModel.BankAccountId);

			var oldBalance = decimal.Parse(bankAccount.Balance, new CultureInfo("se-SV"));
			if (transactionViewModel.IsDeposition)
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
					return RedirectToAction("Create", new { id = transactionViewModel.BankAccountId });
				}
			}
			await Utils.Put<BankAccount>("api/bankaccount/" + bankAccount.Id, bankAccount);
			var transaction = Utils.Post<Transaction>("api/transaction/", transactionViewModel).Result;


			return RedirectToAction("Index", new { id = transactionViewModel.BankAccountId });
		}

		// GET: Transaction/Edit/5
		public async Task<IActionResult> Edit(Guid? id)
		{
			if (id == null)
				return NotFound();

			var transaction = await Utils.Get<Transaction>("api/transaction/" + id);

			var transactionViewModel = new TransactionViewModel
			{
				Id = transaction.Id,
				BankAccountId = transaction.BankAccountId,
				Comment = transaction.Comment,
				CreationTime = transaction.CreationTime,
				Amount = transaction.Deposit == "" ? transaction.Withdraw : transaction.Deposit,
				IsDeposition = transaction.Deposit != ""
			};
			return View(transactionViewModel);
		}

		// POST: Transaction/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Guid id,
			[Bind("Id,BankAccountId,CreationTime,Comment,Deposit,Withdraw")] TransactionViewModel transactionViewModel)
		{
			if (!ModelState.IsValid) return View(transactionViewModel);
			var oldTransaction = await Utils.Get<Transaction>("api/transaction/" + id);
			oldTransaction.Comment = transactionViewModel.Comment;
			var transaction = Utils.Put<Transaction>("api/transaction/" + oldTransaction.BankAccountId.ToString(), oldTransaction).Result;

			return RedirectToAction("Index", new { id = transactionViewModel.BankAccountId });
		}

		private async Task<bool> TransactionExists(Guid id)
		{
			var transactions = await Utils.Get<List<Transaction>>("api/transaction/" + id);
			return transactions.Any(e => e.Id == id);
		}
	}
}