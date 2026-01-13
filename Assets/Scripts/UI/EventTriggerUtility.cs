using UnityEngine;
using UnityEngine.EventSystems;

/*
 * EventTriggerUtility
 * -------------------
 * Adds pointer enter/exit events to UI objects.
 * Used by CharacterSelectManager to show/hide info on hover.
 */
public static class EventTriggerUtility
{
    public static void AddHoverEvents(GameObject obj, System.Action onEnter, System.Action onExit)
    {
        EventTrigger trigger = obj.AddComponent<EventTrigger>();

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((_) => onEnter());
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) => onExit());
        trigger.triggers.Add(entryExit);
    }
}
