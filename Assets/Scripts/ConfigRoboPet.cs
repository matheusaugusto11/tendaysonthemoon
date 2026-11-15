using UnityEngine;

/// <summary>
/// Configuração do robozinho que segue o astronauta (Assets/Resources/RoboPet.asset). Troque o prefab, a posição ou a
/// velocidade aqui, sem mexer na cena. Desmarque "ativo" para tirar o robô do jogo.
/// </summary>
[CreateAssetMenu(fileName = "RoboPet", menuName = "Dez Dias/Robô que segue o astronauta")]
public class ConfigRoboPet : ScriptableObject
{
    public bool ativo = true;
    [Tooltip("Modelo do robô (com Animator e o parâmetro Velocidade).")]
    public GameObject prefab;
    [Tooltip("O robô só aparece nesta cena. Vazio = em qualquer cena com o astronauta.")]
    public string somenteNaCena = "Game";

    [Header("Onde ele fica (em relação ao astronauta)")]
    [Tooltip("x = direita, z = frente. (0.9, -1.2) = um pouco atrás, do lado direito.")]
    public Vector2 posicao = new Vector2(0.9f, -1.2f);
    [Tooltip("Perto assim do ponto de seguir, o robô para.")]
    public float distanciaParar = 0.4f;

    [Header("Velocidade (m/s)")]
    public float velocidade = 4.5f;
    [Tooltip("Velocidade para alcançar quando fica para trás (o astronauta corre a 8).")]
    public float velocidadeAlcancar = 8.5f;
    [Tooltip("Mais longe que isso do astronauta (ou preso numa parede), o robô reaparece atrás dele.")]
    public float distanciaReaparecer = 9f;

    [Header("Som do robô (Resources/Sons/RobotSound)")]
    [Tooltip("De tempos em tempos o robô toca um trecho do som, como se estivesse interagindo.")]
    public bool somAtivo = true;
    [Range(0f, 1f)] public float volumeSom = 0.25f;
    [Tooltip("Duração de cada trecho (s).")]
    public float duracaoSom = 2f;
    [Tooltip("Tempo entre um som e outro (s): sorteado entre x e y.")]
    public Vector2 intervaloSom = new Vector2(8f, 20f);
}
