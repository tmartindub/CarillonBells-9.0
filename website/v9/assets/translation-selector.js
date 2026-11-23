(() => {
  'use strict';

  const wrapper = document.querySelector('.gtranslate_wrapper');
  if (!wrapper) return;

  const aliases = { iw: 'he', jw: 'jv', tl: 'fil' };
  const englishOverrides = { 'zh-CN': 'Chinese, Simplified', 'zh-TW': 'Chinese, Traditional' };
  const englishNames = typeof Intl.DisplayNames === 'function'
    ? new Intl.DisplayNames(['en'], { type: 'language' })
    : null;

  function englishName(code) {
    if (englishOverrides[code]) return englishOverrides[code];
    try {
      return englishNames?.of(aliases[code] || code) || code;
    } catch {
      return code;
    }
  }

  function updateLanguageList() {
    wrapper.querySelectorAll('.gt_options').forEach(list => {
      const links = [...list.children].filter(item => item.matches('a[data-gt-lang]'));

      links.forEach(link => {
        if (link.dataset.bilingualLabel) return;

        const english = englishName(link.dataset.gtLang);
        const native = link.textContent.trim();
        const label = native && native.toLocaleLowerCase('en') !== english.toLocaleLowerCase('en')
          ? `${english} (${native})`
          : english;
        const flag = link.querySelector('img');

        link.dataset.bilingualLabel = label;
        link.setAttribute('aria-label', label);
        link.replaceChildren(...(flag ? [flag] : []), document.createTextNode(label));
      });

      const sorted = [...links].sort((first, second) =>
        first.dataset.bilingualLabel.localeCompare(second.dataset.bilingualLabel, 'en'));
      if (sorted.some((link, index) => link !== links[index])) list.append(...sorted);
    });
  }

  new MutationObserver(updateLanguageList).observe(wrapper, { childList: true, subtree: true });
  updateLanguageList();
})();
