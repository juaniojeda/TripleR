using UnityEngine;

public class StopButton : MonoBehaviour
{
    [Header("Sistemas de la Planta")]
    [Tooltip("El componente que genera los residuos.")]
    [SerializeField] private ObjectSpawner spawner;

    [Tooltip("El componente que mueve los residuos.")]
    [SerializeField] private CintaTransportadora cinta;

    // Detiene completamente la generación de objetos y el movimiento de la cinta.
    public void PauseSystem()
    {
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        if (cinta != null)
        {
            cinta.TurnOff();
        }
    }

    public void ResumeSystem()
    {
        if (spawner != null)
        {
            spawner.StartSpawning();
        }

        if (cinta != null)
        {
            cinta.TurnOn();
        }
    }

    public void ToggleSystem()
    {
        if (cinta != null)
        {
            // Usamos el estado de la cinta como referencia principal del sistema
            if (cinta.isOn)
            {
                PauseSystem();
            }
            else
            {
                ResumeSystem();
            }
        }
    }
}
