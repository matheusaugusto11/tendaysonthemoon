// Funções do navegador usadas pelo jogo na build WebGL (ver Assets/Scripts/Navegador.cs).
mergeInto(LibraryManager.library, {
  // 1 quando o sistema pede movimento reduzido (prefers-reduced-motion).
  DD_PrefereMenosMovimento: function () {
    try { return (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) ? 1 : 0; }
    catch (e) { return 0; }
  },
  // 1 = navegador em português, 0 = outro idioma, -1 = não deu para ler.
  DD_LinguaNavegadorPt: function () {
    try {
      var l = (navigator.language || navigator.userLanguage || '').toLowerCase();
      if (!l) return -1;
      return l.indexOf('pt') === 0 ? 1 : 0;
    } catch (e) { return -1; }
  },
  // Abre o link em outra aba. O navegador só deixa abrir aba durante um gesto do usuário:
  // 1) se o gesto ainda vale (navigator.userActivation), abre na hora;
  // 2) senão, abre no próximo "soltar" do clique/toque, mas só até 1,5 s depois (nunca num clique qualquer mais tarde).
  DD_AbrirLinkNoProximoClique: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    try {
      if (navigator.userActivation && navigator.userActivation.isActive) { window.open(url, '_blank', 'noopener'); return; }
    } catch (e) { }
    var limite = Date.now() + 1500;
    var abrir = function () {
      document.removeEventListener('pointerup', abrir, true);
      document.removeEventListener('touchend', abrir, true);
      if (Date.now() <= limite) window.open(url, '_blank', 'noopener');
    };
    document.addEventListener('pointerup', abrir, true);
    document.addEventListener('touchend', abrir, true);
  }
});
