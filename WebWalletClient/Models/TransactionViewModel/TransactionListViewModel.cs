using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebWalletClient.Models.TransactionViewModel
{
    public class TransactionListViewModel : Transaction
    {
        public List<SelectListItem> BankAccountSelectList { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}