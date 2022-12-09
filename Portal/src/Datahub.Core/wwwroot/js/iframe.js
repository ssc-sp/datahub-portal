function ResizeIFrame(id) {

	let iframe = document.querySelector(id);

	window.addEventListener('message', function (e) {
		// message that was passed from iframe page
		let message = e.data;

		iframe.style.height = message.height + 'px';
		iframe.style.width = message.width + 'px';
	}, false);

}


