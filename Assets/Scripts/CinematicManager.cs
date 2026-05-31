using UnityEngine;
using UnityEngine.Playables;

public class CinematicManager : MonoBehaviour
{
    public PlayableDirector Cine;
    public Transform playerCinematicParent;
    public Transform shipCinematicParent;
    public Transform playerTransform;
    public Transform shipTransform;

    public Transform newPlayerParent;
    public Transform newShipParent;

    private bool _cinematicEnded = false;

    void Start()
    {
        if (Cine == null)
        {
            Debug.LogError("falta director");
            return;
        }

        Cine.stopped += OnCinematicStopped;
        PlayCinematic();
    }

    void Update()
    {
        if (!_cinematicEnded && Cine != null)
        {
            bool isPlaying = Cine.state == PlayState.Playing;
            bool reachedEnd = Cine.time >= Cine.duration;

            if (!isPlaying && reachedEnd)
            {
                EndCinematic();
            }
        }
    }

    void PlayCinematic()
    {
        if (BlockPlayerMovement.Instance != null)
            BlockPlayerMovement.Instance.ImmobilizePlayer();

        Cine.Play();
    }

    void OnCinematicStopped(PlayableDirector director)
    {
        EndCinematic();
    }

    void EndCinematic()
    {
        if (_cinematicEnded) return; 
        _cinematicEnded = true;

        ReparentObject(playerTransform, newPlayerParent, "Jugador");
        ReparentObject(shipTransform,   newShipParent,   "Barco");

        if (BlockPlayerMovement.Instance != null)
        {
            BlockPlayerMovement.Instance.FreePlayer();
        }
    }

    void ReparentObject(Transform obj, Transform newParent, string label)
    {
        if (obj == null)
        {
            return;
        }

        obj.SetParent(newParent, worldPositionStays: true);
    }

    void OnDestroy()
    {
        if (Cine != null)
            Cine.stopped -= OnCinematicStopped;
    }
}