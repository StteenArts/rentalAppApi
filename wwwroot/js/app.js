// ── Atelier Dwelling – shared app module ──────────────────────────────────────
const API = '/api';

function getToken() { return localStorage.getItem('token'); }
function getUser() { const u = localStorage.getItem('user'); return u ? JSON.parse(u) : null; }
function isLoggedIn() { return !!getToken(); }
function isOwner() { const u = getUser(); return u?.role === 'Owner'; }
function isAdmin() { const u = getUser(); return u?.role === 'Admin'; }

async function apiFetch(method, path, body, isFormData = false) {
    const headers = {};
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;
    if (!isFormData) headers['Content-Type'] = 'application/json';
    const opts = { method, headers };
    if (body) opts.body = isFormData ? body : JSON.stringify(body);
    const res = await fetch(API + path, opts);
    if (!res.ok) {
        let err;
        try { err = await res.json(); } catch { err = { title: res.statusText }; }
        throw Object.assign(new Error(err.detail || err.title || 'Error'), { status: res.status, data: err });
    }
    const ct = res.headers.get('content-type') || '';
    if (ct.includes('json')) return res.json();
    if (ct.includes('spreadsheet') || ct.includes('octet-stream')) return res.blob();
    return res.text();
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login.html';
}

function showToast(message, isError = false) {
    const t = document.createElement('div');
    t.className = `fixed top-6 left-1/2 -translate-x-1/2 z-[100] px-6 py-3 rounded text-sm font-ui-label ${isError ? 'bg-error text-on-error' : 'bg-secondary text-on-secondary'} shadow-lg`;
    t.textContent = message;
    document.body.appendChild(t);
    setTimeout(() => t.remove(), 3500);
}

function buildNav(activePage) {
    const user = getUser();
    const loggedIn = isLoggedIn();
    const owner = isOwner() || isAdmin();

    const guestLinks = `
        <a href="/index.html" class="nav-link ${activePage==='explore'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">Explore</a>
        <a href="/bookings.html" class="nav-link ${activePage==='bookings'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">My Bookings</a>
        <a href="/wishlist.html" class="nav-link ${activePage==='wishlist'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">Wishlist</a>`;

    const ownerLinks = `
        <a href="/index.html" class="nav-link ${activePage==='explore'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">Explore</a>
        <a href="/my-properties.html" class="nav-link ${activePage==='my-properties'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">My Properties</a>
        <a href="/owner-reservations.html" class="nav-link ${activePage==='owner-reservations'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">Bookings</a>
        <a href="/dashboard.html" class="nav-link ${activePage==='dashboard'?'text-primary border-b border-primary pb-1':'text-on-surface-variant hover:text-primary'} font-ui-label text-ui-label transition-colors duration-300">Dashboard</a>`;

    const trailing = loggedIn ? `
        <a href="/notifications.html" class="relative text-on-surface-variant hover:text-primary" id="notif-btn">
            <span class="material-symbols-outlined">notifications</span>
        </a>
        <a href="/profile.html" class="text-on-surface-variant hover:text-primary">
            <span class="material-symbols-outlined">account_circle</span>
        </a>
        <button onclick="logout()" class="font-ui-label text-ui-label text-on-surface-variant hover:text-primary transition-colors">Logout</button>
    ` : `
        <a href="/login.html" class="font-ui-label text-ui-label text-on-surface-variant hover:text-primary transition-colors">Login</a>
        <a href="/register.html" class="bg-primary-container text-on-primary px-5 py-2 font-ui-label text-ui-label hover:opacity-90 transition-opacity">Register</a>`;

    return `
    <header class="bg-surface sticky top-0 z-50 border-b border-outline-variant">
      <nav class="flex justify-between items-center w-full px-margin-desktop py-6 max-w-container-max mx-auto">
        <div class="flex items-center gap-12">
          <a href="/index.html" class="font-headline-md text-headline-md text-primary tracking-tight">Atelier Dwelling</a>
          <div class="hidden md:flex items-center gap-8">${owner ? ownerLinks : guestLinks}</div>
        </div>
        <div class="flex items-center gap-4">${trailing}</div>
      </nav>
    </header>`;
}

const sharedFooter = `
<footer class="bg-surface-container-low border-t border-outline-variant">
  <div class="flex flex-col md:flex-row justify-between items-start w-full px-margin-desktop py-20 max-w-container-max mx-auto gap-gutter">
    <div class="flex flex-col gap-6 max-w-sm">
      <span class="font-headline-sm text-headline-sm text-on-surface">Atelier Dwelling</span>
      <p class="font-body-md text-body-md text-tertiary">Curating architectural intentionality for the discerning traveler.</p>
      <p class="font-label-caps text-label-caps text-tertiary">© 2025 Atelier Dwelling.</p>
    </div>
    <div class="grid grid-cols-2 md:grid-cols-3 gap-12">
      <div class="flex flex-col gap-4"><span class="font-label-caps text-label-caps text-on-surface-variant">Navigate</span>
        <a href="/index.html" class="text-tertiary font-body-md hover:text-primary transition-colors">Explore</a>
      </div>
      <div class="flex flex-col gap-4"><span class="font-label-caps text-label-caps text-on-surface-variant">Legal</span>
        <a href="#" class="text-tertiary font-body-md hover:text-primary transition-colors">Privacy</a>
        <a href="#" class="text-tertiary font-body-md hover:text-primary transition-colors">Terms</a>
      </div>
    </div>
  </div>
</footer>`;

function getPropertyImage(property) {
    if (property.images && property.images.length > 0) {
        const main = property.images.find(i => i.isMain) || property.images[0];
        return main.url;
    }
    return property.imageUrl || 'https://images.unsplash.com/photo-1449844908441-8829872d2607?w=800&q=80';
}

function formatDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function nightsBetween(checkIn, checkOut) {
    const d1 = new Date(checkIn);
    const d2 = new Date(checkOut);
    return Math.max(0, Math.round((d2 - d1) / (1000 * 60 * 60 * 24)));
}
