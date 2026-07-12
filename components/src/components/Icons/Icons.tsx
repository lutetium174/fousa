import { type JSX, type Component, splitProps, mergeProps } from "solid-js";

export type IconProps = JSX.SvgSVGAttributes<SVGSVGElement> & {
  size?: number;
  color?: string;
  spin?: boolean;
  spinDuration?: number; // seconds
  paused?: boolean;
  rotate?: number; // degrees, single value
  spinDirection?: "normal" | "reverse";
  ariaLabel?: string;
};

export const iconBase = (
  path: () => JSX.Element,
  displayName: string,
): Component<IconProps> => {
  const Comp: Component<IconProps> = (rawProps) => {
    const props = mergeProps(
      {
        size: 20,
        color: "currentColor",
        spin: false,
        spinDuration: 1,
        paused: false,
        rotate: undefined,
        spinDirection: "normal",
        ariaLabel: undefined,
      },
      rawProps,
    );

    const [local, rest] = splitProps(props, [
      "size",
      "color",
      "class",
      "spin",
      "spinDuration",
      "paused",
      "rotate",
      "spinDirection",
      "ariaLabel",
      "style",
    ]);

    const inlineStyle: JSX.CSSProperties = {
      ...(local.style as object),
    };

    if (local.spin) {
      inlineStyle.animation = `icon-spin ${local.spinDuration}s linear infinite`;
      inlineStyle["animation-play-state"] = local.paused ? "paused" : "running";
      inlineStyle["animation-direction"] = local.spinDirection;

      inlineStyle["transform-origin"] = "50% 50%";
      inlineStyle["will-change"] = "transform";
    } else {
      if (typeof local.rotate === "number") {
        inlineStyle.transform = `rotate(${local.rotate}deg)`;
      }
    }

    const accessibilityProps =
      local.ariaLabel != null
        ? { role: "img", "aria-label": local.ariaLabel }
        : { "aria-hidden": "true" };

    const classes = `${local.class ?? ""}`.trim();

    return (
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width={local.size}
        height={local.size}
        viewBox="0 0 24 24"
        fill="none"
        stroke={local.color}
        stroke-width={1.75}
        stroke-linecap="round"
        stroke-linejoin={"round"}
        class={classes}
        style={inlineStyle}
        //{...accessibilityProps}
        {...rest}
      >
        {path()}
      </svg>
    );
  };

  (Comp as any).displayName = displayName;
  return Comp;
};

export const HomeIcon = iconBase(
  () => (
    <>
      <path d="M3 9.75L12 3l9 6.75V20a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V9.75z" />
      <path d="M9 21V12h6v9" />
    </>
  ),
  "Home",
);

export const ChatIcon = iconBase(
  () => (
    <>
      <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
    </>
  ),
  "Chat",
);

export const RepeatIcon = iconBase(
  () => (
    <>
      <polyline points="17 1 21 5 17 9" />
      <path d="M3 11V9a4 4 0 0 1 4-4h14" />
      <polyline points="7 23 3 19 7 15" />
      <path d="M21 13v2a4 4 0 0 1-4 4H3" />
    </>
  ),
  "Repeat",
);

export const FavouriteIcon = iconBase(
  () => (
    <>
      <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
    </>
  ),
  "Favourite",
);

export const SearchIcon = iconBase(
  () => (
    <>
      <circle cx="11" cy="11" r="8" />
      <line x1="21" y1="21" x2="16.65" y2="16.65" />
    </>
  ),
  "Search",
);

export const NotificationIcon = iconBase(
  () => (
    <>
      <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
      <path d="M13.73 21a2 2 0 0 1-3.46 0" />
    </>
  ),
  "Notification",
);

export const BookmarkIcon = iconBase(
  () => (
    <>
      <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z" />
    </>
  ),
  "Bookmark",
);

export const ReplyIcon = iconBase(
  () => (
    <>
      <polyline points="9 17 4 12 9 7" />
      <path d="M20 18v-2a4 4 0 0 0-4-4H4" />
    </>
  ),
  "Reply",
);

// --- Navigation Icons ---
export const ArrowLeftIcon = iconBase(
  () => (
    <>
      <line x1="19" y1="12" x2="5" y2="12" />
      <polyline points="12 19 5 12 12 5" />
    </>
  ),
  "ArrowLeft",
);
export const ArrowRightIcon = iconBase(
  () => (
    <>
      <line x1="5" y1="12" x2="19" y2="12" />
      <polyline points="12 5 19 12 12 19" />
    </>
  ),
  "ArrowRight",
);
export const ArrowUpIcon = iconBase(
  () => (
    <>
      <line x1="12" y1="19" x2="12" y2="5" />
      <polyline points="5 12 12 5 19 12" />
    </>
  ),
  "ArrowUp",
);
export const ArrowDownIcon = iconBase(
  () => (
    <>
      <line x1="12" y1="5" x2="12" y2="19" />
      <polyline points="19 12 12 19 5 12" />
    </>
  ),
  "ArrowDown",
);
export const ChevronLeftIcon = iconBase(
  () => (
    <>
      <polyline points="15 18 9 12 15 6" />
    </>
  ),
  "ChevronLeft",
);
export const ChevronRightIcon = iconBase(
  () => (
    <>
      <polyline points="9 18 15 12 9 6" />
    </>
  ),
  "ChevronRight",
);
export const ChevronUpIcon = iconBase(
  () => (
    <>
      <polyline points="18 15 12 9 6 15" />
    </>
  ),
  "ChevronUp",
);
export const ChevronDownIcon = iconBase(
  () => (
    <>
      <polyline points="6 9 12 15 18 9" />
    </>
  ),
  "ChevronDown",
);
export const MenuIcon = iconBase(
  () => (
    <>
      <line x1="3" y1="12" x2="21" y2="12" />
      <line x1="3" y1="6" x2="21" y2="6" />
      <line x1="3" y1="18" x2="21" y2="18" />
    </>
  ),
  "Menu",
);
export const CloseIcon = iconBase(
  () => (
    <>
      <line x1="18" y1="6" x2="6" y2="18" />
      <line x1="6" y1="6" x2="18" y2="18" />
    </>
  ),
  "Close",
);
export const PlusIcon = iconBase(
  () => (
    <>
      <line x1="12" y1="5" x2="12" y2="19" />
      <line x1="5" y1="12" x2="19" y2="12" />
    </>
  ),
  "Plus",
);
export const MinusIcon = iconBase(
  () => (
    <>
      <line x1="5" y1="12" x2="19" y2="12" />
    </>
  ),
  "Minus",
);

// --- UI & Controls Icons ---
export const SettingsIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="3" />
      <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z" />
    </>
  ),
  "Settings",
);
export const HelpIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <line x1="12" y1="16" x2="12" y2="12" />
      <line x1="12" y1="8" x2="12.01" y2="8" />
    </>
  ),
  "Help",
);
export const InfoIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <line x1="12" y1="16" x2="12" y2="12" />
      <line x1="12" y1="8" x2="12.01" y2="8" />
    </>
  ),
  "Info",
);
export const WarningIcon = iconBase(
  () => (
    <>
      <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
      <line x1="12" y1="9" x2="12" y2="13" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </>
  ),
  "Warning",
);
export const ErrorIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <line x1="15" y1="9" x2="9" y2="15" />
      <line x1="9" y1="9" x2="15" y2="15" />
    </>
  ),
  "Error",
);
export const SuccessIcon = iconBase(
  () => (
    <>
      <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
      <polyline points="22 4 12 14.01 9 11.01" />
    </>
  ),
  "Success",
);

// --- File & Document Icons ---
export const FileIcon = iconBase(
  () => (
    <>
      <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z" />
      <polyline points="13 2 13 9 20 9" />
    </>
  ),
  "File",
);
export const FileTextIcon = iconBase(
  () => (
    <>
      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
      <polyline points="14 2 14 8 20 8" />
      <line x1="16" y1="13" x2="8" y2="13" />
      <line x1="16" y1="17" x2="8" y2="17" />
    </>
  ),
  "FileText",
);
export const FolderIcon = iconBase(
  () => (
    <>
      <path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z" />
    </>
  ),
  "Folder",
);
export const DownloadIcon = iconBase(
  () => (
    <>
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
      <polyline points="7 10 12 15 17 10" />
      <line x1="12" y1="15" x2="12" y2="3" />
    </>
  ),
  "Download",
);
export const UploadIcon = iconBase(
  () => (
    <>
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
      <polyline points="17 8 12 3 7 8" />
      <line x1="12" y1="3" x2="12" y2="15" />
    </>
  ),
  "Upload",
);

// --- Action Icons ---
export const EditIcon = iconBase(
  () => (
    <>
      <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
      <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
    </>
  ),
  "Edit",
);
export const DeleteIcon = iconBase(
  () => (
    <>
      <path d="M21 4H8l-7 8v11a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V4z" />
      <line x1="10" y1="11" x2="10" y2="17" />
      <line x1="14" y1="11" x2="14" y2="17" />
    </>
  ),
  "Delete",
);
export const SaveIcon = iconBase(
  () => (
    <>
      <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" />
      <polyline points="17 21 17 13 7 13 7 21" />
      <polyline points="7 3 7 8 15 8" />
    </>
  ),
  "Save",
);
export const RefreshIcon = iconBase(
  () => (
    <>
      <path d="M23 4v6h-6" />
      <path d="M22.5 21.5a10 10 0 1 1-15-7.5 10 10 0 0 1 15 7.5z" />
    </>
  ),
  "Refresh",
);
export const FilterIcon = iconBase(
  () => (
    <>
      <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
    </>
  ),
  "Filter",
);
export const SortIcon = iconBase(
  () => (
    <>
      <line x1="3" y1="6" x2="3" y2="6" />
      <line x1="3" y1="12" x2="3" y2="12" />
      <line x1="3" y1="18" x2="3" y2="18" />
    </>
  ),
  "Sort",
);
export const SortUpIcon = iconBase(
  () => (
    <>
      <polyline points="18 15 12 9 6 15" />
    </>
  ),
  "SortUp",
);
export const SortDownIcon = iconBase(
  () => (
    <>
      <polyline points="6 9 12 15 18 9" />
    </>
  ),
  "SortDown",
);

// --- Media & Communication Icons ---
export const ImageIcon = iconBase(
  () => (
    <>
      <rect x="3" y="3" width="18" height="18" rx="2" ry="2" />
      <line x1="3" y1="9" x2="21" y2="9" />
      <line x1="9" y1="21" x2="9" y2="9" />
    </>
  ),
  "Image",
);
export const CameraIcon = iconBase(
  () => (
    <>
      <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z" />
      <circle cx="12" cy="13" r="4" />
    </>
  ),
  "Camera",
);
export const VideoIcon = iconBase(
  () => (
    <>
      <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z" />
      <line x1="8" y1="21" x2="8" y2="5" />
      <line x1="16" y1="21" x2="16" y2="5" />
    </>
  ),
  "Video",
);
export const MailIcon = iconBase(
  () => (
    <>
      <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
      <polyline points="22,6 12,13 2,6" />
    </>
  ),
  "Mail",
);
export const PhoneIcon = iconBase(
  () => (
    <>
      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
    </>
  ),
  "Phone",
);

// --- Business & Data Icons ---
export const UserIcon = iconBase(
  () => (
    <>
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </>
  ),
  "User",
);
export const UsersIcon = iconBase(
  () => (
    <>
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </>
  ),
  "Users",
);
export const CalendarIcon = iconBase(
  () => (
    <>
      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
      <line x1="16" y1="2" x2="16" y2="6" />
      <line x1="8" y1="2" x2="8" y2="6" />
      <line x1="3" y1="10" x2="21" y2="10" />
    </>
  ),
  "Calendar",
);
export const ClockIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <polyline points="12 6 12 12 16 14" />
    </>
  ),
  "Clock",
);
export const BellIcon = iconBase(
  () => (
    <>
      <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
      <path d="M13.73 21a2 2 0 0 1-3.46 0" />
    </>
  ),
  "Bell",
);
export const ShoppingCartIcon = iconBase(
  () => (
    <>
      <circle cx="9" cy="21" r="1" />
      <circle cx="20" cy="21" r="1" />
      <path d="M1 1h4l2.68 13.39C4.84 15.44 6.48 17 8.5 17h6.58c.9 0 1.68-.34 2.32-1H23v-2a2 2 0 0 0-2-2h-1.42c-.64-.64-1.28-1.32-1.82-2.14L13 3H5.82C4.84 4.06 4 5.5 4 7v11" />
    </>
  ),
  "ShoppingCart",
);
export const TagIcon = iconBase(
  () => (
    <>
      <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" />
      <line x1="7" y1="7" x2="7.01" y2="7" />
    </>
  ),
  "Tag",
);
export const StarIcon = iconBase(
  () => (
    <>
      <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
    </>
  ),
  "Star",
);
export const StarFillIcon = iconBase(
  () => (
    <>
      <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
    </>
  ),
  "StarFill",
);

// --- View & Display Icons ---
export const EyeIcon = iconBase(
  () => (
    <>
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
      <circle cx="12" cy="12" r="3" />
    </>
  ),
  "Eye",
);
export const EyeOffIcon = iconBase(
  () => (
    <>
      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
      <line x1="1" y1="1" x2="23" y2="23" />
    </>
  ),
  "EyeOff",
);
export const ViewIcon = iconBase(
  () => (
    <>
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
      <circle cx="12" cy="12" r="3" />
    </>
  ),
  "View",
);
export const HiddenIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <circle cx="12" cy="12" r="3" />
    </>
  ),
  "Hidden",
);
export const GridIcon = iconBase(
  () => (
    <>
      <rect x="3" y="3" width="7" height="7" />
      <rect x="14" y="3" width="7" height="7" />
      <rect x="14" y="14" width="7" height="7" />
      <rect x="3" y="14" width="7" height="7" />
    </>
  ),
  "Grid",
);
export const ListIcon = iconBase(
  () => (
    <>
      <line x1="8" y1="6" x2="21" y2="6" />
      <line x1="8" y1="12" x2="21" y2="12" />
      <line x1="8" y1="18" x2="21" y2="18" />
      <line x1="3" y1="6" x2="3.01" y2="6" />
      <line x1="3" y1="12" x2="3.01" y2="12" />
      <line x1="3" y1="18" x2="3.01" y2="18" />
    </>
  ),
  "List",
);

// --- System & Status Icons ---
export const LoaderIcon = iconBase(
  () => (
    <>
      <line x1="12" y1="2" x2="12" y2="6" />
      <line x1="12" y1="18" x2="12" y2="22" />
      <line x1="4.93" y1="4.93" x2="7.76" y2="7.76" />
      <line x1="16.24" y1="16.24" x2="19.07" y2="19.07" />
      <line x1="2" y1="12" x2="6" y2="12" />
      <line x1="18" y1="12" x2="22" y2="12" />
      <line x1="4.93" y1="19.07" x2="7.76" y2="16.24" />
      <line x1="16.24" y1="7.76" x2="19.07" y2="4.93" />
    </>
  ),
  "Loader",
);
export const SpinnerIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" stroke-width="3" fill="none" class="spinner-ring" />
    </>
  ),
  "Spinner",
);
export const CheckCircleIcon = iconBase(
  () => (
    <>
      <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
      <polyline points="22 4 12 14.01 9 11.01" />
    </>
  ),
  "CheckCircle",
);
export const TimesCircleIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <line x1="15" y1="9" x2="9" y2="15" />
      <line x1="9" y1="9" x2="15" y2="15" />
    </>
  ),
  "TimesCircle",
);
export const QuestionCircleIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </>
  ),
  "QuestionCircle",
);
export const ExclamationCircleIcon = iconBase(
  () => (
    <>
      <circle cx="12" cy="12" r="10" />
      <line x1="12" y1="8" x2="12" y2="12" />
      <line x1="12" y1="16" x2="12.01" y2="16" />
    </>
  ),
  "ExclamationCircle",
);
