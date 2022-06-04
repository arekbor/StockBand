CKEDITOR.editorConfig = function (config) {
	config.enterMode = CKEDITOR.ENTER_DIV;
	config.toolbar = [
		{ name: 'clipboard', items: ['Cut', 'Copy', 'Paste','-', 'Undo', 'Redo'] },
		{ name: 'basicstyles', items: ['Bold', 'Italic', 'Underline'] },
	];
};