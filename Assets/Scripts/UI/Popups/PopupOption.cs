/*
 * PopupOption
 * -----------
 * Represents a single button option inside a popup.
 *
 * Fields:
 *   - Label: text shown on the button
 *   - Callback: action executed when the button is pressed
 *   - IsConfirm: marks this option as the "confirm" action
 *
 * Used by OptionPopupManager to build popup buttons dynamically.
 */
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
