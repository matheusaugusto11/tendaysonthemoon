using System;
using UnityEngine;

/// <summary>Os 3 recursos da base. Valores de 0 a 100.</summary>
[Serializable]
public class Recursos
{
    public const float Max = 100f;
    public float energia = Max;
    public float oxigenio = Max;
    public float comida = Max;

    public void Aplicar(float dEnergia, float dOxigenio, float dComida)
    {
        energia = Mathf.Clamp(energia + dEnergia, 0f, Max);
        oxigenio = Mathf.Clamp(oxigenio + dOxigenio, 0f, Max);
        comida = Mathf.Clamp(comida + dComida, 0f, Max);
    }
}
