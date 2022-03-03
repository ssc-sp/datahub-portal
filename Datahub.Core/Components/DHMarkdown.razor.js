export function styleCodeblocks(element){
    if(hljs){
        element.querySelectorAll('pre code')
            .forEach(el => {
                hljs.highlightElement(el);
                console.log("hi")
                
                let copyIcon = document.createElement('i');
                let iconClasses = ['fas', 'fa-copy', 'codeblock-copy-icon'];
                let iconAnimatingClasses = ['fa-copy', 'fa-check', 'animating'];
                copyIcon.classList.add(...iconClasses);
                
                let animating = false;
                
                copyIcon.addEventListener('click', () => {
                    if(animating) {
                        return;
                    }
                    
                    console.log("copying", el.textContent)
                    toggleClassList(copyIcon, iconAnimatingClasses);
                    
                    navigator.clipboard.writeText(el.textContent)
                        .then(() => {
                            
                            animating = true;
                            setTimeout(() => {
                                toggleClassList(copyIcon, iconAnimatingClasses);
                                animating = false;
                                
                            }, 1000);
                        })
                        .catch(e => console.error(e));
                });
                
                el.appendChild(copyIcon);
            });
    }
}


function toggleClassList(el, classes = []){
    classes.forEach(name => {
        el.classList.toggle(name);
    })
}