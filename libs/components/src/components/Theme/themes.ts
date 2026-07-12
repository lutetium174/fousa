/**
 * Theme definitions for ThemeProvider.
 *
 * Each theme defines light + dark token overrides.
 * Only semantic tokens need overriding — primitives are
 * re-declared per theme so the correct palette feeds in.
 *
 * Built-in themes:
 *   aura      — blue, the default (matches PrimeVue Aura)
 *   lara      — indigo/violet, softer (matches PrimeVue Lara)
 *   nora      — teal, high-contrast
 *   rose      — rose/pink
 *   amber     — amber/orange, warm
 *   emerald   — green
 */

export interface ThemeColorScheme {
  /** CSS custom-property overrides applied as inline vars on the provider element */
  tokens: Record<string, string>;
}

export interface ThemeDefinition {
  name:   string;
  light:  ThemeColorScheme;
  dark:   ThemeColorScheme;
}

// ─── helpers ───────────────────────────────────────────────
export const scheme = (light: Record<string, string>, dark: Record<string, string>): ThemeDefinition["light"] & { _dark: ThemeColorScheme } =>
  ({ tokens: light, _dark: { tokens: dark } } as any);

// Shared severity tokens (same across themes, differ only light/dark)
const severityLight = {
  "--severity-success-bg":     "#f0fdf4",
  "--severity-success-text":   "#16a34a",
  "--severity-success-border": "#bbf7d0",
  "--severity-info-bg":        "#eff6ff",
  "--severity-info-text":      "#2563eb",
  "--severity-info-border":    "#bfdbfe",
  "--severity-warn-bg":        "#fefce8",
  "--severity-warn-text":      "#a16207",
  "--severity-warn-border":    "#fef08a",
  "--severity-danger-bg":      "#fef2f2",
  "--severity-danger-text":    "#dc2626",
  "--severity-danger-border":  "#fecaca",
};

const severityDark = {
  "--severity-success-bg":     "#052e16",
  "--severity-success-text":   "#4ade80",
  "--severity-success-border": "#166534",
  "--severity-info-bg":        "#172554",
  "--severity-info-text":      "#93c5fd",
  "--severity-info-border":    "#1e40af",
  "--severity-warn-bg":        "#1c1400",
  "--severity-warn-text":      "#facc15",
  "--severity-warn-border":    "#713f12",
  "--severity-danger-bg":      "#450a0a",
  "--severity-danger-text":    "#f87171",
  "--severity-danger-border":  "#991b1b",
};

// ─── Aura (Blue) ───────────────────────────────────────────
export const aura: ThemeDefinition = {
  name: "aura",
  light: {
    tokens: {
      "--primary":          "#3b82f6",
      "--primary-hover":    "#2563eb",
      "--primary-active":   "#1d4ed8",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#eff6ff",
      "--primary-100":      "#dbeafe",
      "--border-focus":     "#3b82f6",
      "--text-color":       "#1e293b",
      "--text-muted":       "#64748b",
      "--bg":               "#ffffff",
      "--bg-subtle":        "#f8fafc",
      "--bg-emphasis":      "#f1f5f9",
      "--border":           "#e2e8f0",
      "--severity-secondary-bg":     "#f1f5f9",
      "--severity-secondary-text":   "#475569",
      "--severity-secondary-border": "#e2e8f0",
      ...severityLight,
    },
  },
  dark: {
    tokens: {
      "--primary":          "#60a5fa",
      "--primary-hover":    "#3b82f6",
      "--primary-active":   "#2563eb",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#172554",
      "--primary-100":      "#1e3a8a",
      "--border-focus":     "#60a5fa",
      "--text-color":       "#f1f5f9",
      "--text-muted":       "#94a3b8",
      "--bg":               "#0f172a",
      "--bg-subtle":        "#1e293b",
      "--bg-emphasis":      "#334155",
      "--border":           "#334155",
      "--severity-secondary-bg":     "#1e293b",
      "--severity-secondary-text":   "#cbd5e1",
      "--severity-secondary-border": "#475569",
      ...severityDark,
    },
  },
};

// ─── Lara (Indigo/Violet) ──────────────────────────────────
export const lara: ThemeDefinition = {
  name: "lara",
  light: {
    tokens: {
      "--primary":          "#6366f1",
      "--primary-hover":    "#4f46e5",
      "--primary-active":   "#4338ca",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#eef2ff",
      "--primary-100":      "#e0e7ff",
      "--border-focus":     "#6366f1",
      "--text-color":       "#1e293b",
      "--text-muted":       "#64748b",
      "--bg":               "#ffffff",
      "--bg-subtle":        "#f8fafc",
      "--bg-emphasis":      "#f1f5f9",
      "--border":           "#e2e8f0",
      "--severity-secondary-bg":     "#f1f5f9",
      "--severity-secondary-text":   "#475569",
      "--severity-secondary-border": "#e2e8f0",
      ...severityLight,
    },
  },
  dark: {
    tokens: {
      "--primary":          "#818cf8",
      "--primary-hover":    "#6366f1",
      "--primary-active":   "#4f46e5",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#1e1b4b",
      "--primary-100":      "#312e81",
      "--border-focus":     "#818cf8",
      "--text-color":       "#f1f5f9",
      "--text-muted":       "#94a3b8",
      "--bg":               "#0f172a",
      "--bg-subtle":        "#1e293b",
      "--bg-emphasis":      "#334155",
      "--border":           "#334155",
      "--severity-secondary-bg":     "#1e293b",
      "--severity-secondary-text":   "#cbd5e1",
      "--severity-secondary-border": "#475569",
      ...severityDark,
    },
  },
};

// ─── Nora (Teal) ───────────────────────────────────────────
export const nora: ThemeDefinition = {
  name: "nora",
  light: {
    tokens: {
      "--primary":          "#0d9488",
      "--primary-hover":    "#0f766e",
      "--primary-active":   "#115e59",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#f0fdfa",
      "--primary-100":      "#ccfbf1",
      "--border-focus":     "#0d9488",
      "--text-color":       "#134e4a",
      "--text-muted":       "#5eead4",
      "--bg":               "#ffffff",
      "--bg-subtle":        "#f0fdfa",
      "--bg-emphasis":      "#ccfbf1",
      "--border":           "#99f6e4",
      "--severity-secondary-bg":     "#f0fdfa",
      "--severity-secondary-text":   "#0f766e",
      "--severity-secondary-border": "#99f6e4",
      ...severityLight,
    },
  },
  dark: {
    tokens: {
      "--primary":          "#2dd4bf",
      "--primary-hover":    "#0d9488",
      "--primary-active":   "#0f766e",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#042f2e",
      "--primary-100":      "#134e4a",
      "--border-focus":     "#2dd4bf",
      "--text-color":       "#f0fdfa",
      "--text-muted":       "#5eead4",
      "--bg":               "#042f2e",
      "--bg-subtle":        "#134e4a",
      "--bg-emphasis":      "#115e59",
      "--border":           "#115e59",
      "--severity-secondary-bg":     "#134e4a",
      "--severity-secondary-text":   "#99f6e4",
      "--severity-secondary-border": "#0f766e",
      ...severityDark,
    },
  },
};

// ─── Rose ──────────────────────────────────────────────────
export const rose: ThemeDefinition = {
  name: "rose",
  light: {
    tokens: {
      "--primary":          "#f43f5e",
      "--primary-hover":    "#e11d48",
      "--primary-active":   "#be123c",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#fff1f2",
      "--primary-100":      "#ffe4e6",
      "--border-focus":     "#f43f5e",
      "--text-color":       "#1e293b",
      "--text-muted":       "#64748b",
      "--bg":               "#ffffff",
      "--bg-subtle":        "#fff1f2",
      "--bg-emphasis":      "#ffe4e6",
      "--border":           "#fecdd3",
      "--severity-secondary-bg":     "#f1f5f9",
      "--severity-secondary-text":   "#475569",
      "--severity-secondary-border": "#e2e8f0",
      ...severityLight,
    },
  },
  dark: {
    tokens: {
      "--primary":          "#fb7185",
      "--primary-hover":    "#f43f5e",
      "--primary-active":   "#e11d48",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#4c0519",
      "--primary-100":      "#881337",
      "--border-focus":     "#fb7185",
      "--text-color":       "#f1f5f9",
      "--text-muted":       "#94a3b8",
      "--bg":               "#0f0a0b",
      "--bg-subtle":        "#1c0f12",
      "--bg-emphasis":      "#2d1a1f",
      "--border":           "#3d1f26",
      "--severity-secondary-bg":     "#1e293b",
      "--severity-secondary-text":   "#cbd5e1",
      "--severity-secondary-border": "#475569",
      ...severityDark,
    },
  },
};

// ─── Amber ─────────────────────────────────────────────────
export const amber: ThemeDefinition = {
  name: "amber",
  light: {
    tokens: {
      "--primary":          "#d97706",
      "--primary-hover":    "#b45309",
      "--primary-active":   "#92400e",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#fffbeb",
      "--primary-100":      "#fef3c7",
      "--border-focus":     "#d97706",
      "--text-color":       "#1e293b",
      "--text-muted":       "#64748b",
      "--bg":               "#ffffff",
      "--bg-subtle":        "#fffbeb",
      "--bg-emphasis":      "#fef3c7",
      "--border":           "#fde68a",
      "--severity-secondary-bg":     "#f1f5f9",
      "--severity-secondary-text":   "#475569",
      "--severity-secondary-border": "#e2e8f0",
      ...severityLight,
    },
  },
  dark: {
    tokens: {
      "--primary":          "#fbbf24",
      "--primary-hover":    "#d97706",
      "--primary-active":   "#b45309",
      "--primary-contrast": "#1c1917",
      "--primary-50":       "#451a03",
      "--primary-100":      "#78350f",
      "--border-focus":     "#fbbf24",
      "--text-color":       "#fefce8",
      "--text-muted":       "#a3a3a3",
      "--bg":               "#1c1a14",
      "--bg-subtle":        "#292518",
      "--bg-emphasis":      "#3a3420",
      "--border":           "#4a4228",
      "--severity-secondary-bg":     "#292518",
      "--severity-secondary-text":   "#fde68a",
      "--severity-secondary-border": "#4a4228",
      ...severityDark,
    },
  },
};

// ─── Emerald ───────────────────────────────────────────────
export const emerald: ThemeDefinition = {
  name: "emerald",
  light: {
    tokens: {
      "--primary":          "#10b981",
      "--primary-hover":    "#059669",
      "--primary-active":   "#047857",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#ecfdf5",
      "--primary-100":      "#d1fae5",
      "--border-focus":     "#10b981",
      "--text-color":       "#1e293b",
      "--text-muted":       "#64748b",
      "--bg":               "#ffffff",
      "--bg-subtle":        "#ecfdf5",
      "--bg-emphasis":      "#d1fae5",
      "--border":           "#a7f3d0",
      "--severity-secondary-bg":     "#f1f5f9",
      "--severity-secondary-text":   "#475569",
      "--severity-secondary-border": "#e2e8f0",
      ...severityLight,
    },
  },
  dark: {
    tokens: {
      "--primary":          "#34d399",
      "--primary-hover":    "#10b981",
      "--primary-active":   "#059669",
      "--primary-contrast": "#ffffff",
      "--primary-50":       "#022c22",
      "--primary-100":      "#064e3b",
      "--border-focus":     "#34d399",
      "--text-color":       "#ecfdf5",
      "--text-muted":       "#6ee7b7",
      "--bg":               "#022c22",
      "--bg-subtle":        "#064e3b",
      "--bg-emphasis":      "#065f46",
      "--border":           "#065f46",
      "--severity-secondary-bg":     "#064e3b",
      "--severity-secondary-text":   "#a7f3d0",
      "--severity-secondary-border": "#059669",
      ...severityDark,
    },
  },
};

// ─── Registry ──────────────────────────────────────────────
export const builtinThemes: Record<string, ThemeDefinition> = {
  aura, lara, nora, rose, amber, emerald,
};

export type BuiltinThemeName = keyof typeof builtinThemes;