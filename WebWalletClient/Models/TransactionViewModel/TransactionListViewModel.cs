using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebWalletClient.Models.TransactionViewModel
{
    public class TransactionListViewModel : Transaction
    {
        public List<SelectListItem> BankAccounts { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}