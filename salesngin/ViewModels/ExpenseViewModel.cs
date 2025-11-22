using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using salesngin.Models;
using System;
using System.Collections.Generic;

namespace salesngin.ViewModels
{
    public class ExpenseViewModel : BaseViewModel
    {
        public int? Id { get; set; }

        public int CategoryId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTime DateIncurred { get; set; }

        public IFormFile Attachment { get; set; }

        public string ExistingAttachmentPath { get; set; }

        public IEnumerable<Expense> Expenses { get; set; }

        public IEnumerable<ExpenseCategory> Categories { get; set; }

        public SelectList CategorySelectList { get; set; }
    }
}
