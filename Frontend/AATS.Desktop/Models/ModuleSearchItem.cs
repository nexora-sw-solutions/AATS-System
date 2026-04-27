using System.Windows.Input;

namespace AATS.Desktop.Models;

public class ModuleSearchItem
{
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ICommand? Command { get; set; }
}
