using System.Threading.Tasks;

namespace AATS.Desktop.ViewModels.Shared
{
    public interface IDraftFormViewModel
    {
        bool HasUnsavedChanges { get; }
        Task SaveAsDraftAsync();
        Task DiscardChangesAsync();
    }
}
