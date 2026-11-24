// ===================================================================
// KOTOBA ODYSSEY - PROCEDURAL WEB AUDIO SYNTHESIZER
// Generates retro SFX, typewriter clicks & chimes via Web Audio API
// ===================================================================

let audioCtx = null;
let soundEnabled = true;

function initAudio() {
  if (!audioCtx) {
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (AudioContextClass) {
      audioCtx = new AudioContextClass();
    }
  }
  if (audioCtx && audioCtx.state === 'suspended') {
    audioCtx.resume();
  }
}

function getAudioCtx() {
  initAudio();
  return audioCtx;
}

function playTypewriterClick() {
  if (!soundEnabled) return;
  try {
    initAudio();
    if (!audioCtx) return;
    const osc = audioCtx.createOscillator();
    const gain = audioCtx.createGain();
    const now = audioCtx.currentTime;
    osc.type = 'triangle';
    osc.frequency.setValueAtTime(800 + Math.random() * 300, now);
    osc.frequency.exponentialRampToValueAtTime(150, now + 0.04);
    gain.gain.setValueAtTime(0.05, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.04);
    osc.connect(gain);
    gain.connect(audioCtx.destination);
    osc.start(now);
    osc.stop(now + 0.04);
  } catch (e) {}
}

function playSuccessChime() {
  if (!soundEnabled) return;
  try {
    initAudio();
    if (!audioCtx) return;
    const now = audioCtx.currentTime;
    [659.25, 987.77].forEach((freq, i) => { // E5 -> B5
      const osc = audioCtx.createOscillator();
      const gain = audioCtx.createGain();
      const t = now + i * 0.1;
      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, t);
      gain.gain.setValueAtTime(0.08, t);
      gain.gain.exponentialRampToValueAtTime(0.001, t + 0.35);
      osc.connect(gain);
      gain.connect(audioCtx.destination);
      osc.start(t);
      osc.stop(t + 0.35);
    });
  } catch (e) {}
}

function playCardFlip() {
  if (!soundEnabled) return;
  try {
    initAudio();
    if (!audioCtx) return;
    const now = audioCtx.currentTime;
    const osc = audioCtx.createOscillator();
    const gain = audioCtx.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(320, now);
    osc.frequency.exponentialRampToValueAtTime(480, now + 0.08);
    gain.gain.setValueAtTime(0.05, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.08);
    osc.connect(gain);
    gain.connect(audioCtx.destination);
    osc.start(now);
    osc.stop(now + 0.08);
  } catch (e) {}
}

function toggleAudioFx() {
  soundEnabled = !soundEnabled;
  initAudio();
  const btn = document.getElementById('btnAudioToggle');
  if (btn) {
    btn.innerHTML = soundEnabled ? '🔊 Âm thanh: Bật' : '🔇 Âm thanh: Tắt';
    btn.classList.toggle('muted', !soundEnabled);
  }
}
