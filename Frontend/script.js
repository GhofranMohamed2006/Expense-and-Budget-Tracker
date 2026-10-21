// ###### Landing Page ######
document.addEventListener("DOMContentLoaded", function () {
  const hamburger = document.getElementById("hamburger");
  const navContent = document.getElementById("nav-content");
  const navLinks = document.querySelectorAll(".sections a");

  // لو العناصر موجودة في الصفحة
  if (hamburger && navContent) {
    hamburger.addEventListener("click", function () {
      navContent.classList.toggle("active");
      const isOpen = navContent.classList.contains("active");
      hamburger.setAttribute("aria-expanded", isOpen);
    });

    navLinks.forEach(function (link) {
      link.addEventListener("click", function () {
        navContent.classList.remove("active");
        hamburger.setAttribute("aria-expanded", "false");
      });
    });
  }
});