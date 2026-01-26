namespace FsPlay

module Bootstrap =
  let bootstrapScript = """
(function () {
  if (window.__fsDriver) {
    return true;
  }

// ------------------------------------------------------------
// Find deepest element at (x,y), descending through shadow DOM
// Handles Android WebView quirk where elementFromPoint may use
// document coordinates instead of viewport coordinates when scrolled
// ------------------------------------------------------------
function deepElementFromPoint(x, y, root) {
    if (!root) root = document;

    let el = root.elementFromPoint(x, y);

    // On Android WebView, elementFromPoint may use document coordinates
    // when the page is scrolled. Detect this by checking if the returned
    // element's bounding rect actually contains the viewport point.
    if (el && root === document && (window.scrollX !== 0 || window.scrollY !== 0)) {
        const rect = el.getBoundingClientRect();
        const pointInRect = x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom;

        if (!pointInRect) {
            // Element doesn't contain the viewport point - likely Android quirk
            // Try with scroll-adjusted coordinates (document/page coordinates)
            const adjustedX = x + window.scrollX;
            const adjustedY = y + window.scrollY;
            const adjustedEl = root.elementFromPoint(adjustedX, adjustedY);
            if (adjustedEl) {
                el = adjustedEl;
                // For shadow DOM descent, also use adjusted coordinates
                if (el.shadowRoot) {
                    const deeper = deepElementFromPoint(adjustedX, adjustedY, el.shadowRoot);
                    return deeper || el;
                }
                return el;
            }
        }
    }

    if (!el) return null;

    if (el.shadowRoot) {
        const deeper = deepElementFromPoint(x, y, el.shadowRoot);
        return deeper || el;
    }
    return el;
}

// ------------------------------------------------------------
// Determine whether an element lives inside a shadow DOM
// ------------------------------------------------------------
function isInsideShadow(el) {
    let node = el;
    while (node) {
        if (node instanceof ShadowRoot) return true;
        node = node.parentNode;
    }
    return false;
}

// ------------------------------------------------------------
// Check if an element can actually scroll
// ------------------------------------------------------------
function canElementScroll(el) {
    if (!el) return false;
    const style = window.getComputedStyle(el);
    const overflowY = style.overflowY;
    const overflowX = style.overflowX;

    // Check if overflow allows scrolling
    const canScrollY = overflowY === "auto" || overflowY === "scroll" || overflowY === "overlay";
    const canScrollX = overflowX === "auto" || overflowX === "scroll" || overflowX === "overlay";

    // Check if there's content to scroll
    const hasVerticalScroll = el.scrollHeight > el.clientHeight;
    const hasHorizontalScroll = el.scrollWidth > el.clientWidth;

    return (canScrollY && hasVerticalScroll) || (canScrollX && hasHorizontalScroll);
}

// ------------------------------------------------------------
// Find the main scrollable container in the document
// This searches the entire DOM for elements that are likely scroll containers
// ------------------------------------------------------------
function findMainScrollContainer() {
    // Check document.scrollingElement first - but only if it can scroll
    const docScroller = document.scrollingElement;
    if (docScroller && canElementScroll(docScroller)) {
        return docScroller;
    }

    // Check body
    if (canElementScroll(document.body)) {
        return document.body;
    }

    // Search for elements that have scrollable overflow and significant scroll height
    // These are common patterns in SPAs and modern web apps
    const candidates = document.querySelectorAll('div, main, section, article, [role="main"]');
    let bestCandidate = null;
    let bestScrollHeight = 0;

    for (const el of candidates) {
        const style = window.getComputedStyle(el);
        const overflowY = style.overflowY;

        // Must have scrollable overflow
        if (overflowY !== "auto" && overflowY !== "scroll" && overflowY !== "overlay") {
            continue;
        }

        // Must have content exceeding its height
        if (el.scrollHeight <= el.clientHeight) {
            continue;
        }

        // Prefer elements with larger scroll areas (more likely to be the main container)
        const scrollableArea = el.scrollHeight - el.clientHeight;
        if (scrollableArea > bestScrollHeight) {
            bestScrollHeight = scrollableArea;
            bestCandidate = el;
        }
    }

    if (bestCandidate) {
        return bestCandidate;
    }

    // Last resort: find any element with scrollHeight > clientHeight + threshold
    // even if overflow is "visible" - some frameworks handle scroll differently
    const threshold = 100;
    for (const el of candidates) {
        if (el.scrollHeight > el.clientHeight + threshold) {
            // Try to scroll it directly to see if it works
            const before = el.scrollTop;
            el.scrollTop = before + 1;
            if (el.scrollTop !== before) {
                el.scrollTop = before; // Reset
                return el;
            }
        }
    }

    // Absolute fallback
    return document.scrollingElement || document.documentElement;
}

// ------------------------------------------------------------
// Find nearest scrollable ancestor (including shadow hosts)
// Special case: Ionic ion-content → shadowRoot → .inner-scroll
// ------------------------------------------------------------
function findScrollableParent(el) {
    let node = el;

    while (node && node !== document) {

        // Ionic special case
        if (node.tagName === "ION-CONTENT" && node.shadowRoot) {
            const inner = node.shadowRoot.querySelector(".inner-scroll");
            if (inner && canElementScroll(inner)) return inner;
        }

        // Generic scrollable container check
        if (canElementScroll(node)) {
            return node;
        }

        // Shadow DOM boundary
        if (node.parentNode instanceof ShadowRoot) {
            node = node.parentNode.host;
        } else {
            node = node.parentNode;
        }
    }

    // Fallback: search for the main scroll container in the document
    return findMainScrollContainer();
}

// ------------------------------------------------------------
// Simulate wheel event on an element
// ------------------------------------------------------------
function simulateWheelEvent(target, deltaX, deltaY) {
  if (!target) return false;

  const wheelEvent = new WheelEvent('wheel', {
    deltaX: deltaX,
    deltaY: deltaY,
    deltaMode: 0, // DOM_DELTA_PIXEL
    bubbles: true,
    cancelable: true,
    view: window
  });

  target.dispatchEvent(wheelEvent);
  return true;
}

// ------------------------------------------------------------
// Scroll a specific target element
// ------------------------------------------------------------
function performScroll(target, scrollX, scrollY) {
  if (!target) return "no-scroll-target";

  const normalizeDelta = (value) => {
    if (typeof value === "number") return Number.isFinite(value) ? value : 0;
    if (typeof value === "string") {
      const parsed = parseFloat(value);
      return Number.isFinite(parsed) ? parsed : 0;
    }
    return 0;
  };

  const deltaX = normalizeDelta(scrollX);
  const deltaY = normalizeDelta(scrollY);

  // Capture position before scroll (both target and window)
  const beforeTop = target.scrollTop || 0;
  const beforeLeft = target.scrollLeft || 0;
  const beforeWindowY = window.scrollY;
  const beforeWindowX = window.scrollX;

  // Method 1: Use scrollBy/scrollTo on target (instant behavior)
  if (typeof target.scrollBy === "function") {
    target.scrollBy({ left: deltaX, top: deltaY, behavior: "instant" });
  } else {
    const nextLeft = beforeLeft + deltaX;
    const nextTop = beforeTop + deltaY;
    target.scrollTo({ left: nextLeft, top: nextTop, behavior: "instant" });
  }

  // Check if scroll worked
  let afterTop = target.scrollTop || 0;
  let afterLeft = target.scrollLeft || 0;

  if (afterTop !== beforeTop || afterLeft !== beforeLeft) {
    return "scrolled";
  }

  // Method 2: Direct property assignment (some elements respond to this)
  target.scrollTop = beforeTop + deltaY;
  target.scrollLeft = beforeLeft + deltaX;

  afterTop = target.scrollTop || 0;
  afterLeft = target.scrollLeft || 0;

  if (afterTop !== beforeTop || afterLeft !== beforeLeft) {
    return "scrolled-direct";
  }

  // Method 3: Use window.scrollBy (works when target is html/body but element scroll doesn't)
  window.scrollBy({ left: deltaX, top: deltaY, behavior: "instant" });

  if (window.scrollY !== beforeWindowY || window.scrollX !== beforeWindowX) {
    return "scrolled-window";
  }

  // Method 4: Simulate wheel event (triggers event listeners)
  simulateWheelEvent(target, deltaX, deltaY);

  // Check if wheel event handler scrolled anything
  afterTop = target.scrollTop || 0;
  afterLeft = target.scrollLeft || 0;

  if (afterTop !== beforeTop || afterLeft !== beforeLeft) {
    return "scrolled-wheel";
  }

  if (window.scrollY !== beforeWindowY || window.scrollX !== beforeWindowX) {
    return "scrolled-wheel-window";
  }

  return "scroll-no-change";
}

// ------------------------------------------------------------
// PUBLIC API: scrollByPoint(x, y, scrollX, scrollY)
// ------------------------------------------------------------
function scrollByPoint(x, y, scrollX, scrollY) {
    const el = deepElementFromPoint(x, y);

    // Determine if we have a meaningful starting point for ancestor search
    // If x,y is near origin or element is html/body, skip ancestor walk
    const isOriginPoint = (x <= 1 && y <= 1);
    const isRootElement = !el || el === document.documentElement || el === document.body || el.tagName === "HTML" || el.tagName === "BODY";

    let scrollTarget;
    let insideShadow = false;

    if (isOriginPoint || isRootElement) {
        // Skip ancestor walk, go directly to main scroll container search
        scrollTarget = findMainScrollContainer();
    } else {
        insideShadow = isInsideShadow(el);
        scrollTarget = findScrollableParent(el);
    }

    // Capture scroll info before and after for diagnostics
    const scrollBefore = scrollTarget ? { top: scrollTarget.scrollTop || 0, left: scrollTarget.scrollLeft || 0 } : null;
    let result = performScroll(scrollTarget, scrollX, scrollY);
    let scrollAfter = scrollTarget ? { top: scrollTarget.scrollTop || 0, left: scrollTarget.scrollLeft || 0 } : null;
    let usedFallback = false;

    // If scroll didn't work on the found target, try fallback targets
    if (result === "scroll-no-change") {
        const fallbackTargets = [
            document.scrollingElement,
            document.documentElement,
            document.body
        ].filter(t => t && t !== scrollTarget);

        for (const fallback of fallbackTargets) {
            const fallbackBefore = { top: fallback.scrollTop || 0, left: fallback.scrollLeft || 0 };
            const fallbackResult = performScroll(fallback, scrollX, scrollY);
            const fallbackAfter = { top: fallback.scrollTop || 0, left: fallback.scrollLeft || 0 };

            if (fallbackResult !== "scroll-no-change") {
                result = fallbackResult + "-fallback";
                scrollAfter = fallbackAfter;
                usedFallback = true;
                break;
            }
        }

        // Last resort: fire wheel event on scroll target or element under point
        if (!usedFallback) {
            simulateWheelEvent(scrollTarget || el || document.body, scrollX, scrollY);
            result = "wheel-event-dispatched";
        }
    }

    return {
        elementUnderPoint: el ? el.tagName : "none",
        insideShadowDOM: insideShadow,
        scrollTarget: scrollTarget ? (scrollTarget.tagName || "shadow-root") : null,
        scrollTargetClass: scrollTarget?.className || null,
        canScroll: scrollTarget ? (scrollTarget.scrollHeight > scrollTarget.clientHeight || scrollTarget.scrollWidth > scrollTarget.clientWidth) : false,
        scrollBefore: scrollBefore,
        scrollAfter: scrollAfter,
        result: result,
        usedFallback: usedFallback,
        originPointUsed: (x <= 1 && y <= 1)
    };
}


  function isEditableElement(element) {
    if (!element) return false;
    if (element.isContentEditable) return true;
    if (typeof element.value === 'string' && element.readOnly !== true) return true;
    return false;
  }

  function resolveTypingTarget() {
    const active = document.activeElement;
    if (isEditableElement(active)) {
      return active;
    }
    const shadowRoot = active && active.shadowRoot;
    if (shadowRoot && isEditableElement(shadowRoot.activeElement)) {
      return shadowRoot.activeElement;
    }
    return null;
  }

  const focusableSelector = 'input, textarea, select, button, [contenteditable], [tabindex]';

  function isFocusable(element) {
    if (!element || typeof element !== 'object') return false;
    if (element.disabled || element.getAttribute?.('disabled') !== null) return false;
    if (element.isContentEditable) return true;
    if (typeof element.tabIndex === 'number' && element.tabIndex >= 0) return true;
    const tag = element.tagName;
    return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || tag === 'BUTTON';
  }

  function findFocusable(element) {
    if (!element) return null;
    if (isFocusable(element)) return element;
    if (element.shadowRoot) {
      const active = element.shadowRoot.activeElement;
      if (isFocusable(active)) return active;
      const shadowDesc = element.shadowRoot.querySelector(focusableSelector);
      if (isFocusable(shadowDesc)) return shadowDesc;
    }
    if (element.matches?.('label[for]')) {
      const targetId = element.getAttribute('for');
      if (targetId) {
        const labelled = document.getElementById(targetId);
        if (isFocusable(labelled)) return labelled;
      }
    }
    const fallback = element.querySelector?.(focusableSelector);
    if (isFocusable(fallback)) return fallback;
    return null;
  }

  function ensureFocus(element) {
    const focusTarget = findFocusable(element);
    if (!focusTarget || typeof focusTarget.focus !== 'function') return;
    try {
      focusTarget.focus({ preventScroll: true });
    } catch (err) {
      try {
        focusTarget.focus();
      } catch (e) { /* ignore */ }
    }
  }

  // call: await webView.EvaluateJavaScriptAsync($"({typeIntoActiveElement.toString()})('{text}', {delay})");
function typeIntoActiveElement(text, delayMs = 10) {
  const el = resolveTypingTarget();
  if (!el) return false;
  const str = String(text ?? "");

  // helper to set value via prototype setter if available
  function setValueWithSetter(element, newVal) {
    if (!('value' in element)) { element.textContent = newVal; return; }
    const proto = Object.getPrototypeOf(element);
    const desc = Object.getOwnPropertyDescriptor(proto, 'value');
    if (desc && desc.set) {
      desc.set.call(element, newVal);
    } else {
      element.value = newVal;
    }
  }

  // send events for a single character
  function sendCharEvents(element, ch) {
    const key = ch.length === 1 ? ch : 'Unidentified';
    try {
      element.dispatchEvent(new KeyboardEvent('keydown', { bubbles: true, cancelable: true, key }));
      element.dispatchEvent(new KeyboardEvent('keypress', { bubbles: true, cancelable: true, key }));
    } catch (e) { /* ignore */ }

    // update value using selection/caret if possible
    if ('value' in element) {
      const start = element.selectionStart ?? element.value.length;
      const end = element.selectionEnd ?? element.value.length;
      const before = element.value.substring(0, start);
      const after = element.value.substring(end);
      setValueWithSetter(element, before + ch + after);
      const pos = before.length + ch.length;
      if (element.setSelectionRange) element.setSelectionRange(pos, pos);
    } else if (element.isContentEditable) {
      element.textContent = (element.textContent ?? "") + ch;
    } else {
      return;
    }

    // InputEvent with data and inputType
    try {
      const ie = new InputEvent('input', {
        bubbles: true, cancelable: true, composed: true,
        inputType: 'insertText', data: ch
      });
      element.dispatchEvent(ie);
    } catch (e) {
      const ev = document.createEvent('Event');
      ev.initEvent('input', true, true);
      element.dispatchEvent(ev);
    }

    try {
      element.dispatchEvent(new KeyboardEvent('keyup', { bubbles: true, cancelable: true, key }));
    } catch (e) { /* ignore */ }
  }

  // type each character with optional delay
  return new Promise(resolve => {
    let i = 0;
    function step() {
      if (i >= str.length) {
        // final change event to commit
        el.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));
        resolve(true);
        return;
      }
      sendCharEvents(el, str[i]);
      i++;
      if (!delayMs) step();
      else setTimeout(step, delayMs);
    }
    step();
  });
}

  // Input coordinates are viewport/client coordinates (from screenshot)
  // They should be used directly without scroll adjustment
  function toClientPoint(viewportX, viewportY) {
    return {
      clientX: Math.round(viewportX),
      clientY: Math.round(viewportY)
    };
  }

  function clampToViewport(clientPoint) {
    const width = window.innerWidth;
    const height = window.innerHeight;
    return {
      clientX: Math.min(Math.max(clientPoint.clientX, 0), width - 1),
      clientY: Math.min(Math.max(clientPoint.clientY, 0), height - 1)
    };
  }

  function mouseInit(clientPoint, buttonCode, buttonsMask) {
    return {
      clientX: clientPoint.clientX,
      clientY: clientPoint.clientY,
      button: buttonCode || 0,
      buttons: typeof buttonsMask === 'number' ? buttonsMask : 1,
      bubbles: true,
      cancelable: true,
      composed: true,
      view: window
    };
  }

  function fireMouse(target, type, init) {
    const event = new MouseEvent(type, init);
    target.dispatchEvent(event);
  }

  window.__fsDriver = {
    clickAt: function (pageX, pageY, buttonCode) {
      const client = clampToViewport(toClientPoint(pageX, pageY));
      const element = deepElementFromPoint(client.clientX, client.clientY);
      if (!element) {
        return false;
      }

      const init = mouseInit(client, buttonCode || 0, 1);
      fireMouse(element, 'mousemove', init);
      fireMouse(element, 'mousedown', init);
      ensureFocus(element);
      fireMouse(element, 'mouseup', mouseInit(client, buttonCode || 0, 0));
      fireMouse(element, 'click', init);

      // For links and buttons, also call native click() to ensure navigation/action works
      // Synthetic events don't always trigger default browser behavior
      const clickable = element.closest('a, button, [role="button"], [role="link"]') || element;
      if (clickable && typeof clickable.click === 'function') {
        try {
          clickable.click();
        } catch (e) { /* ignore */ }
      }

      return true;
    },

    doubleClick: function (pageX, pageY) {
      const client = clampToViewport(toClientPoint(pageX, pageY));
      const element = deepElementFromPoint(client.clientX, client.clientY);
      if (!element) {
        return false;
      }
      const init = mouseInit(client, 0, 1);
      fireMouse(element, 'mousemove', init);
      fireMouse(element, 'mousedown', init);
      ensureFocus(element);
      fireMouse(element, 'mouseup', mouseInit(client, 0, 0));
      fireMouse(element, 'click', init);
      fireMouse(element, 'dblclick', init);
      return true;
    },

    moveTo: function (pageX, pageY) {
      const client = clampToViewport(toClientPoint(pageX, pageY));
      const element = deepElementFromPoint(client.clientX, client.clientY);
      if (!element) {
        return false;
      }
      fireMouse(element, 'mousemove', mouseInit(client, 0, 0));
      return true;
    },

    dragDrop: function (startX, startY, endX, endY) {
      const start = clampToViewport(toClientPoint(startX, startY));
      const end = clampToViewport(toClientPoint(endX, endY));
      const source = deepElementFromPoint(start.clientX, start.clientY);
      const target = deepElementFromPoint(end.clientX, end.clientY);
      if (!source || !target) {
        return false;
      }

      const startInit = mouseInit(start, 0, 1);
      const moveInit = mouseInit(end, 0, 1);
      const upInit = mouseInit(end, 0, 0);

      fireMouse(source, 'mousemove', startInit);
      fireMouse(source, 'mousedown', startInit);
      fireMouse(source, 'mousemove', moveInit);
      fireMouse(target, 'mousemove', moveInit);
      fireMouse(target, 'mouseup', upInit);
      fireMouse(target, 'drop', upInit);
      fireMouse(source, 'mouseup', upInit);
      return true;
    },

    scrollBy: function (deltaX, deltaY) {
      window.scrollBy(deltaX || 0, deltaY || 0);
      return { scrollX: window.scrollX, scrollY: window.scrollY };
    },

    scrollTo: function (x, y) {
      window.scrollTo(x || 0, y || 0);
      return { scrollX: window.scrollX, scrollY: window.scrollY };
    },

    touchScroll: function (startX, startY, scrollX, scrollY) {
      return scrollByPoint(startX,startY,scrollX,scrollY);
    },

    typeText: function (text) {
      const target = resolveTypingTarget();
      if (!target) {
        return false;
      }
      typeIntoActiveElement(text);
      return true;
    },

    pressKey: function (key, modifiers) {
      const target = document.activeElement || document.body || document.documentElement;
      const init = Object.assign(
        {
          key: key,
          code: key,
          keyCode: key === ' ' ? 32 : key?.length === 1 ? key.toUpperCase().charCodeAt(0) : 0,
          which: key === ' ' ? 32 : key?.length === 1 ? key.toUpperCase().charCodeAt(0) : 0,
          bubbles: true,
          cancelable: true
        },
        modifiers || {}
      );
      try {
        target.dispatchEvent(new KeyboardEvent('keydown', init));
        target.dispatchEvent(new KeyboardEvent('keypress', init));
        target.dispatchEvent(new KeyboardEvent('keyup', init));
        if (key === 'Enter') {
          target.blur();
          target.dispatchEvent(new Event('change', { bubbles: true }));
        }
        return true;
      } catch (err) {
        console.error(err);
        return false;
      }
    },

    meta: function () {
      return {
        scrollX: window.scrollX,
        scrollY: window.scrollY,
        width: window.innerWidth,
        height: window.innerHeight,
        url: window.location.href || null,
        title: document.title || null
      };
    },

    navigate: function (targetUrl) {
      if (!targetUrl) {
        return false;
      }
      if (window.location.href === targetUrl) {
        window.location.reload();
      } else {
        window.location.href = targetUrl;
      }
      return true;
    },
  };

  return true;
})();
"""
