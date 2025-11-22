using Microsoft.AspNetCore.Mvc.Rendering;
using salesngin.Models;
using System.Collections.Generic;

namespace salesngin.ViewModels
{
    public class ExpenseCategoryViewModel : BaseViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public List<ExpenseCategory> Categories { get; set; }
        public SelectList StatusList { get; set; }
    }
}
