(() => {
  'use strict';

  const wrapper = document.querySelector('.gtranslate_wrapper');
  if (!wrapper) return;

  wrapper.classList.add('notranslate');
  wrapper.setAttribute('translate', 'no');

  window.gtranslateSettings = {
    default_language: 'en',
    languages: [
      'ar', 'bg', 'bn', 'cs', 'de', 'el', 'en', 'es', 'fa', 'fi', 'tl', 'fr',
      'he', 'hi', 'hr', 'hu', 'id', 'it', 'ja', 'ko', 'pl', 'ps', 'pt', 'ro',
      'ru', 'sk', 'sr', 'sv', 'th', 'tr', 'uk', 'ur', 'vi', 'zh-CN', 'zh-TW'
    ],
    native_language_names: true,
    detect_browser_language: true,
    wrapper_selector: '.gtranslate_wrapper',
    switcher_horizontal_position: 'right',
    switcher_vertical_position: 'bottom',
    float_switcher_open_direction: 'top'
  };

  const labels = document.createElement('script');
  labels.src = new URL('translation-selector.js', document.currentScript.src).href;
  document.head.append(labels);

  const widget = document.createElement('script');
  widget.src = 'https://cdn.gtranslate.net/widgets/latest/float.js';
  widget.addEventListener('load', () => {
    wrapper.dispatchEvent(new Event('focusin', { bubbles: true }));
  }, { once: true });
  document.head.append(widget);
})();
