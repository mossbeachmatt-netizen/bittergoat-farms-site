// gameBridge.js
// The only thing a mini-game should import to talk to the hub.
//
// Real folder layout on your site:
//   public/casino-hub/js/gameBridge.js   <- this file
//   public/games/<id>/index.html         <- games import it from here
// so games reach it via "../../casino-hub/js/gameBridge.js" (two levels
// up to public/, then into casino-hub/js/).
//
// Games should NOT import playerState.js directly: routing everything
// through this bridge keeps the surface area small and stable even if
// playerState's internals change later.

import {
  addCoins,
  addGems,
  addXP,
  getState,
  subscribe as subscribeState,
} from "./playerState.js";

export function reportCoins(amount) {
  addCoins(amount);
}

export function reportGems(amount) {
  addGems(amount);
}

export function reportXP(amount) {
  addXP(amount);
}

export function reportBet(amount) {
  // Level progress is driven by cumulative gold bet, not winnings —
  // call this whenever a game deducts a real (non-free) bet.
  addXP(amount);
}

export function getPlayerSummary() {
  const s = getState();
  return { coins: s.coins, gems: s.gems, level: s.level, xp: s.xp, xpToNextLevel: s.xpToNextLevel };
}

export function subscribeToPlayer(fn) {
  return subscribeState((s) =>
    fn({ coins: s.coins, gems: s.gems, level: s.level, xp: s.xp, xpToNextLevel: s.xpToNextLevel })
  );
}

export function returnToHub() {
  // From public/games/<id>/index.html: up to games/, up to public/,
  // into casino-hub/. Trailing slash matters here — without it, the
  // static server's clean-URL redirect (index.html -> casino-hub) can
  // leave the browser treating "casino-hub" as a file rather than a
  // folder, which breaks every relative asset path (css/js/images) on
  // the hub page.
  window.location.href = "../../casino-hub/";
}
