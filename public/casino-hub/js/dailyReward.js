// dailyReward.js
// Renders an SVG wheel, spins it via a CSS transform transition,
// and lands on a weighted-random reward segment.

import { claimDailyReward, canClaimDailyReward } from "./playerState.js";

const SEGMENTS = [
  { label: "100", coins: 100, weight: 30, color: "#6B7A4F" },
  { label: "250", coins: 250, weight: 25, color: "#8C5A2B" },
  { label: "500", coins: 500, weight: 20, color: "#6B7A4F" },
  { label: "1,000", coins: 1000, weight: 12, color: "#8C5A2B" },
  { label: "2,500", coins: 2500, weight: 8, color: "#6B7A4F" },
  { label: "JACKPOT", coins: 10000, weight: 5, color: "#E8495C" },
];

const SEGMENT_ANGLE = 360 / SEGMENTS.length;

function pickWeightedSegment() {
  const totalWeight = SEGMENTS.reduce((sum, s) => sum + s.weight, 0);
  let roll = Math.random() * totalWeight;
  for (let i = 0; i < SEGMENTS.length; i++) {
    roll -= SEGMENTS[i].weight;
    if (roll <= 0) return i;
  }
  return SEGMENTS.length - 1;
}

function buildWheelSVG() {
  const cx = 150, cy = 150, r = 145;
  let paths = "";
  let labels = "";

  SEGMENTS.forEach((seg, i) => {
    const startAngle = i * SEGMENT_ANGLE;
    const endAngle = startAngle + SEGMENT_ANGLE;
    const startRad = (Math.PI / 180) * startAngle;
    const endRad = (Math.PI / 180) * endAngle;

    const x1 = cx + r * Math.cos(startRad);
    const y1 = cy + r * Math.sin(startRad);
    const x2 = cx + r * Math.cos(endRad);
    const y2 = cy + r * Math.sin(endRad);

    paths += `<path d="M${cx},${cy} L${x1.toFixed(2)},${y1.toFixed(2)} A${r},${r} 0 0,1 ${x2.toFixed(2)},${y2.toFixed(2)} Z"
      fill="${seg.color}" stroke="#171210" stroke-width="2" />`;

    const labelAngle = (Math.PI / 180) * (startAngle + SEGMENT_ANGLE / 2);
    const lx = cx + (r * 0.62) * Math.cos(labelAngle);
    const ly = cy + (r * 0.62) * Math.sin(labelAngle);
    const rot = startAngle + SEGMENT_ANGLE / 2;

    labels += `<text x="${lx.toFixed(2)}" y="${ly.toFixed(2)}"
      transform="rotate(${rot.toFixed(2)} ${lx.toFixed(2)} ${ly.toFixed(2)})"
      class="wheel-label">${seg.label}</text>`;
  });

  return `<svg viewBox="0 0 300 300" class="wheel-svg">${paths}${labels}</svg>`;
}

export function initDailyReward(container, { onClaimed } = {}) {
  container.innerHTML = `
    <div class="daily-reward-card ${canClaimDailyReward() ? "" : "is-claimed"}">
      <div class="marquee-frame">
        <div class="wheel-wrap">
          <div class="wheel-pointer">▼</div>
          <div class="wheel-spinner">${buildWheelSVG()}</div>
        </div>
        <button class="spin-btn" type="button">
          ${canClaimDailyReward() ? "SPIN FOR FREE COINS" : "COME BACK TOMORROW"}
        </button>
      </div>
    </div>
  `;

  const spinner = container.querySelector(".wheel-spinner");
  const btn = container.querySelector(".spin-btn");

  btn.addEventListener("click", () => {
    if (!canClaimDailyReward() || btn.disabled) return;
    btn.disabled = true;

    const winningIndex = pickWeightedSegment();
    const targetSegmentCenter = winningIndex * SEGMENT_ANGLE + SEGMENT_ANGLE / 2;
    // Spin several full rotations, then land the winning segment under the
    // pointer (pointer is fixed at top / 0deg, segment 0 starts at 0deg).
    const fullSpins = 5 * 360;
    const finalRotation = fullSpins + (360 - targetSegmentCenter);

    spinner.style.transition = "transform 4.2s cubic-bezier(0.12, 0.72, 0.15, 1)";
    spinner.style.transform = `rotate(${finalRotation}deg)`;

    spinner.addEventListener(
      "transitionend",
      () => {
        const reward = SEGMENTS[winningIndex];
        claimDailyReward(reward.coins);
        btn.textContent = `+${reward.coins.toLocaleString()} COINS!`;
        setTimeout(() => {
          const card = container.querySelector(".daily-reward-card");
          card.classList.add("is-claimed");
          btn.textContent = "COME BACK TOMORROW";
          if (onClaimed) onClaimed(reward.coins);
        }, 1800);
      },
      { once: true }
    );
  });
}
