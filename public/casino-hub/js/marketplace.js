// marketplace.js
// Store modal. Coin packages are stubbed as instant grants for the
// prototype — swap the button handler for a real IAP/payment call
// when that's wired up. unlockGame() is used for early-unlock goods.

import { addCoins, unlockGame, getState } from "./playerState.js";

const COIN_PACKAGES = [
  { id: "pack-small", coins: 1000, price: "$0.99" },
  { id: "pack-medium", coins: 5500, price: "$4.99", bonus: "+10%" },
  { id: "pack-large", coins: 12000, price: "$9.99", bonus: "+20%" },
  { id: "pack-mega", coins: 65000, price: "$49.99", bonus: "+35% BEST VALUE" },
];

const EARLY_UNLOCKS = [
  { id: "coin-dozer-plus", gameId: "coin-dozer", label: "Coin Dozer: Animal Catapult", cost: 2000 },
];

export function openMarketplace(modalRoot) {
  modalRoot.innerHTML = `
    <div class="modal-backdrop">
      <div class="modal-panel marketplace-panel">
        <button class="modal-close" type="button" aria-label="Close">&times;</button>
        <h2 class="marquee-heading">THE GENERAL STORE</h2>

        <h3 class="modal-subheading">Coin Packages</h3>
        <div class="coin-grid">
          ${COIN_PACKAGES.map(
            (pkg) => `
            <button class="coin-package" data-pkg="${pkg.id}" type="button">
              ${pkg.bonus ? `<span class="coin-package__bonus">${pkg.bonus}</span>` : ""}
              <span class="coin-package__amount">${pkg.coins.toLocaleString()}</span>
              <span class="coin-package__price">${pkg.price}</span>
            </button>`
          ).join("")}
        </div>

        <h3 class="modal-subheading">Early Unlocks</h3>
        <div class="unlock-list">
          ${EARLY_UNLOCKS.map(
            (item) => `
            <div class="unlock-row">
              <span>${item.label}</span>
              <button class="unlock-btn" data-unlock="${item.id}" data-game="${item.gameId}" data-cost="${item.cost}" type="button">
                🪙 ${item.cost.toLocaleString()}
              </button>
            </div>`
          ).join("")}
        </div>
      </div>
    </div>
  `;

  modalRoot.querySelector(".modal-close").addEventListener("click", () => {
    modalRoot.innerHTML = "";
  });
  modalRoot.querySelector(".modal-backdrop").addEventListener("click", (e) => {
    if (e.target.classList.contains("modal-backdrop")) modalRoot.innerHTML = "";
  });

  modalRoot.querySelectorAll(".coin-package").forEach((btn) => {
    btn.addEventListener("click", () => {
      const pkg = COIN_PACKAGES.find((p) => p.id === btn.dataset.pkg);
      // TODO: replace with real payment flow. Instant grant for prototype.
      addCoins(pkg.coins);
      btn.classList.add("coin-package--purchased");
      setTimeout(() => btn.classList.remove("coin-package--purchased"), 600);
    });
  });

  modalRoot.querySelectorAll(".unlock-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
      const cost = Number(btn.dataset.cost);
      const state = getState();
      if (state.coins < cost) {
        btn.classList.add("unlock-btn--denied");
        setTimeout(() => btn.classList.remove("unlock-btn--denied"), 400);
        return;
      }
      addCoins(-cost);
      unlockGame(btn.dataset.game);
      btn.textContent = "UNLOCKED";
      btn.disabled = true;
    });
  });
}
