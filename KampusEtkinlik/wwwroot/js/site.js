// Scroll fonksiyonu - belirli miktar kadar
window.scrollElement = (element, amount) => {
    if (element) {
        element.scrollLeft += amount;
    }
};

// Scroll fonksiyonu - tam genişlik kadar (4 kart)
window.scrollElementByWidth = (element, forward = false) => {
    if (element) {
        const scrollAmount = element.clientWidth;
        element.scrollLeft += forward ? scrollAmount : -scrollAmount;
    }
};
