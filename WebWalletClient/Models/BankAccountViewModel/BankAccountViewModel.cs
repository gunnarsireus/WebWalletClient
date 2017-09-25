using System.ComponentModel.DataAnnotations;

namespace WebWalletClient.Models.BankAccountViewModel
{
    public class BankAccountViewModel : BankAccount
    {
        [Display(Name = "Användare")]
        public string Username { get; set; }
    }
}