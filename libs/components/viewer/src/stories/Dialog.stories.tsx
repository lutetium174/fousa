import { Dialog, DialogHeader, DialogContent, DialogFooter, Button } from "components";
import { createSignal, type JSX } from "solid-js";

export default {
  title: "Components/Dialog",
  component: Dialog,
  argTypes: {
    variant: {
      control: "select",
      options: ["default", "rounded", "borderless"],
    },
    size: {
      control: "select",
      options: ["sm", "md", "lg", "xl", "fullscreen"],
    },
    open: {
      control: "boolean",
    },
    closeOnBackdrop: {
      control: "boolean",
    },
    closeOnEscape: {
      control: "boolean",
    },
    trapFocus: {
      control: "boolean",
    },
    fullscreenOnMobile: {
      control: "boolean",
    },
  },
};

// Helper component to demonstrate dialog with state
const DialogDemo = (props: {
  title: string;
  triggerText: string;
  children?: JSX.Element;
  variant?: "default" | "rounded" | "borderless";
  size?: "sm" | "md" | "lg" | "xl" | "fullscreen";
  fullscreenOnMobile?: boolean;
}) => {
  const [isOpen, setIsOpen] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>{props.triggerText}</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
        variant={props.variant}
        size={props.size}
        fullscreenOnMobile={props.fullscreenOnMobile}
      >
        <DialogHeader 
          title={props.title} 
          onClose={() => setIsOpen(false)} 
        />
        <DialogContent>
          {props.children || <p>This is a dialog with {props.variant || 'default'} styling.</p>}
        </DialogContent>
        <DialogFooter>
          <Button variant="secondary" onClick={() => setIsOpen(false)}>Cancel</Button>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Confirm</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

// Basic Dialog examples
export const BasicDialog = () => (
  <DialogDemo 
    title="Basic Dialog" 
    triggerText="Open Basic Dialog"
  />
);

// Variant examples
export const RoundedDialog = () => (
  <DialogDemo 
    title="Rounded Dialog" 
    triggerText="Open Rounded Dialog" 
    variant="rounded"
  />
);

export const BorderlessDialog = () => (
  <DialogDemo 
    title="Borderless Dialog" 
    triggerText="Open Borderless Dialog" 
    variant="borderless"
  />
);

// Size examples
export const SmallDialog = () => (
  <DialogDemo 
    title="Small Dialog" 
    triggerText="Open Small Dialog" 
    size="sm"
    variant="rounded"
  >
    <p>This is a small dialog (max-width: 400px).</p>
  </DialogDemo>
);

export const LargeDialog = () => (
  <DialogDemo 
    title="Large Dialog" 
    triggerText="Open Large Dialog" 
    size="lg"
    variant="rounded"
  >
    <p>This is a large dialog (max-width: 700px).</p>
    <p>It has more space for content.</p>
    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.</p>
  </DialogDemo>
);

export const ExtraLargeDialog = () => (
  <DialogDemo 
    title="Extra Large Dialog" 
    triggerText="Open Extra Large Dialog" 
    size="xl"
    variant="rounded"
  >
    <p>This is an extra large dialog (max-width: 900px).</p>
    <p>Perfect for complex forms or detailed content.</p>
    <div style={{ 
      display: "grid", 
      "grid-template-columns": "repeat(2, 1fr)", 
      gap: "16px", 
      "margin-top": "16px"
    }}>
      <div style={{ background: "var(--bg-subtle)", padding: "16px", "border-radius": "4px" }}>
        <h4>Section 1</h4>
        <p>Content for the first section goes here.</p>
      </div>
      <div style={{ background: "var(--bg-subtle)", padding: "16px", "border-radius": "4px" }}>
        <h4>Section 2</h4>
        <p>Content for the second section goes here.</p>
      </div>
    </div>
  </DialogDemo>
);

export const FullscreenDialog = () => (
  <DialogDemo 
    title="Fullscreen Dialog" 
    triggerText="Open Fullscreen Dialog" 
    size="fullscreen"
  >
    <p>This is a fullscreen dialog that takes up the entire viewport.</p>
    <p>Useful for mobile-first experiences or when you need maximum space.</p>
  </DialogDemo>
);

// Mobile-optimized examples
export const MobileOptimizedDialog = () => {
  const [isOpen, setIsOpen] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>Open Mobile-Optimized Dialog</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
        size="md"
        fullscreenOnMobile={true}
        variant="rounded"
      >
        <DialogHeader 
          title="Mobile-Optimized Dialog" 
          onClose={() => setIsOpen(false)} 
        />
        <DialogContent>
          <p>This dialog automatically becomes fullscreen on mobile devices (≤ 768px).</p>
          <p>On desktop, it maintains its normal sizing.</p>
          <p>Resize your browser window to see the responsive behavior.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="secondary" onClick={() => setIsOpen(false)}>Cancel</Button>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Confirm</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

// Behavior examples
export const NoBackdropClose = () => {
  const [isOpen, setIsOpen] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>Open Persistent Dialog</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
        closeOnBackdrop={false}
      >
        <DialogHeader 
          title="Persistent Dialog" 
          onClose={() => setIsOpen(false)} 
        />
        <DialogContent>
          <p>This dialog cannot be closed by clicking the backdrop.</p>
          <p>You must use the close button or press Escape.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Close</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

export const NoCloseButton = () => {
  const [isOpen, setIsOpen] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>Open Dialog (No Close Button)</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
      >
        <DialogHeader 
          title="No Close Button" 
          closeButton={false}
        />
        <DialogContent>
          <p>This dialog has no close button in the header.</p>
          <p>You can still close it by clicking the backdrop or pressing Escape.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Close</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

// Custom content examples
export const CustomHeader = () => {
  const [isOpen, setIsOpen] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>Open Dialog with Custom Header</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
      >
        <DialogHeader>
          <div style={{ display: "flex", "align-items": "center", gap: "8px" }}>
            <span style={{ 
              width: "32px", 
              height: "32px", 
              "background-color": "var(--primary-50)", 
              "border-radius": "50%",
              "display": "flex",
              "align-items": "center",
              "justify-content": "center",
              color: "var(--primary)"
            }}>📋</span>
            <h2 style={{ margin: 0, "font-size": "1.125rem", "font-weight": 600 }}>Custom Header</h2>
          </div>
        </DialogHeader>
        <DialogContent>
          <p>This dialog has a completely custom header with an icon.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Close</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

export const FooterAlignments = () => {
  const [isOpen, setIsOpen] = createSignal(false);
  const [align, setAlign] = createSignal<"left" | "center" | "right">("right");

  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px" }}>
      <div style={{ display: "flex", gap: "8px" }}>
        <Button onClick={() => { setAlign("left"); setIsOpen(true); }}>Left Aligned Footer</Button>
        <Button onClick={() => { setAlign("center"); setIsOpen(true); }}>Center Aligned Footer</Button>
        <Button onClick={() => { setAlign("right"); setIsOpen(true); }}>Right Aligned Footer</Button>
      </div>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
      >
        <DialogHeader 
          title={`Footer Alignment: ${align()}`} 
          onClose={() => setIsOpen(false)} 
        />
        <DialogContent>
          <p>This dialog demonstrates different footer alignments.</p>
        </DialogContent>
        <DialogFooter align={align()}>
          <Button variant="secondary" onClick={() => setIsOpen(false)}>Cancel</Button>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Confirm</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

// Content examples
export const ScrollableContent = () => {
  const [isOpen, setIsOpen] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>Open Scrollable Dialog</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
        size="md"
      >
        <DialogHeader 
          title="Scrollable Content" 
          onClose={() => setIsOpen(false)} 
        />
        <DialogContent style={{ "max-height": "300px", overflow: "auto" }}>
          <p>This dialog has scrollable content.</p>
          <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl. Sed egestas, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.</p>
          <p>Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl. Sed egestas, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.</p>
          <p>Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl. Sed egestas, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.</p>
          <p>Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl. Sed egestas, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.</p>
          <p>Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl. Sed egestas, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="primary" onClick={() => setIsOpen(false)}>Close</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

// Real-world examples
export const ConfirmationDialog = () => {
  const [isOpen, setIsOpen] = createSignal(false);
  const [result, setResult] = createSignal<string>("");

  const handleConfirm = () => {
    setResult("Action confirmed!");
    setIsOpen(false);
  };

  const handleCancel = () => {
    setResult("Action cancelled.");
    setIsOpen(false);
  };

  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px" }}>
      <Button variant="danger" onClick={() => setIsOpen(true)}>Delete Item</Button>
      
      {result() && <p style={{ color: "var(--text-muted)" }}>{result()}</p>}
      
      <Dialog 
        open={isOpen()} 
        onClose={handleCancel}
        size="sm"
      >
        <DialogHeader 
          title="Confirm Deletion" 
          onClose={handleCancel} 
        />
        <DialogContent>
          <p>Are you sure you want to delete this item? This action cannot be undone.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="secondary" onClick={handleCancel}>Cancel</Button>
          <Button variant="danger" onClick={handleConfirm}>Delete</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};

export const FormDialog = () => {
  const [isOpen, setIsOpen] = createSignal(false);

  const handleSubmit = (e: Event) => {
    e.preventDefault();
    alert("Form submitted!");
    setIsOpen(false);
  };

  return (
    <div style={{ display: "flex", gap: "16px", "align-items": "center" }}>
      <Button onClick={() => setIsOpen(true)}>Open Form Dialog</Button>
      
      <Dialog 
        open={isOpen()} 
        onClose={() => setIsOpen(false)} 
        size="md"
        fullscreenOnMobile={true}
      >
        <DialogHeader 
          title="Create New Item" 
          onClose={() => setIsOpen(false)} 
        />
        <form onSubmit={handleSubmit}>
          <DialogContent>
            <div style={{ display: "flex", "flex-direction": "column", gap: "16px" }}>
              <div>
                <label style={{ display: "block", "margin-bottom": "4px", "font-weight": 500 }}>Name</label>
                <input 
                  type="text" 
                  style={{ 
                    width: "100%", 
                    padding: "8px 12px", 
                    "border-radius": "4px", 
                    border: "1px solid var(--border)",
                    "font-family": "inherit",
                    "font-size": "inherit"
                  }} 
                  placeholder="Enter name"
                  required
                />
              </div>
              <div>
                <label style={{ display: "block", "margin-bottom": "4px", "font-weight": 500 }}>Description</label>
                <textarea 
                  style={{ 
                    width: "100%", 
                    padding: "8px 12px", 
                    "border-radius": "4px", 
                    border: "1px solid var(--border)",
                    "font-family": "inherit",
                    "font-size": "inherit",
                    "min-height": "80px",
                    resize: "vertical"
                  }} 
                  placeholder="Enter description"
                />
              </div>
            </div>
          </DialogContent>
          <DialogFooter>
            <Button variant="secondary" type="button" onClick={() => setIsOpen(false)}>Cancel</Button>
            <Button variant="primary" type="submit">Save</Button>
          </DialogFooter>
        </form>
      </Dialog>
    </div>
  );
};

// Multiple dialogs example
export const MultipleDialogs = () => {
  const [isOpen1, setIsOpen1] = createSignal(false);
  const [isOpen2, setIsOpen2] = createSignal(false);

  return (
    <div style={{ display: "flex", gap: "16px", "flex-wrap": "wrap" }}>
      <Button onClick={() => setIsOpen1(true)}>Open Dialog 1</Button>
      <Button onClick={() => setIsOpen2(true)}>Open Dialog 2</Button>
      
      <Dialog 
        open={isOpen1()} 
        onClose={() => setIsOpen1(false)} 
        size="sm"
      >
        <DialogHeader 
          title="First Dialog" 
          onClose={() => setIsOpen1(false)} 
        />
        <DialogContent>
          <p>This is the first dialog.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="primary" onClick={() => setIsOpen1(false)}>Close</Button>
        </DialogFooter>
      </Dialog>
      
      <Dialog 
        open={isOpen2()} 
        onClose={() => setIsOpen2(false)} 
        size="md"
      >
        <DialogHeader 
          title="Second Dialog" 
          onClose={() => setIsOpen2(false)} 
        />
        <DialogContent>
          <p>This is the second dialog.</p>
          <p>You can open both dialogs to see stacking behavior.</p>
        </DialogContent>
        <DialogFooter>
          <Button variant="primary" onClick={() => setIsOpen2(false)}>Close</Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
};