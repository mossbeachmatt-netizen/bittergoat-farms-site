// playerState.js
// Single source of truth for everything that persists across sessions.
// Every system (marketplace, daily wheel, mini-games) should mutate
// state ONLY through the exported functions below, never by reaching
// into the object directly — that's what keeps balances in sync across
// screens and keeps autosave reliable.

import { loadRaw, saveRaw } from "./saveSystem.js";

// Progress toward the next level is driven by cumulative gold coins
// bet (not won) — see addXP(). The curve is easy early and steep
// later: exactly 1,000 coins bet to go from level 1 to 2, but ~125,000
// by the time you're going from level 5 to 6. Tune BASE/EXPONENT here
// if the pacing needs adjusting later.
const LEVEL_CURVE_BASE = 1000;
const LEVEL_CURVE_EXPONENT = 3;
function xpRequiredForLevel(level) {
  return Math.round(LEVEL_CURVE_BASE * Math.pow(level, LEVEL_CURVE_EXPONENT));
}

const DEFAULT_STATE = {
  coins: 5000,
  gems: 100,
  level: 1,
  xp: 0,
  xpToNextLevel: xpRequiredForLevel(1),
  lastSeenLevel: 1, // last level the hub UI has shown a level-up toast for
  unlockedGames: ["golden-farms"],
  lastDailyRewardClaim: null, // ISO timestamp or null
  inventory: {},
  settings: {
    sound: true,
    music: true,
  },
};

let state = loadRaw(DEFAULT_STATE);
// Guard against saves written before these fields existed.
if (state.lastSeenLevel === undefined) state.lastSeenLevel = state.level;
if (state.gems === undefined) state.gems = DEFAULT_STATE.gems;

const listeners = new Set();

function persistAndNotify() {
  saveRaw(state);
  listeners.forEach((fn) => fn(state));
}

export function subscribe(fn) {
  listeners.add(fn);
  fn(state); // fire immediately so UI can render current state
  return () => listeners.delete(fn);
}

export function getState() {
  return state;
}

export function addCoins(amount) {
  state.coins = Math.max(0, state.coins + amount);
  persistAndNotify();
}

export function spendCoins(amount) {
  if (state.coins < amount) return false;
  state.coins -= amount;
  persistAndNotify();
  return true;
}

export function addGems(amount) {
  state.gems = Math.max(0, state.gems + amount);
  persistAndNotify();
}

export function spendGems(amount) {
  if (state.gems < amount) return false;
  state.gems -= amount;
  persistAndNotify();
  return true;
}

export function addXP(amount) {
  // "xp" here is cumulative gold bet — see the comment on the curve
  // helper above. Kept the name to avoid touching every UI file that
  // already reads state.xp / state.xpToNextLevel.
  state.xp += amount;
  while (state.xp >= state.xpToNextLevel) {
    state.xp -= state.xpToNextLevel;
    state.level += 1;
    state.xpToNextLevel = xpRequiredForLevel(state.level);
  }
  persistAndNotify();
}

export function claimDailyReward(coinAmount) {
  state.coins += coinAmount;
  state.lastDailyRewardClaim = new Date().toISOString();
  persistAndNotify();
}

export function canClaimDailyReward() {
  if (!state.lastDailyRewardClaim) return true;
  const last = new Date(state.lastDailyRewardClaim).getTime();
  const now = Date.now();
  return now - last >= 24 * 60 * 60 * 1000;
}

export function unlockGame(gameId) {
  if (!state.unlockedGames.includes(gameId)) {
    state.unlockedGames.push(gameId);
    persistAndNotify();
  }
}

export function acknowledgeLevel() {
  // Called by the hub UI after it has shown a level-up toast, so the
  // same jump doesn't get re-announced on the next render/page load.
  state.lastSeenLevel = state.level;
  persistAndNotify();
}

export function updateSetting(key, value) {
  state.settings[key] = value;
  persistAndNotify();
}

export function resetSave() {
  state = structuredClone(DEFAULT_STATE);
  persistAndNotify();
}
