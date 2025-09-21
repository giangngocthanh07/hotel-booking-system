window.initMultiCardCarousel = function () {
    const carousel = document.querySelector("#multiCardCarousel");
    const wrapper = document.querySelector("#carouselWrapper");
    const prevBtn = document.querySelector("#prevBtn");
    const nextBtn = document.querySelector("#nextBtn");

    if (!carousel || !wrapper || !prevBtn || !nextBtn) return;

    let cards = Array.from(carousel.querySelectorAll(".destination-card"));
    if (cards.length === 0) return;

    let gap = parseInt(window.getComputedStyle(carousel).gap) || 0;
    let cardWidth = cards[0].offsetWidth + gap;
    let visibleCards = Math.floor(wrapper.offsetWidth / cardWidth);

    let currentIndex = visibleCards;
    let isTransitioning = false;

    // Function to create clones (first & last)
    const createClones = () => {
        // Remove old clones
        carousel.querySelectorAll(".clone").forEach(c => c.remove());

        // Clone first & last
        for (let i = 0; i < visibleCards; i++) {
            if (!cards[i] || !cards[cards.length - 1 - i]) continue;

            const firstClone = cards[i].cloneNode(true);
            firstClone.classList.add("clone");
            carousel.appendChild(firstClone);

            const lastClone = cards[cards.length - 1 - i].cloneNode(true);
            lastClone.classList.add("clone");
            carousel.insertBefore(lastClone, carousel.firstChild);
        }
    };

    const updateCarousel = (transition = true) => {
        carousel.style.transition = transition ? "transform 0.5s ease" : "none";
        const offset = (wrapper.offsetWidth - cardWidth) / 2; // căn giữa
        carousel.style.transform = `translateX(${-currentIndex * cardWidth + offset}px)`;
    };

    const next = () => {
        if (isTransitioning) return;
        isTransitioning = true;
        currentIndex++;
        updateCarousel();

        setTimeout(() => {
            if (currentIndex >= cards.length + visibleCards) currentIndex = visibleCards;
            updateCarousel(false);
            isTransitioning = false;
        }, 500);
    };

    const prev = () => {
        if (isTransitioning) return;
        isTransitioning = true;
        currentIndex--;
        updateCarousel();

        setTimeout(() => {
            if (currentIndex < visibleCards) currentIndex = cards.length + visibleCards - 1;
            updateCarousel(false);
            isTransitioning = false;
        }, 500);
    };

    // Initialize clones and carousel
    createClones();
    updateCarousel(false);

    // Event listeners
    nextBtn.addEventListener("click", next);
    prevBtn.addEventListener("click", prev);

    // Auto slide
    let autoSlide = setInterval(next, 2000);
    wrapper.addEventListener("mouseenter", () => clearInterval(autoSlide));
    wrapper.addEventListener("mouseleave", () => autoSlide = setInterval(next, 3000));

    // Responsive handling
    window.addEventListener("resize", () => {
        cards = Array.from(carousel.querySelectorAll(".destination-card:not(.clone)"));
        if (cards.length === 0) return;

        gap = parseInt(window.getComputedStyle(carousel).gap) || 0;
        cardWidth = cards[0].offsetWidth + gap;
        visibleCards = Math.floor(wrapper.offsetWidth / cardWidth);

        createClones();
        currentIndex = visibleCards;
        updateCarousel(false);
    });
};

document.addEventListener("DOMContentLoaded", function () {
    flatpickr("#dateRange", {
      mode: "range",
      minDate: "today",
      dateFormat: "D, d \\t\\há\\ng m", // hiển thị T5, 18 tháng 9
      locale: "vn",
    });
  });
