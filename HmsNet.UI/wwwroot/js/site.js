$(document).ready(function () {
    // Navbar scroll effect
    $(window).scroll(function () {
        if ($(window).scrollTop() > 50) {
            $('.navbar').addClass('scrolled');
        } else {
            $('.navbar').removeClass('scrolled');
        }
    });

    // Highlight active nav link based on exact URL match
    const currentPath = window.location.pathname;
    $('.nav-link').each(function () {
        const linkPath = $(this).attr('href');
        if (linkPath && currentPath === linkPath) {
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });
});