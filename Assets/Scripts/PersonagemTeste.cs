using UnityEngine;

/// <summary>Só para validar: WASD/setas/analógico movem, Shift corre. Gira para a direção do movimento.</summary>
[RequireComponent(typeof(AnimadorPersonagem))]
public class PersonagemTeste : MonoBehaviour
{
    public float velocidadeAndar = 1.5f;
    public float velocidadeCorrer = 4.5f;
    public float giro = 12f;
    AnimadorPersonagem _anim;

    void Awake() { _anim = GetComponent<AnimadorPersonagem>(); }

    void Update()
    {
        Vector2 input = Vector2.ClampMagnitude(Entrada.Mover, 1f);
        Vector3 dir = new Vector3(input.x, 0, input.y);
        float v = Input.GetKey(KeyCode.LeftShift) ? velocidadeCorrer : velocidadeAndar;
        float vel = dir.magnitude * v;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), giro * Time.deltaTime);
            transform.position += dir.normalized * (vel * Time.deltaTime);
        }
        _anim.SetVelocidade(vel);
    }
}
