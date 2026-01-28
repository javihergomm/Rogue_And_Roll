public class PopupOption
{
    public string Label { get; }
    public System.Action Callback { get; }
    public bool IsConfirm { get; }

    public PopupOption(string label, System.Action callback, bool isConfirm = false)
    {
        Label = label;
        Callback = callback;
        IsConfirm = isConfirm;
    }
}
