document.querySelectorAll('.nav-links a').forEach(function(link) {
    if (window.location.pathname.toLowerCase().includes(
        link.getAttribute('href').toLowerCase().split('/').pop()
    )) {
        link.style.background = 'rgba(255, 203, 5, 0.12)';
        link.style.color = '#ffcb05';
    }
});
