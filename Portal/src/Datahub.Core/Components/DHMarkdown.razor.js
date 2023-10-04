export function styleCodeblocks(element) {
    try {
        if (typeof hljs !== 'undefined' && element && element.querySelectorAll) {
            element.querySelectorAll('pre code:not(.copy-icon-added)')
                .forEach(el => {
                    hljs.highlightElement(el);
                    appendCopyIcon(el);
                });
        }
    } catch (error) {
        console.error(error);
    }

    try {
        if (typeof mermaid !== 'undefined' && element && element.querySelectorAll) {
            mermaid.mermaidAPI.initialize({});
            element.querySelectorAll('code.language-mermaid')
                .forEach(el => {
                    const id = "mermaid-".concat(window.crypto.randomUUID());
                    const cb = function (svgGraph) {
                        el.parentElement.innerHTML = svgGraph
                    };
                    mermaid.mermaidAPI.render(id, el.textContent, cb);
                })
        }
    } catch (error) {
        console.error(error);
    }
}

export function appendCopyIcon(element) {
    let copyIcon = document.createElement('i');
    let iconClasses = ['fas', 'fa-copy', 'codeblock-copy-icon'];
    let iconAnimatingClasses = ['fa-copy', 'fa-check', 'animating'];
    copyIcon.classList.add(...iconClasses);

    let animating = false;

    copyIcon.addEventListener('click', () => {
        if (animating) {
            return;
        }

        toggleClassList(copyIcon, iconAnimatingClasses);

        navigator.clipboard.writeText(element.textContent)
            .then(() => {

                animating = true;
                setTimeout(() => {
                    toggleClassList(copyIcon, iconAnimatingClasses);
                    animating = false;

                }, 1000);
            })
            .catch(e => console.error(e));
    });

    element.appendChild(copyIcon);
    element.classList.add('copy-icon-added');
}

function toggleClassList(el, classes = []){
    classes.forEach(name => {
        el.classList.toggle(name);
    })
}

let ruleAdded = false;

export function addStyleSheetRule(rule) {
    if (!ruleAdded) {
        var stylesheet = document.createElement("style");
        stylesheet.type = "text/css";
        document.head.appendChild(stylesheet);

        if (stylesheet.sheet) {
            if (stylesheet.sheet.insertRule) {
                stylesheet.sheet.insertRule(rule, 0);
            } else if (stylesheet.sheet.addRule) {
                stylesheet.sheet.addRule(rule.split("{")[0].trim(), rule.split("{")[1].trim(), 0);
            }
        }
    }
    ruleAdded = true;
}