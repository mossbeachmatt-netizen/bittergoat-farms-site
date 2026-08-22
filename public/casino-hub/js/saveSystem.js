// saveSystem.js
// Thin wrapper around localStorage with a versioned schema key.
// Bump SAVE_VERSION and add a migration in loadRaw() if the shape of
// playerState ever changes, so old saves don't crash new code.

const SAVE_KEY = "bittergoat_save";
const SAVE_VERSION = 1;

export function loadRaw(defaultState) {
  try {
    const raw = localStorage.getItem(SAVE_KEY);
    if (!raw) return structuredClone(defaultState);

    const parsed = JSON.parse(raw);
    if (parsed.version !== SAVE_VERSION) {
      // No migrations yet — fall back to default on version mismatch.
      console.warn("[saveSystem] save version mismatch, resetting");
      return structuredClone(defaultState);
    }
    return parsed.data;
  } catch (err) {
    console.error("[saveSystem] failed to load save, using default", err);
    return structuredClone(defaultState);
  }
}

export function saveRaw(state) {
  try {
    localStorage.setItem(
      SAVE_KEY,
      JSON.stringify({ version: SAVE_VERSION, data: state })
    );
    return true;
  } catch (err) {
    console.error("[saveSystem] failed to save", err);
    return false;
  }
}

export function clearSave() {
  localStorage.removeItem(SAVE_KEY);
}
