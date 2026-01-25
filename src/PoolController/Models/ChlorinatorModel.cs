
namespace PoolController.Models;

public partial class ChlorinatorModel : ObservableObject
{
    [ObservableProperty]
    private int _percentage = 25;

    [ObservableProperty]
    private int _saltLevel = 3600;

    [ObservableProperty]
    private int _status = 0;
}
