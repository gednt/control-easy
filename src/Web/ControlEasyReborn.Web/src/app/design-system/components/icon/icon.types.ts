/** App-level icon names aligned with mockup/assets/icons.js and shell usage. */
export type LucideIconName =
  | 'dashboard'
  | 'users'
  | 'user'
  | 'building'
  | 'calendar'
  | 'car'
  | 'briefcase'
  | 'settings'
  | 'search'
  | 'bell'
  | 'sun'
  | 'moon'
  | 'menu'
  | 'eye'
  | 'eye-off'
  | 'x'
  | 'plus'
  | 'refresh'
  | 'more-horizontal'
  | 'edit'
  | 'trash-2'
  | 'log-out'
  | 'chevron-down'
  | 'chevron-left'
  | 'chevron-right'
  | 'check'
  | 'info'
  | 'alert-triangle'
  | 'alert-circle'
  | 'home'
  | 'layout-grid'
  | 'camera'
  | 'upload'
  | 'image';

const ICON_ALIASES: Partial<Record<LucideIconName, string>> = {
  dashboard: 'layout-dashboard',
  refresh: 'refresh-cw',
  edit: 'pencil',
  'more-horizontal': 'ellipsis',
  'alert-triangle': 'triangle-alert',
  'alert-circle': 'circle-alert',
  home: 'house',
};

export function resolveLucideIconName(name: LucideIconName): string {
  return ICON_ALIASES[name] ?? name;
}
