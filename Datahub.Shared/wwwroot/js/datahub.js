/** ********************************************** **
	DataHub Javascript

*************************************************** **/

/* Initialize Swiper */
/*var swiper = new Swiper('.swiper-container', {
  slidesPerView: 1,
  speed: 1500,
  effect: 'slide',
  loop: true,
  centeredSlides: true,
  autoplay: {
    delay: 6000,
    disableOnInteraction: true,
  },
  pagination: {
    el: '.swiper-pagination',
    clickable: true,
  },
  navigation: {
    nextEl: '.swiper-button-next',
    prevEl: '.swiper-button-prev',
  },
});*/


window.onload = function() {

  var lastIndex = 0;

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
    onSlideNextStart: function(swiperObj) {
      if ( swiperObj.activeIndex < lastIndex ) {
        swiperObj.container[0].classList.toggle('alt-bg');
      }
      lastIndex = swiperObj.activeIndex;
    }
  };

  const swiper = new Swiper('.swiper-container', defaultOptions)
}


