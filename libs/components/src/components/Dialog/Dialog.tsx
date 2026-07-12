import { type Component, type JSX, splitProps, mergeProps, createSignal, onMount, onCleanup, Show, createEffect } from "solid-js";
import styles from "./dialog.module.css";
import { toPascalCase } from "../../utils/pascalCase";

export interface DialogProps {
  children?: JSX.Element;
  class?: string;
  variant?: "default" | "rounded" | "borderless";
  size?: "sm" | "md" | "lg" | "xl" | "fullscreen";
  open?: boolean;
  onClose?: () => void;
  closeOnBackdrop?: boolean;
  closeOnEscape?: boolean;
  backdropClass?: string;
  disableBackdrop?: boolean;
  trapFocus?: boolean;
  header?: JSX.Element;
  footer?: JSX.Element;
  fullscreenOnMobile?: boolean;
}

/**
 * Dialog component for displaying content in a focused overlay.
 * Reactive and mobile-friendly with responsive sizing.
 */
export const Dialog: Component<DialogProps> = (rawProps) => {
  const props = mergeProps(
    { 
      variant: "default" as const, 
      size: "md" as const,
      open: false,
      closeOnBackdrop: true,
      closeOnEscape: true,
      disableBackdrop: false,
      trapFocus: true,
      onClose: () => {},
      fullscreenOnMobile: true,
    },
    rawProps
  );
  
  const [local, rest] = splitProps(props, [
    "children", "class", "variant", "size", "open", "onClose", 
    "closeOnBackdrop", "closeOnEscape", "backdropClass", 
    "disableBackdrop", "trapFocus", "header", "footer", "fullscreenOnMobile"
  ]);

  const [dialogRef, setDialogRef] = createSignal<HTMLDivElement | null>(null);
  const [previousActiveElement, setPreviousActiveElement] = createSignal<Element | null>(null);
  const [isMobile, setIsMobile] = createSignal(false);

  // Check if we're on mobile size (768px breakpoint)
  createEffect(() => {
    if (typeof window !== 'undefined') {
      const checkMobile = () => {
        setIsMobile(window.matchMedia('(max-width: 768px)').matches);
      };
      
      checkMobile();
      
      const mediaQuery = window.matchMedia('(max-width: 768px)');
      const handler = () => checkMobile();
      
      mediaQuery.addEventListener('change', handler);
      
      onCleanup(() => {
        mediaQuery.removeEventListener('change', handler);
      });
    }
  });

  // Determine effective size based on mobile detection
  const effectiveSize = () => {
    if (local.fullscreenOnMobile && isMobile()) {
      return "fullscreen";
    }
    return local.size;
  };

  // Handle escape key
  const handleKeyDown = (e: KeyboardEvent) => {
    if (local.open && local.closeOnEscape && e.key === "Escape") {
      local.onClose?.();
    }
  };

  // Handle backdrop click
  const handleBackdropClick = (e: MouseEvent) => {
    if (local.closeOnBackdrop && e.target === e.currentTarget) {
      local.onClose?.();
    }
  };

  // Focus trapping logic
  createEffect(() => {
    if (local.open && local.trapFocus && dialogRef()) {
      const focusableElements = dialogRef()?.querySelectorAll<HTMLElement>(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      );
      
      if (focusableElements && focusableElements.length > 0) {
        // Focus the first focusable element when dialog opens
        focusableElements[0]?.focus();
      } else {
        // If no focusable elements, focus the dialog itself
        dialogRef()?.focus();
      }
    }
  });

  // Handle focus trap
  const handleFocusTrap = (e: FocusEvent) => {
    if (local.open && local.trapFocus && dialogRef()) {
      const focusableElements = Array.from(
        dialogRef()?.querySelectorAll<HTMLElement>(
          'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        ) || []
      );
      
      if (focusableElements.length === 0) return;
      
      const firstElement = focusableElements[0];
      const lastElement = focusableElements[focusableElements.length - 1];
      
      // Check if this is a keyboard navigation (Tab key)
      const isKeyboardEvent = e instanceof KeyboardEvent;
      const isTabKey = isKeyboardEvent && e.key === 'Tab';
      const isShiftTab = isTabKey && e.shiftKey;
      
      if (!isKeyboardEvent || !isTabKey) return;
      
      if (e.target === lastElement && !isShiftTab) {
        e.preventDefault();
        firstElement?.focus();
      } else if (e.target === firstElement && isShiftTab) {
        e.preventDefault();
        lastElement?.focus();
      }
    }
  };

  // Store previous active element and add event listeners
  onMount(() => {
    if (local.open) {
      setPreviousActiveElement(document.activeElement);
      document.addEventListener("keydown", handleKeyDown);
      document.addEventListener("focusin", handleFocusTrap);
      document.body.style.overflow = "hidden";
    }
  });

  // Clean up event listeners and restore state
  onCleanup(() => {
    document.removeEventListener("keydown", handleKeyDown);
    document.removeEventListener("focusin", handleFocusTrap);
    document.body.style.overflow = "";
    
    if (previousActiveElement()) {
      (previousActiveElement() as HTMLElement)?.focus();
    }
  });

  // Update previous active element when open changes
  createEffect(() => {
    if (local.open) {
      setPreviousActiveElement(document.activeElement);
      document.addEventListener("keydown", handleKeyDown);
      document.addEventListener("focusin", handleFocusTrap);
      document.body.style.overflow = "hidden";
    } else {
      document.removeEventListener("keydown", handleKeyDown);
      document.removeEventListener("focusin", handleFocusTrap);
      document.body.style.overflow = "";
      
      if (previousActiveElement()) {
        (previousActiveElement() as HTMLElement)?.focus();
      }
    }
  });

  const dialogClasses = [
    styles.pvDialog,
    styles[`pvDialog${toPascalCase(local.variant)}`],
    styles[`pvDialog${toPascalCase(effectiveSize())}`],
    isMobile() && local.fullscreenOnMobile ? styles.pvDialogMobile : "",
    local.class ?? "",
  ].filter(Boolean).join(" ");

  const backdropClasses = [
    styles.pvDialogBackdrop,
    local.backdropClass ?? "",
  ].filter(Boolean).join(" ");

  return (
    <Show when={local.open}>
      <div 
        class={backdropClasses}
        onClick={handleBackdropClick}
        aria-hidden="true"
      >
        <div 
          ref={setDialogRef}
          class={dialogClasses}
          role="dialog"
          aria-modal="true"
          tabindex={local.trapFocus ? "-1" : undefined}
          onKeyDown={(e) => {
            if (e.key === "Tab") {
              handleFocusTrap(e as unknown as FocusEvent);
            }
          }}
          {...rest}
        >
          <Show when={local.header}>
            <div class={styles.pvDialogHeader}>
              {local.header}
            </div>
          </Show>
          
          <div class={styles.pvDialogContent}>
            {local.children}
          </div>
          
          <Show when={local.footer}>
            <div class={styles.pvDialogFooter}>
              {local.footer}
            </div>
          </Show>
        </div>
      </div>
    </Show>
  );
};

export interface DialogHeaderProps {
  children?: JSX.Element;
  class?: string;
  title?: string;
  onClose?: () => void;
  closeButton?: boolean;
  closeButtonAriaLabel?: string;
}

/**
 * Dialog header component with optional title and close button
 */
export const DialogHeader: Component<DialogHeaderProps> = (rawProps) => {
  const props = mergeProps(
    { 
      closeButton: true,
      closeButtonAriaLabel: "Close dialog"
    },
    rawProps
  );
  const [local, rest] = splitProps(props, ["children", "class", "title", "onClose", "closeButton", "closeButtonAriaLabel"]);

  return (
    <div 
      class={[styles.pvDialogHeader, local.class ?? ""].filter(Boolean).join(" ")}
      {...rest}
    >
      <div class={styles.pvDialogHeaderContent}>
        {local.title && <h2 class={styles.pvDialogTitle}>{local.title}</h2>}
        {local.children}
      </div>
      
      <Show when={local.closeButton && local.onClose}>
        <button 
          type="button"
          class={styles.pvDialogCloseButton}
          onClick={local.onClose}
          aria-label={local.closeButtonAriaLabel}
        >
          <svg 
            viewBox="0 0 24 24" 
            width="20" 
            height="20" 
            fill="none" 
            stroke="currentColor" 
            stroke-width="2" 
            stroke-linecap="round" 
            stroke-linejoin="round"
          >
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </Show>
    </div>
  );
};

export interface DialogContentProps {
  children?: JSX.Element;
  class?: string;
}

/**
 * Dialog main content area
 */
export const DialogContent: Component<DialogContentProps> = (rawProps) => {
  const props = mergeProps({}, rawProps);
  const [local, rest] = splitProps(props, ["children", "class"]);

  return (
    <div 
      class={[styles.pvDialogContent, local.class ?? ""].filter(Boolean).join(" ")}
      {...rest}
    >
      {local.children}
    </div>
  );
};

export interface DialogFooterProps {
  children?: JSX.Element;
  class?: string;
  align?: "left" | "center" | "right";
}

/**
 * Dialog footer section for actions
 */
export const DialogFooter: Component<DialogFooterProps> = (rawProps) => {
  const props = mergeProps({ align: "right" as const }, rawProps);
  const [local, rest] = splitProps(props, ["children", "class", "align"]);

  return (
    <div 
      class={[ 
        styles.pvDialogFooter, 
        styles[`pvDialogFooter${toPascalCase(local.align)}`],
        local.class ?? ""
      ].filter(Boolean).join(" ")}
      {...rest}
    >
      {local.children}
    </div>
  );
};

export default Dialog;