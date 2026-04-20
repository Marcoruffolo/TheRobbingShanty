using UnityEngine;
using UnityEngine.Playables;

public class CinematicManager : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector Cine;

    [Header("Objetos de la cinemática")]
    public Transform playerCinematicParent;
    public Transform shipCinematicParent;
    public Transform playerTransform;
    public Transform shipTransform;

    [Header("Nuevos padres al terminar (vacío = raíz de la escena)")]
    public Transform newPlayerParent;
    public Transform newShipParent;

    private bool _cinematicEnded = false;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        if (Cine == null)
        {
            Debug.LogError("CinematicManager: no hay PlayableDirector asignado.");
            return;
        }

        Cine.stopped += OnCinematicStopped;
        PlayCinematic();
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        // Fallback: if the director finished but the event never fired
        if (!_cinematicEnded && Cine != null)
        {
            bool isPlaying = Cine.state == PlayState.Playing;
            bool reachedEnd = Cine.time >= Cine.duration;

            if (!isPlaying && reachedEnd)
            {
                Debug.LogWarning("CinematicManager: stopped event did not fire, using fallback.");
                EndCinematic();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    void PlayCinematic()
    {
        if (BlockPlayerMovement.Instance != null)
            BlockPlayerMovement.Instance.ImmobilizePlayer();
        else
            Debug.LogWarning("CinematicManager: no se encontró BlockPlayerMovement.Instance.");

        Cine.Play();
        Debug.Log("CinematicManager: cinemática iniciada.");
    }

    // ─────────────────────────────────────────────────────────────
    void OnCinematicStopped(PlayableDirector director)
    {
        Debug.Log("CinematicManager: stopped event fired.");
        EndCinematic();
    }

    // ─────────────────────────────────────────────────────────────
    void EndCinematic()
    {
        if (_cinematicEnded) return; // make sure it only runs once
        _cinematicEnded = true;

        ReparentObject(playerTransform, newPlayerParent, "Jugador");
        ReparentObject(shipTransform,   newShipParent,   "Barco");

        if (BlockPlayerMovement.Instance != null)
        {
            BlockPlayerMovement.Instance.FreePlayer();
            Debug.Log("CinematicManager: FreePlayer() called successfully.");
        }
        else
        {
            Debug.LogError("CinematicManager: BlockPlayerMovement.Instance is NULL at end — player cannot be freed!");
        }
    }

    // ─────────────────────────────────────────────────────────────
    void ReparentObject(Transform obj, Transform newParent, string label)
    {
        if (obj == null)
        {
            Debug.LogWarning($"CinematicManager: Transform de '{label}' no asignado.");
            return;
        }

        obj.SetParent(newParent, worldPositionStays: true);
        Debug.Log($"CinematicManager: '{label}' reparentado a '{(newParent != null ? newParent.name : "raíz de la escena")}'.");
    }

    // ─────────────────────────────────────────────────────────────
    void OnDestroy()
    {
        if (Cine != null)
            Cine.stopped -= OnCinematicStopped;
    }
}