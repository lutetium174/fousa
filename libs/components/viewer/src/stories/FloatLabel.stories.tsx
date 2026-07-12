import { FloatLabel } from "components";
import { Input } from "components";

export default {
  title: "Components/FloatLabel",
  component: FloatLabel,
};

export const Basic = () => (
  <FloatLabel>
    <Input id="username" placeholder=" " />
    <label for="username">Username</label>
  </FloatLabel>
);

export const PreFilled = () => (
  <FloatLabel>
    <Input id="prefilled" value="John Doe" placeholder=" " />
    <label for="prefilled">Username</label>
  </FloatLabel>
);

export const VariantIn = () => (
  <FloatLabel variant="in">
    <Input id="in" placeholder=" " />
    <label for="in">In Label</label>
  </FloatLabel>
);

export const VariantOn = () => (
  <FloatLabel variant="on">
    <Input id="on" placeholder=" " />
    <label for="on">On Label</label>
  </FloatLabel>
);

export const KitchenSink = () => (
  <div style={{ display: "flex", "flex-direction": "column", gap: "1rem", "max-width": "20rem" }}>
    <FloatLabel>
      <Input id="ks1" placeholder=" " />
      <label for="ks1">Default (empty)</label>
    </FloatLabel>
    
    <FloatLabel>
      <Input id="ks2" value="Has value" placeholder=" " />
      <label for="ks2">Pre-filled</label>
    </FloatLabel>
    
    <FloatLabel variant="in">
      <Input id="ks3" placeholder=" " />
      <label for="ks3">Variant: In</label>
    </FloatLabel>
    
    <FloatLabel variant="in">
      <Input id="ks4" value="Has value" placeholder=" " />
      <label for="ks4">In with value</label>
    </FloatLabel>
    
    <FloatLabel variant="on">
      <Input id="ks5" placeholder=" " />
      <label for="ks5">Variant: On</label>
    </FloatLabel>
    
    <FloatLabel variant="on">
      <Input id="ks6" value="Has value" placeholder=" " />
      <label for="ks6">On with value</label>
    </FloatLabel>
  </div>
);
