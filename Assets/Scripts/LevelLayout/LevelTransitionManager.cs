using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelTransitionManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool deactivateOnStart = true;
    [SerializeField] private FadeInAndOutScript fade;

    private List<RoomScript> allRooms = new();
    private RoomScript currentRoom;
    public RoomScript CurrentRoom => currentRoom;

    public void Initialize(List<RoomScript> rooms)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("no rooms to initialize");
            return;
        }

        allRooms = new List<RoomScript>(rooms);
        currentRoom = rooms.FirstOrDefault(r => r != null && r.gameObject != null);

        if (deactivateOnStart)
            UpdateActiveRooms();
    }

    public void TransitionToRoom(RoomScript newRoom)
    {
        if (newRoom == null || newRoom.gameObject == null) return;
        if (newRoom == currentRoom) return;

        currentRoom = newRoom;
        UpdateActiveRooms();
    }

    public IEnumerator TransitionToRoomWithFade(RoomScript newRoom, Action onMidFade = null)
    {
        if (fade == null)
        {
            onMidFade?.Invoke();
            TransitionToRoom(newRoom);
            yield break;
        }

        yield return fade.PlayFadeOutAndIn(
            onMidpoint: () =>
            {
                onMidFade?.Invoke();
                TransitionToRoom(newRoom);
            }
        );
    }

    private void UpdateActiveRooms()
    {
        foreach (var room in allRooms)
        {
            if (room == null || room.gameObject == null) continue;
            room.gameObject.SetActive(room == currentRoom);
        }
    }
}
