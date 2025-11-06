(function () {
    const html = document.documentElement;
    const selector = document.getElementById('themeSelector');
    const storageKey = 'theme';

    // Geçerli temayı uygula
    function applyTheme(themeKey) {
        //drawStaticPacman();
        // Eskilerini temizle
        html.classList.forEach(c => {
            if (c.startsWith('theme-')) html.classList.remove(c);
        });
        // Yeni temayı ekle
        html.classList.add('theme-' + themeKey);
        localStorage.setItem(storageKey, themeKey);
    }

    document.addEventListener('DOMContentLoaded', () => {
        // 1) Kaydedilmiş tema varsa uygula, yoksa light
        const saved = localStorage.getItem(storageKey) || 'light';
        //drawStaticPacman();
        applyTheme(saved);

        // 2) Seçicideki değeri güncelle
        if (selector) selector.value = saved;

        // 3) Tema değişince olayı yakala
        selector?.addEventListener('change', e => {
            applyTheme(e.target.value);
        });
    });
})();

/** Statik Pac-Man çizer (ağzı sabit 45° açık) */
//function drawStaticPacman(canvas) {
//    const size = getNum('--pacman-size');
//    const fill = getCSS('--pacman-color');
//    const bg = getCSS('--pacman-bg');
//    canvas.width = canvas.height = size;
//    const ctx = canvas.getContext('2d');
//    ctx.clearRect(0, 0, size, size);
//    ctx.fillStyle = fill;
//    ctx.beginPath();
//    ctx.moveTo(size / 2, size / 2);
//    ctx.arc(size / 2, size / 2, size / 2, Math.PI / 4, -Math.PI / 4, false);
//    ctx.closePath();
//    ctx.fill();
//}

/** Animasyonu başlatır: düz çizgide ilerleyen Pac-Man + toplar */
function startPacmanAnimation(canvas) {
    const size = getNum('--pacman-size');
    const fill = getCSS('--pacman-color');
    const bg = getCSS('--pacman-bg');
    const speed = getNum('--pacman-speed') / 1000;       // px / ms
    const mouthSpeed = getNum('--pacman-mouth-speed');   // derece / frame
    const pelletSize = getNum('--pacman-pellet-size');
    const pelletSpacing = getNum('--pacman-pellet-spacing');

    const ctx = canvas.getContext('2d');
    let x = -size;            // Pac-Man başlangıç x’i (canvas solunun solunda)
    let mouth = 0;            // şu anki ağız açısı (0–45 derece)
    let opening = true;       // açıyor mu, kapatıyor mu
    const pellets = [];
    // Pellets: canvas boyunca her pelletSpacing px’de bir top
    for (let px = size / 2 + pelletSpacing; px < size * 3; px += pelletSpacing) {
        pellets.push({ x: px, eaten: false });
    }

    const startTime = performance.now();
    function frame(now) {
        const elapsed = now - startTime;
        // Süre dolduysa animasyon bitsin
        if (elapsed > 4000) {
            //drawStaticPacman(canvas);
            return;
        }

        // Pac-Man’i ilerlet
        x += speed * (now - (frame.lastTime || now));
        frame.lastTime = now;

        // Ağız aç/kapa
        if (opening) {
            mouth += mouthSpeed;
            if (mouth >= 45) opening = false;
        } else {
            mouth -= mouthSpeed;
            if (mouth <= 0) opening = true;
        }

        // Çizimi yap
        ctx.clearRect(0, 0, size, size);
        // 1) Pellet’leri çiz & yeme
        ctx.fillStyle = fill;
        pellets.forEach(p => {
            if (!p.eaten && x + size / 2 > p.x) {
                p.eaten = true;
            }
            if (!p.eaten) {
                ctx.beginPath();
                ctx.arc(p.x - x, size / 2, pelletSize / 2, 0, 2 * Math.PI);
                ctx.fill();
            }
        });

        // 2) Pac-Man’i çiz
        const rad = mouth * Math.PI / 180;
        ctx.beginPath();
        ctx.moveTo(size / 2, size / 2);
        ctx.arc(size / 2, size / 2, size / 2, rad, -rad, false);
        ctx.closePath();
        ctx.fill();

        requestAnimationFrame(frame);
    }

    requestAnimationFrame(frame);
}

/** CSS’tan sayısal değişken oku */
function getNum(prop) {
    return parseFloat(getComputedStyle(document.documentElement).getPropertyValue(prop)) || 0;
}
/** CSS’tan renk vb. oku */
function getCSS(prop) {
    return getComputedStyle(document.documentElement).getPropertyValue(prop).trim() || '#000';
}

document.addEventListener('keydown', e => {
    if (e.key === 'x' || e.key === 'X') {
        // Tüm canvas’lara uygula
        document.querySelectorAll('.pacman-canvas').forEach(canvas => {
            startPacmanAnimation(canvas);
        });
    }
});