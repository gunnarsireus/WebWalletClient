using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebWalletClient.Models
{
    public class BankAccount
    {
        public BankAccount()
        {
            InterestRate = "";
            Comment = "";
            Balance = "0";
            Transactions = new Collection<Transaction>();
            CreationTime = DateTime.Now.ToString(new CultureInfo("se-SE"));
        }
        [Display(Name = "Kontonummer")]
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        [Display(Name = "Skapat datum")]
        public string CreationTime { get; set; }

        [Required]
        [Display(Name = "Ränta")]
        [RegularExpression(@"^(\+|\-|)[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0}n anges med upp till två decimaler. Ex: 4,75")]
        public string InterestRate { get; set; }
        [Display(Name = "Kontonamn")]
        public string Comment { get; set; }
        [Required]
        [Display(Name = "Saldo")]
        [RegularExpression(@"^(\+|\-|)[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0}t anges med upp till två decimaler. Ex: 4,75")]
        public string Balance { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
