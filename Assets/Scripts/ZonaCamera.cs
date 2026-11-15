using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Região da base (hub, túnel, sala, exterior...) com o enquadramento ideal da câmera. Quando o personagem está dentro,
/// a CameraTerceiraPessoa mistura suavemente para estes valores. Fora de qualquer zona valem os padrões da câmera.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ZonaCamera : MonoBehaviour
{
    public static readonly List<ZonaCamera> Todas = new List<ZonaCamera>();

    [Tooltip("Distância da câmera ao personagem (m).")]
    [Range(1f, 12f)] public float distancia = 4.5f;
    [Tooltip("Inclinação padrão da câmera (graus para baixo).")]
    [Range(-5f, 60f)] public float inclinacao = 20f;
    [Tooltip("Campo de visão (graus).")]
    [Range(35f, 80f)] public float fov = 60f;
    [Tooltip("Se duas zonas se sobrepõem, vale a de maior prioridade.")]
    public int prioridade = 0;
    public Color corGizmo = new Color(0.3f, 0.8f, 1f, 0.25f);

    BoxCollider _box;

    void OnEnable()
    {
        _box = GetComponent<BoxCollider>();
        _box.isTrigger = true; // nunca bloqueia nada
        if (!Todas.Contains(this)) Todas.Add(this);
    }

    void OnDisable() { Todas.Remove(this); }

    /// <summary>O ponto está dentro da caixa (considera rotação; zonas sem escala).</summary>
    public bool Contem(Vector3 p)
    {
        if (_box == null) _box = GetComponent<BoxCollider>();
        Vector3 l = transform.InverseTransformPoint(p) - _box.center;
        Vector3 m = _box.size * 0.5f;
        return Mathf.Abs(l.x) <= m.x && Mathf.Abs(l.y) <= m.y && Mathf.Abs(l.z) <= m.z;
    }

    public float Volume { get { if (_box == null) _box = GetComponent<BoxCollider>(); return _box.size.x * _box.size.y * _box.size.z; } }

    void OnDrawGizmos()
    {
        var b = GetComponent<BoxCollider>(); if (b == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = corGizmo; Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = new Color(corGizmo.r, corGizmo.g, corGizmo.b, 0.9f); Gizmos.DrawWireCube(b.center, b.size);
    }
}
