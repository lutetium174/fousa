import { type Component, type JSX, For, createSignal, Show } from "solid-js";
import styles from "./navbar.module.css";

export interface NavbarItem {
  label: string;
  key: string;
  icon?: JSX.Element;
  subItems?: NavbarItem[];
}

export interface NavbarProps {
  items: NavbarItem[];
  onSelect?: (key: string) => void;
  selectedKey?: string;
  class?: string;
}

const ChevronDown = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="14"
    height="14"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="2.5"
    stroke-linecap="round"
    stroke-linejoin="round"
  >
    <polyline points="6 9 12 15 18 9" />
  </svg>
);

const Navbar: Component<NavbarProps> = (rawProps) => {
  const props = rawProps;
  const [expandedKeys, setExpandedKeys] = createSignal<string[]>([]);

  const handleClick = (key: string, hasSubItems: boolean, firstChildKey?: string) => {
    if (hasSubItems) {
      setExpandedKeys(prev => {
        if (prev.includes(key)) {
          return prev.filter(k => k !== key);
        } else {
          if (firstChildKey) {
            props.onSelect?.(firstChildKey);
          }
          return [...prev, key];
        }
      });
    } else {
      props.onSelect?.(key);
    }
  };

  const isActive = (key: string): boolean => props.selectedKey === key;
  const isExpanded = (key: string): boolean => expandedKeys().includes(key);

  const NavbarItemComponent = (itemProps: {
    item: NavbarItem;
    level?: number;
  }) => {
    const level = itemProps.level || 0;
    const hasSubItems =
      (itemProps.item.subItems && itemProps.item.subItems.length > 0) || false;
    const isSelected = () => isActive(itemProps.item.key);
    const shouldBeExpanded = () => isExpanded(itemProps.item.key);

    return (
      <>
        <div
          class={[
            level === 0 ? styles.pvNavbarItem : styles.pvNavbarSubnavItem,
            isSelected()
              ? level === 0
                ? styles.pvNavbarItemActive
                : styles.pvNavbarSubnavItemActive
              : "",
          ]
            .filter(Boolean)
            .join(" ")}
          onClick={() => handleClick(itemProps.item.key, hasSubItems, itemProps.item.subItems?.[0]?.key)}
        >
          {level === 0 ? (
            <>
              <span class={styles.pvNavbarItemLeft}>
                {itemProps.item.icon && (
                  <span class={styles.pvNavbarItemIcon}>{itemProps.item.icon}</span>
                )}
                <span class={styles.pvNavbarItemLabel}>{itemProps.item.label}</span>
              </span>
              {hasSubItems && (
                <span
                  class={[
                    styles.pvNavbarChevron,
                    shouldBeExpanded() ? styles.pvNavbarChevronExpanded : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                >
                  <ChevronDown />
                </span>
              )}
            </>
          ) : (
            <>
              {itemProps.item.icon && (
                <span class={styles.pvNavbarItemIcon}>{itemProps.item.icon}</span>
              )}
              <span class={styles.pvNavbarItemLabel}>{itemProps.item.label}</span>
            </>
          )}
        </div>
        <Show when={hasSubItems && shouldBeExpanded()}>
          <div class={styles.pvNavbarSubnav}>
            <For each={itemProps.item.subItems}>
              {(subItem) => (
                <NavbarItemComponent item={subItem} level={level + 1} />
              )}
            </For>
          </div>
        </Show>
      </>
    );
  };

  return (
    <nav class={[styles.pvNavbar, props.class ?? ""].filter(Boolean).join(" ")}>
      <For each={props.items}>
        {(item) => <NavbarItemComponent item={item} />}
      </For>
    </nav>
  );
};

export default Navbar;