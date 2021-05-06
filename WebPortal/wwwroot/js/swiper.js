window.initializeSwiper = (container, page) => {
    const defaultOptions = {
        slidesPerView: 1,
        speed: 1500,
        effect: 'slide',
        loop: true,
        centeredSlides: true,
        autoplay: {
            delay: 6000,
            disableOnInteraction: false,
        },
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
        },
        navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
        },
        watchSlidesVisibility: true,
        onSlideNextStart: function (swiperObj) {
            swiperObj.container[0].classList.toggle('alt-bg');
        }
    };

    const swiper = new Swiper('.swiper-container', defaultOptions)
};

