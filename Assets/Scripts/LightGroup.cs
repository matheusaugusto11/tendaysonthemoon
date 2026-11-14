using UnityEngine;

/// <summary>Marca uma luz da base com seu grupo (sol, terra, int, ext, emerg) e a energia calibrada de referência.</summary>
[RequireComponent(typeof(Light))]
public class LightGroup : MonoBehaviour
{
    public string grupo;
    public float energiaBase = 1f;

    Light _luz;
    public Light Luz { get { if (_luz == null) _luz = GetComponent<Light>(); return _luz; } }
}
