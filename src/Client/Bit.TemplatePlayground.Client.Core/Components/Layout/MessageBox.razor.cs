﻿namespace Bit.TemplatePlayground.Client.Core.Components.Layout;

public partial class MessageBox : IDisposable
{
    private bool isOpen;
    private string? title;
    private string? body;

    private TaskCompletionSource<object?>? tcs;

    private async Task OnCloseClick()
    {
        isOpen = false;
        tcs?.SetResult(null);
        tcs = null;
    }

    private async Task OnOkClick()
    {
        isOpen = false;
        tcs?.SetResult(null);
        tcs = null;
    }

    Action? dispose;
    bool disposed = false;

    protected override Task OnInitAsync()
    {
        dispose = PubSubService.Subscribe(PubSubMessages.SHOW_MESSAGE, async args =>
        {
            (var message, string title, TaskCompletionSource<object?> tcs) = ((string message, string title, TaskCompletionSource<object?> tcs))args!;
            await (this.tcs?.Task ?? Task.CompletedTask);
            this.tcs = tcs;
            await ShowMessageBox(message, title);
        });

        return base.OnInitAsync();
    }

    private async Task ShowMessageBox(string message, string title = "")
    {
        await InvokeAsync(() =>
        {
            isOpen = true;
            this.title = title;
            body = message;

            StateHasChanged();
        });
    }

    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed || disposing is false) return;

        tcs?.TrySetResult(null);
        tcs = null;
        dispose?.Invoke();

        disposed = true;
    }
}
