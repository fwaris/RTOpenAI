namespace WebFlows

module Scripts =
  
  ///Flag to escape some characters in JavaSript before it evaluation in browser
  let mutable ESCAPE_EVAL_JS = true //set false for Android and Edge
  
  ///Flag to put quotes around JavaScript returned from evaluation
  let mutable WRAP_EVALED_JS_IN_QUOTES = true //set false for Edge (android?)
 

  let gotoUrlRaw = """
function goToUrl(targetUrl) {
  if (window.location.href !== targetUrl) {
    window.location.href = targetUrl;
  }
}
"""
  
  let findElementRaw = """
function getByAnyClass(classes) {
  const selector = classes.map(c => `.${c}`).join(", ");
  return Array.from(document.querySelector(selector));
}

function findByXPath(xpath) {
  const snapshot = document.evaluate(
    xpath,
    document,
    null,
    XPathResult.ORDERED_NODE_SNAPSHOT_TYPE,
    null
  );
  const results = [];
  for (let i = 0; i < snapshot.snapshotLength; i++) {
    results.push(snapshot.snapshotItem(i));
  }
  return results.length > 0 ? results[0] : null;
}

function findByInnerTextContains(substring) {
  return Array.from(document.querySelectorAll("*"))
    .filter(el =>
      el.innerText && 
      el.innerText.toLowerCase().includes(substring.toLowerCase())
  );
}

function intersect(setA, setB)  {
  return new Set([...setA].filter(x => setB.has(x)));
}

function findElemById(id) {
    const elems = Array.from(document.querySelectorAll(`#${id}`));
    const visible = elems.filter(el => {
      const rect = el.getBoundingClientRect();
      return rect.width > 0 && rect.height > 0;
    });
    return visible.length > 0 ? visible[0] : document.getElementById(id);
}

function findElement(params) {  
  if (params.elementId) return findElemById(params.elementId);
  if (params.path) return document.querySelector(params.path);
  if (params.xpath) return findByXPath(params.xpath);

  const byAria = params.aria_label
    ? new Set(
        Array.from(document.querySelectorAll(`[aria-label="${params.aria_label}"]`))
      )
    : null;

  const byInnerText = params.inner_text
    ? new Set(
        findByInnerTextContains(params.inner_text)
      )
    : null;

  const byCss = params.classList && params.classList.length > 0
    ? new Set(
        getByAnyClass(params.classList)
      )
    : null;

  const sets = [byAria, byInnerText, byCss].filter(s => s !== null);
  if (sets.length === 0) return null;

  const intersection = sets.reduce(intersect);
  const visible = [...intersection].filter(el => {
    const rect = el.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0;
  });

  const first = visible.length > 0 ? visible[0] : null;

  return first;
}
"""
  
  let  clickElementRaw = """
function clickElement(params) {
  console.log("about to click")
  const elem = findElement(params);
  if (elem) {
    elem.click();
    console.log(`clicked: ${JSON.stringify(params)}`);
  } else {
    console.log(`element not found: ${JSON.stringify(params)}`);
  }
}
"""
  
  let getElementValueRaw = """
function getElementValue(params) {
  console.log("extracting value")
  const elem = findElement(params);
  if (elem) {
    console.log(`found: ${JSON.stringify(params)}`);
    let v = elem?.innerText?.trim() || null;
    v = v ? v : elem?.getAttribute("value")?.trim() || null;
    console.log(`value: "${v}"`)
    return v;
  } else {
    console.log(`not found: ${JSON.stringify(params)}`);
    return null;
  }
}
"""
  
  let private findClickablesRaw = """
function findClickables(params) {

  const selectors = [
    // Native interactive elements
    'button',
    'a[href]',
    
    //'input:not([type="hidden"])',
    //'select',
    //'textarea',

    // ARIA / generic
    '[role="button"]',
    '[role="link"]',
    '[onclick]',
    '[tabindex]:not([tabindex="-1"])',

    // Ionic
    'ion-button',
    'ion-item[button]',
    'ion-card[button]',
    'ion-fab-button',

    // Material UI / Angular Material
    'mat-button',
    'mat-icon-button',
    'mat-raised-button',

    // Other common patterns
    '[data-clickable]',    // custom marker some apps use
    '[data-action]',       // generic "action" attributes
  ];

  const candidates = Array.from(document.querySelectorAll(selectors));

  const clickables = candidates.filter(el => {
    if (params.elementId && el.id !== params.elementId) return false;

    if (params.aria_label && el.getAttribute('aria-label') !== params.aria_label) return false

    if (params.inner_text && el.innerText?.trim() !== params.inner_text) return false
  
    if (params.classList && params.classList.length > 0) {
      const hasClass = params.classList.some(cls => el.classList.contains(cls));
      if (!hasClass) return false;
    }

    return true;
  })
  return clickables;
}
"""
  
  let findBoundingBoxesRaw="""
function findBoundingBoxes(params) {
  const e1 = findElement(params);
  const elems = e1 ? [e1] : [];
  const clickables = elems.map(el => {
    const rect = el.getBoundingClientRect();
    return {
      aria_label: el.getAttribute('aria-label') || null,
      inner_text : el.innerText?.trim() || null,
      tag: el.tagName.toLowerCase(),
      id: el.id || null,
      classList: Array.from(el.classList),
      role: el.getAttribute('role') || null,
      path: el.tagName.toLowerCase(), // Simplified path
      x: rect.left,
      y: rect.top,
      width: rect.width,
      height: rect.height,
      zIndex: el.zIndex
    };
  });
           
  return JSON.stringify({
    zoom: window.devicePixelRatio,
    scrollX: window.scrollX,
    scrollY: window.scrollY,
    viewportWidth: window.innerWidth,
    viewportHeight: window.innerHeight,
    documentWidth: document.documentElement.scrollWidth,
    documentHeight: document.documentElement.scrollHeight,
    clickables: clickables
  });
}
"""
 
  let private clickablesRaw = """(function() {
  const zoom = window.devicePixelRatio;
  const scrollX = window.scrollX;
  const scrollY = window.scrollY;
  const viewportWidth = window.innerWidth;
  const viewportHeight = window.innerHeight;
  const documentWidth = document.documentElement.scrollWidth;
  const documentHeight = document.documentElement.scrollHeight;

  const selectors = [
    // Native interactive elements
    'button',
    'a[href]',

    //'input:not([type="hidden"])',
    //'select',
    //'textarea',

    // ARIA / generic
    '[role="button"]',
    '[role="link"]',
    '[onclick]',
    '[tabindex]:not([tabindex="-1"])',

    // Ionic
    'ion-button',
    'ion-item[button]',
    'ion-card[button]',
    'ion-fab-button',

    // Material UI / Angular Material
    'mat-button',
    'mat-icon-button',
    'mat-raised-button',

    // Other common patterns
    '[data-clickable]',    // custom marker some apps use
    '[data-action]',       // generic "action" attributes
  ];

  const elements = document.querySelectorAll(selectors.join(','));
  const clickables = [];

  elements.forEach(el => {
    const rect = el.getBoundingClientRect();

    const aria_label = el.getAttribute('aria-label') || null;
    const inner_text = el.innerText?.trim() || null;
    const id = el.id || null;
    const classList = Array.from(el.classList);
    const role = el.getAttribute('role') || null;
    const tag = el.tagName.toLowerCase();
    const path = [];

    let node = el;
    while (node && node.nodeType === 1) {
      let desc = node.tagName.toLowerCase();
      if (node.id) {
        desc += `#${node.id}`;
      } else if (node.classList.length > 0) {
        desc += '.' + Array.from(node.classList).join('.');
      }
      path.unshift(desc);
      node = node.parentElement;
    }
    
    clickables.push({
      aria_label,
      inner_text,
      tag,
      id,
      classList,
      role,
      path: path.join(' > '),
      x: (rect.left + scrollX) * zoom,
      y: (rect.top + scrollY) * zoom,
      width: rect.width * zoom,
      height: rect.height * zoom,
      zIndex: el.zIndex
    });
  });

  return JSON.stringify({
    zoom,
    scrollX,
    scrollY,
    viewportWidth,
    viewportHeight,
    documentWidth,
    documentHeight,
    clickables
  });
})();
"""
  let private escapeScript_ (rawScript:string) =
  
    rawScript
      .Replace("\\", "\\\\")     // Escape backslashes first
      .Replace("\"", "\\\"")     // Escape double quotes
      .Replace("\r", "")         // Remove carriage returns
      .Replace("\n", "\\n");     // Escape newlines
      
  let escapeSomeChars rawScript =
    if not ESCAPE_EVAL_JS then
      rawScript
    else
      escapeScript_ rawScript
      
  let quoteWrap rawScript =
    if not WRAP_EVALED_JS_IN_QUOTES then
      rawScript
    else
      "\"" + rawScript + "\""

  let clickables = lazy(escapeSomeChars clickablesRaw)
  
  let findClickables = lazy(escapeSomeChars findClickablesRaw)
  
  let findBoundingBoxes = lazy(escapeSomeChars findBoundingBoxesRaw)
  
  let  findElement = lazy(escapeSomeChars findElementRaw)
  
  let clickElement = lazy(escapeSomeChars clickElementRaw)
  
  let getElementValue = lazy(escapeSomeChars getElementValueRaw)

  let gotoUrl = lazy(escapeSomeChars gotoUrlRaw)