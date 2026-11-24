// ===================================================================
// KOTOBA ODYSSEY - UTILITY FUNCTIONS & TOAST NOTIFICATIONS
// ===================================================================

function escapeHtml(text) {
  if (!text) return '';
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

function showToastNotification(msg, type = 'info') {
  let toastContainer = document.getElementById('kotobaToastContainer');
  if (!toastContainer) {
    toastContainer = document.createElement('div');
    toastContainer.id = 'kotobaToastContainer';
    toastContainer.style.cssText = 'position:fixed;bottom:28px;right:28px;z-index:999999;display:flex;flex-direction:column;gap:10px;pointer-events:none;';
    document.body.appendChild(toastContainer);
  }

  const toast = document.createElement('div');
  toast.className = 'kotoba-toast-item';
  toast.style.cssText = 'background:rgba(15,23,42,0.94);backdrop-filter:blur(16px);border:1px solid rgba(139,92,246,0.35);box-shadow:0 12px 32px rgba(0,0,0,0.5),0 0 16px rgba(139,92,246,0.2);color:#F8FAFC;padding:12px 20px;border-radius:12px;font-size:13.5px;font-weight:500;display:flex;align-items:center;gap:10px;opacity:0;transform:translateY(16px) scale(0.96);transition:all 0.28s cubic-bezier(0.16,1,0.3,1);pointer-events:auto;';

  const icon = type === 'error' ? '⚠️' : (msg.includes('+') ? '✨' : '✦');
  toast.innerHTML = `<span style="font-size:16px;">${icon}</span><span>${escapeHtml(msg)}</span>`;
  toastContainer.appendChild(toast);

  requestAnimationFrame(() => {
    toast.style.opacity = '1';
    toast.style.transform = 'translateY(0) scale(1)';
  });

  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateY(-12px) scale(0.96)';
    setTimeout(() => {
      if (toast.parentNode) toast.parentNode.removeChild(toast);
    }, 300);
  }, 3200);
}

function claimReward(btn, xp) {
  playSuccessChime();
  if (btn) {
    btn.textContent = '✓ Đã nhận';
    btn.disabled = true;
    btn.style.opacity = '0.6';
  }
}

function triggerImportPrompt() {
  openImportModal();
}
