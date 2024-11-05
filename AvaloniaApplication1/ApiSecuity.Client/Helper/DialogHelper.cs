using System.Threading.Tasks;
using Avalonia.Controls;
using Ursa.Controls;

namespace ApiSecuity.Client.Helper;

public static class DialogHelper
{
    public static Task<TResult?> ShowAsync<TView, TViewModel, TResult>(TViewModel vm,
        string? hostId = null,
        OverlayDialogOptions? options = null)
        where TView : Control, new()
    {
        return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            OverlayDialog.ShowCustomModal<TView, TViewModel, TResult>(vm, hostId, options));
    }
}