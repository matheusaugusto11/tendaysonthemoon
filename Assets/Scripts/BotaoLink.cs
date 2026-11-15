using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Botão de link externo: no pointer down agenda a abertura (WebGL) e no clique abre (editor).</summary>
public class BotaoLink : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public string url;
    public void OnPointerDown(PointerEventData e) { if (!string.IsNullOrEmpty(url)) Navegador.PrepararLink(url); }
    public void OnPointerClick(PointerEventData e) { if (!string.IsNullOrEmpty(url)) Navegador.AbrirLinkDireto(url); }
}
