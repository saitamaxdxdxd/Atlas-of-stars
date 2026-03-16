using System;
using UnityEngine;

namespace Retropolis.Input
{
    public interface IInputHandler
    {
        /// <summary>Toque/click rápido sin movimiento.</summary>
        event Action<Vector2> OnTap;

        /// <summary>Deslizamiento. arg1 = dirección normalizada, arg2 = vector completo en pixels.</summary>
        event Action<Vector2, Vector2> OnSwipe;

        /// <summary>Toque sostenido superó HoldDuration.</summary>
        event Action<Vector2> OnHoldStart;

        /// <summary>Dedo/botón levantado después de un hold.</summary>
        event Action OnHoldEnd;

        /// <summary>Primer contacto con la pantalla.</summary>
        event Action<Vector2> OnPointerDown;

        /// <summary>Contacto levantado (siempre, sin importar gesto).</summary>
        event Action<Vector2> OnPointerUp;
    }
}
