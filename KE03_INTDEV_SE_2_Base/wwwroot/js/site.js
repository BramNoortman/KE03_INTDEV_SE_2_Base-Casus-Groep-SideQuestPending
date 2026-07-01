// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


//highlight a viewfolders pages for mapping purposes, basically so you easily know where you are on the site
//goal is basically to make sure you dont confuse one edit page for another
//example being by highlighting product in the sidenavbar if your editing a product or order it highlighs order in sidenavbar
document.addEventListener("DOMContentLoaded", function () {
    const path = window.location.pathname.toLowerCase();
    const links = document.querySelectorAll(".sidebar-nav a[data-controller]");

    links.forEach(link => link.classList.remove("active-nav"));
    links.forEach(link => {
        const controller = link.getAttribute("data-controller").toLowerCase();
        // dashboard exception because in the url structure it doesnt use the folder like others
        if (controller === "home") {
            if (path === "/" || path === "/home" || path.startsWith("/home/index")) {
                link.classList.add("active-nav");
            }
            return;
        }

        // all the other cases with the view folder structuring
        if (path.includes("/" + controller.toLowerCase())) {
            link.classList.add("active-nav");
        }
    });
});