using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebWalletClient.Models;
using WebWalletClient.Models.BankAccountViewModel;

namespace WebWalletClient.Controllers
{
	public class BankAccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		public BankAccountController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
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
			var bankAccounts = await Utils.Get<List<BankAccount>>("api/bankaccount");
			if (bankAccounts == null)
				return NotFound();
			var ownUserId = Guid.NewGuid(); //To pass unit test where User=null
			if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));
			bankAccounts = bankAccounts.Where(o => o.OwnerId == ownUserId).ToList();
			var bankAccountsViewModels = bankAccounts.Select(bankAccount => new BankAccountViewModel
				{
					Id = bankAccount.Id,
					Username = GetUserNameFromId(bankAccount.OwnerId),
					Balance = bankAccount.Balance,
					Comment = bankAccount.Comment,
					CreationTime = bankAccount.CreationTime,
					InterestRate = bankAccount.InterestRate,
					Transactions = bankAccount.Transactions
				})
				.ToList();
			return View(bankAccountsViewModels);
		}

		// GET: BankAccount/Details/5
		public async Task<IActionResult> Details(Guid? id)
		{
			if (id == null)
				return NotFound();

			var bankAccount = await Utils.Get<BankAccount>("api/bankaccount/" + id);
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
			var ownUserId = Guid.NewGuid(); //To pass unit test where User=null
			if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));
			bankAccount.OwnerId = ownUserId;
			if (!ModelState.IsValid) return View(bankAccount);
			bankAccount.Id = Guid.NewGuid();
			await Utils.Post<BankAccount>("api/bankaccount/", bankAccount);

			return RedirectToAction(nameof(Index));
		}

		// GET: BankAccount/Edit/5
		public async Task<IActionResult> Edit(Guid? id)
		{
			if (id == null)
				return NotFound();

			var bankAccount = await Utils.Get<BankAccount>("api/bankaccount/" + id);

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

			if (!ModelState.IsValid) return View(bankAccountViewModel);
			var oldBankAccount = await Utils.Get<BankAccount>("api/bankaccount/" + id);
			if (oldBankAccount == null)
				return NotFound();
			oldBankAccount.Comment = bankAccountViewModel.Comment;
			oldBankAccount.InterestRate = bankAccountViewModel.InterestRate;
			await Utils.Put<BankAccount>($"api/bankaccount/0", oldBankAccount);

			return RedirectToAction(nameof(Index));
		}

		// GET: BankAccount/Delete/5
		public async Task<IActionResult> Delete(Guid? id)
		{
			if (id == null)
				return NotFound();

			var bankAccount = await Utils.Get<BankAccount>("api/bankaccount/" + id);
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
			await Utils.Delete<BankAccount>("api/bankaccount/" + id);
			return RedirectToAction(nameof(Index));
		}

		private async Task<bool> BankAccountExists(Guid id)
		{
			var bankAccounts = await Utils.Get<List<BankAccount>>("api/bankaccount");
			return bankAccounts.Any(e => e.Id == id);
		}
	}
}