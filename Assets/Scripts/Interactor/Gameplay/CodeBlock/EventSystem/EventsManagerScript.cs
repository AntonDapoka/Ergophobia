using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class EventsManagerScript : MonoBehaviour
{
    private Dictionary<EventTypes, UnityEvent> _eventDictionary;
    private Dictionary<EventTypesBlock, EventScript> _eventDictionaryBlock;

    public EventsManagerScript()
    {
        _eventDictionary ??= new Dictionary<EventTypes, UnityEvent>();
        _eventDictionaryBlock ??= new Dictionary<EventTypesBlock, EventScript>();
    }

    public void StartListening(EventTypes eventName, UnityAction listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            _eventDictionary.Add(eventName, thisEvent);
        }
   }

    public void StartListening(EventTypesBlock eventName, UnityAction<ICodeBlock> listener)
    {
        if (_eventDictionaryBlock.TryGetValue(eventName, out EventScript thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new EventScript();
            thisEvent.AddListener(listener);
            _eventDictionaryBlock.Add(eventName, thisEvent);
        }
   }

    public void StopListening(EventTypes eventName, UnityAction listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
   }

    public void StopListening(EventTypesBlock eventName, UnityAction<ICodeBlock> listener)
    {
        if (_eventDictionaryBlock.TryGetValue(eventName, out EventScript thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
   }

    public void TriggerEvent(EventTypes eventName)
    {
        if (_eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.Invoke();
        }
   }

    public void TriggerEvent(EventTypesBlock eventName, ICodeBlock block)
    {
        if (_eventDictionaryBlock.TryGetValue(eventName, out EventScript thisEvent))
        {
            thisEvent.Invoke(block);
        }
   }
}
