// hubScreen.js
// Entry point. Renders the game strip, wires the status bar (level
// pill, coin/gem odometers), shows the daily wheel as a first-run
// modal, and opens modals for the marketplace and settings.

import { subscribe, getState, acknowledgeLevel, canClaimDailyReward, updateSetting } from "./playerState.js";
import { initDailyReward } from "./dailyReward.js";
import { initLevelProgress } from "./levelProgress.js";
import { openMarketplace } from "./marketplace.js";
import { openSettings } from "./settings.js";
import { showLevelUpToast } from "./levelUpToast.js";

// All entries after the first are still placeholders — no pages
// exist for them yet, so they don't navigate anywhere. Golden Farms
// is the first real game: it links out to the actual game folder,
// which lives at public/games/golden-farms/ (one level up from this
// casino-hub folder, alongside your other existing games).
const GAMES = [
  {
    id: "golden-farms",
    title: "Golden Farms",
    locked: false,
    url: "../games/golden-farms/index.html",
    thumb: "assets/thumbnails/golden-farms.gif",
  },
  {
    id: "rocket",
    title: "Rocket",
    locked: false,
    url: "../games/rocket/index.html",
    thumb: "assets/thumbnails/rocket.gif",
  },
  {
    id: "luck-truck",
    title: "Luck Truck",
    locked: false,
    url: "../games/luck-truck/index.html",
    thumb: "assets/thumbnails/luck-truck.gif",
  },
  ...Array.from({ length: 17 }, (_, i) => ({
    id: `placeholder-${i + 4}`,
    title: `Game ${i + 4}`,
    locked: true,
    placeholder: true,
  })),
];

function renderOdometer(container, value) {
  const digits = value.toLocaleString("en-US").split("");
  container.innerHTML = digits
    .map((ch) =>
      ch === ","
        ? `<span class="odometer-comma">,</span>`
        : `<span class="odometer-digit">${ch}</span>`
    )
    .join("");
}

function renderGameGrid(container, unlockedGames) {
  container.innerHTML = GAMES.map((game) => {
    const unlocked = game.placeholder ? false : unlockedGames.includes(game.id) || !game.locked;
    const artStyle = game.thumb ? ` style="background-image:url('${game.thumb}');background-size:contain;background-repeat:no-repeat;background-position:center;background-color:var(--bg-panel-raised);"` : "";
    return `
      <button class="game-card ${unlocked ? "" : "game-card--locked"}" type="button" data-game="${game.id}" ${game.placeholder ? "disabled" : ""}>
        <div class="game-card__art"${artStyle} aria-hidden="true"></div>
        <span class="game-card__title">${game.title}</span>
        ${!unlocked && !game.placeholder ? `<span class="game-card__lock">🔒</span>` : ""}
      </button>
    `;
  }).join("");

  container.querySelectorAll(".game-card:not(.game-card--locked):not([disabled])").forEach((btn) => {
    btn.addEventListener("click", () => {
      const game = GAMES.find((g) => g.id === btn.dataset.game);
      // Real games (like Golden Farms) specify an explicit url since
      // they live outside this folder, alongside your other deployed
      // games under public/games/. Fall back to the local convention
      // for anything without one.
      const target = game && game.url ? game.url : `games/${btn.dataset.game}/index.html`;
      window.location.href = target;
    });
  });
}

let levelUpToastActive = false;

function checkForLevelUp(state) {
  if (levelUpToastActive) return;
  if (state.level <= state.lastSeenLevel) return;

  levelUpToastActive = true;
  const fromLevel = state.lastSeenLevel;
  const toLevel = state.level;
  showLevelUpToast(fromLevel, toLevel, () => {
    levelUpToastActive = false;
  });
  acknowledgeLevel();
}

function showDailyRewardModal() {
  if (!canClaimDailyReward()) return;

  const root = document.querySelector("#daily-modal-root");
  const overlay = document.createElement("div");
  overlay.className = "daily-modal-overlay";
  overlay.innerHTML = `<div class="daily-modal-inner" id="daily-modal-inner"></div>`;
  root.appendChild(overlay);

  const inner = overlay.querySelector("#daily-modal-inner");

  const dismiss = () => {
    overlay.classList.add("daily-modal-overlay--out");
    setTimeout(() => overlay.remove(), 250);
  };

  initDailyReward(inner, {
    onClaimed: () => {
      const continueBtn = document.createElement("button");
      continueBtn.className = "continue-btn";
      continueBtn.type = "button";
      continueBtn.textContent = "CONTINUE ▸";
      continueBtn.addEventListener("click", dismiss);
      inner.appendChild(continueBtn);
    },
  });
}

function initMusic() {
  const audio = document.querySelector("#bg-music");
  const btn = document.querySelector("#music-btn");
  audio.volume = 0.45;

  function applyMusicState(isOn) {
    btn.textContent = isOn ? "\u{1F50A}" : "\u{1F507}"; // 🔊 / 🔇
    if (isOn) {
      audio.play().catch(() => {
        // iOS/Safari blocks autoplay-with-sound until the player has
        // interacted with the page at least once. Retry on the very
        // next tap anywhere, then stop listening.
        const resumeOnFirstTap = () => {
          audio.play().catch(() => {});
          document.removeEventListener("pointerdown", resumeOnFirstTap);
        };
        document.addEventListener("pointerdown", resumeOnFirstTap, { once: true });
      });
    } else {
      audio.pause();
    }
  }

  // Single source of truth: whether music plays is driven entirely by
  // settings.music, so this stays in sync whether it was toggled from
  // this button or from the Settings modal.
  subscribe((state) => applyMusicState(state.settings.music));

  btn.addEventListener("click", () => {
    updateSetting("music", !getState().settings.music);
  });
}

export function initHub() {
  const coinEl = document.querySelector("#coin-odometer");
  const gemEl = document.querySelector("#gem-odometer");
  const levelEl = document.querySelector("#level-progress");
  const gridEl = document.querySelector("#game-grid");
  const modalRoot = document.querySelector("#modal-root");

  initLevelProgress(levelEl);
  initMusic();

  subscribe((state) => {
    renderOdometer(coinEl, state.coins);
    renderOdometer(gemEl, state.gems);
    renderGameGrid(gridEl, state.unlockedGames);
    checkForLevelUp(state);
  });

  document.querySelector("#add-coins-btn").addEventListener("click", () => {
    openMarketplace(modalRoot, "coins");
  });
  document.querySelector("#add-gems-btn").addEventListener("click", () => {
    openMarketplace(modalRoot, "gems");
  });
  document.querySelector("#open-settings").addEventListener("click", () => {
    openSettings(modalRoot);
  });

  // Daily wheel is the first thing the player sees, if unclaimed today.
  showDailyRewardModal();
}

document.addEventListener("DOMContentLoaded", initHub);
