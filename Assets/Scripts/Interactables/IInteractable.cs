
/// Interfaz que implementa cualquier objeto interactuable del juego.
/// El jugador nunca sabe qué objeto es, solo llama a Interact().

public interface IInteractable
{
    /// Texto que se muestra en el HUD cuando el jugador apunta al objeto.
    string InteractionPrompt { get; }

    /// Se llama cuando el jugador presiona E.
    void Interact();
}
