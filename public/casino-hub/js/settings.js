// settings.js
// Simple settings modal: sound/music toggles + reset save (dev convenience).

import { getState, updateSetting, resetSave } from "./playerState.js";

export function openSettings(modalRoot) {
  const state = getState();

  modalRoot.innerHTML = `
    <div class="modal-backdrop">
      <div class="modal-panel settings-panel">
        <button class="modal-close" type="button" aria-label="Close">&times;</button>
        <h2 class="marquee-heading">SETTINGS</h2>

        <div class="settings-row">
          <span>Sound Effects</span>
          <label class="switch">
            <input type="checkbox" id="toggle-sound" ${state.settings.sound ? "checked" : ""}>
            <span class="switch__track"></span>
          </label>
        </div>
        <div class="settings-row">
          <span>Music</span>
          <label class="switch">
            <input type="checkbox" id="toggle-music" ${state.settings.music ? "checked" : ""}>
            <span class="switch__track"></span>
          </label>
        </div>

        <button class="danger-btn" id="reset-save-btn" type="button">Reset Save (Dev)</button>
      </div>
    </div>
  `;

  modalRoot.querySelector(".modal-close").addEventListener("click", () => {
    modalRoot.innerHTML = "";
  });
  modalRoot.querySelector(".modal-backdrop").addEventListener("click", (e) => {
    if (e.target.classList.contains("modal-backdrop")) modalRoot.innerHTML = "";
  });

  modalRoot.querySelector("#toggle-sound").addEventListener("change", (e) => {
    updateSetting("sound", e.target.checked);
  });
  modalRoot.querySelector("#toggle-music").addEventListener("change", (e) => {
    updateSetting("music", e.target.checked);
  });
  modalRoot.querySelector("#reset-save-btn").addEventListener("click", () => {
    if (confirm("Reset all progress? This can't be undone.")) {
      resetSave();
      modalRoot.innerHTML = "";
    }
  });
}
