using System;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.AuditAndAccounts;

public partial class TaxAccountViewModel : AuditTableViewModelBase
{
    public override string PageTitle => "Tax Accountings";
    public override string SearchPlaceholder => "Search Client, Assignment, ID...";
    public override string StatusHeader => "Status";
    public override string GuideLinkText => "Learn more about Tax Accountings";

    public TaxAccountViewModel()
    {
        ProcessFilters = new ObservableCollection<string> { "All Process", "BOOKKEEP", "TAX AMOUNT", "FINALIZE", "TAX PAID", "SUBMIT", "-" };
    }
}
