// levelProgress.js
// Renders the level badge (star icon + number) and the level-progress
// pill (label on top, bar with the percentage overlaid on the fill —
// matching the reference art's layout). Subscribes to playerState so
// it re-renders on any XP/level change.

import { subscribe } from "./playerState.js";

export function initLevelProgress(container) {
  container.innerHTML = `
    <div class="level-pill">
      <span class="level-pill__star">&#9733;</span>
      <div class="level-pill__text">
        <span class="level-pill__label">LEVEL</span>
        <span class="level-pill__num">1</span>
      </div>
    </div>
    <div class="progress-pill">
      <span class="progress-pill__label">LEVEL PROGRESS</span>
      <div class="progress-pill__track">
        <div class="progress-pill__fill"></div>
        <span class="progress-pill__pct">0%</span>
      </div>
    </div>
  `;

  const num = container.querySelector(".level-pill__num");
  const fill = container.querySelector(".progress-pill__fill");
  const pct = container.querySelector(".progress-pill__pct");

  subscribe((state) => {
    num.textContent = state.level;
    const percent = Math.min(100, Math.round((state.xp / state.xpToNextLevel) * 100));
    fill.style.width = `${percent}%`;
    pct.textContent = `${percent}%`;
  });
}
