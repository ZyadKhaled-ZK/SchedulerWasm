const CACHE_NAME = 'scheduler-v1';
const ASSETS = [
  './',
  './index.html',
  './css/bootstrap.min.css',
  './css/bootstrap-icons.min.css',
  './css/app.css',
  './css/bootstrap.bundle.min.js',
  './_framework/blazor.webassembly.js',
  './manifest.json'
];

self.addEventListener('install', e => {
  e.waitUntil(caches.open(CACHE_NAME).then(c => c.addAll(ASSETS)));
  self.skipWaiting();
});

self.addEventListener('activate', e => {
  e.waitUntil(caches.keys().then(ks => Promise.all(ks.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))));
  self.clients.claim();
});

self.addEventListener('fetch', e => {
  e.respondWith(caches.match(e.request).then(r => r || fetch(e.request)));
});
