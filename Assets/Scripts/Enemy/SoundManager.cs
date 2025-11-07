using UnityEngine;
using System;

// A static class that manages sound events for all enemies to hear
public static class SoundManager
{
    public static event Action<Vector3> OnSoundMade;
    public static void MakeSound(Vector3 location)
    {
        OnSoundMade?.Invoke(location);
    }
}