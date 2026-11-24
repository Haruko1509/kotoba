// ===================================================================
// KOTOBA ODYSSEY - APPLICATION ENTRYPOINT & HASH ROUTER
// ===================================================================

function handleRouteHash() {
  const hash = window.location.hash || '';
  if (!hash) return;

  if (hash.startsWith('#read/')) {
    const parts = hash.replace('#read/', '').split('/');
    const novelId = parseInt(parts[0]) || 1;
    const chapNum = parseInt(parts[1]) || 1;
    
    let bookKey = Object.keys(booksData).find(k => booksData[k].dbId === novelId);
    if (!bookKey) bookKey = `db_novel_${novelId}`;

    openBook(bookKey, chapNum);
  } else if (hash.startsWith('#detail/')) {
    const bookId = hash.replace('#detail/', '').trim();
    if (typeof openBookDetail === 'function') {
      openBookDetail(bookId);
    }
  } else if (hash === '#explore' || hash === '#library') {
    switchPane('explore');
  } else if (hash === '#srs' || hash === '#learning') {
    switchPane('learning');
  } else if (hash.startsWith('#vn')) {
    const rawTarget = hash.replace('#vn', '').replace(/^\//, '').split('/')[0];
    const targetBook = rawTarget || currentInspectedId || currentVnBookKey || 'ginga';
    openVN(targetBook);
  } else if (hash === '#profile') {
    switchPane('profile');
  } else if (hash === '#home') {
    switchPane('home');
  }
}

window.addEventListener('hashchange', handleRouteHash);

// Immediate initialization on page load
function initKotobaApp() {
  if (typeof renderHomeBookshelfFallback === 'function') {
    renderHomeBookshelfFallback();
  }
  handleRouteHash();
  selectBook('ginga');
  syncWithBackend();
  setTimeout(handleRouteHash, 80);
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initKotobaApp);
} else {
  initKotobaApp();
}



