/// <summary>
/// Interfaz que implementa cualquier objeto interactuable del juego.
/// El jugador nunca sabe qué objeto es, solo llama a Interact().
/// 
/// Ejemplos: timón, cofre, máquina de navegación, puerta, puzzle.
/// </summary>
public interface IInteractable
{
    /// <summary>Texto que se muestra en el HUD cuando el jugador apunta al objeto.</summary>
    string InteractionPrompt { get; }

    /// <summary>Se llama cuando el jugador presiona E.</summary>
    void Interact();
}
