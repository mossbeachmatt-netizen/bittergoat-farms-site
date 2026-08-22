// levelUpToast.js
// Full-screen celebratory overlay shown when the hub detects the
// player's level is higher than lastSeenLevel. Auto-dismisses, or the
// player can tap through immediately.

export function showLevelUpToast(fromLevel, toLevel, onDismiss) {
  const overlay = document.createElement("div");
  overlay.className = "levelup-overlay";
  overlay.innerHTML = `
    <div class="levelup-card">
      <div class="levelup-card__eyebrow">LEVEL UP</div>
      <div class="levelup-card__levels">
        <span class="levelup-card__from">${fromLevel}</span>
        <span class="levelup-card__arrow">&#8594;</span>
        <span class="levelup-card__to">${toLevel}</span>
      </div>
      <div class="levelup-card__sub">Tap anywhere to continue</div>
    </div>
  `;

  document.body.appendChild(overlay);

  let dismissed = false;
  const dismiss = () => {
    if (dismissed) return;
    dismissed = true;
    overlay.classList.add("levelup-overlay--out");
    setTimeout(() => {
      overlay.remove();
      if (onDismiss) onDismiss();
    }, 250);
  };

  overlay.addEventListener("click", dismiss);
  setTimeout(dismiss, 3200);
}
